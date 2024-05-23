//Holds algorithms that operate on disease states
using System;

namespace CJSim {
	class SimAlgorithms {
		public delegate float ReactionFunctionTypes(int stateIdx, ref DiseaseState state, SimModel model, int[] argv);
		//Very important number! The static initializer will error out if this is too low!
		public const int propensityFunctionTypeCount = 3;

		public static ReactionFunctionTypes[] reactionFuncTypes;

		//Runs the correct propensity function given the magic numbers to describe it
		public static float dispatchPropensityFunction(int stateIdx, ref DiseaseState state, SimModel model, int[] argv) {
			return reactionFuncTypes[argv[0]](stateIdx, ref state, model, argv);
		}

		//Static initializer
		//If this function errors and the static initializer ran on a thread, unity will not display any error message
		static SimAlgorithms() {
			//Set up propensity functions
			reactionFuncTypes = new ReactionFunctionTypes[propensityFunctionTypeCount];

			//Basic type, state (idx 1) * param (idx 2)
			reactionFuncTypes[0] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				return (float)state.state[argv[1]] * model.parameters[argv[2]];
			};

			//Grey arrow, page 16 of the book, thing that depends on the density of infected
			// (param * state1 * state2) / NumberOfPeopleInState
			// (idx3 * idx2 * idx1) / Num
			reactionFuncTypes[1] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				return model.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
			};

			//Here we would include the neighbor movement factor, which also needs some neighbor getting function and the cell conectivity param
			//We'll put all of that into simulation model I think?
			// (param(beta) * state1(sus)) * (neighborStuff * state2(infected))
			// (idx1 * idx2) * (neighborStuff * idx3)
			reactionFuncTypes[2] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				int[] neighbors = model.movementModel.getNeighbors(stateIdx);

				float neighborFactor = 0.0f;
				for (int q = 0; q < neighbors.Length; q++) {
					throw new System.NotImplementedException();
				}
				return model.parameters[argv[1]] * state.state[argv[2]] * (neighborFactor);
			};
		}

		//Does a basic deterministic tick of a disease state
		public static void deterministicTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, float reqTime) {
			writeState.setTo(readState);
			for (int q = 0; q < model.reactionCount; q++) {
				float res = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]) * reqTime;
				writeState.state[model.stoichiometry[q].Item2] += (int)res;
				writeState.state[model.stoichiometry[q].Item1] -= (int)res;
			}

			writeState.timeSimulated += reqTime;
		}

		//Returns the sum of all the propensity functions for this state
		private static float sumOfPropensityFunctions(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model) {
			float res = 0.0f;
			for (int q = 0; q < model.reactionCount; q++) {
				res += dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]);
			}
			return res;
		}

		//Does a single reaction via the gillespie algorithm
		public static void gillespieTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, Random random) {
			writeState.setTo(readState);
			float sumProps = sumOfPropensityFunctions(stateIdx, ref readState, ref writeState, model);
			float sumPropsR2 = sumProps * (float)random.NextDouble();
			float tau = (float)((1.0 / sumProps) * Math.Log(1.0 / random.NextDouble()));
			
			float sum = 0.0f;
			for (int q = 0; q < model.reactionCount; q++) {
				float currProp = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]);
				sum += currProp;
				if (sum > sumPropsR2) {
					//This is the reaction we do
					writeState.state[model.stoichiometry[q].Item1] -= 1;
					writeState.state[model.stoichiometry[q].Item2] += 1;
					break;
				}
			}
			writeState.timeSimulated += tau;
		}
	}
}