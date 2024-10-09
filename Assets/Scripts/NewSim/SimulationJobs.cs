using Drawing;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;


namespace OutbreakSim
{

    public interface SimulationAlgorithm
    {

        public void ExecuteAlgorithm();


    }


    public struct SimulationAlgorithmGillespie : SimulationAlgorithm
    {

        public void ExecuteAlgorithm()
        {


        }


    }

    public struct SingleAlgorithmGridSimulationUpdate : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> readCells;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> writeCells;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> stateMovementInfo;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<TransferRelationship> transfers;
        public int executeSize;
        public int stateCount;
        public int rows;
        public int columns;
        public int offset;

        public float elapsedTime;

        public SingleAlgorithmGridSimulationUpdate(int offset, int stateCount, int executeSize, int rows, int columns,
            float elapsedTime,  NativeArray<double> readCells, NativeArray<double> writeCells, 
            NativeArray<double> stateMovementInfo, NativeArray<TransferRelationship> transferRates)
        {


            this.readCells = readCells;
            this.writeCells = writeCells;

            this.stateMovementInfo = stateMovementInfo;

            this.transfers = transferRates;
            this.executeSize = executeSize;
            this.stateCount = stateCount;
            this.rows = rows;
            this.columns = columns;
            this.elapsedTime = elapsedTime;
            this.offset = offset;


        }


        public void Execute(int index)
        {

            //this is for a specific cell
            //so just do the gillespie algorithm for now.

            /* 
             * for this specific block
             * do all the reactions for the specific cells, since it seems like that's where the current simulation is
             *      no movement is there.
             * 
             * 
             * two stage update here in order to avoid thread clashes especially when dealing with movement
             * 
             * 
             * simple SIR model
             * 
             * 
             */
            Random r = Random.CreateFromIndex((uint)((index + 1) * 474567 * elapsedTime));
            int nodeStart = ((index * 2) + offset) * executeSize;
            double transfer = 0;
            double leaving = 0;
            double scalar = 0.0;
            TransferRelationship t = default;

            for (int i = nodeStart; i < nodeStart + executeSize; i++)
            {

                //for each cell/node get the sum of the transfers between each state that has a transfer rate/relationship.
                //first 

                //if node is empty, return
    
                if (GetCellPopulation(i) < 0.001)
                    continue;
                
                for (int j = 0; j < transfers.Length; j++)
                {
                    t = transfers[j];

                    //need to make sure that the value doesn't go below zero
                    //spread behavior and density behavior
                    //transfer = SpreadBehaviorBasic(i, transfers[j]);
                    transfer += SpreadBehaviorDensityBased(i, transfers[j]);

                    //spread gets priority over leaving

                    //then the ones that will move out of the cell
                    leaving = SpreadBehaviorMovementBased(i, transfers[j]);

                    //then randomly scale both of those values
                    scalar = r.NextDouble(0, 1);
                    transfer *= scalar;
                    leaving *= scalar;

                    //making sure that transferring from one state to another in the same cell won't make it go below 0
                    if (writeCells[(i * stateCount) + transfers[j].fromState] < transfer)
                    {
                        writeCells[(i * stateCount) + transfers[j].toState]
                            += writeCells[(i * stateCount) + transfers[j].fromState];
                        writeCells[(i * stateCount) + transfers[j].fromState] = 0;
                        continue;
                    }
                    else
                    {
                        writeCells[(i * stateCount) + transfers[j].toState] += transfer;
                        writeCells[(i * stateCount) + transfers[j].fromState] -= transfer;
                    }

                    //making sure that people leaving from this cell to another won't make it go belwo 0
                    if (writeCells[(i * stateCount) + transfers[j].fromState] < leaving)
                    {
                        leaving = writeCells[(i * stateCount) + transfers[j].fromState];
                        writeCells[(i * stateCount) + transfers[j].fromState] = 0;

                    }
                    else
                    {
                        writeCells[(i * stateCount) + transfers[j].fromState] -= leaving;

                    }

                    if (leaving > 0.001)
                        MovementSpreadToNeighbors(i, transfers[j], leaving);



                    //then for stage two, will add the ones that moved into the cell


                }


            }
            



        }

        private double GetCellPopulation(int cell)
        {
            double d = 0;

            for (int i = 0; i < stateCount - 1; i++)
            {
                d += readCells[(cell * stateCount) + i];

            }


            return d;
        }

        private double SpreadBehaviorBasic(int cell, TransferRelationship transfer)
        {
            //amount of people in state multiplied by the transfer rate
            return readCells[(cell * stateCount) + transfer.fromState] * transfer.rate;
        }


        private double SpreadBehaviorDensityBased(int cell, TransferRelationship transfer)
        {

            double density = (readCells[(cell * stateCount) + transfer.fromState] 
                * readCells[(cell * stateCount) + transfer.toState]);
            density /= GetCellPopulation(cell);

            return transfer.rate * density;
        }

        private double SpreadBehaviorMovementBased(int cell, TransferRelationship transfer)
        {

            return readCells[(cell * stateCount) + transfer.fromState] * 0.01;
        }

        private void MovementSpreadToNeighbors(int cell, TransferRelationship transfer, double transferAmount)
        {
            //for this one, just do four neighbors to make it more simple
            //so cell -1, cell + 1, + columns, - columns

            int neighborCell = cell - 1;
            double div = 0;
            if (neighborCell % columns > 0)
                div++;
            if (neighborCell % columns < columns - 1)
                div++;
            if (neighborCell < (rows * columns))
                div++;
            if (neighborCell > 0)
                div++;

            transferAmount /= div;


            //if not on the left edge
            if ((neighborCell % columns) > 0)
                stateMovementInfo[(neighborCell * stateCount) + transfer.fromState] += transferAmount;

            //if not on the right edge
            neighborCell = cell + 1;
            if ((neighborCell % columns) < (columns - 1) && neighborCell < (rows * columns))
                stateMovementInfo[(neighborCell * stateCount) + transfer.fromState] += transferAmount;

            //if not on the top edge
            neighborCell = cell + columns;
            if (neighborCell < (rows * columns))
                stateMovementInfo[(neighborCell * stateCount) + transfer.fromState] += transferAmount;

            //if not on the bottom edge
            neighborCell = cell - columns;
            if (neighborCell > 0)
                stateMovementInfo[(neighborCell * stateCount) + transfer.fromState] += transferAmount;




        }



        /*
         * different state functions 
         * 
         * parameters seem to be the percentage of population that move from one state to another.
         * 
         * FUNCTION 1
         * just param * state
         * return state.state[argv[1]] * properties.parameters[argv[2]];
         * 
         * 
         * FUNCTION 2
         * depends on the density of infected
         * (param * state1 * state2) / NumberOfPeopleInState
         * return properties.parameters[argv[3]] * (((double)state.state[argv[2]] * state.state[argv[1]]) / state.numberOfPeople);
         * 
         * FUNCTION 3
         * Movement parameter, how many of my people bump into my neighbors
         * (param(beta) * state1(sus)) * (neighborStuff * state2(infected))
         * 
         * 
         * 
         * 
         */




    }

    public struct CellMovementTransfers : IJobParallelFor
    {
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> writeCells;
        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> stateMovementInfo;


        public void Execute(int index)
        {

            writeCells[index] += stateMovementInfo[index];
            stateMovementInfo[index] = 0;

        }



    }


    public struct DrawGridWithStatesJob : IJobParallelFor
    {

        public int rows;
        public int columns;
        public int stateCount;
        public float nodeSize;

        [NativeDisableContainerSafetyRestriction]
        public NativeArray<double> writeCells;
        public CommandBuilder builder;



        public void Execute(int index)
        {
            int row = index / columns;
            int col = index % columns;
            int i = index * stateCount;

            float3 pos = new float3(row * nodeSize, 0, col * nodeSize);

            float3 c = new float3((float)writeCells[i], (float)writeCells[i + 1], (float)writeCells[i + 2]);
            float total = c.x + c.y + c.z;
            c /= total;

            builder.SolidBox(pos, quaternion.identity, 
                new float3(nodeSize * .95f), new Color(c.x, c.y, c.z, 1));
            //builder.Label2D(pos, index.ToString(), 16, Color.white);

        }

    }



}


