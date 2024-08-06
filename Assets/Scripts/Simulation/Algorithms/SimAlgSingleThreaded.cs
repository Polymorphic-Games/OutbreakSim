using System;

namespace CJSim {
	public class SimAlgSingleThreaded : SimModelAlgorithm {
		//The algorithm to run single threaded
		public SimModelAlgorithm algorithm {get; private set;}
		public SimAlgSingleThreaded(SimModelAlgorithm algorithm) : base() {
			this.algorithm = algorithm;
		}

		public override void onModelCreate(SimModel model) {
			base.onModelCreate(model);
			algorithm.onModelCreate(model);
		}

		//Pass these function to the real algorithm
		public override double getNextReactionsTime(int stateIdx, double time) {
			return algorithm.getNextReactionsTime(stateIdx, time);
		}
		public override void performReactions(int stateIdx, ref DiseaseState writeState, double time) {
			algorithm.performReactions(stateIdx, ref writeState, time);
		}

		public void updateAll(double time) {
			//Get the smallest update any cell wants to do
			double minTime = double.MaxValue;
			int cellIdx = -1;
			for (int q = 0; q < model.properties.cellCount; q++) {
				double nextTime = getNextReactionsTime(q, time);
				if (minTime > nextTime) {
					cellIdx = q;
					minTime = nextTime;
				}
			}
			//Briefly verify that we found a cell with a reaction before infinity time
			if (cellIdx >= 0) {
				//Do the single reaction and update all the times in all the cells
				for (int q = 0; q < model.properties.cellCount; q++) {
					if (q == cellIdx) {
						performReactions(q, ref model.properties.writeCells[q], minTime);
					} else {
						model.properties.writeCells[q].timeSimulated += minTime;
					}
				}
			}
		}
	}
}
