

//Holds algorithms that operate on disease states


namespace CJSim {
	class SimAlgorithms {
		public delegate float ReactionFunctionTypes(ref DiseaseState state, ref SimModel model, int[] argv);
		public const int propensityFunctionTypeCount = 2;

		public static ReactionFunctionTypes[] reactionFuncTypes;

		//When railroading dt with tau leaping, how much are we willing to raise tau to meet the railroad demand
		//Also basically a "minimum" amount, you probably shouldn't lower from .1
		const float maxTauRaiseAmount = 0.1f;

		//Runs the correct propensity function given the magic numbers to describe it
		public static float dispatchPropensityFunction(ref DiseaseState state, ref SimModel model, int[] argv) {
			return reactionFuncTypes[argv[0]](ref state, ref model, argv);
		}

		//Static initializer
		static SimAlgorithms() {
			//Set up propensity functions
			reactionFuncTypes = new ReactionFunctionTypes[propensityFunctionTypeCount];

			//Basic type, state (idx 1) * param (idx 2)
			reactionFuncTypes[0] = (ref DiseaseState state, ref SimModel model, int[] argv) => {
				return (float)state.state[argv[1]] * model.parameters[argv[2]];
			};

			//Grey arrow, page 16 of the book, thing that depends on the density of infected
			// (param * state1 * state2) / NumberOfPeopleInState
			// (idx3 * idx2 * idx1) / Num
			reactionFuncTypes[1] = (ref DiseaseState state, ref SimModel model, int[] argv) => {
				return model.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
			};

			//Here we would include the neighbor movement factor, which also needs some neighbor getting function and the cell conectivity param
			//We'll put all of that into simulation model I think?
		}


		public static void deterministicTick(ref DiseaseState readState, ref DiseaseState writeState, SimModel model, float reqTime) {
			writeState.setTo(readState);

			for (int q = 0; q < model.reactionCount; q++) {
				float res = dispatchPropensityFunction(ref writeState, ref model, model.reactionFunctionDetails[q]) * reqTime;
				for (int stoich = 0; stoich < model.compartmentCount; stoich++) {
					//writeState.state[stoich] += (int)(model.stoichiometry[q, stoich] * res);
					writeState.state[model.stoichiometry[stoich].Item2] += (int)res;
					writeState.state[model.stoichiometry[stoich].Item1] -= (int)res;
				}
			}
		}
	}
}