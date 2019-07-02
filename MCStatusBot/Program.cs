using System;
using System.Threading.Tasks;
using Discord;
using System.IO;
using Newtonsoft.Json;

namespace MCStatusBot
{
	public class Program
	{
		private readonly Bot bot;
		private const string kCfgFile = "McBotConfig.json";

		public Config Config => config ?? (config = GetConfig());
		private Config config;
		
		static void Main(string[] args)
		{
			new Program();
		}

		private Program() => bot = new Bot(this);

		public static Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		public static Config GetConfig()
		{
			if (File.Exists(kCfgFile))
				return JsonConvert.DeserializeObject<Config>(File.ReadAllText(kCfgFile));
			File.WriteAllText(kCfgFile, JsonConvert.SerializeObject(Config.Default, Formatting.Indented));
			return Config.Default;
		}
		
	}

	public class Config
	{
		public string BotPrefix { get; set; }
		public string BotToken { get; set; }
		public string Url { get; set; }
		public string Port { get; set; }

		public static readonly Config Default = new Config { BotPrefix = "(", BotToken = "", Url = "", Port = "" };
	}
}