using System;

//Class representing a disease model
//Currently just holds an algorithm for no reason
//But in the future could hold all sorts of things
//Mainly metadata about the model, things like saved colors and names for things

namespace CJSim
{
    [System.Serializable]
    public class SimModel
    {
        public SimModelAlgorithm algorithm { get; private set; }
        public SimModelProperties properties
        {
            get
            {
                return algorithm.properties;
            }
        }
        public SimMovementModel movementModel
        {
            get
            {
                return algorithm.movementModel;
            }
        }

        //Build a model
        public SimModel(SimModelAlgorithm algorithm)
        {
            this.algorithm = algorithm;
        }

        //Loads a model from a file (in a constructor)
        public SimModel(string filename)
        {
            throw new System.NotImplementedException();
        }

        //Writes a model to a file
        public void writeToFile(string filename)
        {
            throw new System.NotImplementedException();
        }

        //Loads a model from a file (in a public member function)
        public void loadFromFile(string filename)
        {
            throw new System.NotImplementedException();
        }
    }
}
