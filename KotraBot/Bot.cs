using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace KotraBot
{
    public interface IBot
    {
        Task StartAsync(ServiceProvider services);

        Task StopAsync();
    }

    public class Bot : IBot
    {
        private ServiceProvider? _serviceProvider;

        private readonly ILogger<Bot> _logger;
        private readonly IConfiguration _configuration;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;

        public Bot(ILogger<Bot> logger, IConfiguration configuration)
        {
            this._logger = logger;
            this._configuration = configuration;

            var adminIds = _configuration.GetSection("admins").Get<ulong[]>() ?? new ulong[0]; // get the admin ids from the configuration file
            Admin.Init(adminIds);

            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            };

            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
        }

        public async Task StartAsync(ServiceProvider services)
        {
            string DiscordToken = _configuration["DiscordToken"] ?? throw new Exception("Discord Token not found");

            _logger.LogInformation($"Starting up with token {(Program.debug ? DiscordToken :  "[hidden]" )}");

            _serviceProvider = services;

            await _commands.AddModulesAsync(Assembly.GetExecutingAssembly(), _serviceProvider);

            await _client.LoginAsync(TokenType.Bot, DiscordToken);
            await _client.StartAsync();

            _client.MessageReceived += HandleCommandReceive;
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Shutting down");
            
            if (_client is not null)
            {
                await _client.LogoutAsync();
                await _client.StopAsync();
                await _client.DisposeAsync();
            }
        }

        private async Task HandleCommandReceive(SocketMessage arg)
        {
            if (arg is not SocketUserMessage message || message.Author.IsBot)
                return;

            _logger.LogInformation($"[{DateTime.Now.ToShortTimeString()}] {message.Author}: {message.Content}");

            int position = 0;
            if (message.HasStringPrefix("k!", ref position))
            {
                await _commands.ExecuteAsync(new SocketCommandContext(_client, message), position, _serviceProvider);  
            }
        }
    }
}
