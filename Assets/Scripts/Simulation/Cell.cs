
namespace CJSim {
    //Cell struct, contains all the individual information for a cell
	public struct Cell {
		public DiseaseState state;

		public Cell(Cell otherCell) {
			state = new DiseaseState(otherCell.state);
		}
	}
}
