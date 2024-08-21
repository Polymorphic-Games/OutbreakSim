using NUnit.Framework;
using CJSim;


//Test the SimModelProps class
public class SimModelPropsTests {
	//Can I use this class without crashing and burning?
	[Test]
	public void SimModelPropsTest_Generic() {
		//The most basic model you can make here, just make sure this code doesn't error

		SimModelProperties props = new SimModelProperties(3, 2, 2, 1);
		//S,I,R,,,S->I,I->R,,,B,R
		props.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		props.reactionFunctionDetails[1] = new int[]{0,1,1};

		props.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		props.stoichiometry[1] = new System.Tuple<int, int>(1,2);

		props.parameters[0] = 1.0f;
		props.parameters[1] = 0.1f;

		props.readCells[0].state[0] = 1000000;
		props.readCells[0].state[1] = 100;
		props.readCells[0].state[2] = 0;
	}
	[Test]
	public void SimModelPropsTests_CopyConstructor() {
		//Does the copy constructor work properly?
		//And does it copy into new memory like it should?

		SimModelProperties propsOG = new SimModelProperties(3, 2, 2, 1);
		//S,I,R,,,S->I,I->R,,,B,R
		propsOG.reactionFunctionDetails[0] = new int[]{1,0,1,0};
		propsOG.reactionFunctionDetails[1] = new int[]{0,1,1};

		propsOG.stoichiometry[0] = new System.Tuple<int, int>(0,1);
		propsOG.stoichiometry[1] = new System.Tuple<int, int>(1,2);

		propsOG.parameters[0] = 1.0f;
		propsOG.parameters[1] = 0.1f;

		propsOG.readCells[0].state[0] = 1000000;
		propsOG.readCells[0].state[1] = 100;
		propsOG.readCells[0].state[2] = 0;

		SimModelProperties propsNew = new SimModelProperties(propsOG);

		//Check that everything is the same
		Assert.AreEqual(propsOG.reactionCount, propsNew.reactionCount);
		Assert.AreEqual(propsOG.compartmentCount, propsNew.compartmentCount);
		Assert.AreEqual(propsOG.cellCount, propsNew.cellCount);
		Assert.AreEqual(propsOG.parameterCount, propsNew.parameterCount);

		//Check that values are the same and that they can be changed (new memory)
		for (int q = 0; q < propsNew.reactionCount; q++) {
			//Check rfd's
			Assert.AreEqual(propsOG.reactionFunctionDetails[q].Length, propsNew.reactionFunctionDetails[q].Length);
			for (int rfd = 0; rfd < propsOG.reactionFunctionDetails[q].Length; rfd++) {
				Assert.AreEqual(propsOG.reactionFunctionDetails[q][rfd], propsNew.reactionFunctionDetails[q][rfd]);
				propsNew.reactionFunctionDetails[q][rfd]++;
				Assert.AreNotEqual(propsOG.reactionFunctionDetails[q][rfd], propsNew.reactionFunctionDetails[q][rfd]);
			}

			//Also check stoich
			Assert.AreEqual(propsOG.stoichiometry[q].Item1, propsNew.stoichiometry[q].Item1);
			Assert.AreEqual(propsOG.stoichiometry[q].Item2, propsNew.stoichiometry[q].Item2);

			propsNew.stoichiometry[q] = new System.Tuple<int, int>(propsOG.stoichiometry[q].Item1 + 1, propsOG.stoichiometry[q].Item2 + 1);
			Assert.AreNotEqual(propsOG.stoichiometry[q].Item1, propsNew.stoichiometry[q].Item1);
			Assert.AreNotEqual(propsOG.stoichiometry[q].Item2, propsNew.stoichiometry[q].Item2);
		}

		//Parameters
		for (int q = 0; q < propsOG.parameterCount; q++) {
			Assert.AreEqual(propsOG.parameters[q], propsNew.parameters[q]);
			propsNew.parameters[q] += 1.0;
			Assert.AreNotEqual(propsOG.parameters[q], propsNew.parameters[q]);
		}

		//Cells
		for (int q = 0; q < propsOG.cellCount; q++) {
			for (int comp = 0; comp < propsOG.compartmentCount; comp++) {
				Assert.AreEqual(propsOG.readCells[q][comp], propsNew.readCells[q][comp]);
				propsNew.readCells[q][comp] += 1;
				Assert.AreNotEqual(propsOG.readCells[q][comp], propsNew.readCells[q][comp]);

				Assert.AreEqual(propsOG.writeCells[q][comp], propsNew.writeCells[q][comp]);
				propsNew.writeCells[q][comp] += 1;
				Assert.AreNotEqual(propsOG.writeCells[q][comp], propsNew.writeCells[q][comp]);
			}
		}
	}
}
