using System.Collections;
using System.Collections.Generic;

//This is a base class, please extend it to take advantage of it
//Feeds the simulation its cells and does something useful with the output
//Implementations for textures and single cells are coming soon

namespace CJSim {
	public abstract class SimFeeder {
		
		//Tell the simulation how many cells it needs
		public int howManyCells {get; private set;}

	}
}
