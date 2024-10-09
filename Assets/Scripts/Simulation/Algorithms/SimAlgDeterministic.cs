
namespace CJSim
{
    public class SimAlgDeterministic : SimAlgApproximate
    {
        public SimAlgDeterministic(SimModelProperties props, SimMovementModel movement, double step) : base(props, movement, step)
        {
        }

        public override double getNextReactionTime(int stateIdx, ref DiseaseState readState)
        {
            return 0.0;
        }
        public override void performSingleReaction(int stateIdx, ref DiseaseState readState,
            ref DiseaseState writeState, double unknown = 0.0)
        {
            writeState.setTo(readState);

            for (int q = 0; q < properties.reactionCount; q++)
            {
                //Calculate this propensity function value, also add .5 for rounding
                double res = ((double)dispatchPropensityFunction(
                    ref readState, stateIdx, properties.reactionFunctionDetails[q]) * timestep);
                writeState.state[properties.stoichiometry[q].Item2] += res;
                writeState.state[properties.stoichiometry[q].Item1] -= res;
            }

            writeState.timeSimulated += timestep;
        }
    }
}
