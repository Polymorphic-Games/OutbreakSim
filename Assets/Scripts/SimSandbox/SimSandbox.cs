using CJSim;
using UnityEngine;
using DataVisualizer;
using UnityEngine.Profiling;

public class SimSandbox : SimSandboxBase
{
    public DataSeriesChart chart;

    [SerializeField]
    private Simulation simulation;

    [SerializeField]
    private SimModelProperties props;

    private void Start()
    {
        Application.targetFrameRate = 60;
        //Make a basic simulation
        SimMovementModel movementModel = new SimMovementNone();

        props = new SimModelProperties(3, 2, 2, 1);
        //S,I,R,,,S->I,I->R,,,B,R
        props.reactionFunctionDetails[0] = new int[] { 1, 0, 1, 0 };
        props.reactionFunctionDetails[1] = new int[] { 0, 1, 1 };

        props.stoichiometry[0] = new System.Tuple<int, int>(0, 1);
        props.stoichiometry[1] = new System.Tuple<int, int>(1, 2);

        props.parameters[0] = 1.0f;
        props.parameters[1] = 0.1f;

        props.readCells[0].state[0] = 20000;
        props.readCells[0].state[1] = 2;
        props.readCells[0].state[2] = 0;

        simulation = new Simulation(
            new SimCore(
            new SimModel(
                new SimAlgHybrid(props, movementModel, 
                    new SimAlgDeterministic(props, movementModel, step),
                    new SimAlgGillespie(props, movementModel))), 1));

        initChart(chart);
    }

    double step = 0.3;
    double time = 0.0;
    double lastTime = 0.0f;
    private void Update()
    {
        //Press N to do 10 steps
        //if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.N))
        //{

        Profiler.BeginSample("Simulation Update");

            for (int q = 0; q < (Input.GetKeyDown(KeyCode.N) ? 10 : 1); q++)
            {
                //With gillespie the very last reaction will be at infinity time it's rough
                if (simulation.model.properties.readCells[0].timeSimulated - lastTime >= 1000.0f)
                {
                    break;
                }

            //Profiler.BeginSample("Update Chart");
            //    updateChart(chart, simulation.model.properties.readCells[0]);
            //Profiler.EndSample();

                lastTime = simulation.model.properties.readCells[0].timeSimulated;
                time += step;
                //simulation.model.algorithm.performSingleReaction(0, ref simulation.model.algorithm.properties.readCells[0], ref simulation.model.algorithm.properties.writeCells[0]);
                //simulation.model.algorithm.properties.readCells[0].setTo(simulation.model.algorithm.properties.writeCells[0]);
                simulation.core.tickSimulation(time);
                //dumpSim();
            }

        Profiler.EndSample();
        //}
    }

    private void dumpSim()
    {
        Debug.Log("Sim Dump 0 At " + simulation.model.properties.readCells[0].timeSimulated.ToString() + "\n" + simulation.model.properties.readCells[0].ToString());
    }
}
