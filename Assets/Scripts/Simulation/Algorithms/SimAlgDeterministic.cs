
namespace CJSim {
	public class SimAlgDeterministic : SimModelAlgorithm {
		public double timestep {get; set;}
		public SimAlgDeterministic(SimModelProperties props, SimMovementModel movement, double step) : base(props, movement) {
			timestep = step;
		}

		public override double getNextReactionTime(int stateIdx, ref DiseaseState readState) {
			return 0.0;
		}
		public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double _ = 0.0) {
			writeState.setTo(readState);

			for (int q = 0; q < properties.reactionCount; q++) {
				//Calculate this propensity function value, also add .5 for rounding
				double res = ((double)dispatchPropensityFunction(
					ref readState, stateIdx, properties.reactionFunctionDetails[q]
					) * timestep);
				writeState.state[properties.stoichiometry[q].Item2] += res;
				writeState.state[properties.stoichiometry[q].Item1] -= res;
			}

			writeState.timeSimulated += timestep;
		}
	}
}
