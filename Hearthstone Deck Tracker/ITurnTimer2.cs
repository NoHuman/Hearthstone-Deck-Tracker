namespace Hearthstone_Deck_Tracker
{
	public interface ITurnTimer2
	{
		void MulliganDone(Turn player);
		void SetCurrentPlayer(Turn player);
		void Restart();
		void Stop();
	}
}