using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Threading.Tasks;
using Google.Apis.CustomSearchAPI.v1;
using Google.Apis.Services;
using Google.Apis.Auth;
using System.Net.Http;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Runtime.InteropServices;
using static System.Net.WebRequestMethods;
using DiscordBotTemplate.Database;

namespace DiscordBot.Commands
{
    public class Basic : BaseCommandModule
    {

        [Command("anlegen")]

        public async Task StoreCommand(CommandContext ctx)
        {
            var DBEngine = new DBEngine();

            var userInfo = new DUser
            {
                userName = ctx.User.Username,
                serverName = ctx.Guild.Name,
                serverID = ctx.Guild.Id,
                avatarURL = ctx.Member.AvatarUrl,
                level = 1,
                XP = 0,
                xplimit = 100
            };

            var isStored = await DBEngine.StoreUserAsync(userInfo);

            if (isStored)
            {
                await ctx.Channel.SendMessageAsync("Erfolgreich in Datenbank gespeichert");
            }
            else
            {

                await ctx.Channel.SendMessageAsync("Benutzer existiert bereits, bitte prüfen");

                var profileEmbed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Red, 
                    Title = $"{ctx.User.Username}'s Profil",
                    Description = $"Server Name {ctx.Guild.Name}\n"
                                
                };

                await ctx.Channel.SendMessageAsync(profileEmbed);
            }

        }

        [Command ("profile")]

        public async Task Profile(CommandContext ctx)
        {
            var DBEngine = new DBEngine();

            var userToRetrieve = await DBEngine.GetUserAsync(ctx.User.Username, ctx.Guild.Id);


            if (userToRetrieve.Item1)
            {
                var profileEmbed = new DiscordMessageBuilder()
                        .AddEmbed(new DiscordEmbedBuilder()
                            .WithColor(DiscordColor.Gold)
                            .WithTitle($"{userToRetrieve.Item2.userName}'s Profile")
                            .WithThumbnail(userToRetrieve.Item2.avatarURL)
                            .AddField("Level", userToRetrieve.Item2.level.ToString())
                            .AddField("XP", $"{userToRetrieve.Item2.XP} - {userToRetrieve.Item2.xplimit}"));

                await ctx.Channel.SendMessageAsync(profileEmbed);
            }
            else
            {
                await ctx.Channel.SendMessageAsync("Etwas ist beim abrufen ihres Profils schiefgelaufen");
            }
        }


    }
}
