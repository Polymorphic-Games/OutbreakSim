using System;

//Holds properties of a simulation model, things like compartments, stoichiometry, reactions, etc.
namespace CJSim {
	public class SimModelProperties {
		//Sim parameters, just floats that impact the compartmental model
		public float[] parameters;

		//stoichiometry[0] = (1,2): the 1st reaction goes from 2nd compartment to 3rd compartment
		public Tuple<int,int>[] stoichiometry;

		//Contains arguments to the SimAlgorithms class, defines a set of reactions
		//reactionFunctionDetails[0] = [0,1,2,3]  -  the first reactions, pass these args to SimAlgos
		public int[][] reactionFunctionDetails;
		//Number of compartments this model expects
		public int compartmentCount;

		public SimModelProperties(int compartmentCount, int reactionCount, int parameterCount) {
			stoichiometry = new Tuple<int, int>[reactionCount];
			reactionFunctionDetails = new int[reactionCount][];
			parameters = new float[parameterCount];
			this.compartmentCount = compartmentCount;
		}
	}
}