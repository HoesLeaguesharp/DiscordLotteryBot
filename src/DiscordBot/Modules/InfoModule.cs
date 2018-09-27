using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using DiscordBot.Services;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DiscordBot.Modules
{
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
       
        private static Random rng = new Random();
        public static bool MidRaffle = false;
        public static void Shuffle<T>(IList<T> list)
        {
            
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static List<ulong> lottotickets = new List<ulong>();
        public class lotto
        {
            public lotto(ulong name, int count)
            {
                Name = name;
                Count = count;
            }

            public lotto()
            {
            }
            public ulong Name { get; set; }
            public int Count { get; set; }
        }

        public static void WriteToJson(object _data)
        {
            string users = JsonConvert.SerializeObject(
                new
                {
                    TicketCount = _data
                });

            File.WriteAllText(Directory.GetCurrentDirectory() + @"\test.json", JToken.Parse(users).ToString());
        }

        [Group("Lotto")]
        [RequireContext(ContextType.Guild)]
        public class Lotto : ModuleBase
        {
            
            public bool HasPerms()
            {
                
                var User = Context.User as SocketGuildUser;
                var role = Context.Guild.Roles.FirstOrDefault(x => x.Name == "Kingpin");
                return !User.Roles.Contains(role);
            }

            [Command("rules")]
            [Summary("Rules explanation")]
            public async Task Ping()
            {
                await Context.Message.DeleteAsync();
                var builder = new EmbedBuilder();
                builder.WithColor(Color.Green);
                builder.WithThumbnailUrl(
                    @"https://cdn.discordapp.com/attachments/462612844966576128/463385294008483846/image.png");
                builder.WithTitle("This is your mayor speaking!");
                builder.WithDescription(
                    "Welcome to July, this month I want to focus on a Guild Gamble.\n\nI will be collecting a pot of money from everyone participating.\nYou will be giving me one perin in exchange for a raffle ticket.\nAt the end of this month, I will be hosting a raffle.\n\nI will choose one name.\nThat one name will win the jackpot of perins.\nThe more tickets you purchase the higher chance of winning.\nEveryone can participate. Kingpins through Guest.\nYou will be trading a kingpin your Perin for a ticket on discord.\n\n\nAt any time, you will be able to see the participants and the amount of tickets they have.\ntype ~help for more information!");
                await ReplyAsync("", false, builder.Build());
            }
            [Command("Add"), Priority(0)]
            [Summary("Add Lottery tickets to a user")]
            public async Task Add(SocketGuildUser user, [Remainder] string amount)
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }

                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,

                };
                string description = null;
                bool num = Int32.TryParse(amount, out int result);
                if (!num   || result <= 0)
                {
                    builder.Color = Color.Red;
                    builder.Description = "<:0h:438433957978177539> \nWrong input!";
                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                for (int i = 0; i < result; i++)
                {
                    lottotickets.Add(user.Id);
                }
                var first = Program.LottoList.TicketCount.FirstOrDefault(x => x.Name == user.Id);
                if (first != null)
                {
                    first.Count += result;
                }
                else
                {
                    Program.LottoList.TicketCount.Add(new lotto(user.Id, result));
                }
                WriteToJson(Program.LottoList.TicketCount);
                

                if (user.Nickname == null)
                {
                    description += $"Added {result} <:lotto:463108548348608512> to {user.Username}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                else
                {
                    description += $"Added {result} <:lotto:463108548348608512> to {user.Nickname}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                builder.Description = description;
                await ReplyAsync("", false, builder.Build());

            }

            [Command("Add"), Priority(1)]
            [Summary("Add a single Lottery ticket to a user")]
            public async Task Add(SocketGuildUser user)
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }
                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,
                    
                };
                string description = null;
                lottotickets.Add(user.Id);
                var first = Program.LottoList.TicketCount.FirstOrDefault(x => x.Name == user.Id);
                if (first != null)
                {
                    first.Count += 1;
                }
                else
                {
                    Program.LottoList.TicketCount.Add(new lotto(user.Id, 1));
                }
                WriteToJson(Program.LottoList.TicketCount);

                if (user.Nickname == null)
                {
                    description += $"Added 1 <:lotto:463108548348608512> to {user.Username}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                else
                {
                    description += $"Added 1 <:lotto:463108548348608512> to {user.Nickname}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                builder.Description = description;
                await ReplyAsync("", false, builder.Build());
            }



            [Command("Clear"), Priority(0)]
            [Summary("Clear current Lottery entries")]
            public async Task Clear()
            {
                //:wrench: 
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }
                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,
                };
                string description = null;
                lottotickets.Clear();
                Program.LottoList.TicketCount.Clear();
                WriteToJson(Program.LottoList.TicketCount);
                description += $"Lottery entries cleared!";
                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = ":wrench:";
                        x.Value = description;
                        x.IsInline = false;
                    });
                    await ReplyAsync("", false, builder.Build());
                }
            }


            [Command("Clear"), Priority(1)]
            [Summary("Clear user's Lottery Tickets")]
            public async Task Clear(SocketGuildUser user)
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }
                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,
                };
                string description = null;
                lottotickets.RemoveAll(x => x == user.Id);
                Program.LottoList.TicketCount.RemoveAll(x => x.Name == user.Id);
                WriteToJson(Program.LottoList.TicketCount);
                description += $"Cleared {user.Mention} Entries!";
                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = ":wrench:";
                        x.Value = description;
                        x.IsInline = false;
                    });
                    await ReplyAsync("", false, builder.Build());
                }
            }


            [Command("Sub"), Priority(0)]
            [Summary("Subtract Lottery tickets from a user")]
            public async Task Sub(SocketGuildUser user, [Remainder] string amount)
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }

                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,

                };
                string description = null;
                bool num = Int32.TryParse(amount, out int result);
                if (!num || result <= 0)
                {
                    builder.Color = Color.Red;
                    builder.Description = "<:0h:438433957978177539> \nWrong input!";
                    await ReplyAsync("", false, builder.Build());
                    return;
                }
                var count = 0;
                for (var i = lottotickets.Count - 1; i >= 0; --i)
                {
                    if (lottotickets[i] == user.Id)
                    {
                        lottotickets.RemoveAt(i);
                        count++;
                    }
                    if (count == result)
                        break;
                }
                var first = Program.LottoList.TicketCount.FirstOrDefault(x => x.Name == user.Id);
                if (first != null)
                {
                    first.Count -= result;
                }
                WriteToJson(Program.LottoList.TicketCount);
                if (user.Nickname == null)
                {
                    description += $"Subtracted {result} <:lotto:463108548348608512> From {user.Username}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                else
                {
                    description += $"Subtracted {result} <:lotto:463108548348608512> From {user.Nickname}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                builder.Description = description;
                await ReplyAsync("", false, builder.Build());

            }

            [Command("Sub"), Priority(1)]
            [Summary("Subtract a single Lottery ticket from a user")]
            public async Task Sub(SocketGuildUser user)
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }
                var builder = new EmbedBuilder()
                {
                    Color = Color.Green,

                };
                string description = null;

                int index = lottotickets.FindLastIndex(x => x == user.Id);
                lottotickets.RemoveAt(index);
                var first = Program.LottoList.TicketCount.FirstOrDefault(x => x.Name == user.Id);
                if (first != null)
                {
                    first.Count -= 1;
                }
                WriteToJson(Program.LottoList.TicketCount);
                if (user.Nickname == null)
                {
                    description += $"Subtracted 1 <:lotto:463108548348608512> to {user.Username}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                else
                {
                    description += $"Subtracted 1 <:lotto:463108548348608512> to {user.Nickname}. \nTotal: {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>";
                }
                builder.Description = description;
                await ReplyAsync("", false, builder.Build());
            }

            [Command("Raffle")]
            [Summary("Start the Lottery raffle")]
            public async Task Raffle()
            {
                if (MidRaffle) return;
                if (!HasPerms())
                {
                    var build = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string desc = null;
                    desc += $"Command reserved for Kingpins!";
                    build.Description = desc;
                    await ReplyAsync("", false, build.Build());
                    return;
                }
                

                if (Program.LottoList.TicketCount.Count <= 1)
                {
                    var builder = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string description = null;
                    description += $"Not enough participants to start the raffle! <:omg:438433957173002242>";
                    builder.Description = description;
                    await ReplyAsync("", false, builder.Build());
                    return;
                }

                MidRaffle = true;
                Random rng = new Random();
                Random list = new Random();
                Random RandomMember = new Random();

                Shuffle(lottotickets);
                var builders = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = $"Total pot: {lottotickets.Count} <:perin:462691285308932098>, Good Luck!"
                };
                string descriptions = $"Starting Lottery in 3...";
                builders.AddField(x =>
                {
                    x.Name = "Start up";
                    x.Value = descriptions;
                    x.IsInline = false;
                });
                var Message = await ReplyAsync("", false, builders.Build());
                Thread.Sleep(1000);
                for (int i = 2; i >= 1; i--)
                {
                    descriptions = $"Starting Lottery in {i}...";

                    if (!string.IsNullOrWhiteSpace(descriptions))
                    {
                        builders.Fields.FirstOrDefault(x => x.Name == "Start up").Value =
                            $"Starting Lottery in {i}...";
                        await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                    }
                    Thread.Sleep(1000);
                }

                builders.Fields.FirstOrDefault(x => x.Name == "Start up").Value = "In Progress...";
                await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                int loops = list.Next(16, 30);
                var next = RandomMember.Next(0, Program.LottoList.TicketCount.Count);
                var messageToDisplay = Program.LottoList.TicketCount[next].Name;
                var mention = Context.Guild.GetUserAsync(messageToDisplay).Result;
                builders.AddField(x =>
                {
                    x.Name = "Raffle";
                    x.Value = mention.Nickname ?? mention.Username;
                    x.IsInline = false;
                });
                await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                for (int i = 0; i <= loops; i++)
                {
                    Thread.Sleep(rng.Next(50, 150));
                    var m = messageToDisplay;
                    while (messageToDisplay == m)
                    {
                        next = RandomMember.Next(0, Program.LottoList.TicketCount.Count);
                        messageToDisplay = Program.LottoList.TicketCount[next].Name;
                    }
                    var ment = Context.Guild.GetUserAsync(messageToDisplay).Result;
                    builders.Fields.FirstOrDefault(x => x.Name == "Raffle").Value =
                        ment.Nickname ?? ment.Username;
                    await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                }
                next = RandomMember.Next(0, lottotickets.Count);
                mention = Context.Guild.GetUserAsync(lottotickets[next]).Result;
                builders.Fields.FirstOrDefault(x => x.Name == "Raffle").Value =
                    mention.Nickname ?? mention.Username;

                await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                Thread.Sleep(800);
                builders.Fields.FirstOrDefault(x => x.Name == "Start up").Value = "Finished";
                builders.AddField(x =>
                {
                    x.Name = ":trophy:";
                    x.Value = $"{mention.Mention} won the lottery! Congratulations! <:rich:438433957755748353>";
                    x.IsInline = false;
                });
                await Message.ModifyAsync(msg => msg.Embed = builders.Build());
                lottotickets.Clear();
                Program.LottoList.TicketCount.Clear();
                WriteToJson(Program.LottoList.TicketCount);
                MidRaffle = false;
            }
            [Command("Display")]
            [Summary("Display the current participants")]
            public async Task Display()
            {
                if (MidRaffle) return;
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = "<:0h:438433957978177539>"
                };
                string description = null;

                if (Program.LottoList.TicketCount.Count == 0)
                {
                    var builders = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string descriptions = null;
                    descriptions += $"No one has entered the lottery.";
                    builder.Description = descriptions;
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    foreach (var x in Program.LottoList.TicketCount)
                    {
                        var mention = Context.Guild.GetUserAsync(x.Name).Result;
                        if (mention.Nickname != null)
                        {
                            description += $"{mention.Nickname} : {x.Count} <:lotto:463108548348608512>" + '\n';
                        }
                        else
                        {
                            description += $"{mention.Username} : {x.Count} <:lotto:463108548348608512>" + '\n';
                        }

                    }

                    description += $"Total pot: {lottotickets.Count}<:perin:462691285308932098>";
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        builder.AddField(x =>
                        {
                            x.Name = "Participants";
                            x.Value = description;
                            x.IsInline = true;
                        });
                    }
                    await ReplyAsync("", false, builder.Build());
                }
            }
            [Command("Display")]
            [Summary("Display user's ticket count")]
            public async Task Display(SocketGuildUser user)
            {
                if (MidRaffle) return;
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = "<:0h:438433957978177539>"
                };
                string description = null;

                if (lottotickets.Count(x => x == user.Id) == 0)
                {
                    var builders = new EmbedBuilder()
                    {
                        Color = Color.Red,
                    };
                    string descriptions = null;
                    if (user.Nickname != null)
                    {
                        descriptions += $"{user.Nickname} has no tickets.";
                    }
                    else
                    {
                        descriptions += $"{user.Username} has no tickets.";
                    }

                    builder.Description = descriptions;
                    await ReplyAsync("", false, builder.Build());
                }
                else
                {
                    if (user.Nickname != null)
                    {
                        description +=
                            $"{user.Nickname} has {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>" +
                            '\n';
                    }
                    else
                    {
                        description +=
                            $"{user.Username} has {lottotickets.Count(x => x == user.Id)} <:lotto:463108548348608512>" +
                            '\n';
                    }

                    builder.Description = description;
                    await ReplyAsync("", false, builder.Build());
                }
            }

            [Command("Display"), Priority(0)]
            [Summary("Display user's ticket count")]
            public async Task Display([Remainder] string amount)
            {
                if (MidRaffle) return;
                var builder = new EmbedBuilder()
                {
                    Color = new Color(114, 137, 218),
                    Description = "<:0h:438433957978177539>"
                };
                string description = null;
                description = "Such user does not exist.";
                builder.Description = description;
                await ReplyAsync("", false, builder.Build());
            }
        }
    }
}
