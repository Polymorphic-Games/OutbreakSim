using CJSim;
using UnityEngine;
using DataVisualizer;

public class SimSandboxMulti : SimSandboxBase
{
    public DataSeriesChart chartTL;
    public DataSeriesChart chartBL;
    public DataSeriesChart chartTR;
    public DataSeriesChart chartBR;

    [SerializeField]
    private Simulation simulation;
    [SerializeField]
    private SimModelProperties props;

    private void Start()
    {
        Application.targetFrameRate = 60;
        //Make a basic simulation
        SimMovementAllConnected movementModel = new SimMovementAllConnected(4);
        movementModel.setCellConnectivity(0, 1, 0.01f);
        movementModel.setCellConnectivity(1, 3, 0.01f);
        movementModel.setCellConnectivity(3, 2, 0.01f);
        movementModel.setCellConnectivity(2, 0, 0.01f);

        props = new SimModelProperties(3, 3, 2, 4);
        //S,I,R,,,S->I,I->R,,,B,R
        props.reactionFunctionDetails[0] = new int[] { 1, 0, 1, 0 };
        props.reactionFunctionDetails[1] = new int[] { 0, 1, 1 };
        props.reactionFunctionDetails[2] = new int[] { 2, 0, 0, 1 };

        props.stoichiometry[0] = new System.Tuple<int, int>(0, 1);
        props.stoichiometry[1] = new System.Tuple<int, int>(1, 2);
        props.stoichiometry[2] = new System.Tuple<int, int>(0, 1);

        props.parameters[0] = 1.0;
        props.parameters[1] = 0.1;


        /*
         * think there is a bug either in the gillepsi algorithm
         * OR a bug with this while setting up multiple buckets
         * happens when read cells two and three have no infected
         * 
         * then after that it seems like it doesn't update amount of sick or dead.
         * 
         */


        props.readCells[0].state[0] = 100;
        props.readCells[0].state[1] = 10;
        props.readCells[0].state[2] = 10;

        props.readCells[1].state[0] = 100;
        //props.readCells[1].state[1] = 10;
        //props.readCells[1].state[2] = 10;

        props.readCells[2].state[0] = 100;
        //props.readCells[2].state[1] = 10;
        //props.readCells[2].state[2] = 10;

        props.readCells[3].state[0] = 100;
        //props.readCells[3].state[1] = 10;
        //props.readCells[3].state[2] = 10;

        SimModelAlgorithm algorithm = new SimAlgGillespie(props, movementModel);

        SimModel model = new SimModel(algorithm);

        SimCore core = new SimCore(model, 1);

        simulation = new Simulation(core);

        initChart(chartTL);
        initChart(chartBL);
        initChart(chartTR);
        initChart(chartBR);
    }

    float step = 0.3f;
    double lastTime = 0.0f;
    private void Update()
    {
        //Press N to do 10 steps
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.N))
        {
            for (int q = 0; q < (Input.GetKeyDown(KeyCode.N) ? 10 : 1); q++)
            {
                //With gillespie the very last reaction will be at infinity time it's rough
                if (simulation.model.properties.readCells[0].timeSimulated - lastTime >= 1000.0f)
                {
                    break;
                }

                updateChart(chartTL, simulation.model.properties.readCells[0]);
                updateChart(chartBL, simulation.model.properties.readCells[1]);
                updateChart(chartTR, simulation.model.properties.readCells[2]);
                updateChart(chartBR, simulation.model.properties.readCells[3]);

                lastTime = simulation.model.properties.readCells[0].timeSimulated;
                simulation.core.tickSimulation(step);
                dumpSim();
            }
        }
    }

    private void dumpSim()
    {
        Debug.Log("Sim Dump 0 At " + simulation.model.properties.readCells[0].timeSimulated.ToString() + 
            "\n" + simulation.model.properties.readCells[0].ToString());

        Debug.Log("Sim Dump 1 At " + simulation.model.properties.readCells[1].timeSimulated.ToString() + 
            "\n" + simulation.model.properties.readCells[1].ToString());

        Debug.Log("Sim Dump 2 At " + simulation.model.properties.readCells[2].timeSimulated.ToString() + 
            "\n" + simulation.model.properties.readCells[2].ToString());

        Debug.Log("Sim Dump 3 At " + simulation.model.properties.readCells[3].timeSimulated.ToString() + 
            "\n" + simulation.model.properties.readCells[3].ToString());
    }
}
