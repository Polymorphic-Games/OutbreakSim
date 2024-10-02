
//Holds the disease state of a single cell, in other words,
//represents a single bucket in a compartment model

namespace CJSim
{

    [System.Serializable]
    public struct DiseaseState
    {
        public readonly double[] state;
        public double timeSimulated;

        //The main constrcutor, requires the state (compartment) count
        public DiseaseState(int stateCount)
        {
            state = new double[stateCount];
            timeSimulated = 0.0;
            setToZero();
        }

        //Allow for indexing this struct directly
        public double this[int index]
        {
            get { return state[index]; }
            set { state[index] = value; }
        }

        //Copy constructor, this struct contains a reference type that needs to be explicitly copied
        public DiseaseState(DiseaseState other)
        {
            state = new double[other.stateCount];
            timeSimulated = other.timeSimulated;
            for (int q = 0; q < other.stateCount; q++)
            {
                state[q] = other.state[q];
            }
        }

        //Constructors for the lazy man
        public DiseaseState(SimModel model) 
            : this(model.properties.compartmentCount) 
        { 
        
        }

        public DiseaseState(SimModelProperties props) 
            : this(props.compartmentCount) 
        {
        
        }

        //Shorthand for getting the number of states (compartments)
        public int stateCount
        {
            get
            {
                return state.Length;
            }
        }

        //Calculates the sum of all the compartments
        public double numberOfPeople
        {
            get
            {
                double ret = 0;
                for (int q = 0; q < stateCount; q++)
                {
                    ret += state[q];
                }
                return ret;
            }
        }

        //Sets every value in the state array to zero
        public void setToZero()
        {
            for (int q = 0; q < stateCount; q++)
            {
                state[q] = 0;
            }
            timeSimulated = 0.0;
        }

        //Tostring function, largely for easy debugging purposes
        public override string ToString()
        {
            if (stateCount <= 0)
            {
                return "";
            }
            string ret = "";
            for (int q = 0; q < stateCount - 1; q++)
            {
                ret += state[q] + ", ";
            }
            ret += state[stateCount - 1];
            return ret;
        }

        //Sets a state to another one
        public void setTo(DiseaseState other)
        {
            if (other.stateCount == stateCount)
            {
                for (int q = 0; q < stateCount; q++)
                {
                    state[q] = other.state[q];
                }
                timeSimulated = other.timeSimulated;
            }
            else
            {
                throw new System.Exception("Disease states must have same state count");
            }
        }

        //Addition and subtraction operators
        public static DiseaseState operator -(DiseaseState a, DiseaseState b)
        {
            DiseaseState ret = new DiseaseState(a);
            if (a.stateCount != b.stateCount)
            {
                throw new System.Exception("Disease States must have the same number of states for subtraction");
            }
            else
            {
                for (int q = 0; q < ret.stateCount; q++)
                {
                    ret.state[q] -= b.state[q];
                }
            }
            return ret;
        }
        public static DiseaseState operator +(DiseaseState a, DiseaseState b)
        {
            DiseaseState ret = new DiseaseState(a);
            if (a.stateCount != b.stateCount)
            {
                throw new System.Exception("Disease States must have the same number of states for subtraction");
            }
            else
            {
                for (int q = 0; q < ret.stateCount; q++)
                {
                    ret.state[q] += b.state[q];
                }
            }
            return ret;
        }

        //Round the numbers in the disease state
        public void roundNumbers()
        {
            for (int q = 0; q < stateCount; q++)
            {
                state[q] = (int)(state[q] + .5);
            }
        }

    }
}
