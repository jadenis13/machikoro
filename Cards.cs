using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MachiKoro
{
	public enum CardId
	{
		INVALID,

		// Landmarks
		TrainStation, // choose to roll 1 or 2 dice per turn
		ShoppingMall, // extra coin per Restaurant/Industry
		AmusementPark, // doubles (when rolling 2 dice) allows 1 extra full turn
		RadioTower, // once per turn, choose to reroll. only second roll counts

		// Establishments
		WheatField, // 1 coin from the bank, anyone's turn
		Ranch, // 1 coin from the bank, anyone's turn
		Bakery, // 1 coin from the bank, your turn only
		Cafe, // get 1 coin from the player who rolled the dice
		ConvenienceStore, // 3 coins from the bank, your turn only
		Forest, // 1 coin from the bank, anyone's turn
		Stadium, // 2 coins from all players, your turn only
		TvStation, // 5 coins from any one player on your turn
		BusinessCenter, // trade one non-establishment with another player on your turn
		CheeseFactory, // 3 coins from the bank for each ranch on your turn
		FurnitureFactory, // 3 coins from the bank for each industry on your turn
		Mine, // 5 coins from the bank, anyone's turn
		FamilyRestaurant, // 2 coins from the player who rolled
		AppleOrchard, // 3 coins from the bank, anyone's turn
		ProduceMarket // 2 coins from the bank for each farm
	}

	internal enum EstablishmentType // in order of activation
	{
		INVALID,

		Landmarks,
		Restaurants, // red
		SecondaryIndustry, // green
		PrimaryIndustry, // blue
		MajorEstablishments // purple - only one of each type allowed to be built
	}

	internal readonly struct CardInfo
	{
		public CardId Id { get; }

		public EstablishmentType Type { get; }

		public int Cost { get; }

		public int InitialCount { get; }

		public CardInfo(CardId id, EstablishmentType type, int cost, int initialCount)
		{
			Id = id;
			Type = type;
			Cost = cost;
			InitialCount = initialCount;
		}

		public static readonly ReadOnlyCollection<CardId> ValidEstablishments;
		public static readonly ReadOnlyCollection<CardId> ValidLandmarks;

		static CardInfo()
		{
			var establishments = new List<CardId>
			{
				CardId.WheatField,
				CardId.Ranch,
				CardId.Bakery,
				CardId.Cafe,
				CardId.ConvenienceStore,
				CardId.Forest,
				CardId.Stadium,
				CardId.TvStation,
				CardId.BusinessCenter,
				CardId.CheeseFactory,
				CardId.FurnitureFactory,
				CardId.Mine,
				CardId.FamilyRestaurant,
				CardId.AppleOrchard,
				CardId.ProduceMarket
			};
			ValidEstablishments = new ReadOnlyCollection<CardId>(establishments);

			var landmarks = new List<CardId>
			{
				CardId.TrainStation,
				CardId.ShoppingMall,
				CardId.AmusementPark,
				CardId.RadioTower
			};
			ValidLandmarks = new ReadOnlyCollection<CardId>(landmarks);

		}
	}

	internal class CardInfoServer
	{
		public static readonly ReadOnlyDictionary<CardId, CardInfo> Lookup;

		static CardInfoServer()
		{
			var init = new List<CardInfo>
			{
				new CardInfo(CardId.WheatField,       EstablishmentType.PrimaryIndustry,     cost:  1, initialCount:  6),
				new CardInfo(CardId.Ranch,            EstablishmentType.PrimaryIndustry,     cost:  1, initialCount:  6),
				new CardInfo(CardId.Bakery,           EstablishmentType.SecondaryIndustry,   cost:  1, initialCount:  6),
				new CardInfo(CardId.Cafe,             EstablishmentType.Restaurants,         cost:  2, initialCount:  6),
				new CardInfo(CardId.ConvenienceStore, EstablishmentType.SecondaryIndustry,   cost:  2, initialCount:  6),
				new CardInfo(CardId.Forest,           EstablishmentType.PrimaryIndustry,     cost:  3, initialCount:  6),
				new CardInfo(CardId.Stadium,          EstablishmentType.MajorEstablishments, cost:  6, initialCount: -1),
				new CardInfo(CardId.TvStation,        EstablishmentType.MajorEstablishments, cost:  7, initialCount: -1),
				new CardInfo(CardId.BusinessCenter,   EstablishmentType.MajorEstablishments, cost:  8, initialCount: -1),
				new CardInfo(CardId.CheeseFactory,    EstablishmentType.SecondaryIndustry,   cost:  5, initialCount:  6),
				new CardInfo(CardId.FurnitureFactory, EstablishmentType.SecondaryIndustry,   cost:  3, initialCount:  6),
				new CardInfo(CardId.Mine,             EstablishmentType.PrimaryIndustry,     cost:  6, initialCount:  6),
				new CardInfo(CardId.FamilyRestaurant, EstablishmentType.Restaurants,         cost:  3, initialCount:  6),
				new CardInfo(CardId.AppleOrchard,     EstablishmentType.PrimaryIndustry,     cost:  3, initialCount:  6),
				new CardInfo(CardId.ProduceMarket,    EstablishmentType.SecondaryIndustry,   cost:  2, initialCount:  6),
				new CardInfo(CardId.TrainStation,     EstablishmentType.Landmarks,           cost:  4, initialCount: -1),
				new CardInfo(CardId.ShoppingMall,     EstablishmentType.Landmarks,           cost: 10, initialCount: -1),
				new CardInfo(CardId.AmusementPark,    EstablishmentType.Landmarks,           cost: 16, initialCount: -1),
				new CardInfo(CardId.RadioTower,       EstablishmentType.Landmarks,           cost: 22, initialCount: -1)
			};
			Lookup = new ReadOnlyDictionary<CardId, CardInfo>(init.ToDictionary(x => x.Id));
		}

	}
}