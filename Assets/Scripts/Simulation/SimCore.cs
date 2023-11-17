

//Holds the core of the simulation
//Operates on an array of cells and handles movement between them

using System;
using System.Threading;

namespace CJSim {
	public class SimCore {
		#region Events

		//Called before the cell update process starts
		public event Action preCellUpdates;

		//Called in a thread before a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> preCellThreadedUpdate;
		//Called in a thread after a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> postCellThreadedUpdate;

		//Called after every cell has been updated
		public event Action postCellUpdates;


		//Called when the thread count changes, useful for classes that hook into our threads
		public event Action threadCountChanged;

		#endregion

		#region Properties

		//Check if the simulation is running
		public bool isRunning {
			get {
				throw new System.NotImplementedException();
			}
		}

		public int cellCount {
			get {
				return readCells.Length;
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
				_threadCount = value;
				threadCountChanged?.Invoke();
			}
			get {
				return _threadCount;
			}
		}

		#endregion

		#region Member Variables

		private int _threadCount;

		private Thread[] threads;

		public Cell[] readCells;
		private Cell[] writeCells;
		

		#endregion

		#region Functions

		//Initializes cell arrays in a very basic way
		//Anything complex needs to be done by you
		private void initCells(int _cellCount) {
			readCells = new Cell[_cellCount];
			writeCells = new Cell[_cellCount];
		}

		//Begins a new tick
		public void beginTick(float dt) {
			threads = new Thread[threadCount];
			
		}

		//If the processing for the current tick is done, clean up the things and invoke events
		public void tryEndTick() {

		}

		//Forces a tick to end, may cause stuttering as threads are joined
		public void forceEndTick() {

		}


		//The joke lives on (although I doubt it is comprehensible as a joke anymore)
		//Ticks the simulation in full, synchronously
		//Still makes threads and fires events, things just happen faster and will likely cause a stutter in framerate
		public void tickSimulation(float dt) {
			beginTick(dt);
			forceEndTick();
		}

		#endregion

		//Basic simulation isnitialization
		public SimCore(SimModel model, int cellCount, int? threadCount = null) {
			if (threadCount == null) {
				threadCount = System.Environment.ProcessorCount;
			}
			initCells(cellCount);
		}

	}
}
