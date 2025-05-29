using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MachiKoro
{
	public enum Variant { Default, Unique10, Unique8 };

	public interface ISupply
	{
		ReadOnlyDictionary<CardId, int> Cards { get; }

		int Cost(CardId cardId);

		bool IsAvailable(CardId cardId);
	}

	internal class Supply : ISupply
	{
		#region ISupply
		public ReadOnlyDictionary<CardId, int> Cards => new ReadOnlyDictionary<CardId, int>(m_Supply);

		public int Cost(CardId cardId) => CardInfoServer.Lookup[cardId].Cost;

		public bool IsAvailable(CardId cardId) => m_Supply.ContainsKey(cardId) && m_Supply[cardId] > 0;
		#endregion

		public override string ToString() => string.Join(",", m_Supply.Where(z => z.Value > 0).OrderBy(y => Cost(y.Key)).Select(x => $"{x.Value}x {x.Key}(${Cost(x.Key)})"));

		internal bool RemoveFromMarket(CardId cardId)
		{
			if (IsAvailable(cardId))
			{
				--m_Supply[cardId];
				if (m_Variant == Variant.Default)
				{
					return true;
				}

				if (m_Supply[cardId] == 0)
				{
					m_Supply.Remove(cardId);
					while (RestockMarket()) { }
				}
			}
			return false;
		}

		internal void OnBegin(int numberOfPlayers)
		{
			var deckList = new List<CardId>();
			CardInfo.ValidEstablishments.ToList().ForEach(id =>
			{
				var initialCount = CardInfoServer.Lookup[id].InitialCount;
				initialCount = initialCount >= 0 ? initialCount : numberOfPlayers;
				foreach (var card in Enumerable.Range(0, initialCount))
				{
					deckList.Add(id);
				}
			});

			deckList.Shuffle();
			m_Deck.Clear();
			deckList.ForEach(c => m_Deck.Enqueue(c));

			var uniquePileCount = 0;
			switch (m_Variant)
			{
				case Variant.Default:
					uniquePileCount = CardInfo.ValidEstablishments.Count();
					break;

				case Variant.Unique10:
					uniquePileCount = 10;
					break;

				case Variant.Unique8:
					uniquePileCount = 8;
					break;
			}

			while (uniquePileCount > 0)
			{
				if (RestockMarket() == false)
				{
					--uniquePileCount;
				}
			}

			if (m_Variant == Variant.Default)
			{
				while (m_Deck.Any())
				{
					++m_Supply[m_Deck.Dequeue()];
				}
			}
		}

		internal Supply(Variant variant)
		{
			m_Variant = variant;
		}

		private bool RestockMarket()
		{
			CardId card = m_Deck.Dequeue();
			if (m_Supply.ContainsKey(card) == false)
			{
				m_Supply.Add(card, 1);
				return false;
			}
			++m_Supply[card];
			return true;
		}

		private readonly Variant m_Variant;

		private readonly Queue<CardId> m_Deck = new Queue<CardId>();
		private readonly Dictionary<CardId, int> m_Supply = new Dictionary<CardId, int>();
	}
}