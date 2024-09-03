using System;
using System.Collections.Generic;

//Based on https://doi.org/10.1063/1.4896985
namespace CJSim {
	//Data for the model
	public struct SimAlgRejectionCellData {
		//Mins and max for the popluation values chosen
		public DiseaseState stateMins;
		public DiseaseState stateMaxs;
		//Computed propensities for each 
		public double[] propensityMins;
		public double[] propensityMaxs;

		public double propensitySumMax;
	}

	public class SimAlgRejection : SimModelAlgorithm {
		//Holds data we need to remember about every cell
		SimAlgRejectionCellData[] cellData;

		//Which compartments are associated with which reactions
		int[][] speciesReactionDependency;

		public double rangePercentage = .15;

		public SimAlgRejection() : base() {
			//No initialization needed (in the constructor, much initialization is needed elsewhere)
		}

		public override double getNextReactionTime(int stateIdx, ref DiseaseState readState) {
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
					//Not to mention the spatial propensity function
					if (model.properties.stoichiometry[reactionId].Item1 == q || model.properties.stoichiometry[reactionId].Item2 == q) {
						currentReactions.Add(reactionId);
					}
				}
				//Now that we've collected our reactions, add these to the graph
				speciesReactionDependency[q] = currentReactions.ToArray();
				currentReactions.Clear();
			}
		}

		//Because of how this model works performing a single reaction is a fair amount slower than running at speed
		public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState) {
			performReactionsWithTime(stateIdx, ref readState, ref writeState, 0.0);
		}

		public override void performReactionsWithTime(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double time) {
			if (writeState.timeSimulated >= time) {
				writeState.setTo(readState);
				return;
			}

			//check if the populations are still in the bounds
			for (int q = 0; q < model.properties.compartmentCount; q++) {
				//If out of bounds
				if (readState[q] < cellData[stateIdx].stateMins[q] || readState[q] > cellData[stateIdx].stateMaxs[q]) {
					//Regenerate the bounds, with rounding because the populations will only be integers for this model
					cellData[stateIdx].stateMins[q] = (int)(((1-rangePercentage) * readState[q]) + .5);
					cellData[stateIdx].stateMaxs[q] = (int)(((1+rangePercentage) * readState[q]) + .5);
					//Don't let min go to 0
					cellData[stateIdx].stateMins[q] = (cellData[stateIdx].stateMins[q] >= 0 ? cellData[stateIdx].stateMins[q] : 0);

					//Regen the propensities
					//not the fastest way to do this because if 2 species both need updates and both are in the same reaction
					//that reaction will get regened twice and only the second regen will be correct and up to date, will have wasted time on the first one
					for (int i = 0; i < speciesReactionDependency[q].Length; i++) {
						int reactionID = speciesReactionDependency[q][i];
						cellData[stateIdx].propensityMaxs[reactionID] = dispatchPropensityFunction(ref cellData[stateIdx].stateMaxs, stateIdx, model.properties.reactionFunctionDetails[reactionID]);
						cellData[stateIdx].propensityMins[reactionID] = dispatchPropensityFunction(ref cellData[stateIdx].stateMins, stateIdx, model.properties.reactionFunctionDetails[reactionID]);
					}
				}
				//Compute total propensity upper bound cjnote not sure if this should go inside the if or not
				//Also not sure about the whole system, feels like we could recalculate less often
				double sumPropsMax = 0.0;
				foreach (double propMax in cellData[stateIdx].propensityMaxs) {
					sumPropsMax += propMax;
				}
				cellData[stateIdx].propensitySumMax = sumPropsMax;
			}
			//Get recomputed so default value doesn't matter
			bool isContainedInStateBounds = false;
			DiseaseState fakeRead = new DiseaseState(readState);
			do {
				double u = 1;
				int uMicro = 0;
				bool accepted = false;

				writeState.setTo(fakeRead);

				do {
					double r1 = ThreadSafeRandom.NextUniform0Exclusive1Exclusive();
					double r2 = ThreadSafeRandom.NextUniform0Exclusive1Exclusive();
					double r3 = ThreadSafeRandom.NextUniform0Exclusive1Exclusive();

					//Select minimum uMicro
					double umicroSelectionSum = 0.0;
					double r1a0Max = r1 * cellData[stateIdx].propensitySumMax;
					for (int q = 0; q < model.properties.reactionCount; q++) {
						umicroSelectionSum += cellData[stateIdx].propensityMaxs[q];
						if (umicroSelectionSum > r1a0Max) {
							uMicro = q;
							break;
						}
					}
					if (cellData[stateIdx].propensityMaxs[uMicro] == 0) return;

					if (r2 <= (cellData[stateIdx].propensityMins[uMicro] / cellData[stateIdx].propensityMaxs[uMicro])) {
						accepted = true;
					} else {
						double auMicro = dispatchPropensityFunction(ref fakeRead, stateIdx, model.properties.reactionFunctionDetails[uMicro]);
						if (r2 <= (auMicro / cellData[stateIdx].propensityMaxs[uMicro])) {
							accepted = true;
						}
					}
					u = u * r3;

				} while (!accepted);
				//Compute firing time
				double tau = (-1 / cellData[stateIdx].propensitySumMax) * Math.Log(u);
				writeState.timeSimulated += tau;
				updateStateViaStoichOneReaction(ref writeState, uMicro);

				isContainedInStateBounds = true;
				for (int q = 0; q < writeState.state.Length; q++) {
					if (writeState[q] < cellData[stateIdx].stateMins[q] || writeState[q] > cellData[stateIdx].stateMaxs[q]) {
						isContainedInStateBounds = false;
						break;
					}
				}
				fakeRead.setTo(writeState);

			} while(isContainedInStateBounds && writeState.timeSimulated < time);

			if (!isContainedInStateBounds) {
				performReactionsWithTime(stateIdx, ref fakeRead, ref writeState, time);
			}
		}
	
	}


	//Private functions
	
}
