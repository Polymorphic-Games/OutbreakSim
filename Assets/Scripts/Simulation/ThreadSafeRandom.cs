using System;

//https://stackoverflow.com/a/11109361
namespace CJSim
{
    public class ThreadSafeRandom
    {
        private static readonly Random _global = new Random();
        [ThreadStatic] private static Random _local;

        private static void checkLocal()
        {
            if (_local == null)
            {
                int seed;
                lock (_global)
                {
                    seed = _global.Next();
                }
                _local = new Random(seed);
            }
        }

        private const double doubleMaxInt = (double)int.MaxValue;

        public static double NextUniform0Exclusive1Exclusive()
        {
            checkLocal();
            double rawNum = _local.Next();
            return (rawNum + 1) / (doubleMaxInt + 1);
        }
        public static double NextUniform0Inclusive1Exclusive()
        {
            checkLocal();
            return _local.NextDouble();
        }
        public static double NextUniform0Exclusive1Inclusive()
        {
            checkLocal();
            double rawNum = _local.Next();
            return (rawNum + 1) / (doubleMaxInt);
        }
        public static double NextUniform0Inclusive1Inclusive()
        {
            checkLocal();
            return (_local.Next()) / (doubleMaxInt - 1);
        }
    }
}
