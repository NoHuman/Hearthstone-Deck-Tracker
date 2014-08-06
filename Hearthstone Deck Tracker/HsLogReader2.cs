#region

#endregion

namespace Hearthstone_Deck_Tracker
{
	using System.IO;
	using System.Threading.Tasks;

	public class HsLogReader2
	{
		private const int MaxFileLength = 3000000;
		private readonly GameEventHandler2 gameEventHandler;

		private readonly int _updateDelay;
		private readonly ILogAnalyzer2 logLogAnalyzer;
		private readonly FileInfo logFile;
		private readonly IMainWindow2 mainWindow;
		private bool _doUpdate;
		private bool _first;
		private long _lastGameEnd;
		private int _powerCount;
		private long _previousSize;
		private int _turnCount;
		private long _currentOffset;

		public HsLogReader2()
		{
			var hsDirPath = Config.Instance.HearthstoneDirectory;
			var updateDelay = Config.Instance.UpdateDelay;

			_updateDelay = updateDelay == 0 ? 100 : updateDelay;
			while (hsDirPath.EndsWith("\\") || hsDirPath.EndsWith("/"))
			{
				hsDirPath = hsDirPath.Remove(hsDirPath.Length - 1);
			}
		}

		public HsLogReader2(GameEventHandler2 gameEventHandler, IMainWindow2 mainWindow, ILogAnalyzer2 logLogAnalyzer, FileInfo logFile)
			: this()
		{
			this.gameEventHandler = gameEventHandler;
			this.mainWindow = mainWindow;
			this.logLogAnalyzer = logLogAnalyzer;
			this.logFile = logFile;
		}

		public int GetTurnNumber()
		{
			return (_turnCount)/2;
		}

		public void Start()
		{
			_first = true;
			_doUpdate = true;
			ReadFileAsync();
		}

		public void Stop()
		{
			_doUpdate = false;
		}

		private async void ReadFileAsync()
		{
			while (_doUpdate)
			{
				if (logFile.Exists)
				{
					//find end of last game (avoids reading the full log on start)
					if (_first)
					{
						using (var fs = logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
						{
							var fileOffset = 0L;
							if (fs.Length > MaxFileLength)
							{
								fileOffset = fs.Length - MaxFileLength;
								fs.Seek(fs.Length - MaxFileLength, SeekOrigin.Begin);
							}
							_previousSize = FindLastGameEnd(fs) + fileOffset;
							_currentOffset = _previousSize;
							_first = false;
						}
					}

					using (var fs = logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						fs.Seek(_previousSize, SeekOrigin.Begin);
						if (fs.Length == _previousSize)
						{
							await Task.Delay(_updateDelay);
							continue;
						}
						var newLength = fs.Length;

						using (var sr = new StreamReader(fs))
						{
							var newLines = sr.ReadToEnd();
							if (!newLines.EndsWith("\n"))
							{
								//hearthstone log apparently does not append full lines
								await Task.Delay(_updateDelay);
								continue;
							}
							logLogAnalyzer.Analyze(newLines);
							mainWindow.Refresh();
						}

						_previousSize = newLength;
					}
				}

				await Task.Delay(_updateDelay);
			}
		}

		private long FindLastGameEnd(FileStream fs)
		{
			using (var sr = new StreamReader(fs))
			{
				long offset = 0,
					tempOffset = 0;
				var lines = sr.ReadToEnd().Split('\n');

				foreach (var line in lines)
				{
					tempOffset += line.Length + 1;
					if (line.StartsWith("[Bob] legend rank"))
					{
						offset = tempOffset;
					}
				}

				return offset;
			}
		}

		internal void Reset(bool full)
		{
			if (full)
			{
				_previousSize = 0;
				_first = true;
			}
			else
			{
				_currentOffset = _lastGameEnd;
				_previousSize = _lastGameEnd;
			}
			_turnCount = 0;
		}
	}
}