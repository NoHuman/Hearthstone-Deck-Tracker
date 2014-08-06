namespace Hearthstone_Deck_Tracker
{
	public interface IOverlayWindow2
	{
		void Refresh();
		void ShowSecrets(string playingAgainst);
		void HideSecrets();
		void HideTimers();
	}
}