using Drawing;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Profiling;


namespace OutbreakSim
{


    public class SimulationManager : MonoBehaviour
    {

        //public ModelProperties properties;

        public TransferRelationship[] relationships;

        public int rows;
        public int columns;
        public int stateCount;
        public float nodeSize;
        public int updatesPerSecond;
        public int sickNodes;
        public bool drawGizmos;

        //runtime data
        private NativeArray<double> readCells;
        private NativeArray<double> writeCells;

        //this stores the info of new people in that specific state
        //for each cell, needs to have one int for each cell
        private NativeArray<double> stateMovementInfo;
        private NativeArray<TransferRelationship> transferRelationships;

        private void Awake()
        {
            //Application.targetFrameRate = updatesPerSecond;

            readCells = new NativeArray<double>((rows * columns) * stateCount, Allocator.Persistent);
            writeCells = new NativeArray<double>(readCells.Length, Allocator.Persistent);

            for (int i = 0; i < readCells.Length; i += stateCount)
            {
                readCells[i] = 100;
                writeCells[i] = 100;

            }

            //to initialize some random cells with infected
            int index = 0;
            for (int i = 0; i < sickNodes; i++)
            {
                index = Random.Range(0, rows * columns);
                readCells[(index * stateCount) + 1] += 20;
                writeCells[(index * stateCount) + 1] += 20;


            }


            stateMovementInfo = new NativeArray<double>(readCells.Length, Allocator.Persistent);
            transferRelationships = new NativeArray<TransferRelationship>(relationships, Allocator.Persistent);


        }


        // Start is called before the first frame update
        void Start()
        {



        }

        // Update is called once per frame
        void Update()
        {

            Profiler.BeginSample("Update Simulation");

            //update the system every so often
            SingleAlgorithmGridSimulationUpdate update = new SingleAlgorithmGridSimulationUpdate(0, stateCount, (rows * columns) / 20, rows, columns,
                Time.time + 1, readCells, writeCells, stateMovementInfo, transferRelationships);
            //update.Run(10);
            update.Schedule(10, 1).Complete();

            update = new SingleAlgorithmGridSimulationUpdate(1, stateCount, (rows * columns) / 20, rows, columns,
                Time.time + 1, readCells, writeCells, stateMovementInfo, transferRelationships);
            //update.Run(10);
            update.Schedule(10, 1).Complete();


            Profiler.EndSample();

            Profiler.BeginSample("Transfers");

            CellMovementTransfers transfers = new CellMovementTransfers();
            transfers.writeCells = writeCells;
            transfers.stateMovementInfo = stateMovementInfo;
            transfers.Schedule(writeCells.Length, 128).Complete();

            Profiler.EndSample();

            //for (int i = 0; i < writeCells.Length; i++)
            //{
            //    readCells[i] = writeCells[i];
            //}

            //Profiler.BeginSample("Copy Memory");
            //writeCells.CopyTo(readCells);
            //Profiler.EndSample();
            Profiler.BeginSample("MemCpy");
            unsafe
            {
                UnsafeUtility.MemCpy(writeCells.GetUnsafePtr(), readCells.GetUnsafePtr(), readCells.Length * 8);
            }


            Profiler.EndSample();

            //now draw the grid after the update


            if (drawGizmos)
            {
                Profiler.BeginSample("Draw Grid");

                DrawGridWithStatesJob draw = new DrawGridWithStatesJob();
                draw.rows = rows;
                draw.columns = columns;
                draw.stateCount = stateCount;
                draw.nodeSize = nodeSize;
                draw.writeCells = writeCells;
                draw.builder = DrawingManager.GetBuilder(true);

                draw.Schedule(rows * columns, 32).Complete();
                draw.builder.Dispose();

                Profiler.EndSample();
            }



        }




        private void OnDestroy()
        {
            
            readCells.Dispose();
            writeCells.Dispose();
            stateMovementInfo.Dispose();

            transferRelationships.Dispose();

        }




    }

}