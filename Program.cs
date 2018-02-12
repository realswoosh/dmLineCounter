using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace dmLineCounter
{
	class Program
	{
		static string Path { get; set; }
		static string SearchPattern { get; set; } = "*";
		static int Count = 0;

		static void Main(string[] args)
		{
			dmCommandLineParser.Parser lineParser = new dmCommandLineParser.Parser();

			lineParser.Add(new dmCommandLineParser.Option("-path", new dmCommandLineParser.Operator.FuncOperator(SetPath)));
			lineParser.Add(new dmCommandLineParser.Option("-pattern", new dmCommandLineParser.Operator.FuncOperator((string arg) => { if (arg != "") SearchPattern = arg; })));

			if (args.Length > 0)
			{
				lineParser.Process(args);
			}

			if (string.IsNullOrEmpty(Path))
				Path = Environment.CurrentDirectory;

			Stopwatch stopWatch = new Stopwatch();
			stopWatch.Start();

			var directoryList = Searcher.GetDirectories(Path, searchOption : SearchOption.AllDirectories);

			SemaphoreSlim maxThread = new SemaphoreSlim(10);

			foreach (var tmpPath in directoryList)
			{
				maxThread.Wait();

				Task.Factory.StartNew(() =>
				{
					Console.Write(".");

					var files = Directory.GetFiles(tmpPath, SearchPattern);
					Interlocked.Add(ref Count, files.Count());
				}, TaskCreationOptions.LongRunning)
				.ContinueWith((task) => maxThread.Release());
			}

			stopWatch.Stop();

			Console.WriteLine("");
			Console.WriteLine($"Root : {Path}");
			Console.WriteLine($"SearchPattern : {SearchPattern}");
			Console.WriteLine($"Search Duration : {stopWatch.ElapsedMilliseconds}, FileCount={Count}");
		}


		static void SetPath(string arg)
		{
			Path = arg;
		}
	}
}
