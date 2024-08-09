using System;
using System.Xml;

namespace CJSim {
	public class SimAlgGillespie : SimModelAlgorithm {

		public SimAlgGillespie() : base() {
			//No initialization needed
		}

		public override double getNextReactionTime(int stateIdx) {
			double sumProps = sumOfPropensityFunctions(stateIdx);
			return ((1.0 / sumProps) * Math.Log(1.0 / ThreadSafeRandom.NextDouble()));
		}
		//Perform reactions, if the model cares about the time you can include it here
		public override void performSingleReaction(int stateIdx, ref DiseaseState writeState) {
			writeState.setTo( model.properties.readCells[stateIdx]);
			//Pick and do a reaction
			double sumProps = sumOfPropensityFunctions(stateIdx);
			double sumPropsR2 = sumProps * ThreadSafeRandom.NextDouble();
			double sum = 0.0f;
			for (int q = 0; q < model.properties.reactionCount; q++) {
				double currProp = dispatchPropensityFunction(ref model.properties.readCells[stateIdx], stateIdx, model.properties.reactionFunctionDetails[q]);
				sum += currProp;
				if (sum > sumPropsR2) {
					//This is the reaction we do
					writeState.state[model.properties.stoichiometry[q].Item1] -= 1;
					writeState.state[model.properties.stoichiometry[q].Item2] += 1;
					writeState.timeSimulated += getNextReactionTime(stateIdx);
					return;
				}
			}
		}

		public override void performReactionsWithTime(int stateIdx, ref DiseaseState writeState, double time) {
			
		}
	}
}