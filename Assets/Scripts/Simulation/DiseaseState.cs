
//Holds the disease state of a single cell, in other words,
//represents a single bucket in a compartment model

namespace CJSim {
	public struct DiseaseState {
		public readonly int[] state;
		//cjnote might make this a double
		public float timeSimulated;

		//The main constrcutor, requires the state (compartment) count
		public DiseaseState(int stateCount) {
			state = new int[stateCount];
			timeSimulated = 0.0f;
			setToZero();
		}

		//Allow for indexing this struct directly
		public int this[int index] {
			get {return state[index];}
			set {state[index] = value;}
		}

		//Copy constructor, this struct contains a reference type that needs to be explicitly copied
		public DiseaseState(DiseaseState other) {
			state = new int[other.stateCount];
			timeSimulated = other.timeSimulated;
			for (int q = 0; q< other.stateCount; q++) {
				state[q] = other.state[q];
			}
		}

		//Constructors for the lazy man
		public DiseaseState(SimModel model) : this(model.properties.compartmentCount) {}
		public DiseaseState(SimModelProperties props) : this(props.compartmentCount) {}

		//Shorthand for getting the number of states (compartments)
		public int stateCount {
			get {
				return state.Length;
			}
		}

		//Calculates the sum of all the compartments
		public int numberOfPeople {
			get {
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
			timeSimulated = 0.0f;
		}

		public override string ToString() {
			if (stateCount <= 0) {
				return "";
			}
			string ret = "";
			for (int q = 0; q < stateCount-1; q++) {
				ret += state[q] + ", ";
			}
			ret += state[stateCount-1];
			return ret;
		}

		//Sets a state to another one
		public void setTo(DiseaseState other) {
			if (other.stateCount == stateCount) {
				for (int q = 0; q < stateCount; q++) {
					state[q] = other.state[q];
				}
				timeSimulated = other.timeSimulated;
			}
		}
	}
}
