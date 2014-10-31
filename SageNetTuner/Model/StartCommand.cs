namespace SageNetTuner.Model
{
	using System;
	using System.IO;
	using System.Text.RegularExpressions;

	public class StartCommand
	{
		private readonly string _channel;

		private readonly string _fileName;

		private readonly string _title;

		private readonly long _duration;

		public StartCommand(string channel, string filename, long duration)
		{
			if (!Regex.IsMatch(channel, "^\\d+$"))
			{
				throw new ArgumentException("Channel must be a numeric value");
			}
			this._channel = channel;
			this._fileName = filename;
			this._title = Path.GetFileNameWithoutExtension(filename);
			this._duration = duration;
		}

		public StartCommand(string channel, string filename, string duration) : this(channel, filename, Convert.ToInt64(duration))
		{
		}

		public StartCommand(string channel, string filename) : this(channel, filename, 0L)
		{
			
		}

		public override string ToString()
		{
			return string.Format("Channel={0}, Filename={1}, Duration={2}", _channel, _fileName, _duration);
		}

		public string Channel
		{
			get
			{
				return this._channel;
			}
		}

		public long Duration
		{
			get
			{
				return this._duration;
			}
		}

		public string FileName
		{
			get
			{
				return this._fileName;
			}
		}

		public string Title
		{
			get
			{
				return this._title;
			}
		}

	}

}
