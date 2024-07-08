using NUnit.Framework;
using CJSim;


//Generic tests of the simulation
public class SimTestsGeneric {

	//Tests the overall system of creating and briefly running a simulation
	//Here mostly as a sanity check
	[Test]
	public void SimTests_GenericTest() {
		MovementModelAllConnected movementModel = new MovementModelAllConnected(4);
		movementModel.setCellConnectivity(0, 1, 0.01f);
		movementModel.setCellConnectivity(1, 3, 0.01f);
		movementModel.setCellConnectivity(3, 2, 0.01f);
		movementModel.setCellConnectivity(2, 0, 0.01f);
		SimModel model = new SimModel(3, 3, 2, movementModel, ModelType.Deterministic);
		//S,I,R,,,S->I,I->R,,,B,R
		model.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		model.reactionFunctionDetails[1] = new int[]{0,1,1};
		model.reactionFunctionDetails[2] = new int[]{2,0,0,1};

		model.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		model.stoichiometry[1] = new System.Tuple<int, int>(1,2);
		model.stoichiometry[2] = new System.Tuple<int, int>(0,1);

		model.parameters[0] = 1.0f;
		model.parameters[1] = 0.1f;

		Assert.IsTrue(model.validate(), "Test model didn't validate?");
		
		SimCore core = new SimCore(model, 4, 1);
		core.readCells[0].state[0] = 100;
		core.readCells[0].state[1] = 10;
		core.readCells[0].state[2] = 0;

		core.readCells[1].state[0] = 100;
		core.readCells[2].state[0] = 100;
		core.readCells[3].state[0] = 100;
		
		Simulation simulation = new Simulation(core);
		simulation.core.tickSimulation(0.5f);
		Assert.AreEqual(simulation.core.readCells[0].timeSimulated, 0.5f, 0.01f);
		Assert.AreEqual(simulation.core.readCells[1].timeSimulated, 0.5f, 0.01f);
		Assert.AreEqual(simulation.core.readCells[2].timeSimulated, 0.5f, 0.01f);
		Assert.AreEqual(simulation.core.readCells[3].timeSimulated, 0.5f, 0.01f);
	}
}
