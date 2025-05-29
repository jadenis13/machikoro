using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MachiKoro
{
	static internal class Utility
	{
		internal static List<int> GetRollsD6(int numberOfD6)
		{
			var dieResults = new List<int>(numberOfD6);
			dieResults.AddRange(Enumerable.Range(0, numberOfD6).Select(x => m_rng.Next(1, 6)));
			return dieResults;
		}

		internal static void Shuffle<T>(this IList<T> list)
		{
			int n = list.Count;
			while (n > 1)
			{
				--n;
				int k = m_rng.Next(n + 1);
				(list[n], list[k]) = (list[k], list[n]);
			}
		}

		private static readonly Random m_rng = new Random(DateTime.Now.Millisecond);
	}

	public class Options
	{
		[Option('g', "numGames", Required = false, Default = 1, HelpText = "Number of games to execute")]
		public int NumGames { get; set; }
		
		[Option('p', "num_players", Required = false, Default = 4, HelpText = "Number of players")]
		public int NumPlayers { get; set; }

		[Option('v', "variant", Required = false, Default = Variant.Default, HelpText = "Supply variant to use")]
		public Variant Variant { get; set; }
	}

	internal class EntryPoint
	{
		static void Main(string[] args)
		{
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed(RunOptions)
				.WithNotParsed(HandleParserError);
		}

		static void RunOptions(Options options)
		{
			Console.WriteLine($"Using {options.NumPlayers} players to play {options.NumGames} games");
			Console.WriteLine($"Using Variant: {options.Variant}");
			
			var stats = new List<Game.Statistics>();
			for (int i = 0; i < options.NumGames; ++i)
			{
				Game game = new Game(options);
				stats.Add(game.ExecuteGame());
			}

			var wins = new int[options.NumPlayers];
			for(int i = 0; i < options.NumPlayers; ++i)
			{
				wins[i] = stats.Where(x => i == x.winningPlayerIndex).Count();
			}

			Console.WriteLine($"*** Win counts: {string.Join(" vs. ", wins)}");
			Console.WriteLine($"*** Turns: avg/min/max: {stats.Average(x => x.totalRounds)}/{stats.Min(x => x.totalRounds)}/{stats.Max(x => x.totalRounds)}");
		}

		static void HandleParserError(IEnumerable<Error> errors)
		{
			errors.ToList().ForEach(error => Console.Error.WriteLine(error.ToString()));
		}
	}
}