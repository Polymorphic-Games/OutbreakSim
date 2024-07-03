using NUnit.Framework;
using CJSim;


//Tests the model validation function
public class ModelValidateTests {

	[Test]
	public void ModelValidateTests_reactionFunctionDetails() {
		IMovementModel movementModel = new MovementModelNone();
		SimModel model = new SimModel(3, 2, 2, movementModel, ModelType.Deterministic);
		
		SimCore core = new SimCore(model, 1, 1);
		
		Simulation simulation = new Simulation(core);

		simulation.core.tickSimulation(1.0f);

	}
}
