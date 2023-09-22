using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
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
	}

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
	public void DiseaseStateTests_SetTo0() {
		DiseaseState state = new DiseaseState(10);
		state[1] = 100;
		state.setToZero();
		Assert.AreEqual(0, state[1]);
	}
}
