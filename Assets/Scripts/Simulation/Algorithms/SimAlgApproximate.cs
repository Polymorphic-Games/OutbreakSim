
namespace CJSim {
    //Framework for approximate models that allow for setting a specific timestep
	public abstract class SimAlgApproximate : SimModelAlgorithm {
		public double timestep {get; set;}
		public SimAlgApproximate(SimModelProperties props, SimMovementModel movement, double step) : base(props, movement) {
			timestep = step;
		}


	}
}
