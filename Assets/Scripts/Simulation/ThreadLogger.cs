using System.IO;
using System.Runtime.CompilerServices;

public class ThreadLogger {
	private static readonly object locker = new object();

	public static void Log(string msg, [CallerFilePath] string callingFilePath = "",
        [CallerLineNumber] int callingFileLineNumber = 0) {
		lock (locker) {
			StreamWriter streamWriter = File.AppendText("./threadLog.txt");
			streamWriter.WriteLine(callingFilePath + ": " + callingFileLineNumber + " :" + msg);
			streamWriter.Close();
		}
	}
}
