//Holds algorithms that operate on disease states
using System;
using System.Collections.Generic;

namespace CJSim {
	class SimAlgorithms {
		#region Propensity Functions
		public delegate float ReactionFunctionTypes(int stateIdx, ref DiseaseState state, SimModel model, int[] argv);
		//Very important number! The static initializer will error out if this is too low!
		public const int propensityFunctionTypeCount = 3;

		public static ReactionFunctionTypes[] reactionFuncTypes;

		//Runs the correct propensity function given the magic numbers to describe it
		public static float dispatchPropensityFunction(int stateIdx, ref DiseaseState state, SimModel model, int[] argv) {
			return reactionFuncTypes[argv[0]](stateIdx, ref state, model, argv);
		}

		//Static initializer
		//If this function errors and the static initializer ran on a thread, unity will not display any error message
		static SimAlgorithms() {
			//Set up propensity functions
			reactionFuncTypes = new ReactionFunctionTypes[propensityFunctionTypeCount];

			//Basic type, state (idx 1) * param (idx 2)
			reactionFuncTypes[0] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				return (float)state.state[argv[1]] * model.parameters[argv[2]];
			};

			//Grey arrow, page 16 of the book, thing that depends on the density of infected
			// (param * state1 * state2) / NumberOfPeopleInState
			// (idx3 * idx2 * idx1) / Num
			reactionFuncTypes[1] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				return model.parameters[argv[3]] * ((state.state[argv[2]] * (float)state.state[argv[1]]) / (float)state.numberOfPeople);
			};

			//Here we would include the neighbor movement factor, which also needs some neighbor getting function and the cell conectivity param
			//We'll put all of that into simulation model I think?
			// (param(beta) * state1(sus)) * (neighborStuff * state2(infected))
			// (idx1 * idx2) * (neighborStuff * idx3)
			reactionFuncTypes[2] = (int stateIdx, ref DiseaseState state, SimModel model, int[] argv) => {
				int[] neighbors = model.movementModel.getNeighbors(stateIdx);

				float neighborFactor = 0.0f;
				for (int q = 0; q < neighbors.Length; q++) {
					throw new System.NotImplementedException();
				}
				return model.parameters[argv[1]] * state.state[argv[2]] * (neighborFactor);
			};
		}

		public static int getOrderOfReaction(int reactionId) {
			switch(reactionId) {
				case 0:
				return 1;
				case 1:
				return 2;
				default:
				throw new Exception();
			}
		}

		#endregion


		#region Determinstic

		//Does a basic deterministic tick of a disease state
		public static void deterministicTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, float reqTime) {
			writeState.setTo(readState);
			for (int q = 0; q < model.reactionCount; q++) {
				float res = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]) * reqTime;
				writeState.state[model.stoichiometry[q].Item2] += (int)res;
				writeState.state[model.stoichiometry[q].Item1] -= (int)res;
			}

			writeState.timeSimulated += reqTime;
		}

		#endregion

		#region Helpers

		//Returns the sum of all the propensity functions for this state
		private static float sumOfPropensityFunctions(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model) {
			float res = 0.0f;
			for (int q = 0; q < model.reactionCount; q++) {
				res += dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]);
			}
			return res;
		}

		#endregion

		#region Gillespie

		//Does a single reaction via the gillespie algorithm
		public static void gillespieTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, Random random) {
			writeState.setTo(readState);
			float sumProps = sumOfPropensityFunctions(stateIdx, ref readState, ref writeState, model);
			float sumPropsR2 = sumProps * (float)random.NextDouble();
			float tau = (float)((1.0 / sumProps) * Math.Log(1.0 / random.NextDouble()));
			
			float sum = 0.0f;
			for (int q = 0; q < model.reactionCount; q++) {
				float currProp = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q]);
				sum += currProp;
				if (sum > sumPropsR2) {
					//This is the reaction we do
					writeState.state[model.stoichiometry[q].Item1] -= 1;
					writeState.state[model.stoichiometry[q].Item2] += 1;
					break;
				}
			}
			writeState.timeSimulated += tau;
		}

		#endregion

		#region Poisson

		//https://rosettacode.org/wiki/Statistics/Normal_distribution#Lua
		//Returns a normal random variable with mean and variance^2
		public static double gaussian(float mean, float variance, Random random) {
			return Math.Sqrt(-2 * variance * Math.Log(random.NextDouble())) *
					Math.Cos(2 * Math.PI * random.NextDouble()) + mean;
		}


		//https://en.wikipedia.org/wiki/Poisson_distribution#Random_variate_generation
		public static int poissonSlow(float mean, Random random) {
			double L = Math.Exp(mean);
			int k = 0;
			double p = 1;
			do {
				k++;
				p *= random.NextDouble();
			} while(p > L);
			return k - 1;
		}

		public static int poissonFast(float mean, Random random) {
			int n = (int)(gaussian(mean, mean, random) + 0.5);
			return n < 0 ? 0 : n;
		}
		const float fastCutoff = 1.0f;
		public static int poisson(float mean, Random random) {
			return mean > fastCutoff ? poissonFast(mean, random) : poissonSlow(mean, random);
		}

		#endregion

		#region Tau Leaping

		//Auxiliary functions for tau leaping
		private static float auxiliaryFunctionMu(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, int i, List<int> nonCriticalReactions) {
			float ret = 0.0f;

			for (int q = 0; q < nonCriticalReactions.Count; q++) {
				ret += dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q])
				//If this is a negative affector, then we multiply by -1
				//cjnote I think I got the logic right on this not 100% sure can't lie
				* (model.stoichiometry[nonCriticalReactions[q]].Item1 == i ? -1 : 1)
				//If this reaction isn't related to the specified state then we don't count it
				* ((model.stoichiometry[nonCriticalReactions[q]].Item1 == i || model.stoichiometry[nonCriticalReactions[q]].Item2 == i) ? 1 : 0);
			}
			return ret;
		}
		private static float auxiliaryFunctionSigma(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, int i, List<int> nonCriticalReactions) {
			float ret = 0.0f;
			for (int q = 0; q < nonCriticalReactions.Count; q++) {
				//Same as Mu, but the stoichiometry is squared, which for us just mean it'll always be 1
				ret += dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[q])
				//If this reaction isn't related to the specified state then we don't count it
				* ((model.stoichiometry[nonCriticalReactions[q]].Item1 == i || model.stoichiometry[nonCriticalReactions[q]].Item2 == i) ? 1 : 0);
			}
			return ret;
		}

		//Does the Modified Poisson tau-leaping algorithm from https://doi.org/10.1063%2F1.2159468
		const int criticalThreshold = 10; //How many people makes a reaction critical, higher values means slower performance
		const float epsilon = 0.09f; //Error factor, higher means faster but less accurate
		const float multipleThreshold = 10.0f; //Some small multiple of 1/a0(x) that we usually take to be 10, higher values means better performance but less accurate at the lower part of things
		public static void tauLeapingTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, Random random) {
			writeState.setTo(readState);
			ThreadLogger.Log("Here bb");
			//cjnote I don't like making lists every single time
			List<int> nonCriticalReactions = new List<int>();
			List<int> criticalReactions = new List<int>();
			//Step 1, check which reactions are currently 'critical'
			//We do this a little differently than the paper because we don't have complicated reactions, only 1 to 1
			//cjnote will need to do something different once we include neighbor cells, maybe make a dedicated function to check populations?
			for (int q = 0; q < model.reactionCount; q++) {
				if (readState.state[model.stoichiometry[q].Item1] > criticalThreshold) {
					nonCriticalReactions.Add(q);
				} else {
					criticalReactions.Add(q);
				}
			}

			float tauCandidate1 = float.MaxValue;
			try {
				//Step 2, calculate tauCandidate1
				//Take the minimum of everything
				for (int i = 0; i < model.compartmentCount; i++) {
					tauCandidate1 = MathF.Min(
						tauCandidate1,
						//Because none of our reactions require 2 people gi is always just the HOR
						MathF.Max(epsilon * (readState.state[i] / (float)model.getHOR(i)), 1) / MathF.Abs(auxiliaryFunctionMu(stateIdx, ref readState, ref writeState, model, i, nonCriticalReactions))
					);
					tauCandidate1 = MathF.Min(
						tauCandidate1,
						//Because none of our reactions require 2 people gi is always just the HOR
						MathF.Pow(MathF.Max(epsilon * (readState.state[i] / (float)model.getHOR(i)), 1),2) / auxiliaryFunctionSigma(stateIdx, ref readState, ref writeState, model, i, nonCriticalReactions)
					);
				}
			} catch (System.Exception e) {
				ThreadLogger.Log(e.Message);
				return;
			}


			bool wouldCauseNegative = false;
			do {
			ThreadLogger.Log("Here bb");
				wouldCauseNegative = false;

				//Step 3, if tauCandidate1 is too small then we do some gillespie steps
				float propSum = sumOfPropensityFunctions(stateIdx, ref readState, ref writeState, model);
				if (tauCandidate1 < (multipleThreshold / propSum)) {
					DiseaseState fakeRead = new DiseaseState(readState);
					for (int q = 0; q < 100; q++) {

						SimAlgorithms.gillespieTick(stateIdx, ref fakeRead, ref writeState, model, random);
						fakeRead.setTo(writeState);
					}
					ThreadLogger.Log("Did 100 gillespie ticks");
					//tauLeapingTick(stateIdx, ref readState, ref writeState, model, random);
					return;
				}

				//Step 4, get the sum of all the critical reactions and use that to get a second tau candidate
				float sumCritical = 0.0f;
				foreach (int idx in criticalReactions) {
					float thisOne = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[idx]);
					sumCritical += thisOne;
				}
				float tauCandidate2 = (float)(-Math.Log(random.NextDouble()) / sumCritical);

				//Step 5, picking the option
				int[] kReactions = new int[model.reactionCount];
				Array.Fill<int>(kReactions, 0);
				float tau;
				if (tauCandidate1 < tauCandidate2) {
					tau = tauCandidate1;
					//5.1, fire only non critical reactions
					//We do this no matter the tau chosen so it's outside the if condition

				} else {
					tau = tauCandidate2;
					//Collect point probabilities
					List<float> pointProbabilities = new List<float>();
					foreach (int idx in criticalReactions) {
						float thisOne = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[idx]);
						pointProbabilities.Add(thisOne / sumCritical);
					}
					pointProbabilities.Sort();
					float rand = (float)random.NextDouble();
					for (int q = 0; q < pointProbabilities.Count; q++) {
						//List is sorted so can do easy comparison
						if (rand < pointProbabilities[q]) {
							//This is the critical reaction that will be ran this step
							kReactions[q] = 1;
							//Briefly verify that this would not cause negative population
							//This would presumably never be true, but whatever better safe than sorry
							if (readState.state[model.stoichiometry[q].Item1] < 1) {
								ThreadLogger.Log("P sure this ain't supposed to happen");
								wouldCauseNegative = true;
								tauCandidate1 /= 2.0f;
								continue;
							}
						}
					}
				}

				//Non critical reactions
				foreach (int reactionIdx in nonCriticalReactions) {
					float prop = dispatchPropensityFunction(stateIdx, ref readState, model, model.reactionFunctionDetails[reactionIdx]) * tau;
					int reactions = poissonFast(prop, random);
					kReactions[reactionIdx] = reactions;

					if (readState.state[model.stoichiometry[reactionIdx].Item1] < reactions) {
						ThreadLogger.Log("Would go negative");
						wouldCauseNegative = true;
						tauCandidate1 /= 2.0f;
						continue;
					}
				}

				//Step 6, finally do the reactions
				//cjnote, the entirety of this system needs to be tests, this code has never been run
				for (int q = 0; q < kReactions.Length; q++) {
					writeState.state[model.stoichiometry[q].Item1] -= kReactions[q];
					writeState.state[model.stoichiometry[q].Item2] += kReactions[q];
				}
				writeState.timeSimulated += tau;
			ThreadLogger.Log("Done!");

			} while (wouldCauseNegative);

		}

		#endregion
	}
}
