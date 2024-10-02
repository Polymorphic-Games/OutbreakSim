
namespace CJSim
{
    [System.Serializable]
    //No movement between anything, useful for 1 bucket simulations
    public class SimMovementNone : SimMovementModel
    {
        //Get the amount of cell connectivity
        public override double getCellConnectivity(int idSource, int idDest)
        {
            return 0.0;
        }

        //Get a list of this cells neighbors
        //Not sure how bad the performance is on the array allocation
        public override void getNeighbors(int idx, int[] output)
        {
            return;
        }

        public override int[] makeOutputArray()
        {
            return new int[0];
        }
    }

    [System.Serializable]
    //Useful for very small very interconnected simulations, largely for easy testing
    public class SimMovementAllConnected : SimMovementModel
    {

        private double[] connectivities;
        private int[] neighborRet;
        private int m_cellCount = 0;

        public SimMovementAllConnected(int cellCount)
        {
            //A bit wasteful because each entry includes itself which isn't needed
            //But this class is just for testing so shouldn't matter
            connectivities = new double[cellCount * cellCount];
            System.Array.Fill<double>(connectivities, 0.0);
            m_cellCount = cellCount;

            //Standard return for the neighbors because every cell is connected
            //Will likely cause strange bugs if you set the connectivity of a cell to itself anything other than 0
            neighborRet = new int[cellCount];
            for (int q = 0; q < cellCount; q++)
            {
                neighborRet[q] = q;
            }
        }

        public void setCellConnectivity(int idSource, int idDest, float val)
        {
            connectivities[(idSource * m_cellCount) + idDest] = val;
        }

        //Get the amount of cell connectivity
        public override double getCellConnectivity(int idSource, int idDest)
        {
            return connectivities[(idSource * m_cellCount) + idDest];
        }

        //Get a list of this cells neighbors
        public override void getNeighbors(int idx, int[] output)
        {
            for (int q = 0; q < m_cellCount; q++)
            {
                output[q] = q;
            }
        }

        public override int[] makeOutputArray()
        {
            return neighborRet;
        }
    }
}
