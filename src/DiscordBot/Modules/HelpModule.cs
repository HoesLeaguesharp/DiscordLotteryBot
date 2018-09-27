using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;

namespace DiscordBot.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;

        public HelpModule(CommandService service)
        {
            _service = service;
        }

        [Command("Help")]
        public async Task HelpAsync()
        {
            var builder = new EmbedBuilder()
            {
                Color = new Color(114, 137, 218),
                Description = "These are the commands you can use"
            };
            string description = null;
            foreach (var module in _service.Modules.Where(x => x.Name != "HelpModule"))
            {
                
                foreach (var cmd in module.Commands.Where(x=> x.Name != "Display"))
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"~{cmd.Aliases.First()} ";

                    foreach (var x in cmd.Parameters.Select(p => p.Name))
                    {
                        description += $"[{x}] ";
                    }

                    description += $"- {cmd.Summary}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            builder.Fields.First(x => x.Name == "Lotto").Value +=
                $"~lotto display - Display the current participants\n~lotto display[user] - Display user's ticket count";
            await ReplyAsync("", false, builder.Build());
        }
    }
}
