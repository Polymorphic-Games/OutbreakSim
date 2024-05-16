
namespace CJSim {
	public class MovementModelNone : IMovementModel {
		//Get the amount of cell connectivity
		public float getCellConnectivity(int idSource, int idDest) {
			return 0.0f;
		}

		//Get a list of this cells neighbors
		//Not sure how bad the performance is on the array allocation
		public int[] getNeighbors(int idx) {
			return new int[0];
		}
	}
}


