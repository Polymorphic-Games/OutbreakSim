
namespace CJSim {
	//No movement between anything, useful for 1 bucket simulations
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

	//Useful for very small very interconnected simulations, largely for testing purposes
	public class MovementModelAllConnected : IMovementModel {

		private float[] connectivities;
		private int[] neighborRet;
		private int m_cellCount = 0;

		public MovementModelAllConnected(int cellCount) {
			//A bit wasteful because each entry includes itself which isn't needed
			//But this class is just for testing so shouldn't matter
			connectivities = new float[cellCount * cellCount];
			System.Array.Fill<float>(connectivities, 0.0f);
			m_cellCount = cellCount;

			//Standard return for the neighbors because every cell is connected
			//Will likely cause strange bugs if you set the connectivity of a cell to itself anything other than 0
			neighborRet = new int[cellCount];
			for (int q = 0; q < cellCount; q++) {
				neighborRet[q] = q;
			}
		}

		public void setCellConnectivity(int idSource, int idDest, float val) {
			connectivities[(idSource * m_cellCount) + idDest] = val;
		}

		//Get the amount of cell connectivity
		public float getCellConnectivity(int idSource, int idDest) {
			return 0.0f;
		}

		//Get a list of this cells neighbors
		//cjnote not sure how bad the performance is on the array allocation
		public int[] getNeighbors(int idx) {
			return new int[0];
		}
	}
}


