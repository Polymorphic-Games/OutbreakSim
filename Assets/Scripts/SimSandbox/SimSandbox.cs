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

		model.parameters[0] = 1.0f;
		model.parameters[1] = 0.1f;
		
		SimCore core = new SimCore(model, 1, 1);
		
		simulation = new Simulation(core);

		var category = chart.DataSource.GetCategory("dataseries-1").Data; // obtain category data
		category.Append(0.0, 0.0); // call append to add a new point to the graph
		category.Append(100.0, 2.0);
		category.Append(200.0, 1.0);
		category.Append(300.0, 5.0);
		category.Append(500.0, 2.0);
	}

	private void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) {
			Debug.Log("Space pressed");
		}
	}
}