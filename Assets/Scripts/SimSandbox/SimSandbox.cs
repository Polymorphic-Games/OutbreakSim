using CJSim;
using UnityEngine;
using DataVisualizer;

public class SimSandbox : MonoBehaviour {
	public DataSeriesChart chart;

	private Simulation simulation;

	private void Start() {
		Application.targetFrameRate = 60;
		//Make a basic simulation
		IMovementModel movementModel = new MovementModelNone();
		SimModel model = new SimModel(3, 2, 2, movementModel, ModelType.Deterministic);
		//S,I,R,,,S->I,I->S,,,B,R
		model.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		model.reactionFunctionDetails[1] = new int[]{0,1,1};

		model.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		model.stoichiometry[1] = new System.Tuple<int, int>(1,2);

		model.parameters[0] = 1.0f;
		model.parameters[1] = 0.1f;
		
		SimCore core = new SimCore(model, 1, 1);
		core.readCells[0].state[0] = 100000;
		core.readCells[0].state[1] = 100;
		
		simulation = new Simulation(core);
	}

	float dt = 0.0f;
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			try {
				CategoryDataHolder category = chart.DataSource.GetCategory("susceptible").Data; // obtain category data
				category.Append(dt, simulation.core.readCells[0].state[0]);

				//category = chart.DataSource.GetCategory("infected").Data; // obtain category data
				//category.Append(dt, simulation.core.readCells[0].state[1]);
			} catch (System.Exception e) {
				ThreadLogger.Log(e.Message);
			}


			dumpSim();
			dt += 0.3f;
			simulation.core.tickSimulation(0.3f);
			dumpSim();
		}
	}

	private void dumpSim() {
		Debug.Log("Sim Dump At " + dt + "\n" + simulation.core.readCells[0].ToString());
	}
}
