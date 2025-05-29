using System;
using System.Collections.Generic;
using System.Linq;

namespace MachiKoro
{
	public interface IPlayerStrategy
	{
		(IPlayer, CardId) ChooseBusinessCenterTarget(IGame game);

		CardId ChooseBusinessCenterSwap(IGame game);

		CardId ChooseConstruction(IGame game);

		bool ChooseTrainStation(IGame game);

		IPlayer ChooseTvStationTarget(IGame game);

		bool ChooseRadioTower(IGame game, List<int> roll);

		string Name { get; }
	}

	public class Human : IPlayerStrategy
	{
		#region IPlayerStrategy
		public (IPlayer, CardId) ChooseBusinessCenterTarget(IGame game)
		{
			Dictionary<int, IPlayer> indexToPlayer = new Dictionary<int, IPlayer>();
			int choice = 0;
			while (choice == 0)
			{
				int i = 1;
				foreach (var player in game.Players)
				{
					if (ReferenceEquals(player, game.CurrentPlayer))
					{
						continue;
					}

					Console.WriteLine($"{i}. {player}");
					indexToPlayer.Add(i, player);
					i++;
				}

				Console.WriteLine("Choose player to swap with:");
				var response = Console.ReadLine();
				if (int.TryParse(response, out choice) == false)
				{
					Console.WriteLine("   ...unrecognized response.");
					choice = 0;
				}

				if (indexToPlayer.ContainsKey(choice) == false)
				{
					Console.WriteLine("   ...invalid response.");
					choice = 0;
				}
			}

			var playerChosen = indexToPlayer[choice];

			Dictionary<int, CardId> indexToCard = new Dictionary<int, CardId>();
			choice = 0;
			while (choice == 0)
			{
				var playerCards = CardInfo.ValidEstablishments.Where(x => playerChosen.GetEstablishmentCount(x) > 0).ToList();
				int i = 1;
				foreach (var card in playerCards)
				{
					Console.WriteLine($"{i}. {card}");
					indexToCard.Add(i, card);
					i++;
				}

				Console.WriteLine("Choose card to swap with:");
				var response = Console.ReadLine();
				if (int.TryParse(response, out choice) == false)
				{
					Console.WriteLine("   ...unrecognized response.");
					choice = 0;
				}

				if (indexToCard.ContainsKey(choice) == false)
				{
					Console.WriteLine("   ...invalid response.");
					choice = 0;
				}
			}

			return (playerChosen, indexToCard[choice]);
		}

		public CardId ChooseBusinessCenterSwap(IGame game)
		{
			Dictionary<int, CardId> indexToCard = new Dictionary<int, CardId>();
			int choice = 0;
			while (choice == 0)
			{
				var playerCards = CardInfo.ValidEstablishments.Where(x => game.CurrentPlayer.GetEstablishmentCount(x) > 0).ToList();
				int i = 1;
				foreach (var card in playerCards)
				{
					Console.WriteLine($"{i}. {card}");
					indexToCard.Add(i, card);
					i++;
				}

				Console.WriteLine("Choose card to swap with:");
				var response = Console.ReadLine();
				if (int.TryParse(response, out choice) == false)
				{
					Console.WriteLine("   ...unrecognized response.");
					choice = 0;
				}

				if (indexToCard.ContainsKey(choice) == false)
				{
					Console.WriteLine("   ...invalid response.");
					choice = 0;
				}
			}

			return indexToCard[choice];
		}

		public CardId ChooseConstruction(IGame game)
		{
			Console.WriteLine("Construct which? ");
			CardId[] allEstablishments = new CardId[]
			{
				CardId.WheatField, CardId.Ranch, CardId.Bakery, CardId.Cafe, CardId.ConvenienceStore, CardId.Forest,
				CardId.CheeseFactory, CardId.FurnitureFactory, CardId.Mine, CardId.FamilyRestaurant, CardId.AppleOrchard, CardId.ProduceMarket
			};
			var choices = new List<CardId> { CardId.INVALID };
			foreach (var option in allEstablishments)
			{
				if (game.CurrentPlayer.Money < game.Supply.Cost(option))
				{
					continue;
				}

				if (game.Supply.IsAvailable(option) == false)
				{
					continue;
				}

				choices.Add(option);
			}

			CardId[] majorEstablishments = new CardId[]
			{
				CardId.Stadium, CardId.TvStation, CardId.BusinessCenter,
			};
			foreach (var option in majorEstablishments)
			{
				if (game.CurrentPlayer.Money < game.Supply.Cost(option))
				{
					continue;
				}

				if (game.Supply.IsAvailable(option) == false)
				{
					continue;
				}

				if(game.CurrentPlayer.GetEstablishmentCount(option) > 0)
				{
					continue;
				}

				choices.Add(option);
			}

			CardId[] allLandmarks = new CardId[]
			{
				CardId.TrainStation, CardId.ShoppingMall, CardId.AmusementPark, CardId.RadioTower
			};
			foreach (var option in allLandmarks)
			{
				if (game.CurrentPlayer.Money < game.Supply.Cost(option))
				{
					continue;
				}

				if (game.CurrentPlayer.IsLandmarkBuilt(option))
				{
					continue;
				}

				choices.Add(option);
			}

			if (choices.Count() == 1)
			{
				return CardId.INVALID;
			}

			for (int i = 0; i < choices.Count(); ++i)
			{
				Console.WriteLine($" {i,2}) {choices[i]}");
			}
			if (int.TryParse(Console.ReadLine(), out int result) == false)
			{
				return CardId.INVALID;
			}

			if (result < 0 || result >= choices.Count())
			{
				return CardId.INVALID;
			}

			return choices[result];
		}

		public bool ChooseTrainStation(IGame game)
		{
			Console.Write("Use the Train Station (roll two dice)?");
			var response = Console.ReadLine();
			if (response.Contains("y"))
			{
				return true;
			}
			return false;
		}

		public IPlayer ChooseTvStationTarget(IGame game)
		{
			Dictionary<int, IPlayer> indexToPlayer = new Dictionary<int, IPlayer>();
			int choice = 0;
			while (choice == 0)
			{
				int i = 1;
				foreach (var player in game.Players)
				{
					if (ReferenceEquals(player, game.CurrentPlayer))
					{
						continue;
					}

					Console.WriteLine($"{i}. {player} ({player.Money} coins)");
					indexToPlayer.Add(i, player);
					i++;
				}

				Console.WriteLine("Choose player to take coins from:");
				var response = Console.ReadLine();
				if (int.TryParse(response, out choice) == false)
				{
					Console.WriteLine("   ...unrecognized response.");
					choice = 0;
				}

				if (indexToPlayer.ContainsKey(choice) == false)
				{
					Console.WriteLine("   ...invalid response.");
					choice = 0;
				}
			}

			return indexToPlayer[choice];
		}

		public bool ChooseRadioTower(IGame game, List<int> roll)
		{
			Console.Write($"Current roll: ({roll[0]}{(roll.Count > 1 ? $", {roll[1]}" : string.Empty)}). Use the Radio Tower (re-roll)?");
			var response = Console.ReadLine();
			if (response.Contains("y"))
			{
				return true;
			}
			return false;
		}
		public string Name => "Human";
		#endregion
	}

	public class LowerRainbow : IPlayerStrategy
	{
		#region IPlayerStrategy
		public (IPlayer, CardId) ChooseBusinessCenterTarget(IGame game)
		{
			// wouldn't buy this card, should not get called
			return (null, CardId.INVALID);
		}

		public CardId ChooseBusinessCenterSwap(IGame game)
		{
			// wouldn't buy this card, should not get called
			return CardId.INVALID;
		}

		public CardId ChooseConstruction(IGame game)
		{
			if (game.CurrentPlayer.CanConstructLandmark(CardId.ShoppingMall)) { return CardId.ShoppingMall; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.RadioTower)) { return CardId.RadioTower; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.AmusementPark)) { return CardId.AmusementPark; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.TrainStation)) { return CardId.TrainStation; }

			if (IsWantedAndAvailable(game, CardId.ConvenienceStore, 3)) { return CardId.ConvenienceStore; }
			if (IsWantedAndAvailable(game, CardId.Cafe, 3)) { return CardId.Cafe; }
			if (IsWantedAndAvailable(game, CardId.Bakery, 3)) { return CardId.Bakery; }
			if (IsWantedAndAvailable(game, CardId.Ranch, 3)) { return CardId.Ranch; }
			if (IsWantedAndAvailable(game, CardId.Forest, 3)) { return CardId.Forest; }
			if (IsWantedAndAvailable(game, CardId.WheatField, 3)) { return CardId.WheatField; }

			return CardId.INVALID;
		}

		public bool ChooseTrainStation(IGame game)
		{
			// only using lower half, would not use
			return false;
		}

		public IPlayer ChooseTvStationTarget(IGame game)
		{
			// wouldn't buy this card, should not get called
			return null;
		}

		public bool ChooseRadioTower(IGame game, List<int> roll)
		{
			return false;
		}
		public string Name => "LowerRainbow";
		#endregion

		private bool IsWantedAndAvailable(IGame game, CardId cardId, int desiredCount)
		{
			if (game.CurrentPlayer.GetEstablishmentCount(cardId) >= desiredCount)
			{
				return false;
			}

			if (game.Supply.IsAvailable(cardId) == false)
			{
				return false;
			}

			return true;
		}
	}

	public class Simple : IPlayerStrategy
	{
		#region IPlayerStrategy
		public (IPlayer, CardId) ChooseBusinessCenterTarget(IGame game)
		{
			// wouldn't buy this card, should not get called
			return (null, CardId.INVALID);
		}

		public CardId ChooseBusinessCenterSwap(IGame game)
		{
			// wouldn't buy this card, should not get called
			return CardId.INVALID;
		}

		public CardId ChooseConstruction(IGame game)
		{
			if (game.CurrentPlayer.CanConstructLandmark(CardId.ShoppingMall)) { return CardId.ShoppingMall; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.RadioTower)) { return CardId.RadioTower; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.AmusementPark)) { return CardId.AmusementPark; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.TrainStation)) { return CardId.TrainStation; }

			if (IsWantedAndAvailable(game, CardId.Bakery, 3)) { return CardId.Bakery; }

			return CardId.INVALID;
		}

		public bool ChooseTrainStation(IGame game)
		{
			// only using lower half, would not use
			return false;
		}

		public IPlayer ChooseTvStationTarget(IGame game)
		{
			// wouldn't buy this card, should not get called
			return null;
		}

		public bool ChooseRadioTower(IGame game, List<int> roll)
		{
			return false;
		}
		public string Name => "Simple";
		#endregion

		private bool IsWantedAndAvailable(IGame game, CardId cardId, int desiredCount)
		{
			if (game.CurrentPlayer.GetEstablishmentCount(cardId) >= desiredCount)
			{
				return false;
			}

			if (game.Supply.IsAvailable(cardId) == false)
			{
				return false;
			}

			return true;
		}
	}

	public class Punisher : IPlayerStrategy
	{
		#region IPlayerStrategy
		public (IPlayer, CardId) ChooseBusinessCenterTarget(IGame game)
		{
			// wouldn't buy this card, should not get called
			return (null, CardId.INVALID);
		}

		public CardId ChooseBusinessCenterSwap(IGame game)
		{
			// wouldn't buy this card, should not get called
			return CardId.INVALID;
		}

		public CardId ChooseConstruction(IGame game)
		{
			if (game.CurrentPlayer.CanConstructLandmark(CardId.ShoppingMall)) { return CardId.ShoppingMall; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.TrainStation)) { return CardId.TrainStation; }
			if (game.CurrentPlayer.CanConstructLandmark(CardId.RadioTower)) { return CardId.RadioTower; }

			if (IsWantedAndAvailable(game, CardId.Cafe, 4)) { return CardId.Cafe; }
			if (IsWantedAndAvailable(game, CardId.Stadium, 1)) { return CardId.Stadium; }
			if (IsWantedAndAvailable(game, CardId.TvStation, 1)) { return CardId.TvStation; }
			if (IsWantedAndAvailable(game, CardId.Bakery, 4)) { return CardId.Bakery; }
			if (IsWantedAndAvailable(game, CardId.Ranch, 2)) { return CardId.Ranch; }
			if (IsWantedAndAvailable(game, CardId.WheatField, 2)) { return CardId.WheatField; }

			if (game.CurrentPlayer.CanConstructLandmark(CardId.AmusementPark)) { return CardId.AmusementPark; }

			return CardId.INVALID;
		}

		public bool ChooseTrainStation(IGame game)
		{
			return true;
		}

		public IPlayer ChooseTvStationTarget(IGame game)
		{
			var list = new List<IPlayer>(game.Players.Where(x => ReferenceEquals(x, game.CurrentPlayer) == false));
			var maxWealth = list.Max(x => x.Money);
			return list.First(x => x.Money == maxWealth);
		}

		public bool ChooseRadioTower(IGame game, List<int> roll)
		{
			List<int> preferred = new List<int> { 1, 11, 12 };
			if (preferred.Contains(roll.Sum()))
			{
				return false;
			}
			return true;
		}

		public string Name => "Punisher";
		#endregion

		private bool IsWantedAndAvailable(IGame game, CardId cardId, int desiredCount)
		{
			if (game.CurrentPlayer.GetEstablishmentCount(cardId) >= desiredCount)
			{
				return false;
			}

			if (game.Supply.IsAvailable(cardId) == false)
			{
				return false;
			}

			return true;
		}
	}

}