using System;

//Class representing a disease model
//Holds information relating to disease states, algorithms, etc.

namespace CJSim {
	public enum ModelType {
		Deterministic
	}

	public class SimModel {
		public float[] parameters;
		public ModelType type {private set; get;}

		//stoichiometry[0] = (1,2): the 1st reaction goes from 2nd compartment to 3rd compartment
		public Tuple<int,int>[] stoichiometry;

		//Contains arguments to the SimAlgorithms class, defines a set of reactions
		//reactionFunctionDetails[0] = [0,1,2,3]  -  the first reactions, pass these args to SimAlgos
		public int[][] reactionFunctionDetails;

		public readonly int compartmentCount;

		#region Properties
		
		//The number of different types of reactions in the model
		public int reactionCount {private set; get;}

		#endregion
		
		//Interface is WIP, need to be able to load from files in the future

		//The 'count' constructor, used for when you have some code to populate the various necessary fields
		public SimModel(int compartmentCount, int reactionCount, int parameterCount, ModelType type) {
			this.reactionCount = reactionCount;
			this.compartmentCount = compartmentCount;

			stoichiometry = new Tuple<int, int>[reactionCount];
			reactionFunctionDetails = new int[reactionCount][];
		}


		//Loads a model from a file
		public SimModel(string filename) {
			throw new System.NotImplementedException();
		}

		//Not sure if I'll leave this function in this class cjnote
		public float getCellConnectivity() {throw new System.NotImplementedException();}

	}
}
