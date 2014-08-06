namespace Hearthstone_Deck_Tracker
{
	public class OpponentStartEvent2 : HearthstoneAction2
	{
		public string ClassName { get; set; }

		public OpponentStartEvent2(string className)
		{
			ClassName = className;
		}
	}
}