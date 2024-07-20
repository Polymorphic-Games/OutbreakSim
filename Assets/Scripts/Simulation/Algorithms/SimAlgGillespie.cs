using System;

namespace CJSim {
	public class SimAlgGillespie : SimModelAlgorithm {

		public SimAlgGillespie(SimModel model) : base(model) {

		}

		public override double getNextReactionsTime(ref DiseaseState readState, int stateIdx) {
			double sumProps = sumOfPropensityFunctions(ref readState, stateIdx);
			return ((1.0 / sumProps) * Math.Log(1.0 / ThreadSafeRandom.Next()));
		}
		//Perform reactions, if the model cares about the time you can include it here
		public override void performReactions(double time) {

		}
		//Does a full tick, the time parameter may or may not be used
		public override void fullTick(double time) {

		}
	}
}