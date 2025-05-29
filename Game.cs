using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MachiKoro
{
	public interface IGame
	{
		IPlayer CurrentPlayer { get; }

		IEnumerable<IPlayer> Players { get; }

		ISupply Supply { get; }

		int RoundNumber { get; }
	}

	internal class Game : IGame
	{
		public IPlayer CurrentPlayer => m_CurrentPlayer;

		public IEnumerable<IPlayer> Players => m_Players.AsReadOnly().Cast<IPlayer>();

		public ISupply Supply => m_Supply;

		public int RoundNumber { get; }

		public struct Statistics
		{
			public int winningPlayerIndex;
			public int totalRounds;
		}

		internal Statistics ExecuteGame()
		{
			int currentPlayerIndex = 0;
			int roundCount = 1;
			Statistics stats = new Statistics();

			TextWriter log = Console.Out; //TextWriter.Null; 

			bool isSecondTurn = false;
			bool gameOver = false;
			while (gameOver == false)
			{
				m_CurrentPlayer = m_Players[currentPlayerIndex];

				log.WriteLine("*** Round #{0,3}-{1} ***", roundCount, currentPlayerIndex + 1);
				log.WriteLine(m_Supply.ToString());
				log.WriteLine();
				log.Write(m_CurrentPlayer.ToString());

				int numberOfD6 = 1;
				numberOfD6 += m_CurrentPlayer.ChooseTrainStation(this) ? 1 : 0;
				if (numberOfD6 == 2)
				{
					log.Write($"{m_CurrentPlayer.Name} uses the Train Station (rolls two dice)");
				}

				var roll = Utility.GetRollsD6(numberOfD6);
				log.WriteLine("   ...rolls {0} ({1})", roll.Sum(), string.Join(",", roll));

				if (m_CurrentPlayer.ChooseRadioTower(this, roll))
				{
					numberOfD6 = 1;
					numberOfD6 += m_CurrentPlayer.ChooseTrainStation(this) ? 1 : 0;
					if (numberOfD6 == 2)
					{
						log.Write($"{m_CurrentPlayer.Name} uses the Train Station (rolls two dice)");
					}

					log.Write($"{m_CurrentPlayer.Name} uses the Radio Tower (re-rolls)");
					roll = Utility.GetRollsD6(numberOfD6);
					log.WriteLine("   ...rolls {0} ({1})", roll.Sum(), string.Join(",", roll));
				}

				List<Player> otherPlayers = new List<Player>();
				int cursor = (currentPlayerIndex + 1) % m_Players.Count();
				while (cursor != currentPlayerIndex)
				{
					otherPlayers.Add(m_Players[cursor]);
					cursor = (cursor + 1) % m_Players.Count();
				}

				// punitive payments (processed in reverse player order)
				otherPlayers.Reverse();
				switch (roll.Sum())
				{
					case 3:
						foreach (var otherPlayer in otherPlayers)
						{
							var fees = otherPlayer.GetEstablishmentCount(CardId.Cafe) * (1 + (otherPlayer.IsLandmarkBuilt(CardId.ShoppingMall) ? 1 : 0));
							if (fees > 0)
							{
								var paid = Math.Min(m_CurrentPlayer.Money, fees);
								log.WriteLine("   ...pays out {0}$ in Cafe fees to {1}", paid, otherPlayer.Name);
								m_CurrentPlayer.AdjustMoney(-paid);
								otherPlayer.AdjustMoney(paid);
							}
						}
						break;

					case 9:
					case 10:
						foreach (var otherPlayer in otherPlayers)
						{
							var fees = otherPlayer.GetEstablishmentCount(CardId.FamilyRestaurant) * (2 + (otherPlayer.IsLandmarkBuilt(CardId.ShoppingMall) ? 1 : 0));
							if (fees > 0)
							{
								var paid = Math.Min(m_CurrentPlayer.Money, fees);
								log.WriteLine("   ...pays out {0}$ in Family Restaurant fees to {1}", paid, otherPlayer.Name);
								m_CurrentPlayer.AdjustMoney(-paid);
								otherPlayer.AdjustMoney(paid);
							}
						}
						break;
				}

				// own roll
				var bankIncome = 0;
				switch (roll.Sum())
				{
					case 1:
						bankIncome += m_CurrentPlayer.GetEstablishmentCount(CardId.WheatField);
						break;
					case 2:
						bankIncome += m_CurrentPlayer.GetEstablishmentCount(CardId.Ranch);
						bankIncome += (1 + (m_CurrentPlayer.IsLandmarkBuilt(CardId.ShoppingMall) ? 1 : 0)) *
							m_CurrentPlayer.GetEstablishmentCount(CardId.Bakery);
						break;
					case 3:
						bankIncome += (1 + (m_CurrentPlayer.IsLandmarkBuilt(CardId.ShoppingMall) ? 1 : 0)) *
							m_CurrentPlayer.GetEstablishmentCount(CardId.Bakery);
						break;
					case 4:
						bankIncome += (3 + (m_CurrentPlayer.IsLandmarkBuilt(CardId.ShoppingMall) ? 1 : 0)) *
							m_CurrentPlayer.GetEstablishmentCount(CardId.ConvenienceStore);
						break;
					case 5:
						bankIncome += m_CurrentPlayer.GetEstablishmentCount(CardId.Forest);
						break;
					case 7:
						bankIncome += 3 * m_CurrentPlayer.GetEstablishmentCount(CardId.Ranch) *
							m_CurrentPlayer.GetEstablishmentCount(CardId.CheeseFactory);
						break;
					case 8:
						bankIncome += 3 * (m_CurrentPlayer.GetEstablishmentCount(CardId.Forest) + m_CurrentPlayer.GetEstablishmentCount(CardId.Mine)) *
							m_CurrentPlayer.GetEstablishmentCount(CardId.FurnitureFactory);
						break;
					case 9:
						bankIncome += 5 * m_CurrentPlayer.GetEstablishmentCount(CardId.Mine);
						break;
					case 10:
						bankIncome += 3 * m_CurrentPlayer.GetEstablishmentCount(CardId.AppleOrchard);
						break;
					case 11:
					case 12:
						bankIncome += 2 * m_CurrentPlayer.GetEstablishmentCount(CardId.ProduceMarket);
						break;
				}

				log.WriteLine("   ...earns {0}$ from the bank", bankIncome);
				m_CurrentPlayer.AdjustMoney(bankIncome);

				// anyone's roll
				otherPlayers.Reverse();
				foreach (var otherPlayer in otherPlayers)
				{
					bankIncome = 0;
					switch (roll.Sum())
					{
						case 1:
							bankIncome += otherPlayer.GetEstablishmentCount(CardId.WheatField);
							break;
						case 2:
							bankIncome += otherPlayer.GetEstablishmentCount(CardId.Ranch);
							break;
						case 5:
							bankIncome += otherPlayer.GetEstablishmentCount(CardId.Forest);
							break;
						case 9:
							bankIncome += 5 * otherPlayer.GetEstablishmentCount(CardId.Mine);
							break;
						case 10:
							bankIncome += 3 * otherPlayer.GetEstablishmentCount(CardId.AppleOrchard);
							break;
					}

					log.WriteLine("      ...player {0} earns {1}$ from the bank", otherPlayer.Name, bankIncome);
					otherPlayer.AdjustMoney(bankIncome);
				}

				otherPlayers.Reverse();
				if (roll.Sum() == 6 && m_CurrentPlayer.GetEstablishmentCount(CardId.Stadium) > 0)
				{
					// take from all other players
					foreach (var otherPlayer in otherPlayers)
					{
						var paid = Math.Min(otherPlayer.Money, 2);
						log.WriteLine("   ... player {0} pays out {1}$ in Stadium fees to {2}", otherPlayer.Name, paid, m_CurrentPlayer.Name);
						m_CurrentPlayer.AdjustMoney(paid);
						otherPlayer.AdjustMoney(-paid);
					}
				}

				if(roll.Sum() == 6 && m_CurrentPlayer.GetEstablishmentCount(CardId.TvStation) > 0)
				{
					// choose target player
					var player = m_CurrentPlayer.ChooseTvStationTarget(this);
					var paid = Math.Min(player.Money, 5);
					log.WriteLine("   ...takes {0}$ from {1}", paid, player.Name);
					m_CurrentPlayer.AdjustMoney(paid);
					player.AdjustMoney(-paid);
				}

				if(roll.Sum() == 6 && m_CurrentPlayer.GetEstablishmentCount(CardId.BusinessCenter) > 0)
				{
					var (player, targetCard) = m_CurrentPlayer.ChooseBusinessCenterTarget(this);
					var swapCard = m_CurrentPlayer.ChooseBusinessCenterSwap(this);

					log.WriteLine("   ...swapping a {0} for player {1}'s {2}", swapCard, player.Name, targetCard);
					m_CurrentPlayer.Deconstruct(swapCard);
					player.Construct(swapCard);
					player.Deconstruct(targetCard);
					m_CurrentPlayer.Construct(targetCard);
				}

				CardId cardChosen = m_CurrentPlayer.ChooseConstruction(this);
				if (cardChosen != CardId.INVALID)
				{
					log.WriteLine("   ...building a{0} {1}", (cardChosen.ToString().StartsWith("A") ? "n" : string.Empty), cardChosen.ToString());
					if (CardInfo.ValidEstablishments.Contains(cardChosen))
					{
						m_CurrentPlayer.AdjustMoney(-CardInfoServer.Lookup[cardChosen].Cost);
						m_CurrentPlayer.Construct(cardChosen);
						m_Supply.RemoveFromMarket(cardChosen);
					}

					if (CardInfo.ValidLandmarks.Contains(cardChosen))
					{
						m_CurrentPlayer.AdjustMoney(-CardInfoServer.Lookup[cardChosen].Cost);
						m_CurrentPlayer.Construct(cardChosen);
					}
				}
				else
				{
					log.WriteLine("   ...does not build.");
				}
				log.WriteLine();

				gameOver = m_CurrentPlayer.AreAllLandmarksBuilt();
				if (!gameOver && !isSecondTurn && m_CurrentPlayer.IsLandmarkBuilt(CardId.AmusementPark) && roll.Count == 2 && roll[0] == roll[1])
				{
					// current player takes another turn
					isSecondTurn = true;
					continue;
				}
				else if(gameOver)
				{
					stats = new Statistics { winningPlayerIndex = currentPlayerIndex, totalRounds = roundCount };

				}

				isSecondTurn = false;
				currentPlayerIndex = (currentPlayerIndex + 1) % m_Players.Count;
				if (currentPlayerIndex == 0)
				{
					++roundCount;
				}
			}

			return stats;
		}

		internal Game(Options options)
		{
			IPlayerStrategy[] strategies = new IPlayerStrategy[]
			{
				new Simple(),
				new Punisher(),
				new LowerRainbow(),
				new Human()
			};

			foreach (var index in Enumerable.Range(0, options.NumPlayers))
			{
				m_Players.Add(new Player(strategies.ElementAt(index).Name, strategies.ElementAt(index)));
			}

			m_Supply = new Supply(options.Variant);
			m_Supply.OnBegin(options.NumPlayers);
		}

		internal readonly List<Player> m_Players = new List<Player>();
		internal readonly Supply m_Supply;

		internal Player m_CurrentPlayer;
	}

}