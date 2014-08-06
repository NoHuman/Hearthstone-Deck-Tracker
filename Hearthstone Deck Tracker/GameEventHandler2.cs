using System.Collections.Generic;
using System.Linq;

namespace Hearthstone_Deck_Tracker
{
	using Hearthstone_Deck_Tracker.Hearthstone;
	using System.Windows.Forms;

	public class GameEventHandler2
	{
		private IEnumerable<Deck> deckRepository;
		private ITurnTimer2 turnTimer;
		private IOverlayWindow2 overlayWindow;
		private IMainWindow2 mainWindow;

		public GameEventHandler2(IEnumerable<Deck> deckRepository, ITurnTimer2 turnTimer, IOverlayWindow2 overlayWindow, IMainWindow2 mainWindow)
		{
			this.deckRepository = deckRepository;
			this.turnTimer = turnTimer;
			this.overlayWindow = overlayWindow;
			this.mainWindow = mainWindow;
		}

		public void HandlePlayerGet(string cardId)
		{
			LogEvent("PlayerGet", cardId);
			Game.PlayerGet(cardId, false);
		}

		public void HandlePlayerBackToHand(string cardId)
		{
			LogEvent("PlayerBackToHand", cardId);
			Game.PlayerGet(cardId, true);
		}

		public async void HandlePlayerDraw(string cardId)
		{
			LogEvent("PlayerDraw", cardId);
			var correctDeck = Game.PlayerDraw(cardId);

			if (!(await correctDeck) && Config.Instance.AutoDeckDetection && !mainWindow.NeedToIncorrectDeckMessage &&
				!mainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				mainWindow.NeedToIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck");
			}
		}

		public void HandlePlayerMulligan(string cardId)
		{
			LogEvent("PlayerMulligan", cardId);
			turnTimer.MulliganDone(Turn.Player);
			Game.PlayerMulligan(cardId);

			//without this update call the overlay deck does not update properly after having Card implement INotifyPropertyChanged
			overlayWindow.Refresh();
			mainWindow.Refresh();
		}

		public void HandlePlayerHandDiscard(string cardId)
		{
			LogEvent("PlayerHandDiscard", cardId);
			Game.PlayerHandDiscard(cardId);
			overlayWindow.Refresh();
			mainWindow.Refresh();
		}

		public void HandlePlayerPlay(string cardId)
		{
			LogEvent("PlayerPlay", cardId);
			Game.PlayerPlayed(cardId);
			overlayWindow.Refresh();
			mainWindow.Refresh();
		}

		public void HandlePlayerDeckDiscard(string cardId)
		{
			LogEvent("PlayerDeckDiscard", cardId);
			var correctDeck = Game.PlayerDeckDiscard(cardId);

			//don't think this will ever detect an incorrect deck but who knows...
			if (!correctDeck && Config.Instance.AutoDeckDetection && !mainWindow.NeedToIncorrectDeckMessage &&
				!mainWindow.IsShowingIncorrectDeckMessage && Game.IsUsingPremade)
			{
				mainWindow.NeedToIncorrectDeckMessage = true;
				Logger.WriteLine("Found incorrect deck", "HandlePlayerDiscard");
			}
		}
		#region Opponent

		public void HandleOpponentPlay(string id, int position, int turn)
		{
			LogEvent("OpponentPlay", id, turn, position);
			Game.OpponentPlay(id, position, turn);
		}

		public void HandleOpponentDraw(int turn)
		{
			LogEvent("OpponentDraw", turn: turn);
			Game.OpponentDraw(turn);
		}

		public void HandleOpponentMulligan(int position)
		{
			LogEvent("OpponentMulligan", from: position);
			Game.OpponentMulligan(position);
			turnTimer.MulliganDone(Turn.Opponent);
		}

		public void HandleOpponentGet(int turn)
		{
			LogEvent("OpponentGet", turn: turn);
			Game.OpponentGet(turn);
		}

		public void HandleOpponentSecretPlayed()
		{
			LogEvent("OpponentSecretPlayed");
			Game.OpponentSecretCount++;
			overlayWindow.ShowSecrets(Game.PlayingAgainst);
		}

		public void HandleOpponentPlayToHand(string cardId, int turn)
		{
			LogEvent("OpponentBackToHand", cardId, turn);
			Game.OpponentBackToHand(cardId, turn);
		}

		public void HandleOpponentSecretTrigger(string cardId)
		{
			LogEvent("OpponentSecretTrigger", cardId);
			Game.OpponentSecretTriggered(cardId);
			Game.OpponentSecretCount--;
			if (Game.OpponentSecretCount <= 0)
				overlayWindow.HideSecrets();
		}

		public void HandleOpponentDeckDiscard(string cardId)
		{
			LogEvent("OpponentDeckDiscard", cardId);
			Game.OpponentDeckDiscard(cardId);

			//there seems to be an issue with the overlay not updating here.
			//possibly a problem with order of logs?
			overlayWindow.Refresh();
			mainWindow.Refresh();
		}

		#endregion

		public void SetOpponentHero(Deck opponentDeck)
		{
			Game.PlayingAgainst = opponentDeck.GetClass;
			Logger.WriteLine("Playing against " + opponentDeck.GetClass, "Hearthstone");
		}

		public void TurnStart(Turn player, int turnNumber)
		{
			Logger.WriteLine(string.Format("{0}-turn ({1})", player, turnNumber + 1), "LogReader");
			//doesn't really matter whose turn it is for now, just restart timer
			//maybe add timer to player/opponent windows
			turnTimer.SetCurrentPlayer(player);
			turnTimer.Restart();
			if (player == Turn.Player && !Game.IsInMenu)
			{
				if (Config.Instance.FlashHsOnTurnStart)
					User32.FlashHs();

				if (Config.Instance.BringHsToForeground)
					User32.BringHsToForeground();
			}
		}

		public void HandleGameStart(Deck playerDeck)
		{
			//avoid new game being started when jaraxxus is played
			if (!Game.IsInMenu)
				return;

			Game.PlayingAs = playerDeck.Class;

			Logger.WriteLine("Game start");

			if (Config.Instance.FlashHsOnTurnStart)
				User32.FlashHs();
			if (Config.Instance.BringHsToForeground)
				User32.BringHsToForeground();

			if (Config.Instance.KeyPressOnGameStart != "None" &&
				mainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameStart))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameStart + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameStart);
			}

			var selectedDeck = playerDeck;
			if (selectedDeck != null)
				Game.SetPremadeDeck((Deck)selectedDeck.Clone());

			Game.IsInMenu = false;
			Game.Reset();

			//select deck based on hero

			if (!Game.IsUsingPremade || !Config.Instance.AutoDeckDetection)
				return;

			if (selectedDeck.Class != Game.PlayingAs)
			{
				var classDecks = deckRepository.Where(d => d.Class == Game.PlayingAs).ToList();
				if (classDecks.Count == 0)
				{
					Logger.WriteLine("Found no deck to switch to", "HandleGameStart");
				}
				else if (classDecks.Count == 1)
				{
					mainWindow.SelectDeck(classDecks[0]);
					Logger.WriteLine("Found deck to switch to: " + classDecks[0].Name, "HandleGameStart");
				}
				else if (mainWindow.GetLastDeck(Game.PlayingAs) != null)
				{
					var lastDeckName = mainWindow.GetLastDeck(Game.PlayingAs).Name;
					Logger.WriteLine("Found more than 1 deck to switch to - last played: " + lastDeckName, "HandleGameStart");

					var deck = deckRepository.FirstOrDefault(d => d.Name == lastDeckName);

					if (deck != null)
					{
						mainWindow.SelectDeck(deck);
						mainWindow.UpdateDeckList(deck);
						mainWindow.UseDeck(deck);
					}
				}
			}
		}

		public void HandleGameEnd()
		{
			Logger.WriteLine("Game end");
			if (Config.Instance.KeyPressOnGameEnd != "None" && mainWindow.EventKeys.Contains(Config.Instance.KeyPressOnGameEnd))
			{
				SendKeys.SendWait("{" + Config.Instance.KeyPressOnGameEnd + "}");
				Logger.WriteLine("Sent keypress: " + Config.Instance.KeyPressOnGameEnd);
			}
			turnTimer.Stop();
			overlayWindow.HideTimers();
			overlayWindow.HideSecrets();
			if (!Game.IsUsingPremade)
			{
				Game.DrawnLastGame = new List<Card>(Game.PlayerDrawn);
			}
			if (Config.Instance.SavePlayedGames && !Game.IsInMenu)
			{
				mainWindow.SavePlayedCards();
			}
			if (!Config.Instance.KeepDecksVisible)
			{
				var deck = mainWindow.GetSelectedDeck();
				if (deck != null)
					Game.SetPremadeDeck((Deck)deck.Clone());

				Game.Reset();
			}
			Game.IsInMenu = true;
		}

		private void LogEvent(string type, string id = "", int turn = 0, int from = -1)
		{
			Logger.WriteLine(string.Format("{0} (id:{1} turn:{2} from:{3})", type, id, turn, from), "LogReader");
		}

		public void PlayerSetAside(string id)
		{
			Game.SetAsideCards.Add(id);
			Logger.WriteLine("set aside: " + id);
		}
	}

}
