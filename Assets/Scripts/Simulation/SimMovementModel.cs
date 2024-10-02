using System.Collections;

//This interface is so that you can extend a simulation model with robust and easy to edit movement parameters
//The benefit of this is that for models with different movement models they will be easy to plug into the simulation
namespace CJSim
{
    public abstract class SimMovementModel
    {
        //Get the amount of cell connectivity
        public abstract double getCellConnectivity(int idSource, int idDest);

        //Get a list of this cells neighbors
        //Promises to fill the rest of the array with -1
        public abstract void getNeighbors(int idx, int[] output);

        public abstract int[] makeOutputArray();
    }
}
