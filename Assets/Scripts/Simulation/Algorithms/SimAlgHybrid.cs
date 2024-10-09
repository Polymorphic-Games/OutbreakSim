using System.Collections.Generic;


//https://doi.org/10.1002/wsbm.1459
//Basically just algorithm 6

//DO NOTE: Doesn't work. Can kinda work sometimes but quite buggy
//Not too sure how to tame that issue
//And frankly, Justin, I have like an hour left in the studio I need to do other things
//This thing is too slow anyway, it's just a proof of concept
//To actually get speed and accuracy gains you should do the model that the paper is actually about
//Really all you gotta do is combine algorithm 6 (this class) and the rejection model that already works and bob's your uncle you got a working thing
namespace CJSim
{
    [System.Serializable]
    public class SimAlgHybrid : SimModelAlgorithm
    {


        SimAlgApproximate fastAlgo;
        SimModelAlgorithm slowAlgo;

        //Once the population of products and reactants of a reaction passes this number, go ahead and go fast
        int slowPopCutoff = 10;

        [System.Serializable]
        public struct SimAlgHybridCellData
        {
            //Mins and max for the popluation values chosen
            public SimModelProperties fastProperties;
            public SimModelProperties slowProperties;
        }

        private SimAlgHybridCellData[] cellData;

        public SimAlgHybrid(SimModelProperties props, SimMovementModel movement, SimAlgApproximate fast, SimModelAlgorithm slow) : base(props, movement)
        {
            slowAlgo = slow;
            fastAlgo = fast;

            cellData = new SimAlgHybridCellData[properties.cellCount];
            for (int q = 0; q < cellData.Length; q++)
            {
                cellData[q].fastProperties = new SimModelProperties(props);
                cellData[q].slowProperties = new SimModelProperties(props);
            }
        }

        public override double getNextReactionTime(int stateIdx, ref DiseaseState readState)
        {
            return fastAlgo.timestep;
        }

        public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double timestep = 0.0)
        {
            partitionReactions(stateIdx, ref readState);

            DiseaseState fastWrite = new DiseaseState(readState);
            DiseaseState slowWrite = new DiseaseState(readState);

            fastAlgo.properties = cellData[stateIdx].fastProperties;
            slowAlgo.properties = cellData[stateIdx].slowProperties;

            //If there are slow reactions
            if (slowAlgo.properties.reactionCount > 0)
            {
                //Do only a single slow reaction
                slowAlgo.performSingleReaction(stateIdx, ref readState, ref slowWrite);
                //And do as many fast reactions as needed to catch up
                if (fastAlgo.properties.reactionCount > 0)
                {
                    if (double.IsFinite(slowWrite.timeSimulated))
                    {
                        DiseaseState fakeRead = new DiseaseState(readState);
                        while (fastWrite.timeSimulated + fastAlgo.timestep < slowWrite.timeSimulated)
                        {
                            fastAlgo.performSingleReaction(stateIdx, ref fakeRead, ref fastWrite);
                            fakeRead.setTo(fastWrite);
                        }
                        if (fastWrite.timeSimulated < slowWrite.timeSimulated)
                        {
                            double tmp = fastAlgo.timestep;
                            fastAlgo.timestep = slowWrite.timeSimulated - fastWrite.timeSimulated;
                            fastAlgo.performSingleReaction(stateIdx, ref fakeRead, ref fastWrite);
                            fastAlgo.timestep = tmp;
                        }
                    }
                    else
                    {

                        fastAlgo.performSingleReaction(stateIdx, ref readState, ref fastWrite);
                    }
                }
                else
                {
                    fastWrite.timeSimulated = slowWrite.timeSimulated;
                }
            }
            else
            {
                fastAlgo.performSingleReaction(stateIdx, ref readState, ref fastWrite);
            }


            double timeDifferential = slowWrite.timeSimulated - fastWrite.timeSimulated;


            DiseaseState slowDifferential = slowWrite - readState;
            fastWrite.roundNumbers();
            fastWrite += slowDifferential;
            writeState.setTo(fastWrite);
        }

        //Puts the original reactions into the fast and slow properties as needed
        private void partitionReactions(int stateIdx, ref DiseaseState readState)
        {
            List<int> fastReactions = new List<int>();
            List<int> slowReactions = new List<int>();
            //For each reaction
            for (int idxReaction = 0; idxReaction < properties.reactionCount; idxReaction++)
            {
                double aj = dispatchPropensityFunction(ref readState, stateIdx, properties.reactionFunctionDetails[idxReaction]);
                if (aj * fastAlgo.timestep < 1.0)
                {
                    slowReactions.Add(idxReaction);
                }
                else if (readState.state[properties.stoichiometry[idxReaction].Item1] < slowPopCutoff || readState.state[properties.stoichiometry[idxReaction].Item2] < slowPopCutoff)
                {
                    slowReactions.Add(idxReaction);
                }
                else
                {
                    fastReactions.Add(idxReaction);
                }
            }

            //Now apply the lists to the properties
            cellData[stateIdx].fastProperties.reactionFunctionDetails = new int[fastReactions.Count][];
            cellData[stateIdx].slowProperties.reactionFunctionDetails = new int[slowReactions.Count][];

            for (int q = 0; q < fastReactions.Count; q++)
            {
                cellData[stateIdx].fastProperties.reactionFunctionDetails[q] = new int[properties.reactionFunctionDetails[fastReactions[q]].Length];
                for (int i = 0; i < cellData[stateIdx].fastProperties.reactionFunctionDetails[q].Length; i++)
                {
                    cellData[stateIdx].fastProperties.reactionFunctionDetails[q][i] = properties.reactionFunctionDetails[fastReactions[q]][i];
                }
            }
            for (int q = 0; q < slowReactions.Count; q++)
            {
                cellData[stateIdx].slowProperties.reactionFunctionDetails[q] = new int[properties.reactionFunctionDetails[slowReactions[q]].Length];
                for (int i = 0; i < cellData[stateIdx].slowProperties.reactionFunctionDetails[q].Length; i++)
                {
                    cellData[stateIdx].slowProperties.reactionFunctionDetails[q][i] = properties.reactionFunctionDetails[slowReactions[q]][i];
                }
            }
        }
    }
}
