using System;

//Class representing a disease model
//Holds information relating to disease states, algorithms, etc.

namespace CJSim {
	public abstract class SimModel {
		public float[] parameters;

		//stoichiometry[0] = (1,2): the 1st reaction goes from 2nd compartment to 3rd compartment
		public Tuple<int,int>[] stoichiometry;

		//Contains arguments to the SimAlgorithms class, defines a set of reactions
		public int[][] reactionFunctionDetails;

		public readonly int compartmentCount;

		#region Properties

		//The number of different types of reactions in the model
		public int reactionCount {
			get {
				//Could also very well be stoichiometry.Length
				return reactionFunctionDetails.Length;
			}
		}

		#endregion
		
		//Interface is WIP, need to be able to load from files in the future

		SimModel(int compartmentCount, int reactionCount, int parameterCount) {
			this.compartmentCount = compartmentCount;

			stoichiometry = new Tuple<int, int>[reactionCount];
		}

		//Not sure if I'll leave this function in this class cjnote
		public abstract float getCellConnectivity();

	}
}
