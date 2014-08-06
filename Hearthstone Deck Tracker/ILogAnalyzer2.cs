namespace Hearthstone_Deck_Tracker
{

	public interface ILogAnalyzer2
	{
		HearthstoneAction2 Analyze(string logLine);
	}
}