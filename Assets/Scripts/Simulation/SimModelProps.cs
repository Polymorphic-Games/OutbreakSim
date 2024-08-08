using System;

//Holds properties of a simulation model, things like compartments, stoichiometry, reactions, etc.
namespace CJSim {
	public class SimModelProperties {
		//Sim parameters, just doubles that impact the compartmental model
		public double[] parameters;

		//stoichiometry[0] = (1,2): the 1st reaction goes from 2nd compartment to 3rd compartment
		public Tuple<int,int>[] stoichiometry;

		//Contains arguments to the SimAlgorithms class, defines a set of reactions
		//reactionFunctionDetails[0] = [0,1,2,3]  -  the first reactions, pass these args to SimAlgos
		public int[][] reactionFunctionDetails;
		//Number of compartments this model expects
		public int compartmentCount;

		//The number of different reactions in the model
		public int reactionCount {get {
			return reactionFunctionDetails.Length;
		}}
		//The number of parameters in this model
		public int parameterCount {get {
			return parameters.Length;
		}}
		public int cellCount {
			get {
				return readCells.Length;
		}}

		//Always read from readCells, write to writeCells
		//Unless we are from outside the simulation, then we must write to readCells when the sim isn't running
		public DiseaseState[] readCells;
		public DiseaseState[] writeCells;

		public SimModelProperties(int compartmentCount, int reactionCount, int parameterCount, int cellCount) {
			stoichiometry = new Tuple<int, int>[reactionCount];
			reactionFunctionDetails = new int[reactionCount][];
			parameters = new double[parameterCount];
			this.compartmentCount = compartmentCount;

			initCells(cellCount);
		}

		//Gets the highest order of reaction for this compartment
		//cjnote should be easy to cache this somewhere, it's also only used for tau leaping tho so not much need to bother
		public int getHOR(int compartment) {
			int HOR = 0;
			//Search reactions for things affecting this compartment
			for (int q = 0; q < reactionCount; q++) {
				if (stoichiometry[q].Item1 == compartment || stoichiometry[q].Item2 == compartment) {
					HOR = Math.Max(HOR, SimModelAlgorithm.getOrderOfReaction(reactionFunctionDetails[q][0]));
				}
			}
			return HOR;
		}

		//Initializes cell arrays in a very basic way
		//Anything complex needs to be done by you
		private void initCells(int _cellCount) {
			readCells = new DiseaseState[_cellCount];
			writeCells = new DiseaseState[_cellCount];
			for (int q = 0; q < _cellCount; q++) {
				readCells[q] = new DiseaseState(this);
				writeCells[q] = new DiseaseState(this);
			}
		}

		//Validate the model properties (makes sure things are initialized and not conflicting)
		//Returns true if everything is alright, false otherwise
		//Doesn't guarantee that a model will actually work properly, but at the very least it should make sure the model won't crash the simulation
		public bool validate() {
			//First verify that the user provided arrays are in fact provided
			//Reaction function details must be provided, how else would we know what parameters go where
			for (int q = 0; q < reactionCount; q++) {
				if (reactionFunctionDetails[q] == null) {
					return false;
				}
				//Also check for stoichiometry, how else would we know which states go to which other states
				if (stoichiometry[q] == null) {
					return false;
				}
			}

			return true;
		}
	}
}