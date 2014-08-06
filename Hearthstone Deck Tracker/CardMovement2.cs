namespace Hearthstone_Deck_Tracker
{
	public class CardMovement2 : HearthstoneAction2
	{
		public string Id { get; set; }
		public CardCollection2 From { get; set; }
		public CardCollection2 To { get; set; }
		public int ZonePosition { get; set; }
	}
}