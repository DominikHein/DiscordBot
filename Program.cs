using DiscordBot.Commands;
using DiscordBot.Config;
using DiscordBot.Database;
using DiscordBot.Slash_Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace DiscordBot
{
    public sealed class Program
    {
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }

        private static HashSet<ulong> erstelleKanalIds = new HashSet<ulong>();

        private static Dictionary<ulong, ulong> userChannelMapping = new Dictionary<ulong, ulong>();

        static async Task Main(string[] args)
        {
            var configJsonFile = new JSONReader();
            await configJsonFile.ReadJSON();

            //Bot Config
            var discordConfig = new DiscordConfiguration()
            {
                Intents = DiscordIntents.All,
                Token = configJsonFile.token,
                TokenType = TokenType.Bot,
                AutoReconnect = true
            };

            Client = new DiscordClient(discordConfig);

            //Timeout bei Zeitüberschreitung
            Client.UseInteractivity(new InteractivityConfiguration()
            {
                Timeout = TimeSpan.FromMinutes(2)
            });

            //Event Handler
            Client.Ready += OnClientReady;
            Client.MessageCreated += MessageCreatedHandler;
            Client.VoiceStateUpdated += VoiceStateUpdatedHandler;
            Client.ChannelDeleted += ChannelDeletedEventHandler;
            Client.GuildMemberAdded += GuildMemberAddedEventHandler;




            //Command Konfiguration
            var commandsConfig = new CommandsNextConfiguration()
            {
                StringPrefixes = new string[] { configJsonFile.prefix },
                EnableMentionPrefix = true,
                EnableDms = true,
                EnableDefaultHelp = true,
            };

            Commands = Client.UseCommandsNext(commandsConfig);
            var slashCommandsConfig = Client.UseSlashCommands();

            //Commands Registrieren
            Commands.RegisterCommands<Basic>();
            slashCommandsConfig.RegisterCommands<NewSlashCommands>(1137465830595104860);


            //Verbinden
            await Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async Task GuildMemberAddedEventHandler(DiscordClient sender, GuildMemberAddEventArgs e)
        {
            var newUser = e.Member.Username;

            var DBEngine = new DBEngine();

            var userInfo = new DUser
            {
                userName = e.Member.Username,
                serverName = e.Guild.Name,
                serverID = e.Guild.Id,
                avatarURL = e.Member.AvatarUrl,
                level = 1,
                XP = 0,
                xplimit = 100
            };

            var isStored = await DBEngine.StoreUserAsync(userInfo);

        }

        private static async Task ChannelDeletedEventHandler(DiscordClient sender, ChannelDeleteEventArgs e)
        {
            Console.WriteLine("Kanal gelöscht");
            var targetChannelTask = Client.GetChannelAsync(1219857307836354731);
            var targetChannel = await targetChannelTask;
            if (e.Channel.Type == ChannelType.Voice)
            {
                await targetChannel.SendMessageAsync($"Der Voice Kanal '{e.Channel.Name}' wurde gelöscht");
                if (userChannelMapping.ContainsKey(e.Channel.Id))
                {
                    userChannelMapping.Remove(e.Channel.Id);
                };
            }
            else
            {
                Console.WriteLine($"Channel {e.Channel.Name} wurde gelöscht");
                if (targetChannel is DiscordChannel discordchannel)
                {
                    await targetChannel.SendMessageAsync($"Der Text Kanal '{e.Channel.Name}' wurde gelöscht");
                }

            };

        }

        //Kanal erstellen durch verbinden in ein anderen Kanal
        private static async Task VoiceStateUpdatedHandler(DiscordClient sender, VoiceStateUpdateEventArgs e)
        {
            //Erstellt Kanal wenn Nutzer noch kein anderen erstellt hat 
            if (e.Before == null && e.Channel.Name == "Create" && !userChannelMapping.ContainsValue(e.User.Id))
            {
                var everyoneRoleId = e.Guild.EveryoneRole;
                //Nutzer der den Channel betreten hat bekommen
                var member = e.Guild.Members.FirstOrDefault(kv => kv.Value.Id == e.User.Id).Value;
                //Kanal erstellen
                var newChannel = await e.Guild.CreateChannelAsync($"{e.User.Username}'s Channel", ChannelType.Voice);
                //User in eigenen Channel verschieben
                await member.ModifyAsync(properties => properties.VoiceChannel = newChannel);

                //Channel privat machen, Zugriff jedem außer Admins und Mods und ersteller Verweigern 
                //Ersteller bekommt rechte diesen Kanal zu editieren
                await newChannel.AddOverwriteAsync(e.Guild.EveryoneRole, Permissions.None, Permissions.All);
                await newChannel.AddOverwriteAsync(member, Permissions.ManageChannels | Permissions.All);
                //Nutzer in Dictionary schreiben damit er nicht unendlich Channel erstellen kann
                userChannelMapping.Add(newChannel.Id, e.User.Id);
            };
            //throw new NotImplementedException();
        }

        //Basically Log Funktion
        private static async Task MessageCreatedHandler(DiscordClient sender, MessageCreateEventArgs e)
        {
            var DBEngine = new DBEngine();
            var userToCheck = await DBEngine.GetUserAsync(e.Author.Username, e.Guild.Id);

            if (userToCheck.Item1)
            {


            if (e.Author.IsBot == false)
            {

                var messageToSend = $"{e.Author.Username} hat folgende Nachricht geschrieben: {e.Message.Content}";
                var targetChannelTask = Client.GetChannelAsync(1184849207115399220);
                var targetChannel = await targetChannelTask;
                var user = await Client.GetUserAsync(114462794102865921);


                await DBEngine.AddXpAsync(e.Author.Username, e.Guild.Id);

                var userLevel = userToCheck.Item2.level;



                if (userToCheck.Item2.XP >= userToCheck.Item2.xplimit)
                {
                    await DBEngine.LevelUpAsync(e.Author.Username, e.Guild.Id);
                    userLevel++;
                    Console.WriteLine(userLevel);
                }

                if (DBEngine.isLevelledUp == true)
                {
                    var levelledUpEmbed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Green,
                        Title = $"{e.Author.Username} ist im level aufgestiegen",
                        Description = $"Level: {userLevel}",
                    };

                    var member = await e.Guild.GetMemberAsync(e.Author.Id);


                    switch (userLevel)
                    {
                        case 5:
                            var role = e.Guild.Roles.FirstOrDefault(r => r.Value.Name == "Level 5");
                            await member.GrantRoleAsync(role.Value);
                            break;
                    }

                    await targetChannel.SendMessageAsync(levelledUpEmbed);

                    DBEngine.isLevelledUp = false;
                }


                if (targetChannel is DiscordChannel discordChannel)
                {
                    // Sende die Nachricht in den Zielkanal
                    await discordChannel.SendMessageAsync(messageToSend);
                    
                }
                else
                {
                    // Handle den Fall, wenn der Zielkanal nicht vom richtigen Typ ist
                    Console.WriteLine($"Fehler: Der Zielkanal ist nicht vom Typ DiscordChannel. Typ: {targetChannel?.GetType().FullName}");
                }

            }
            }

            //throw new NotImplementedException();
        }

        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            Console.WriteLine("Beep Boop Connected");
            return Task.CompletedTask;

        }
    }
}
