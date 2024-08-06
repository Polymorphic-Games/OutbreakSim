
namespace CJSim {
	public class SimAlgDeterministic : SimModelAlgorithm {
		public SimAlgDeterministic() : base() {
			//No initialization needed
		}

		public override double getNextReactionsTime(int stateIdx, double time) {
			return time;
		}
		
		//Perform reactions, if the model cares about the time you can include it here
		public override void performReactions(int stateIdx, ref DiseaseState writeState, double time) {
			writeState.setTo(model.properties.readCells[stateIdx]);

			for (int q = 0; q < model.properties.reactionCount; q++) {
				//Calculate this propensity function value, also add .5 for rounding
				double res = ((double)dispatchPropensityFunction(
					ref model.properties.readCells[stateIdx], stateIdx, model.properties.reactionFunctionDetails[q]
					) * time) + 0.5;
				writeState.state[model.properties.stoichiometry[q].Item2] += (int)res;
				writeState.state[model.properties.stoichiometry[q].Item1] -= (int)res;
			}

			writeState.timeSimulated += time;
		}
	}
}