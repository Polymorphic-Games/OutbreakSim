using CJSim;
using UnityEngine;
using DataVisualizer;


//The point of this class is to compare and contrast different models, not only in how they performs numbers wise by looking at the graphs
//But also how they perform time wise, how long does it take to run the model
public class SimSandboxComparison :  SimSandboxBase {
	public DataSeriesChart chart1;
	public DataSeriesChart chart2;

	Simulation simulation1;
	Simulation simulation2;

	
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

		simulation1 = new Simulation(new SimCore(new SimModel(new SimModelProperties(props), new SimAlgDeterministic(step), movementModel), 1));
		simulation2 = new Simulation(new SimCore(new SimModel(new SimModelProperties(props), new SimAlgGillespie(), movementModel), 1));

		initChart(chart1);
		initChart(chart2);
		chart2.DataSource.GetCategory("susceptible").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color -= new Color(0,0,0,0.5f);
		chart2.DataSource.GetCategory("infected").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color -= new Color(0,0,0,0.5f);
		chart2.DataSource.GetCategory("rec").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color -= new Color(0,0,0,0.5f);
	}

	protected override void initChart(DataSeriesChart chart) {
		base.initChart(chart);
		chart.AxisView.AutomaticHorizontalView = false;
		chart.AxisView.HorizontalViewOrigin = 0;
	}


	protected override void updateChart(DataSeriesChart chart, DiseaseState state) {
		base.updateChart(chart, state);
		chart.AxisView.HorizontalViewSize = maxTimeForGraph;
	}

	double time = 0.0;
	double step = .01;
	double maxTimeForGraph = .2;
	private void Update() {
		//Press N to do 10 steps
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.N) || true) {
			for (int q = 0; q < (Input.GetKeyDown(KeyCode.N) ? 10 : 1); q++) {
				time += step;
				maxTimeForGraph = System.Math.Max(simulation2.model.properties.readCells[0].timeSimulated, simulation1.model.properties.readCells[0].timeSimulated);
				updateChart(chart1, simulation1.model.properties.readCells[0]);
				updateChart(chart2, simulation2.model.properties.readCells[0]);
				Debug.Log("1 time is " + simulation1.model.properties.readCells[0].timeSimulated);
				Debug.Log("2 time is " + simulation2.model.properties.readCells[0].timeSimulated);
				Debug.Log("requested time is " + time);
				simulation1.core.tickSimulation(time);
				simulation2.core.tickSimulation(time);
			}
		}
	}
}
