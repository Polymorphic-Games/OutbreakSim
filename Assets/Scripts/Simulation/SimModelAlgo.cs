
namespace CJSim {
	public abstract class SimModelAlgorithm {
		// Abstract Public Functions \\

		//Not used by the deterministic model, but for many will return the next timestep to go to
		public abstract double getNextReactionsTime(ref DiseaseState readState, int stateIdx);
		//Perform reactions, if the model cares about the time you can include it here
		public abstract void performReactions(ref DiseaseState readState, int stateIdx, double time);
		//Does a full tick, the time parameter may or may not be used
		public abstract void fullTick(double time);

		public SimModel model { get; private set; }
		protected SimModelAlgorithm(SimModel model) {
			this.model = model;

			reactionFuncTypes[0] = propensityFunction0;
			reactionFuncTypes[1] = propensityFunction1;
			reactionFuncTypes[2] = propensityFunction2;
		}

		// Propensity Functions \\

		public delegate float ReactionFunctionTypes(int stateIdx, ref DiseaseState state, int[] argv);
		//Very important number! The static initializer will error out if this is too low!
		public const int propensityFunctionTypeCount = 3;

		protected ReactionFunctionTypes[] reactionFuncTypes;

		//The basic one, just param * state
		//(param * state)
		//(idx2 * idx1)
		public float propensityFunction0(int stateIdx, ref DiseaseState state, int[] argv) {
			return (float)state.state[argv[1]] * model.properties.parameters[argv[2]];
		}
		//Grey arrow, page 16 of the book, thing that depends on the density of infected
		// (param * state1 * state2) / NumberOfPeopleInState
		// (idx3 * idx2 * idx1) / Num
		public float propensityFunction1(int stateIdx, ref DiseaseState state, int[] argv) {
			return model.properties.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
		}
		//Movement parameter, how many of my people bump into my neighbors?
		// (param(beta) * state1(sus)) * (neighborStuff * state2(infected))
		// (idx1 * idx2) * (neighborStuff * idx3)
		public float propensityFunction2(int stateIdx, ref DiseaseState state, int[] argv) {
			int[] neighbors = model.movementModel.getNeighbors(stateIdx);

			float neighborFactor = 0.0f;
			for (int q = 0; q < neighbors.Length; q++) {
				neighborFactor += model.movementModel.getCellConnectivity(neighbors[q], stateIdx)
				* ((float)model.properties.readCells[neighbors[q]].state[argv[3]] / state.numberOfPeople);
			}
			return model.properties.parameters[argv[1]] * state.state[argv[2]] * (neighborFactor);
		}

		// Helper Functions \\

		public float dispatchPropensityFunction(ref DiseaseState readState, int stateIdx, int[] argv) {
			return reactionFuncTypes[argv[0]](stateIdx, ref readState, argv);
		}

		//Returns the sum of all the propensity functions for this state
		public float sumOfPropensityFunctions(ref DiseaseState readState, int stateIdx) {
			float res = 0.0f;
			for (int q = 0; q < model.properties.reactionCount; q++) {
				res += dispatchPropensityFunction(ref readState, stateIdx, model.properties.reactionFunctionDetails[q]);
			}
			return res;
		}

		//Gets a parameter, optionally with parameter noise cjnote doesn't do that right now
		private float getParam(int idx) {
			return model.properties.parameters[idx];
		}
	}
}