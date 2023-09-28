

//Holds the core of the simulation
//Operates on an array of cells and handles movement between them

using System;

namespace CJSim {
	class SimCore {
		#region Events

		//Called before the cell update process starts
		public event Action preCellUpdates;

		//Called in a thread before a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> preCellThreadUpdate;
		//Called in a thread after a specific cell is updated, passes the thread index and the cell index, respectively
		public event Action<Tuple<int,int>> postCellThreadUpdate;

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

		public Cell[] readCells;
		private Cell[] writeCells;
		

		#endregion

		#region Functions

		//Begins a new tick
		public void beginTick() {

		}

		//If the processing for the current tick is done, clean up the things and invoke events
		public void tryEndTick() {

		}

		//Forces a tick to end, may cause stuttering as threads are joined
		public void forceEndTick() {

		}


		//The joke lives on (although I doubt it is comprehensible anymore)
		//Ticks the simulation in full, synchronously
		//Still makes threads and fires events, things just happen faster and will likely cause a stutter in framerate
		public void tickSimulation() {

		}

		#endregion

		//Basic simulation isnitialization
		public SimCore(int cellCount, int? threadCount = null) {
			if (threadCount == null) {
				threadCount = System.Environment.ProcessorCount;
			}
		}

	}
}
