using System;

namespace CJSim
{
    public class SimAlgSingleThreaded : SimModelAlgorithm
    {
        //The algorithm to run single threaded
        public SimModelAlgorithm algorithm { get; private set; }
        public SimAlgSingleThreaded(SimModelAlgorithm algorithm, SimModelProperties props, SimMovementModel movement) : base(props, movement)
        {
            this.algorithm = algorithm;
        }

        //Pass these function to the real algorithm
        public override double getNextReactionTime(int stateIdx, ref DiseaseState readState)
        {
            return algorithm.getNextReactionTime(stateIdx, ref readState);
        }
        public override void performSingleReaction(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double timestep = 0.0)
        {
            algorithm.performSingleReaction(stateIdx, ref readState, ref writeState, timestep);
        }

        public override void performReactionsWithTime(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, double time)
        {
            algorithm.performReactionsWithTime(stateIdx, ref readState, ref writeState, time);
        }

        public void updateAll()
        {
            //Get the smallest update any cell wants to do
            double minTime = double.MaxValue;
            int cellIdx = -1;
            for (int q = 0; q < properties.cellCount; q++)
            {
                double nextTime = algorithm.getNextReactionTime(q, ref properties.readCells[q]);
                if (minTime > nextTime)
                {
                    cellIdx = q;
                    minTime = nextTime;
                }
            }
            //Briefly verify that we found a cell with a reaction before infinity time
            if (cellIdx >= 0)
            {
                //Do the single reaction and update all the times in all the cells
                for (int q = 0; q < properties.cellCount; q++)
                {
                    if (q == cellIdx)
                    {
                        //If this is the lucky cell that gets the reaction, do it
                        algorithm.performSingleReaction(q, ref properties.readCells[q], ref properties.writeCells[q], minTime);
                    }
                    else
                    {
                        //Otherwise just add timeSimulated to the other cells
                        properties.writeCells[q].timeSimulated += minTime;
                    }
                }
            }
        }
    }
}
