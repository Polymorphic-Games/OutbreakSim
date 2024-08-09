using System;
using System.Collections.Generic;

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
				//The abstract propensities would all compute to 0 based on the 0s in state mins and maxs
				Array.Fill(cellData[q].propensityMaxs, 0.0);
				Array.Fill(cellData[q].propensityMins, 0.0);
			}

			//Make the speciesReactionDependency graph
			speciesReactionDependency = new int[model.properties.compartmentCount][];
			//For each species
			List<int> currentReactions = new List<int>();
			for (int q = 0; q < model.properties.compartmentCount; q++) {
				//for each reaction
				for (int reactionId = 0; reactionId < model.properties.reactionCount; reactionId++) {
					//Only check stoichiometry. cjnote might not work if we added a reaction that depended on some other number
					//Which there is already technically one that depends on the number of people in a cell, but if we never change the number of people in a cell we'll be good
					//Or maybe could add number of people to things that we check and regen the propensities if need be, but that sounds like a lot of work that I don't want to do right now
					if (model.properties.stoichiometry[reactionId].Item1 == q || model.properties.stoichiometry[reactionId].Item2 == q) {
						currentReactions.Add(reactionId);
					}
				}
				//Now that we've collected our reactions, add these to the graph
				speciesReactionDependency[q] = currentReactions.ToArray();
				currentReactions.Clear();
			}
		}

		public override void performSingleReaction(int stateIdx, ref DiseaseState writeState) {

		}

		public override void performReactions(int stateIdx, ref DiseaseState writeState, double time) {
			writeState.setTo(model.properties.readCells[stateIdx]);

			//check if the populations are still in the bounds
			for (int q = 0; q < model.properties.compartmentCount; q++) {
				//WriteState is currently equivalent to read state so we're good to read from it right now
				if (writeState[q] < cellData[stateIdx].stateMins[q] || writeState[q] > cellData[stateIdx].stateMaxs[q]) {
					//Regenerate the bounds
					cellData[stateIdx].stateMins[q] = (int)((0.85 * (double)writeState[q]) + 0.5);
					cellData[stateIdx].stateMaxs[q] = (int)((1.15 * (double)writeState[q]) + 0.5);

					//Regen the propensities
					//not the fastest way to do this because if 2 species both need updates and both are in the same reaction
					//that reaction will get regened twice and only the second regen will be correct and up to date, will have wasted time on the first one
					for (int i = 0; i < speciesReactionDependency[q].Length; i++) {
						int reactionID = speciesReactionDependency[q][i];
						cellData[stateIdx].propensityMaxs[reactionID] = dispatchPropensityFunction(ref cellData[stateIdx].stateMaxs, stateIdx, model.properties.reactionFunctionDetails[reactionID]);
						cellData[stateIdx].propensityMins[reactionID] = dispatchPropensityFunction(ref cellData[stateIdx].stateMins, stateIdx, model.properties.reactionFunctionDetails[reactionID]);
					}
				}
			}

			//Compute total propensity upper bound
			double sumPropsMax = 0.0;
			foreach (double propMax in cellData[stateIdx].propensityMaxs) {
				sumPropsMax += propMax;
			}

			do {
				int u = 0;
				bool accepted = false;
			} while(true);
			//Should be U(0,1) cjnote
			double r1 = ThreadSafeRandom.NextDouble();
			double r2 = ThreadSafeRandom.NextDouble();
			double r3 = ThreadSafeRandom.NextDouble();
		}
	}


	//Private functions
	
}
