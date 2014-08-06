namespace Hearthstone_Deck_Tracker
{
	using System;
	using System.Collections.Generic;
	using System.Text.RegularExpressions;

	public class LogAnalyzer2 : ILogAnalyzer2
	{
		private const int PowerCountTreshold = 14;

		private readonly Regex _cardMovementRegex = new Regex(@"\w*(cardId=(?<Id>(\w*))).*(zone\ from\ (?<from>((\w*)\s*)*))((\ )*->\ (?<to>(\w*\s*)*))*.*");

		private readonly Dictionary<string, string> _heroIdDict = new Dictionary<string, string>
		{
			{ "HERO_01", "Warrior" },
			{ "HERO_02", "Shaman" },
			{ "HERO_03", "Rogue" },
			{ "HERO_04", "Paladin" },
			{ "HERO_05", "Hunter" },
			{ "HERO_06", "Druid" },
			{ "HERO_07", "Warlock" },
			{ "HERO_08", "Mage" },
			{ "HERO_09", "Priest" }
		};
		private readonly Regex _opponentPlayRegex = new Regex(@"\w*(zonePos=(?<zonePos>(\d+))).*(zone\ from\ OPPOSING\ HAND).*");
		private readonly Regex _zoneRegex = new Regex(@"\w*(zone=(?<zone>(\w*)).*(zone\ from\ FRIENDLY\ DECK)\w*)");
		private long _currentOffset;
		private GameEventHandler2 GameEventHandler;
		private int _powerCount;
		private long _lastGameEnd;
		private int _turnCount;
		private Dictionary<string, CardCollection2> cardCollection;

		public LogAnalyzer2(GameEventHandler2 gameEventHandler, Dictionary<string, CardCollection2> cardCollection)
		{
			this.GameEventHandler = gameEventHandler;
			this.cardCollection = cardCollection;
		}

		public HearthstoneAction2 Analyze(string log)
		{
			var logLines = log.Split('\n');
			foreach (var logLine in logLines)
			{
				//_currentOffset += logLine.Length + 1;
				if (logLine.StartsWith("[Power]"))
				{
					_powerCount++;
				}
				else if (logLine.StartsWith("[Bob] ---RegisterScreenBox---"))
				{
					return new GameEndEvent2();
				}
				else if (logLine.StartsWith("[Zone]"))
				{
					if (_cardMovementRegex.IsMatch(logLine))
					{
						var match = _cardMovementRegex.Match(logLine);

						var id = match.Groups["Id"].Value.Trim();
						var from = match.Groups["from"].Value.Trim();
						var to = match.Groups["to"].Value.Trim();

						var zonePos = -1;

						// Only for some log lines, should be valid in every action where we need it
						if (_opponentPlayRegex.IsMatch(logLine))
						{
							var match2 = _opponentPlayRegex.Match(logLine);
							zonePos = Int32.Parse(match2.Groups["zonePos"].Value.Trim());
						}
						if (_zoneRegex.IsMatch(logLine))
						{
							GameEventHandler.PlayerSetAside(id);
						}

						//game start/end
						if (id.Contains("HERO"))
						{
							if (!@from.Contains("PLAY"))
							{
								if (to.Contains("FRIENDLY"))
								{
									return new GameStartEvent2();
								}
								if (to.Contains("OPPOSING"))
								{
									return new OpponentStartEvent2(_heroIdDict[id]);
								}
							}
							_powerCount = 0;
							continue;
						}

						return new CardMovement2
						{
							Id =  id,
							From = cardCollection[from],
							To  = cardCollection[to],
							ZonePosition = zonePos
						};
						//switch (@from)
						//{
						//	case "FRIENDLY DECK":
						//		if (to == "FRIENDLY HAND")
						//		{
						//			//player draw
						//			if (_powerCount >= PowerCountTreshold)
						//			{
						//				_turnCount++;
						//				GameEventHandler.TurnStart(Turn.Player, GetTurnNumber());
						//			}
						//			GameEventHandler.HandlePlayerDraw(id);
						//		}
						//		else
						//		{
						//			//player discard from deck
						//			GameEventHandler.HandlePlayerDeckDiscard(id);
						//		}
						//		break;
						//	case "FRIENDLY HAND":
						//		if (to == "FRIENDLY DECK")
						//		{
						//			GameEventHandler.HandlePlayerMulligan(id);
						//		}
						//		else if (to == "FRIENDLY PLAY")
						//		{
						//			GameEventHandler.HandlePlayerPlay(id);
						//		}
						//		else
						//		{
						//			//player discard from hand and spells
						//			GameEventHandler.HandlePlayerHandDiscard(id);
						//		}

						//		break;
						//	case "FRIENDLY PLAY":
						//		if (to == "FRIENDLY HAND")
						//		{
						//			GameEventHandler.HandlePlayerBackToHand(id);
						//		}
						//		break;
						//	case "OPPOSING HAND":
						//		if (to == "OPPOSING DECK")
						//		{
						//			//opponent mulligan
						//			GameEventHandler.HandleOpponentMulligan(zonePos);
						//		}
						//		else
						//		{
						//			if (to == "OPPOSING SECRET")
						//			{
						//				GameEventHandler.HandleOpponentSecretPlayed();
						//			}

						//			GameEventHandler.HandleOpponentPlay(id, zonePos, GetTurnNumber());
						//		}
						//		break;
						//	case "OPPOSING DECK":
						//		if (to == "OPPOSING HAND")
						//		{
						//			if (_powerCount >= HsLogReader2.PowerCountTreshold)
						//			{
						//				_turnCount++;
						//				GameEventHandler.TurnStart(Turn.Opponent, GetTurnNumber());
						//			}

						//			//opponent draw
						//			GameEventHandler.HandlOpponentDraw(GetTurnNumber());
						//		}
						//		else
						//		{
						//			//opponent discard from deck
						//			GameEventHandler.HandleOpponentDeckDiscard(id);
						//		}
						//		break;
						//	case "OPPOSING SECRET":
						//		//opponent secret triggered
						//		GameEventHandler.HandleOpponentSecretTrigger(id);
						//		break;
						//	case "OPPOSING PLAY":
						//		if (to == "OPPOSING HAND") //card from play back to hand (sap/brew)
						//		{
						//			GameEventHandler.HandleOpponentPlayToHand(id, GetTurnNumber());
						//		}
						//		break;
						//	default:
						//		if (to == "OPPOSING HAND")
						//		{
						//			//coin, thoughtsteal etc
						//			GameEventHandler.HandleOpponentGet(GetTurnNumber());
						//		}
						//		else if (to == "FRIENDLY HAND")
						//		{
						//			//coin, thoughtsteal etc
						//			GameEventHandler.HandlePlayerGet(id);
						//		}
						//		else if (to == "OPPOSING GRAVEYARD" && @from == "" && id != "")
						//		{
						//			//todo: not sure why those two are here
						//			//CardMovement(this, new CardMovementArgs(CardMovementType.OpponentPlay, id));
						//		}
						//		else if (to == "FRIENDLY GRAVEYARD" && @from == "")
						//		{
						//			// CardMovement(this, new CardMovementArgs(CardMovementType.PlayerPlay, id));
						//		}
						//		break;
						//}
						_powerCount = 0;
					}
				}
			}
			return null;
		}
	}

	public abstract class HearthstoneAction2
	{
	}
}