
//Holds the disease state of a single cell, in other words,
//represents a single bucket in a compartment model

namespace CJSim {
	public struct DiseaseState {
		public readonly int[] state;
		
		//The only constructor, requires the state (compartment) count
		public DiseaseState(int stateCount) {
			state = new int[stateCount];

			setToZero();
		}

		//Allow for indexing this struct directly
		public int this[int index] {
			get {return state[index];}
			set {state[index] = value;}
		}

		//Copy constructor, this struct contains reference type that need to be explicitly copied
		public DiseaseState(DiseaseState other) {
			state = new int[other.stateCount];
			for (int q = 0; q< other.stateCount; q++) {
				state[q] = other.state[q];
			}
		}

		//Shorthand for getting the number of states (compartments)
		public int stateCount {
			get {
				return state.Length;
			}
		}

		//Get the sum
		public int numberOfPeople {
			get {
				//A for loop would be more general, but are we really going to change the number of states?
				//As it would turn out, yes
				int ret = 0;
				for (int q = 0; q < stateCount; q++) {
					ret += state[q];
				}
				return ret;
			}
		}

		//Sets every value in the state array to zero
		public void setToZero() {
			for (int q = 0; q < stateCount; q++) {
				state[q] = 0;
			}
		}
	}
}
