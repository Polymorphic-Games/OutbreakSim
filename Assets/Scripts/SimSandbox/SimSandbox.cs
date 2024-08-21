using CJSim;
using UnityEngine;
using DataVisualizer;

public class SimSandbox : SimSandboxBase {
	public DataSeriesChart chart;

	private Simulation simulation;

	private void Start() {
		Application.targetFrameRate = 60;
		//Make a basic simulation
		MovementModelNone movementModel = new MovementModelNone();

		SimModelProperties props = new SimModelProperties(3, 2, 2, 1);
		//S,I,R,,,S->I,I->R,,,B,R
		props.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		props.reactionFunctionDetails[1] = new int[]{0,1,1};

		props.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		props.stoichiometry[1] = new System.Tuple<int, int>(1,2);

		props.parameters[0] = 1.0f;
		props.parameters[1] = 0.1f;

		props.readCells[0].state[0] = 10;
		props.readCells[0].state[1] = 1;
		props.readCells[0].state[2] = 0;

		SimModelAlgorithm algorithm = new SimAlgGillespie();

		SimModel model = new SimModel(props, algorithm, movementModel);

		SimCore core = new SimCore(model, 1);
		
		simulation = new Simulation(core);

		initChart(chart);
	}

	double step = 0.3;
	double lastTime = 0.0f;
	private void Update() {
		//Press N to do 10 steps
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.N)) {
			for (int q = 0; q < (Input.GetKeyDown(KeyCode.N) ? 10 : 1); q++) {
				//With gillespie the very last reaction will be at infinity time it's rough
				if (simulation.model.properties.readCells[0].timeSimulated - lastTime >= 1000.0f) {
					break;
				}

				updateChart(chart, simulation.model.properties.readCells[0]);
				
				lastTime = simulation.model.properties.readCells[0].timeSimulated;
				simulation.core.tickSimulation(step);
				dumpSim();
			}
		}
	}

	private void dumpSim() {
		Debug.Log("Sim Dump 0 At " + simulation.model.properties.readCells[0].timeSimulated.ToString() + "\n" + simulation.model.properties.readCells[0].ToString());
	}
}
