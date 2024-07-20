//Holds algorithms that operate on disease states
//Currently in the process of being gutted
using System;
using System.Collections.Generic;

namespace CJSim {
	class SimAlgorithms {


		#region Determinstic

		//Does a basic deterministic tick of a disease state
		public static void deterministicTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, SimCore core, Random random, float reqTime) {
			writeState.setTo(readState);
			for (int q = 0; q < model.reactionCount; q++) {
				float res = (dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[q]) * reqTime) + 0.5f;
				writeState.state[model.stoichiometry[q].Item2] += (int)res;
				writeState.state[model.stoichiometry[q].Item1] -= (int)res;
			}

			writeState.timeSimulated += reqTime;
		}

		public static void deterministicWithGillespieTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, SimCore core, Random random, float reqTime) {
			
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
			double L = Math.Exp(-mean);
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
		private static float auxiliaryFunctionMu(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, SimCore core, Random random, int i, List<int> nonCriticalReactions) {
			float ret = 0.0f;

			for (int q = 0; q < nonCriticalReactions.Count; q++) {
				int reaction = nonCriticalReactions[q];
				ret += dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[reaction])
				//If this is a negative affector, then we multiply by -1
				//cjnote I think I got the logic right on this not 100% sure can't lie
				* (model.stoichiometry[reaction].Item1 == i ? -1 : 1)
				//If this reaction isn't related to the specified state then we don't count it
				* ((model.stoichiometry[reaction].Item1 == i || model.stoichiometry[reaction].Item2 == i) ? 1 : 0);
			}
			return ret;
		}
		private static float auxiliaryFunctionSigma(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, SimCore core, Random random, int i, List<int> nonCriticalReactions) {
			float ret = 0.0f;
			for (int q = 0; q < nonCriticalReactions.Count; q++) {
				int reaction = nonCriticalReactions[q];
				//Same as Mu, but the stoichiometry is squared, which for us just mean it'll always be 1
				ret += dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[reaction])
				//If this reaction isn't related to the specified state then we don't count it
				* ((model.stoichiometry[reaction].Item1 == i || model.stoichiometry[reaction].Item2 == i) ? 1 : 0);
			}
			return ret;
		}

		//Does the Modified Poisson tau-leaping algorithm from https://doi.org/10.1063%2F1.2159468
		const int criticalThreshold = 5; //How many people makes a reaction critical, higher values means slower performance
		const float epsilon = 0.19f; //Error factor, higher means faster but less accurate
		const float multipleThreshold = 10.0f; //Some small multiple of 1/a0(x) that we usually take to be 10, lower values means we'll do gillespie steps less often
		const int gillespieSteps = 100; //How many gillespie steps to run if we need to run gillespie steps
		public static void tauLeapingTick(int stateIdx, ref DiseaseState readState, ref DiseaseState writeState, SimModel model, SimCore core, Random random) {
			writeState.setTo(readState);
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
			//Step 2, calculate tauCandidate1
			//Take the minimum of everything
			for (int i = 0; i < model.compartmentCount; i++) {
				tauCandidate1 = MathF.Min(
					tauCandidate1,
					//Because none of our reactions require 2 people gi is always just the HOR
					MathF.Max(epsilon * (readState.state[i] / (float)model.getHOR(i)), 1) / MathF.Abs(auxiliaryFunctionMu(stateIdx, ref readState, ref writeState, model, core, random, i, nonCriticalReactions))
				);
				tauCandidate1 = MathF.Min(
					tauCandidate1,
					//Because none of our reactions require 2 people gi is always just the HOR
					MathF.Pow(MathF.Max(epsilon * (readState.state[i] / (float)model.getHOR(i)), 1),2) / auxiliaryFunctionSigma(stateIdx, ref readState, ref writeState, model, core, random, i, nonCriticalReactions)
				);
			}


			bool wouldCauseNegative = false;
			do {
				wouldCauseNegative = false;

				//Step 3, if tauCandidate1 is too small then we do some gillespie steps
				float propSum = sumOfPropensityFunctions(stateIdx, ref readState, ref writeState, core, model, random);
				if (tauCandidate1 < (multipleThreshold / propSum)) {
					DiseaseState fakeRead = new DiseaseState(readState);
					for (int q = 0; q < gillespieSteps; q++) {
						SimAlgorithms.gillespieTick(stateIdx, ref fakeRead, ref writeState, model, core, random);
						fakeRead.setTo(writeState);
					}
					return;
				}

				//Step 4, get the sum of all the critical reactions and use that to get a second tau candidate
				float sumCritical = 0.0f;
				foreach (int idx in criticalReactions) {
					float thisOne = dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[idx]);
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
					List<Tuple<float, int>> pointProbabilities = new List<Tuple<float, int>>();
					foreach (int idx in criticalReactions) {
						float thisOne = dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[idx]);
						pointProbabilities.Add(new Tuple<float, int>(thisOne / sumCritical, idx));
					}
					pointProbabilities.Sort();
					float rand = (float)random.NextDouble();
					for (int q = 0; q < pointProbabilities.Count; q++) {
						//List is sorted so can do easy comparison
						if (rand < pointProbabilities[q].Item1) {
							//This is the critical reaction that will be ran this step
							int reaction = pointProbabilities[q].Item2;
							kReactions[reaction] = 1;
							//Briefly verify that this would not cause negative population
							//This would presumably never be true, but whatever better safe than sorry
							if (readState.state[model.stoichiometry[reaction].Item1] < 1) {
								ThreadLogger.Log("P sure this ain't supposed to happen");
								ThreadLogger.Log(model.stoichiometry[reaction].Item1.ToString() + " is compartment and " + reaction + " is reaction");
								wouldCauseNegative = true;
								tauCandidate1 /= 2.0f;
								//Uhh don't worry about it
								//We get caught in an infinite loop without this
								//And it should never happen anyway so how do I know that?
								return;
							}
						} else {
							rand -= pointProbabilities[q].Item1;
						}
					}
				}

				//If tau is infinity don't bother doing anything
				if (float.IsInfinity(tau)) {
					return;
				}

				//Non critical reactions
				foreach (int reactionIdx in nonCriticalReactions) {
					float prop = dispatchPropensityFunction(stateIdx, ref readState, model, core, random, model.reactionFunctionDetails[reactionIdx]) * tau;
					int reactions = poisson(prop, random);
					kReactions[reactionIdx] = reactions;

					if (readState.state[model.stoichiometry[reactionIdx].Item1] < reactions) {
						wouldCauseNegative = true;
						tauCandidate1 /= 2.0f;
						break;
					}
				}
				//Can't continue from the above loop because it's in a loop
				if (wouldCauseNegative) continue;

				//Step 6, finally do the reactions
				//cjnote, the entirety of this system needs to be tests, this code has never been run
				for (int q = 0; q < kReactions.Length; q++) {
					writeState.state[model.stoichiometry[q].Item1] -= kReactions[q];
					writeState.state[model.stoichiometry[q].Item2] += kReactions[q];
				}
				writeState.timeSimulated += tau;

			} while (wouldCauseNegative);

		}

		#endregion
	}
}
