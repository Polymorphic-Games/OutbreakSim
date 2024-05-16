using System.Collections;

//This interface is so that you can extend a simulation model with robust and easy to edit movement parameters
//The benefit of this is that for models with different movement models they will be easy to plug into the simulation
namespace CJSim {
	public interface IMovementModel {
		//Get the amount of cell connectivity
		public float getCellConnectivity(int idSource, int idDest);

		//Get a list of this cells neighbors
		//Not sure how bad the performance is on the array allocation
		public int[] getNeighbors(int idx);
	}
}
