using NUnit.Framework;
using CJSim;

public class DiseaseStateTests {
	[Test]
	public void DiseaseStateTests_Constructor() {
		DiseaseState state = new DiseaseState(10);

		//State count should be set properly
		Assert.AreEqual(10, state.stateCount);
		
		//Elements are initialized to 0
		for (int q = 0; q < state.stateCount; q++) {
			Assert.AreEqual(0, state[q]);
		}
	}

	[Test]
	public void DiseaseStateTests_Index() {
		DiseaseState state = new DiseaseState(10);
		state[4] = 100;
		Assert.AreEqual(100, state[4]);
	}


	//Test the state copy constructor
	[Test]
	public void DiseaseStateTests_CopyConstructor() {
		DiseaseState goodState = new DiseaseState(10);
		goodState[3] = 3;
		goodState[4] = 4;

		DiseaseState copiedState = new DiseaseState(goodState);
		//Copy constructor works
		Assert.AreEqual(3, copiedState[3]);
		copiedState[3] = 100;
		//Make sure it doesn't edit the original
		Assert.AreEqual(3, goodState[3]);
		Assert.AreNotEqual(goodState[3], copiedState[3]);
	}


	//Test the state.numberOfPeople property
	[Test]
	public void DiseaseStateTests_NumberOfPeople() {
		DiseaseState state = new DiseaseState(10);
		Assert.AreEqual(0, state.numberOfPeople);
		state[1] = 100;
		Assert.AreEqual(100, state.numberOfPeople);
		state[5] = 50;
		Assert.AreEqual(150, state.numberOfPeople);
	}

	[Test]
	public void DiseaseStateTests_SetToZero() {
		DiseaseState state = new DiseaseState(10);
		state[1] = 100;
		state.setToZero();
		Assert.AreEqual(0, state[1]);
	}

	[Test]
	public void DiseaseStateTests_SetAs() {
		DiseaseState state1 = new DiseaseState(10);
		DiseaseState state2 = new DiseaseState(10);
		state1.setToZero();
		for (int q = 0; q < state2.stateCount; q++) {
			state2[q] = q + 10;
		}
		state1.setTo(state2);
		for (int q = 0; q < state1.stateCount; q++) {
			Assert.AreEqual(state1[q], state2[q]);
			//Make sure state2 hasn't been effected
			Assert.AreEqual(state2[q], q+10);
		}
		state1 = new DiseaseState(9);
		state1.setTo(state2);
		for (int q = 0; q < state1.stateCount; q++) {
			Assert.AreNotEqual(state1[q], state2[q]);
			//Make sure state2 hasn't been effected
			Assert.AreEqual(state2[q], q+10);
		}
	}

	[Test]
	public void DiseaseStateTests_Operators() {
		DiseaseState state1 = new DiseaseState(2);
		DiseaseState state2 = new DiseaseState(2);
		state1[0] = 1;
		state1[1] = 2;

		state2[0] = 10;
		state2[1] = 20;

		DiseaseState ret = state2 - state1;
		Assert.AreEqual(9, ret[0]);
		Assert.AreEqual(18, ret[1]);
		ret = state1 + state2;
		Assert.AreEqual(11, ret[0]);
		Assert.AreEqual(22, ret[1]);
	}
}
