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
		
		chart.DataSource.GetCategory("susceptible").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));
		chart.DataSource.GetCategory("infected").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));
		chart.DataSource.GetCategory("rec").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));

		chart.DataSource.GetCategory("susceptible").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.white;
		chart.DataSource.GetCategory("infected").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.red;
		chart.DataSource.GetCategory("rec").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.green;
	}

	float dt = 0.0f;
	float step = 0.3f;
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			for (int q = 0; q < 10; q++) {
				CategoryDataHolder category = chart.DataSource.GetCategory("susceptible").Data; // obtain category data
				CategoryDataHolder category2 = chart.DataSource.GetCategory("infected").Data; // obtain category data
				CategoryDataHolder category3 = chart.DataSource.GetCategory("rec").Data; // obtain category data
				
				category.Append(dt, simulation.core.readCells[0].state[0]);
				category2.Append(dt, simulation.core.readCells[0].state[1]);
				category3.Append(dt, simulation.core.readCells[0].state[2]);
				dt += step;
				simulation.core.tickSimulation(step);
			}
		}
	}

	private void dumpSim() {
		Debug.Log("Sim Dump At " + dt + "\n" + simulation.core.readCells[0].ToString());
	}
}
