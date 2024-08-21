
namespace CJSim {
	public class SimAlgDeterministic : SimModelAlgorithm {
		public double timestep {get; set;}
		public SimAlgDeterministic(double step) : base() {
			timestep = step;
		}

		public override double getNextReactionTime(int stateIdx, ref DiseaseState readState) {
			return 0.0;
		}
		public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState) {
			writeState.setTo(readState);

			for (int q = 0; q < model.properties.reactionCount; q++) {
				//Calculate this propensity function value, also add .5 for rounding
				double res = ((double)dispatchPropensityFunction(
					ref readState, stateIdx, model.properties.reactionFunctionDetails[q]
					) * timestep);
				writeState.state[model.properties.stoichiometry[q].Item2] += res;
				writeState.state[model.properties.stoichiometry[q].Item1] -= res;
			}

			writeState.timeSimulated += timestep;
		}

		//Perform reactions, if the model cares about the time you can include it here
		
		
		//Perform reactions, if the model cares about the time you can include it here
		public override void performReactionsWithTime(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double time) {
			DiseaseState fakeRead = new DiseaseState(readState);
			for (double q = time; q >= timestep; q-=timestep) {
				performSingleReaction(stateIdx, ref fakeRead, ref writeState);
				//cjnote there has to be a faster way to do this, avoid copying
				fakeRead.setTo(writeState);
			}
		}
	}
}
