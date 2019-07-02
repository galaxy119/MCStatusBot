using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.Webhook;
using Discord.WebSocket;

namespace MCStatusBot
{
	public class Bot
	{
		private DiscordSocketClient Client => client ?? (client = new DiscordSocketClient());
		public static DiscordSocketClient client;
		private readonly Program program;

		public Bot(Program program)
		{
			this.program = program;
			InitBot().GetAwaiter().GetResult();
		}

		private async Task InitBot()
		{
			Client.Log += Program.Log;
			Client.MessageReceived += OnMessageReceived;
			await Client.LoginAsync(TokenType.Bot, program.Config.BotToken);
			await Client.StartAsync();
			await StatusHandler.DoStatusCheckRequest(program);
			await Task.Delay(-1);
		}

		private async Task OnMessageReceived(SocketMessage message)
		{
			if (message.Content.StartsWith(program.Config.BotPrefix))
			{
				CommandContext context = new CommandContext(Client, (IUserMessage) message);
				HandleCommand(context);
			}
		}

		private async Task HandleCommand(CommandContext context)
		{
			if (context.Message.Content.ToLower().StartsWith(program.Config.BotPrefix + "ping")) await context.Channel.SendMessageAsync("Pong!");

			if (context.Message.Content.ToLower().StartsWith(program.Config.BotPrefix + "status")) 
				await StatusHandler.DoServerStatusCheck(context, program);
			if (context.Message.Content.ToLower().StartsWith(program.Config.BotPrefix + "modme") &&
			    ((IGuildUser) context.Message.Author).RoleIds.Any(p => p == 561736990685528084))
			{
				var role = context.Guild.GetRole(580719914466410509);
				await ((IGuildUser)context.Message.Author).AddRoleAsync(role);
				await context.Channel.SendMessageAsync("<@" + context.Message.Author.Id +
				                                       "> You are now a Minecraft Moderator!");
			}
		}
	}
}