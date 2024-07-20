using System;

namespace CJSim {
	public class SimAlgGillespie : SimModelAlgorithm {

		public SimAlgGillespie(SimModel model) : base(model) {

		}

		public override double getNextReactionsTime(ref DiseaseState readState, int stateIdx) {
			double sumProps = sumOfPropensityFunctions(ref readState, stateIdx);
			return ((1.0 / sumProps) * Math.Log(1.0 / ThreadSafeRandom.NextDouble()));
		}
		//Perform reactions, if the model cares about the time you can include it here
		public override void performReactions(ref DiseaseState readState, ref DiseaseState writeState, int stateIdx, double time) {
			writeState.setTo(readState);
			//Pick and do a reaction
			double sumProps = sumOfPropensityFunctions(ref readState, stateIdx);
			double sumPropsR2 = sumProps * ThreadSafeRandom.NextDouble();
			double sum = 0.0f;
			for (int q = 0; q < model.properties.reactionCount; q++) {
				double currProp = dispatchPropensityFunction(ref readState, stateIdx, model.properties.reactionFunctionDetails[q]);
				sum += currProp;
				if (sum > sumPropsR2) {
					//This is the reaction we do
					writeState.state[model.properties.stoichiometry[q].Item1] -= 1;
					writeState.state[model.properties.stoichiometry[q].Item2] += 1;
					writeState.timeSimulated += time;
					break;
				}
			}
			ThreadLogger.Log("This isn't supposed to happen plz fix");
		}
		//Does a full tick, the time parameter may or may not be used
		public override void fullTick(ref DiseaseState readState, ref DiseaseState writeState, int stateIdx, double time) {
			performReactions(ref readState, ref writeState, stateIdx, getNextReactionsTime(ref readState, stateIdx));
		}
	}
}