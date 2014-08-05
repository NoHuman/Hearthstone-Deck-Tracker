namespace Hearthstone.Statistics.Tests
{
	using Hearthstone_Deck_Tracker;
	using Hearthstone_Deck_Tracker.Hearthstone;
	using Moq;
	using NUnit.Framework;
	using System;
	using System.Collections.Generic;
	using System.Collections.ObjectModel;
	using System.Linq;

	[TestFixture]
	public class GameEventTests
	{
		private string GetRandomCardFromDb()
		{
			return Game._cardDb.Random().Key;
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
						Id = GetRandomCardFromDb()
					}),
				30);
			return deck;
		}

		private string GetRandomCardFromDeck()
		{
			return Game.PlayerDeck.Where(card => card.Count > 0).Random().Id;
		}

		private string GetRandomCardFromHand()
		{
			return Game.PlayerDrawn.Random().Id;
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
			var mainWindow = Mock.Of<IMainWindow>();
			var game = new GameEventHandler2(decks, turnTimer, overlayWindow, mainWindow);
			var unkownOpponentDeck = new Deck
			{
				Class = "Druid"
			};
			game.SetOpponentHero(unkownOpponentDeck);
			game.HandleGameStart(deck);

			Turn0(game);

			Turn1(game);

			game.HandleGameEnd();
		}

		private void Turn1(GameEventHandler2 game)
		{
			const int TurnNumber = 0;
			game.TurnStart(Turn.Player, TurnNumber);
			PlayerDraw(game);
			game.HandlePlayerPlay(GetRandomCardFromHand());

			game.TurnStart(Turn.Opponent, TurnNumber);
			game.HandleOpponentDraw(TurnNumber);
			game.HandleOpponentPlay(GetRandomCardFromDb(), 1, TurnNumber);
		}

		private void Turn0(GameEventHandler2 game)
		{
			const int TurnNumber = -1;
			FuncExtensions.Times(() => game.HandleOpponentDraw(TurnNumber), 4);
			FuncExtensions.Times(() => PlayerDraw(game), 3);
			game.HandleOpponentMulligan(1);
			game.HandleOpponentMulligan(1);
			FuncExtensions.Times(() => game.HandleOpponentDraw(TurnNumber), 2);
			game.HandlePlayerDeckDiscard(GetRandomCardFromHand());
			FuncExtensions.Times(() => PlayerDraw(game), 1);
		}

		private void PlayerDraw(GameEventHandler2 game)
		{
			var cardId = GetRandomCardFromDeck();
			game.PlayerSetAside(cardId);
			game.HandlePlayerDraw(cardId);
		}
	}

	public static class FuncExtensions
	{
		private static readonly Random random = new Random();

		public static void Times(this Action source, int times)
		{
			for (int i = 0; i < times; i++)
			{
				source();
			}
		}

		public static T1 Random<T1>(this IEnumerable<T1> source)
		{
			var maxValue = source.Count();
			return source.Skip(random.Next(0, maxValue)).FirstOrDefault();
		}
	}
}