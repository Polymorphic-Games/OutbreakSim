
//The head honcho of the simulation collection of scripts
//Contains everything you need to run a nice simulation

namespace CJSim {
	public class Simulation {
		public SimCore core;
		public SimClock clock;

		public SimModel model {
			get {
				return core.model;
			}
		}

		public Simulation(SimModel model, int cellCount) {
			core = new SimCore(model, cellCount);
		}

		public Simulation(SimCore core) {
			this.core = core;
		}
	}
}
