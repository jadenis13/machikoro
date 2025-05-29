using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachiKoro
{
	public interface IPlayer
	{
		int GetEstablishmentCount(CardId cardId);

		bool IsLandmarkBuilt(CardId cardId);

		int Money { get; }

		bool CanConstructEstablishment(CardId cardId, ISupply supply);

		bool CanConstructLandmark(CardId cardId);
	}

	internal class Player : IPlayer
	{
		#region IPlayer
		public int GetEstablishmentCount(CardId cardId) { return m_Establishments.ContainsKey(cardId) ? m_Establishments[cardId] : 0; }

		public bool IsLandmarkBuilt(CardId cardId) { return m_Landmarks.ContainsKey(cardId) && m_Landmarks[cardId]; }

		public int Money { get; private set; }

		public bool CanConstructEstablishment(CardId cardId, ISupply supply)
		{
			if (CardInfo.ValidEstablishments.Contains(cardId) == false)
			{
				return false;
			}

			if (Money < CardInfoServer.Lookup[cardId].Cost)
			{
				return false;
			}

			if (supply.IsAvailable(cardId) == false)
			{
				return false;
			}

			return true;
		}

		public bool CanConstructLandmark(CardId cardId)
		{
			if (CardInfo.ValidLandmarks.Contains(cardId) == false)
			{
				return false;
			}

			if (Money < CardInfoServer.Lookup[cardId].Cost)
			{
				return false;
			}

			if (m_Landmarks[cardId])
			{
				return false;
			}

			return true;
		}
		
		public string Name { get; }
		#endregion

		public void AdjustMoney(int income)
		{
			Money += income;
		}

		public bool AreAllLandmarksBuilt()
		{
			return m_Landmarks.Values.All(x => x);
		}

		public (Player, CardId) ChooseBusinessCenterTarget(Game game)
		{
			var (player, card) = m_Strategy.ChooseBusinessCenterTarget(game);
			return (game.m_Players.Single(x => ReferenceEquals(x, player)), card);
		}

		public CardId ChooseBusinessCenterSwap(Game game)
		{
			return m_Strategy.ChooseBusinessCenterSwap(game);
		}

		public CardId ChooseConstruction(Game game)
		{
			var cardId = m_Strategy.ChooseConstruction(game);
			if (CanConstructEstablishment(cardId, game.Supply))
			{
				return cardId;
			}

			if (CanConstructLandmark(cardId))
			{
				return cardId;
			}
			return CardId.INVALID;
		}

		public bool ChooseRadioTower(Game game, List<int> roll) => IsLandmarkBuilt(CardId.RadioTower) && m_Strategy.ChooseRadioTower(game, roll);

		public bool ChooseTrainStation(Game game) => IsLandmarkBuilt(CardId.TrainStation) && m_Strategy.ChooseTrainStation(game);

		public Player ChooseTvStationTarget(Game game)
		{
			var player = m_Strategy.ChooseTvStationTarget(game);
			return game.m_Players.Single(x => ReferenceEquals(x, player));
		}

		public void Construct(CardId cardId)
		{
			if(CardInfo.ValidEstablishments.Contains(cardId))
			{
				if(m_Establishments.ContainsKey(cardId) == false)
				{
					m_Establishments.Add(cardId, 0);
				}
				m_Establishments[cardId]++;
			}

			if(CardInfo.ValidLandmarks.Contains(cardId))
			{
				m_Landmarks[cardId] = true;
			}
		}

		public void Deconstruct(CardId cardId)
		{
			if(GetEstablishmentCount(cardId) > 0)
			{
				m_Establishments[cardId]--;
				if (m_Establishments[cardId] == 0)
				{
					m_Establishments.Remove(cardId);
				}
			}
		}

		public override string ToString()
		{
			var sb = new StringBuilder();
			sb.AppendLine($"Player {Name}: {Money} coins");
			sb.AppendLine($"   {string.Join(" ", m_Establishments.Select(x => $"{x.Value}x {x.Key}(${CardInfoServer.Lookup[x.Key].Cost})"))}");
			sb.AppendLine($"   {string.Join(" ", m_Landmarks.Select(x => $"{(x.Value?"+":"-")}{x.Key}(${CardInfoServer.Lookup[x.Key].Cost})"))}");
			return sb.ToString();
		}

		internal Player(string name, IPlayerStrategy strategy)
		{
			Name = name;
			m_Strategy = strategy;

			m_Establishments = new Dictionary<CardId, int>(2)
				{
					{ CardId.WheatField, 1 },
					{ CardId.Bakery, 1 }
				};

			m_Landmarks = new Dictionary<CardId, bool>(4)
				{
					{ CardId.TrainStation, false },
					{ CardId.ShoppingMall, false },
					{ CardId.AmusementPark, false },
					{ CardId.RadioTower, false }
				};

			Money = 3;
		}

		private readonly Dictionary<CardId, int> m_Establishments;
		private readonly Dictionary<CardId, bool> m_Landmarks;

		private readonly IPlayerStrategy m_Strategy;
	}
}