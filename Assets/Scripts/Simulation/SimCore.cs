//Holds the core of the simulation
//Operates on an array of cells and handles movement between them

using System;
using System.Threading;

namespace CJSim {
	public class SimCore {
		#region Events

		//Called before the cell update process starts
		public event Action preCellUpdates;


		//Gonna need to test the performance impact of these two
		//Called in a thread before a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> preCellThreadedUpdate;
		//Called in a thread after a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> postCellThreadedUpdate;

		//Called after every cell has been updated
		public event Action postCellUpdates;


		//Called when the thread count changes, useful for classes that hook into our threads maybe idk
		public event Action threadCountChanged;

		#endregion

		#region Properties

		//Check if the simulation is running
		public bool isRunning {
			get {
				return threadFinishedHandles.Length != 0 && !WaitHandle.WaitAll(threadFinishedHandles, 0);
			}
		}

		//Get/set thread count
		//Trying to set the threadcount while the simulation is running will error
		//Verify that the simulation is not running before updating this
		public int threadCount {
			set {
				if (isRunning) {
					throw new Exception("Can't update thread count while simulation is running");
				}
				//Check if thread count is actually changing
				if (_threadCount != value) {
					_threadCount = value;
					onThreadCountChange();
					threadCountChanged?.Invoke();
				}
			}
			get {
				return _threadCount;
			}
		}

		#endregion

		#region Member Variables

		private int _threadCount = -1;

		private Thread[] threads;
		//DT passed globally to all threads
		private float threadDT = 1.0f;
		//Set these to fire the threads
		private EventWaitHandle[] threadStartHandles;
		//Threads set these when they're done
		private EventWaitHandle[] threadFinishedHandles;

		public SimModel model {get; private set;}
		

		#endregion

		#region Functions

		private void onThreadCountChange() {
			//If there is anything to dispose of
			if (threads != null) {
				//Clean up the old threads
				for (int q = 0; q < threads.Length; q++) {
					threads[q].Abort();
				}
				//Clean up old wait handles
				foreach (EventWaitHandle q in threadStartHandles) {
					q.Dispose();
				}
				foreach (EventWaitHandle q in threadFinishedHandles) {
					q.Dispose();
				}
			}

			threads = new Thread[threadCount];
			threadStartHandles = new EventWaitHandle[threadCount];
			threadFinishedHandles = new EventWaitHandle[threadCount];
			for (int q = 0; q < threadCount; q++) {
				threadStartHandles[q] = new ManualResetEvent(false);
				threadFinishedHandles[q] = new ManualResetEvent(false);
				
				threads[q] = new Thread(threadUpdate);
				threads[q].Start(q);
			}
		}

		//Begins a new tick
		public void beginTick(float dt = 1.0f) {
			threadDT = dt;
			for (int q = 0; q < threadStartHandles.Length; q++) {
				threadStartHandles[q].Set();
			}
		}

		//If the processing for the current tick is done, clean up the things and invoke events
		public void tryEndTick() {
			//When threads are definitely done, let's do something
			if (!isRunning) {
				onThreadsDone();
			}
		}

		//Forces a tick to end, may cause stuttering as threads are waited on
		public void forceEndTick() {
			//First check that every thread has started
			for (int q = 0; q < threadStartHandles.Length; q++) {
				//Waiting for all of these to be false
				//So if true
				if (threadStartHandles[q].WaitOne(0)) {
					//Then wait a bit
					Thread.Sleep(1);
					forceEndTick();
					return;
				}
			}


			WaitHandle.WaitAll(threadFinishedHandles);
			onThreadsDone();
		}
		
		//Called when threads are finished for the tick
		public void onThreadsDone() {
			//Swap cell buffers
			DiseaseState[] tmp = model.properties.readCells;
			model.properties.readCells = model.properties.writeCells;
			model.properties.writeCells = tmp;
			postCellUpdates?.Invoke();
		}

		//The joke lives on (although I doubt it is comprehensible as a joke anymore)
		//Ticks the simulation in full, synchronously
		//Still makes threads and fires events, things just happen faster and will likely cause a stutter in framerate
		public void tickSimulation(float dt) {
			beginTick(dt);
			forceEndTick();
		}

		private void threadUpdate(object objIndex) {
			int index = (int)objIndex;
			//How many cells does each thread deal with?
			int blockSize = model.properties.cellCount + (threadCount - (model.properties.cellCount % threadCount));
			

			//Calculate our block
			int blockStart = index * blockSize;
			int blockEnd = (index + 1) * blockSize;
			blockEnd = blockEnd <= model.properties.cellCount ? blockEnd : model.properties.cellCount;

			Random random = new Random();
			while (true) {
				//Wait for update to be requested
				threadStartHandles[index].WaitOne();
				//Manual handles need to be reset
				threadStartHandles[index].Reset();
				threadFinishedHandles[index].Reset();
				

				//Update the cells here
				if (model.algorithm is SimAlgSingleThreaded) {
					SimAlgSingleThreaded alg = model.algorithm as SimAlgSingleThreaded;
					alg.updateAll();
				} else {
					for (int q = blockStart; q < blockEnd; q++) {
						model.algorithm.fullTick(q, ref model.properties.writeCells[q], threadDT);
					}
				}

				//Let the main thread know we've finished
				threadFinishedHandles[index].Set();
			}
		}

		#endregion

		//Basic simulation isnitialization
		public SimCore(SimModel simModel, int threadCountParam = -1) {
			//Initialize arrays to nothings
			threads = new Thread[0];
			threadStartHandles = new EventWaitHandle[0];
			threadFinishedHandles = new EventWaitHandle[0];

			if (threadCountParam <= 0) {
				threadCount = System.Environment.ProcessorCount;
			} else {
				threadCount = threadCountParam;
			}
			model = simModel;
		}

	}
}
