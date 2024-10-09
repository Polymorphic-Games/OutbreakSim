using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace OutbreakSim
{

    [System.Serializable]
    public struct TransferRelationship
    {
        public byte fromState;
        public byte toState;
        public double rate;

    }

}
