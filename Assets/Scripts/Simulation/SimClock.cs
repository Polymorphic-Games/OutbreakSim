using System.Diagnostics;
using System.Threading.Tasks;

//This class handles runnning the simulation properly
//Has options to run at fastest speed, specific speed, pause, etc.
//Not necessary to use this class for every single simulation, ones that want custom time control should not use this

namespace CJSim {
	public class SimClock {

		//Desired max speed of the simulation in ticks per second
		//value of 2 means 2 ticks per second, .5 means one tick per 2 seconds
		public float desiredSimSpeed = 1.0f;

		//Actual speed of the simulation (time it took to run the previous tick)
		public float actualSimSpeed {private set; get;}

		//Is the clock running, not the simulation, the clock
		public bool running {private set; get;}

		private Stopwatch stopwatch;
		private SimCore core;


		public void run() {
			running = true;
		}

		public void pause() {
			running = false;
		}

		public void onPreCellUpdates() {
			stopwatch.Reset();
			stopwatch.Start();
		}

		public void onPostCellUpdates() {
			stopwatch.Stop();
			actualSimSpeed = (float)stopwatch.Elapsed.TotalSeconds;
			//If we took less time than we needed to take to run the tick
			if (1.0f / actualSimSpeed > desiredSimSpeed) {
				//Then wait to do the next tick

				//The next 2 lines of code may or may not work with unity, as it is they don't technically work anyway
				//But even if they did technically work, I'm not sure how this would interact with unity
				//So we're not going to use it

				//float desiredSecondsPerTick = (1.0f / desiredSimSpeed);
				//Task.Delay((int)(1000 * (desiredSecondsPerTick - actualSimSpeed))).ContinueWith(startSimulation);
			} else {
				startSimulation();
			}
		}

		private void startSimulation() {
			stopwatch.Reset();
			core.beginTick();
		}

		//Clock needs a core
		public SimClock(SimCore core) {
			this.core = core;
			core.preCellUpdates += onPreCellUpdates;
			core.postCellUpdates += onPostCellUpdates;
		}
	}
}
