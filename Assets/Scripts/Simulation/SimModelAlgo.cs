
namespace CJSim {
	public abstract class SimModelAlgorithm {
		//Not used by the deterministic model
		public abstract double getNextReactionsTime();
		public abstract double performReactions(double time);
		public abstract double fullTick(double time);
	}
}