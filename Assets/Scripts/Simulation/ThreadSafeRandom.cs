using System;

//https://stackoverflow.com/a/11109361
namespace CJSim {
	public class ThreadSafeRandom {
		private static readonly Random _global = new Random();
		[ThreadStatic] private static Random _local;

		public static double Next() {
			if (_local == null) {
				int seed;
				lock (_global) {
					seed = _global.Next();
				}
				_local = new Random(seed);
			}
			//cjnote this would be a really good spot to fix the returning 0 issue
			return _local.NextDouble();
		}
	}
}
