
using System;

namespace CJSim {
	//Data for the model
	public struct SimAlgRejectionCellData {
		//Mins and max for the popluation values chosen
		public DiseaseState stateMins;
		public DiseaseState stateMaxs;
		//Computed propensities for each 
		public double[] propensityMins;
		public double[] propensityMaxs;
	}

	public class SimAlgRejection : SimModelAlgorithm {
		//Holds data we need to remember about every cell
		SimAlgRejectionCellData[] cellData;

		//Which compartments are associated with which reactions
		int[][] speciesReactionDependency;

		public override double getNextReactionsTime(int stateIdx, double time) {
			return 0.0;
		}

		public override void onModelCreate(SimModel model) {
			base.onModelCreate(model);

			//Initialize the cell data array, and the structs members
			cellData = new SimAlgRejectionCellData[model.properties.cellCount];
			for (int q = 0; q < cellData.Length; q++) {
				cellData[q].stateMins = new DiseaseState(model);
				cellData[q].stateMaxs = new DiseaseState(model);

				cellData[q].propensityMaxs = new double[model.properties.reactionCount];
				cellData[q].propensityMins = new double[model.properties.reactionCount];
				Array.Fill(cellData[q].propensityMaxs, 0.0);
				Array.Fill(cellData[q].propensityMins, 0.0);
			}

			//Make the speciesReactionDependency graph
			speciesReactionDependency = new int[model.properties.compartmentCount][];
			//For each species
			for (int q = 0; q < model.properties.compartmentCount; q++) {
				
			}
		}
		

		public override void performReactions(int stateIdx, ref DiseaseState writeState, double time) {
			writeState.setTo(model.properties.readCells[stateIdx]);

			//check if the populations are still in the bounds
			for (int q = 0; q < model.properties.compartmentCount; q++) {
				//WriteState is currently equivalent to read state so we're good to read from it right now
				if (writeState[q] < cellData[q].stateMins[q]) {

				}
			}
		}
	}
}
