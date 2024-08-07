
namespace CJSim {
	public class SimAlgRejection : SimModelAlgorithm
	{

		public override double getNextReactionsTime(int stateIdx, double time) {
			throw new System.NotImplementedException();
		}

		public override void onModelCreate(SimModel model) {
			base.onModelCreate(model);
		}

		public override void performReactions(int stateIdx, ref DiseaseState writeState, double time) {
			throw new System.NotImplementedException();
		}
	}
}