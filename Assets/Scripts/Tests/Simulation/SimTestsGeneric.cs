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

		SimModelProperties props = new SimModelProperties(3, 3, 2, 4);
		//S,I,R,,,S->I,I->R,,,B,R
		props.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		props.reactionFunctionDetails[1] = new int[]{0,1,1};
		props.reactionFunctionDetails[2] = new int[]{2,0,0,1};

		props.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		props.stoichiometry[1] = new System.Tuple<int, int>(1,2);
		props.stoichiometry[2] = new System.Tuple<int, int>(0,1);

		props.parameters[0] = 1.0f;
		props.parameters[1] = 0.1f;

		props.readCells[0].state[0] = 100;
		props.readCells[0].state[1] = 10;
		props.readCells[0].state[2] = 0;

		props.readCells[1].state[0] = 100;
		props.readCells[2].state[0] = 100;
		props.readCells[3].state[0] = 100;

		SimModelAlgorithm algorithm = new SimAlgGillespie();

		SimModel model = new SimModel(props, algorithm, movementModel);

		SimCore core = new SimCore(model, 1);
		
		Simulation simulation = new Simulation(core);
		
		simulation.core.tickSimulation(0.5f);
		Assert.AreEqual(0.5f, simulation.model.properties.readCells[0].timeSimulated, 0.01f);
		Assert.AreEqual(0.5f, simulation.model.properties.readCells[1].timeSimulated, 0.01f);
		Assert.AreEqual(0.5f, simulation.model.properties.readCells[2].timeSimulated, 0.01f);
		Assert.AreEqual(0.5f, simulation.model.properties.readCells[3].timeSimulated, 0.01f);
	}
}
