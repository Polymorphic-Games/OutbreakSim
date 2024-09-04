using UnityEngine;
using CJSim;
using DataVisualizer;

//Base class for sim sandbox purposes, really jsut holds some common functions for the different sandboxes

public abstract class SimSandboxBase : MonoBehaviour {
	protected virtual void initChart(DataSeriesChart chart) {
		chart.DataSource.GetCategory("susceptible").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));
		chart.DataSource.GetCategory("infected").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));
		chart.DataSource.GetCategory("rec").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial = new Material(Shader.Find("DataVisualizer/Canvas/Solid"));

		chart.DataSource.GetCategory("susceptible").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.white;
		chart.DataSource.GetCategory("infected").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.red;
		chart.DataSource.GetCategory("rec").GetVisualFeature<GraphLineVisualFeature>("Graph Line-0").LineMaterial.color = Color.green;
	}

	protected virtual void updateChart(DataSeriesChart chart, DiseaseState state) {
		if (double.IsInfinity(state.timeSimulated)) {
			return;
		}
		
		CategoryDataHolder category = chart.DataSource.GetCategory("susceptible").Data; // obtain category data
		CategoryDataHolder category2 = chart.DataSource.GetCategory("infected").Data; // obtain category data
		CategoryDataHolder category3 = chart.DataSource.GetCategory("rec").Data; // obtain category data
		
		category.Append(state.timeSimulated, state.state[0]);
		category2.Append(state.timeSimulated, state.state[1]);
		category3.Append(state.timeSimulated, state.state[2]);
	}
}
