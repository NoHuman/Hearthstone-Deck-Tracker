namespace Hearthstone.Statistics.Tests
{
	using Hearthstone_Deck_Tracker;
	using Hearthstone_Deck_Tracker.Hearthstone;
	using Moq;
	using NUnit.Framework;
	using System;
	using System.Collections.ObjectModel;

	[TestFixture]
	public class GameEventTests
	{
		private string GetRandomCard()
		{
			return "EX1_363e";
		}

		private Deck CreatePlayerDeck()
		{
			var deck = new Deck
			{
				Name = "Deck under test",
				Class = "Paladin"
			};
			FuncExtensions.Times(
				() => deck.Cards.Add(
					new Card
					{
						Id = GetRandomCard()
					}),
				30);
			return deck;
		}

		[Test]
		public void TestMethod()
		{
			Game.LoadCardDb("enUS");
			var decks = new ObservableCollection<Deck>();
			var deck = CreatePlayerDeck();

			decks.Add(deck);
			var turnTimer = Mock.Of<ITurnTimer>();
			var overlayWindow = Mock.Of<IOverlayWindow>();
			var game = new GameEventHandler2(decks, turnTimer, overlayWindow);
			game.HandleGameStart(deck);
			var unkownDeck = new Deck
			{
				Class = "Druid"
			};
			game.SetOpponentHero(unkownDeck);
			FuncExtensions.Times(() => game.HandleOpponentDraw(0), 4);
			FuncExtensions.Times(() => game.HandlePlayerDraw(GetRandomCard()), 3);
			game.HandleOpponentDraw(0);
			game.HandleOpponentMulligan(2);
			game.HandleOpponentMulligan(1);
			game.HandleGameEnd();
		}
	}

	public static class FuncExtensions
	{
		public static void Times(this Action source, int times)
		{
			for (int i = 0; i < times; i++)
			{
				source();
			}
		}
	}
}