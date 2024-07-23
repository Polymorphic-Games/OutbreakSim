using NUnit.Framework;
using CJSim;


//Tests the model validation function
public class ModelValidateTests {

	[Test]
	public void ModelValidateTests_all() {
		SimModelProperties props = new SimModelProperties(3, 2, 2, 1);
		
		Assert.IsFalse(props.validate());
		props.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		Assert.IsFalse(props.validate());
		props.reactionFunctionDetails[1] = new int[]{0,1,1};
		Assert.IsFalse(props.validate());

		props.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		Assert.IsFalse(props.validate());
		props.stoichiometry[1] = new System.Tuple<int, int>(1,2);
		Assert.IsTrue(props.validate());

	}
}
