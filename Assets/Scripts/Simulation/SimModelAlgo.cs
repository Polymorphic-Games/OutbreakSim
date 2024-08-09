
namespace CJSim {
	public abstract class SimModelAlgorithm {
		// Abstract Public Functions \\

		//Not used by the deterministic model, but for many will return the next timestep to go to
		//Pass the time parameter to request a specific time
		public abstract double getNextReactionTime(int stateIdx);
		//Performs a single reaction, if the model allows for it
		public abstract void performSingleReaction(int stateIdx, ref DiseaseState writeState);

		//Max time related algorithm functions

		//Perform reactions, if the model lets you specify the time it will go until it reaches that time as a maximum time to simulate
		//If the model choosing timesteps itself it will not exceed the given time
		public abstract void performReactionsWithTime(int stateIdx, ref DiseaseState writeState, double time);
		//Does a full tick with time
		public void fullTick(int stateIdx, ref DiseaseState writeState, double time) {
			performReactionsWithTime(stateIdx, ref writeState, time);
		}

		public SimModel model { get; private set; }
		protected SimModelAlgorithm() {
			//I suppose this could be a list...
			reactionFuncTypes = new ReactionFunctionTypes[propensityFunctionTypeCount];
			reactionFuncTypes[0] = propensityFunction0;
			reactionFuncTypes[1] = propensityFunction1;
			reactionFuncTypes[2] = propensityFunction2;
		}

		//Please call this when we assemble a SimModel, algos needs to know a lot of stuff
		//Couldn't figure out a different way of doing this what can I say
		public virtual void onModelCreate(SimModel model) {
			this.model = model;
		}

		// Propensity Functions \\

		public delegate double ReactionFunctionTypes(int stateIdx, ref DiseaseState state, int[] argv);
		//Very important number! The static initializer will error out if this is too low!
		public const int propensityFunctionTypeCount = 3;

		protected ReactionFunctionTypes[] reactionFuncTypes;

		//The basic one, just param * state
		//(param * state)
		//(idx2 * idx1)
		public double propensityFunction0(int stateIdx, ref DiseaseState state, int[] argv) {
			return state.state[argv[1]] * model.properties.parameters[argv[2]];
		}
		//Grey arrow, page 16 of the book, thing that depends on the density of infected
		// (param * state1 * state2) / NumberOfPeopleInState
		// (idx3 * idx2 * idx1) / Num
		public double propensityFunction1(int stateIdx, ref DiseaseState state, int[] argv) {
			return model.properties.parameters[argv[3]] * ((state.state[argv[2]] * state.state[argv[1]]) / state.numberOfPeople);
		}
		//Movement parameter, how many of my people bump into my neighbors?
		// (param(beta) * state1(sus)) * (neighborStuff * state2(infected))
		// (idx1 * idx2) * (neighborStuff * idx3)
		public double propensityFunction2(int stateIdx, ref DiseaseState state, int[] argv) {
			int[] neighbors = model.movementModel.getNeighbors(stateIdx);

			double neighborFactor = 0.0f;
			for (int q = 0; q < neighbors.Length; q++) {
				neighborFactor += model.movementModel.getCellConnectivity(neighbors[q], stateIdx)
				* (model.properties.readCells[neighbors[q]].state[argv[3]] / state.numberOfPeople);
			}
			return model.properties.parameters[argv[1]] * state.state[argv[2]] * neighborFactor;
		}

		public static int getOrderOfReaction(int reactionId) {
			switch(reactionId) {
				case 0: return 1;
				case 1: return 2;
				default: ThreadLogger.Log("Default case in getOrderOfReaction switch");
				throw new System.Exception();
			}
		}

		// Helper Functions \\

		public double dispatchPropensityFunction(ref DiseaseState readState, int stateIdx, int[] argv) {
			return reactionFuncTypes[argv[0]](stateIdx, ref readState, argv);
		}

		//Returns the sum of all the propensity functions for this state
		public double sumOfPropensityFunctions(int stateIdx) {
			double res = 0.0f;
			for (int q = 0; q < model.properties.reactionCount; q++) {
				res += dispatchPropensityFunction(ref model.properties.readCells[stateIdx], stateIdx, model.properties.reactionFunctionDetails[q]);
			}
			return res;
		}

		//Gets a parameter, optionally with parameter noise cjnote doesn't do that right now
		private double getParam(int idx) {
			return model.properties.parameters[idx];
		}
	}
}