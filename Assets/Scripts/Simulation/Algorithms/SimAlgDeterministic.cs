
namespace CJSim {
	public class SimAlgDeterministic : SimModelAlgorithm {
		public double timestep {get; set;}
		public SimAlgDeterministic(double step) : base() {
			timestep = step;
		}

		public override double getNextReactionTime(int stateIdx) {
			return 0.0;
		}
		public override void performSingleReaction(int stateIdx, ref DiseaseState writeState) {
			//Could just do one tick of the timestep that makes sense right
			throw new System.NotImplementedException();
		}

		//Perform reactions, if the model cares about the time you can include it here
		
		
		//Perform reactions, if the model cares about the time you can include it here
		public override void performReactionsWithTime(int stateIdx, ref DiseaseState writeState, double time) {
			writeState.setTo(model.properties.readCells[stateIdx]);
			//Needs to go by timestep sry bb
			throw new System.NotImplementedException();

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
