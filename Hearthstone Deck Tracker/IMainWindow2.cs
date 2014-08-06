namespace Hearthstone_Deck_Tracker
{
	using Hearthstone_Deck_Tracker.Hearthstone;
	using System.Collections.ObjectModel;

	public interface IMainWindow2
	{
		bool NeedToIncorrectDeckMessage { get; set; }
		bool IsShowingIncorrectDeckMessage { get; set; }
		ReadOnlyCollection<string> EventKeys { get; set; }
		void Refresh();
		void SelectDeck(Deck deck);
		DeckInfo GetLastDeck(string playingAs);
		void UpdateDeckList(Deck deck);
		void UseDeck(Deck deck);
		void SavePlayedCards();
		Deck GetSelectedDeck();
	}
}