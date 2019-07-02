using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Newtonsoft.Json;

namespace MCStatusBot
{
	public class StatusHandler
	{
		private static WebClient _webClient = new WebClient();
		private static Bot boi;
		public StatusHandler(Bot bot) => boi = bot;

		public static async Task DoServerStatusCheck(CommandContext context, Program program)
		{
			byte[] response = await _webClient.DownloadDataTaskAsync(
				"http://mcapi.us/server/status?ip=" + program.Config.Url + "&port" + program.Config.Port);
			StatusResponse result = JsonConvert.DeserializeObject<StatusResponse>(Encoding.UTF8.GetString(response));
			
			if (result.online != "true")
			{
				await context.Channel.SendMessageAsync("Server offline.");
				return;
			}

			await context.Channel.SendMessageAsync("Server Status: Online" + "\n" + "Address: " +
			                                       program.Config.Url + "\n" + "Port: " + program.Config.Port + "\n" +
			                                       "Online Players: " + result.players.Now + " / " +
			                                       result.players.Max + "\n" + "MOTD: " + result.motd + "\n" + "*Status delayed by up to 5 minutes.*");
		}

		public static async Task DoStatusCheckRequest(Program program)
		{
			 if (!ushort.TryParse(program.Config.Port, out ushort port))
				 throw new ArgumentException("Invalid port.");
			 while (true) 
			 {
				MineStat ms = new MineStat(program.Config.Url, port);

				if (!ms.ServerUp)
				{
					await Bot.client.SetStatusAsync(UserStatus.DoNotDisturb);
					await Bot.client.SetActivityAsync(new Game("Waiting for server startup.."));
				}
				else
				{
					int.TryParse(ms.CurrentPlayers, out int count);
					int.TryParse(ms.MaximumPlayers, out int max);
					if (count == 0)
						await Bot.client.SetStatusAsync(UserStatus.Idle);
					else
						await Bot.client.SetStatusAsync(UserStatus.Online);
					
					await Bot.client.SetActivityAsync(new Game(count + " / " + max + " Ping: " + ms.Latency + "ms"));
				}
				
				await Task.Delay(5000);
			 }
			
		}
	}

	public class StatusResponse
	{
		public string status { get; set; }
		public string online { get; set; }
		public string motd { get; set; }
		public string error { get; set; }
		public Players players { get; set; }
		public string last_online { get; set; }
		public string last_updated { get; set; }
		public string duration { get; set; }
	}

	public class Players
	{
		public int Max { get; set; }
		public int Now { get; set; }
	}
}