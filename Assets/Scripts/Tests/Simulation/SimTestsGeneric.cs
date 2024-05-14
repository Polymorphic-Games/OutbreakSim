using NUnit.Framework;
using CJSim;


//Generic tests of the simulation
public class SimTestsGeneric {

	//Tests the overall system of creating a simulation
	[Test]
	public void SimTests_GenericTest() {
		SimModel model = new SimModel(3, 2, 2, ModelType.Deterministic);
		//S,I,R,,,S->I,I->S,,,B,R
		model.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		model.reactionFunctionDetails[1] = new int[]{0,1,1};

		model.parameters[0] = 1.0f;
		model.parameters[1] = 0.1f;
		


		SimCore core = new SimCore(model, 1, 1);
		
		Simulation simulation = new Simulation(core);

		
	}
}
