

//Holds algorithms that operate on disease states


namespace CJSim {
	class SimAlgorithms {
		public delegate float PropensityFunctionTypes(ref DiseaseState state, ref SimModel model, int[] argv);
		public const int propensityFunctionTypeCount = 2;

		public static PropensityFunctionTypes[] propensityFuncTypes;

		//When railroading dt with tau leaping, how much are we willing to raise tau to meet the railroad demand
		//Also basically a "minimum" amount, you probably shouldn't lower from .1
		const float maxTauRaiseAmount = 0.1f;

		//Static initializer
		static SimAlgorithms() {
			//Set up propensity functions
			propensityFuncTypes = new PropensityFunctionTypes[propensityFunctionTypeCount];

			//Basic type, state (idx 1) * param (idx 2)
			propensityFuncTypes[0] = (ref DiseaseState state, ref SimModel model, int[] argv) => {
				return (float)state.state[argv[1]] * model.parameters[argv[2]];
			};

			//Grey arrow, page 16 of the book, thing that depends on the density of infected
			// (param * state1 * state2) / NumberOfPeopleInState
			// (idx3 * idx2 * idx1) / Num
			propensityFuncTypes[1] = (ref DiseaseState state, ref SimModel model, int[] argv) => {
				return model.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
			};

			//Here we would include the neighbor movement factor, which also needs some neighbor getting function and the cell conectivity param
			//We'll put all of that into simulation model I think?
		}
	}
}