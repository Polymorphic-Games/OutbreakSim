using System;

//Class representing a disease model
//Holds information relating to disease states, algorithms, etc.

namespace CJSim {

	//Model type determines how the model is processed, i.e deterministically or stochastically
	public enum ModelType {
		Deterministic,
		Stochastic
	}

	public class SimModel {
		public float[] parameters;

		//stoichiometry[0] = (1,2): the 1st reaction goes from 2nd compartment to 3rd compartment
		public Tuple<int,int>[] stoichiometry;

		//Contains arguments to the SimAlgorithms class, defines a set of reactions
		//reactionFunctionDetails[0] = [0,1,2,3]  -  the first reactions, pass these args to SimAlgos
		public int[][] reactionFunctionDetails;
		
		public IMovementModel movementModel;

		#region Properties

		//The tpye of this model
		public ModelType modelType {private set; get;}
		
		//The number of different reactions in the model
		public int reactionCount {private set; get;}

		//The number of compartments in this model
		public int compartmentCount {private set; get;}

		//The number of parameters in this model
		public int parameterCount {private set; get;}

		#endregion
		
		//Interface is WIP, need to be able to load from files in the future

		//The 'count' constructor, used for when you have some code to populate the various necessary fields
		//Makes the basic data structures have the correct size, just unpopulated
		public SimModel(int compartmentCount, int reactionCount, int parameterCount, IMovementModel movement, ModelType modelType) {
			this.reactionCount = reactionCount;
			this.compartmentCount = compartmentCount;
			this.parameterCount = parameterCount;
			this.modelType = modelType;
			this.movementModel = movement;

			stoichiometry = new Tuple<int, int>[reactionCount];
			reactionFunctionDetails = new int[reactionCount][];
			parameters = new float[parameterCount];
		}


		//Loads a model from a file
		
		public SimModel(string filename) {
			throw new System.NotImplementedException();
		}

	}
}
