

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

		//Get/set thread count
		//Trying to set the threadcount while the simulation is running will update the thread count at the next end of tick
		public int threadCount {
			set {
				if (isRunning) {
					
				}					
				_threadCount = value;
				threadCountChanged?.Invoke();
			}
		}

		#endregion

		#region Member Variables

		int _threadCount;

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


		//The joke lives on
		//Ticks the simulation in full, synchronously
		//Still makes threads and fires events, things just happen faster and will likely cause a stutter in framerate
		public void tickSimulation() {

		}

		#endregion

		//Basic simulation isnitialization
		public SimCore(int cellCount) {
		}

	}
}
