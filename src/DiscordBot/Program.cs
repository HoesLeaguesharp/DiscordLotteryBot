using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Modules;
using DiscordBot.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot
{
    class Program
    {

        static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        public  IConfiguration _config;
        public class LottoNumbers
        {
            [JsonProperty("TicketCount")]
            public List<InfoModule.lotto> TicketCount { get; set; }
        }
        public static LottoNumbers LottoList = new LottoNumbers();
        public async Task MainAsync()
        {
            _client = new DiscordSocketClient();
            _config = BuildConfig();
            var services = ConfigureServices();
            services.GetRequiredService<LogService>();
            await services.GetRequiredService<CommandHandlingService>().InitializeAsync(services);
            LottoList.TicketCount = new List<InfoModule.lotto>();
            LottoList =
                JsonConvert.DeserializeObject<LottoNumbers>(File.ReadAllText(Directory.GetCurrentDirectory() + @"\test.json"));
            foreach (var x in LottoList.TicketCount)
            {
                for (int i = 0; i < x.Count; i++)
                {
                    InfoModule.lottotickets.Add(x.Name);
                }
            }
            await _client.LoginAsync(TokenType.Bot, _config["token"]);
            await _client.StartAsync();

            await Task.Delay(-1);
        }

        private IServiceProvider ConfigureServices()
        {
            return new ServiceCollection()
                // Base
                .AddSingleton(_client)
                .AddSingleton<CommandService>()
                .AddSingleton<CommandHandlingService>()
                // Logging
                .AddLogging()
                .AddSingleton<LogService>()
                // Extra
                .AddSingleton(_config)
                // Add additional services here...
                .BuildServiceProvider();
        }

        private IConfiguration BuildConfig()
        {
            return new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json")
                .Build();
        }
        
    }
}