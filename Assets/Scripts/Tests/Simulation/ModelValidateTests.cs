using NUnit.Framework;
using CJSim;


//Tests the model validation function
public class ModelValidateTests {

	[Test]
	public void ModelValidateTests_reactionFunctionDetails() {
		IMovementModel movementModel = new MovementModelNone();
		SimModel model = new SimModel(3, 2, 2, movementModel, ModelType.Deterministic);
		
		Assert.IsFalse(model.validate());
		model.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		Assert.IsFalse(model.validate());
		model.reactionFunctionDetails[1] = new int[]{0,1,1};
		Assert.IsFalse(model.validate());

		model.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		Assert.IsFalse(model.validate());
		model.stoichiometry[1] = new System.Tuple<int, int>(1,2);
		Assert.IsTrue(model.validate());

	}
}
