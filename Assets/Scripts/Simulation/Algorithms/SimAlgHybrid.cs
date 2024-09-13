using System.Collections.Generic;
using UnityEditor.Compilation;


//https://doi.org/10.1002/wsbm.1459
//Basically just algorithm 6
namespace CJSim {
	public class SimAlgHybrid : SimModelAlgorithm {
		

		SimAlgApproximate fastAlgo;
		SimModelAlgorithm slowAlgo;

		//Once the population of products and reactants of a reaction passes this number, go ahead and go fast
		int slowPopCutoff = 10;

		public struct SimAlgHybridCellData {
			//Mins and max for the popluation values chosen
			public SimModelProperties fastProperties;
			public SimModelProperties slowProperties;
		}

		private SimAlgHybridCellData[] cellData;

		public SimAlgHybrid(SimModelProperties props, SimMovementModel movement, SimAlgApproximate fast, SimModelAlgorithm slow) : base(props, movement) {
			slowAlgo = slow;
			fastAlgo = fast;

			cellData = new SimAlgHybridCellData[properties.cellCount];
			for (int q = 0; q < cellData.Length; q++) {
				cellData[q].fastProperties = new SimModelProperties(props);
				cellData[q].slowProperties = new SimModelProperties(props);
			}
		}

		public override double getNextReactionTime(int stateIdx, ref DiseaseState readState) {
			return fastAlgo.timestep;
		}

		public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double timestep = 0.0) {
			partitionReactions(stateIdx, ref readState);

			DiseaseState fastWrite = new DiseaseState(readState);
			DiseaseState slowWrite = new DiseaseState(readState);

			fastAlgo.properties = cellData[stateIdx].fastProperties;
			slowAlgo.properties = cellData[stateIdx].slowProperties;

			//If there are slow reactions
			if (slowAlgo.properties.reactionCount > 0) {
				ThreadLogger.Log("there are " + slowAlgo.properties.reactionCount + " slow reactions");
				//Do only a single slow reaction
				slowAlgo.performSingleReaction(stateIdx, ref readState, ref slowWrite);
				//And do as many fast reactions as needed to catch up
				if (fastAlgo.properties.reactionCount > 0) {
					if (double.IsFinite(slowWrite.timeSimulated)) {
						DiseaseState fakeRead = new DiseaseState(readState);
						while (fastWrite.timeSimulated + fastAlgo.timestep < slowWrite.timeSimulated) {
							ThreadLogger.Log("While loop doing fast algo to catch up because time simmed is " + slowWrite.timeSimulated);
							fastAlgo.performSingleReaction(stateIdx, ref fakeRead, ref fastWrite);
							fakeRead.setTo(fastWrite);
						}
						if (fastWrite.timeSimulated < slowWrite.timeSimulated) {
							double tmp = fastAlgo.timestep;
							fastAlgo.timestep = slowWrite.timeSimulated - fastWrite.timeSimulated;
							ThreadLogger.Log("Doing one more fast tick at " + fastAlgo.timestep);
							fastAlgo.performSingleReaction(stateIdx, ref fakeRead, ref fastWrite);
							fastAlgo.timestep = tmp;
						}
					} else {

						ThreadLogger.Log("Only doing the fast tick because slow is infinite");
						fastAlgo.performSingleReaction(stateIdx, ref readState, ref fastWrite);
					}
				}
			} else {
				ThreadLogger.Log("Only doing the fast tick");
				fastAlgo.performSingleReaction(stateIdx, ref readState, ref fastWrite);
			}


			double timeDifferential = slowWrite.timeSimulated - fastWrite.timeSimulated;


			DiseaseState slowDifferential = slowWrite - readState;
			fastWrite.roundNumbers();
			ThreadLogger.Log("Fast write is " + fastWrite + " and slow is " + slowWrite + " and readState is " + readState + " and differential is " + slowDifferential);
			fastWrite += slowDifferential;
			writeState.setTo(fastWrite);
		}

		//Puts the original reactions into the fast and slow properties as needed
		private void partitionReactions(int stateIdx, ref DiseaseState readState) {
			List<int> fastReactions = new List<int>();
			List<int> slowReactions = new List<int>();
			ThreadLogger.Log("    The partition:");
			//For each reaction
			for (int idxReaction = 0; idxReaction < properties.reactionCount; idxReaction++) {
				double aj = dispatchPropensityFunction(ref readState, stateIdx, properties.reactionFunctionDetails[idxReaction]);
				if (aj * fastAlgo.timestep < 1.0) {
					ThreadLogger.Log($"    reaction {idxReaction} is slow because {aj} * {fastAlgo.timestep} < 1.0");
					slowReactions.Add(idxReaction);
				} else if (readState.state[properties.stoichiometry[idxReaction].Item1] < slowPopCutoff || readState.state[properties.stoichiometry[idxReaction].Item2] < slowPopCutoff) {
					ThreadLogger.Log($"    reaction {idxReaction} is slow because {readState.state[properties.stoichiometry[idxReaction].Item1]} || {readState.state[properties.stoichiometry[idxReaction].Item2]} < 10");
					slowReactions.Add(idxReaction);
				} else {
					ThreadLogger.Log($"    reaction {idxReaction} is fast");
					fastReactions.Add(idxReaction);
				}
			}

			//Now apply the lists to the properties
			cellData[stateIdx].fastProperties.reactionFunctionDetails = new int[fastReactions.Count][];
			cellData[stateIdx].slowProperties.reactionFunctionDetails = new int[slowReactions.Count][];
			
			for (int q = 0; q < fastReactions.Count; q++) {
				cellData[stateIdx].fastProperties.reactionFunctionDetails[q] = new int[properties.reactionFunctionDetails[fastReactions[q]].Length];
				for (int i = 0; i < cellData[stateIdx].fastProperties.reactionFunctionDetails[q].Length; i++) {
					cellData[stateIdx].fastProperties.reactionFunctionDetails[q][i] = properties.reactionFunctionDetails[fastReactions[q]][i];
				}
			}
			for (int q = 0; q < slowReactions.Count; q++) {
				cellData[stateIdx].slowProperties.reactionFunctionDetails[q] = new int[properties.reactionFunctionDetails[slowReactions[q]].Length];
				for (int i = 0; i < cellData[stateIdx].slowProperties.reactionFunctionDetails[q].Length; i++) {
					cellData[stateIdx].slowProperties.reactionFunctionDetails[q][i] = properties.reactionFunctionDetails[slowReactions[q]][i];
				}
			}
			ThreadLogger.Log("    End partition");
		}
	}
}
