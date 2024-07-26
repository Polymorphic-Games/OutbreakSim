using System;

//Class representing a disease model
//Holds information relating to disease states, algorithms, etc.

namespace CJSim {
	public class SimModel {
		public SimModelProperties properties;
		public SimModelAlgorithm algorithm;
		public IMovementModel movementModel;
		
		//Build a model from the various components
		public SimModel(SimModelProperties properties, SimModelAlgorithm algorithm, IMovementModel movement) {
			this.properties = properties;
			this.movementModel = movement;
			this.algorithm = algorithm;
			
			algorithm.onModelCreate(this);
		}


		//Loads a model from a file (in a constructor)
		
		public SimModel(string filename) {
			throw new System.NotImplementedException();
		}

		//Writes a model to a file
		public void writeToFile(string filename) {
			throw new System.NotImplementedException();
		}

		public void loadFromFile(string filename) {
			throw new System.NotImplementedException();
		}
	}
}
