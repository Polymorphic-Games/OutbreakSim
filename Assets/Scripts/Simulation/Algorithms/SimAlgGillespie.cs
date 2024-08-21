using System;
using System.Xml;

namespace CJSim {
	public class SimAlgGillespie : SimModelAlgorithm {

		public SimAlgGillespie() : base() {
			//No initialization needed
		}

		public override double getNextReactionTime(int stateIdx, ref DiseaseState readState) {
			double sumProps = sumOfPropensityFunctions(stateIdx, ref readState);
			return ((1.0 / sumProps) * Math.Log(1.0 / ThreadSafeRandom.NextDouble()));
		}
		//Perform reactions, if the model cares about the time you can include it here
		public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState) {
			writeState.setTo(readState);
			//Pick and do a reaction
			double sumProps = sumOfPropensityFunctions(stateIdx, ref readState);
			double sumPropsR2 = sumProps * ThreadSafeRandom.NextDouble();
			double sum = 0.0;
			for (int q = 0; q < model.properties.reactionCount; q++) {
				double currProp = dispatchPropensityFunction(ref readState, stateIdx, model.properties.reactionFunctionDetails[q]);
				sum += currProp;
				if (sum >= sumPropsR2) {
					//This is the reaction we do
					writeState.state[model.properties.stoichiometry[q].Item1] -= 1;
					writeState.state[model.properties.stoichiometry[q].Item2] += 1;
					double step = getNextReactionTime(stateIdx, ref readState);
					writeState.timeSimulated += step;
					return;
				}
			}
			//If we didn't do a reaction, and therefore didn't return, the time should go to infinity because there is no next reaction
			writeState.timeSimulated = double.PositiveInfinity;
		}
	}
}