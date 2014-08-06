using NUnit.Framework;

namespace Hearthstone.Statistics.Tests
{
	using Hearthstone_Deck_Tracker;
	using Hearthstone_Deck_Tracker.Hearthstone;
	using Moq;
	using System.Collections.Generic;
	using System.IO;
	using System.Threading;

	[TestFixture]
	public class HsLogReaderTests
	{
		[Test]
		public void HsLogReaderTest()
		{
			var deckRepository = Mock.Of<IEnumerable<Deck>>();
			var turnTimer = Mock.Of<ITurnTimer2>();
			var overlayWindow = Mock.Of<IOverlayWindow2>();
			var mainWindow = Mock.Of<IMainWindow2>();
			var gameEventHandler2 = new GameEventHandler2(deckRepository, turnTimer, overlayWindow, mainWindow);
			var logAnalyzer = Mock.Of<ILogAnalyzer2>();
			var logReader = new HsLogReader2(gameEventHandler2, mainWindow, logAnalyzer, new FileInfo(@"C:\Program Files (x86)\Hearthstone\Hearthstone_Data\output_log.txt"));

			logReader.Start();
			Thread.Sleep(1000);
			logReader.Stop();
		}
	}
}
