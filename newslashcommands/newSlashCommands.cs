using DSharpPlus.SlashCommands;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.Interactivity.Extensions;
using Emzi0767;
using System;

namespace DiscordBot.Slash_Commands
{
    public class NewSlashCommands : ApplicationCommandModule
    {

        [SlashCommand("poll", "Umfrage erstellen")]

        public async Task slashPoll(InteractionContext ctx, [Option("Frage", "Umfrage Thema")] string Question,
                                                            [Option("Zeitlimit", "Zeitlimit")] long TimeLimit,
                                                            [Option("option1", "Option 1")] string Option1,
                                                            [Option("option2", "Option 2")] string Option2,
                                                            [Option("option3", "Option 3")] string Option3,
                                                            [Option("option4", "Option 4")] string Option4)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder()
                                                                                .WithContent("..."));

            var interactvity = Program.Client.GetInteractivity(); //Holen des Interaktivitätsmodul 
            TimeSpan timer = TimeSpan.FromSeconds(TimeLimit); //Zeitlimit in TimeSpan Objekt Converten

            DiscordEmoji[] optionEmojis = { DiscordEmoji.FromName(Program.Client, ":one:", false),
                                            DiscordEmoji.FromName(Program.Client, ":two:", false),
                                            DiscordEmoji.FromName(Program.Client, ":three:", false),
                                            DiscordEmoji.FromName(Program.Client, ":four:", false) }; //Array zum speichern der Emoji

            string optionsString = optionEmojis[0] + " | " + Option1 + "\n" +
            optionEmojis[1] + " | " + Option2 + "\n" +
            optionEmojis[2] + " | " + Option3 + "\n" +
                                   optionEmojis[3] + " | " + Option4; //Emoji den Optionen zuordnen

            var pollMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithTitle(string.Join(" ", Question))
                    .WithDescription(optionsString)); //Umfrage Nachricht erstellen

            var putReactOn = await ctx.Channel.SendMessageAsync(pollMessage); 

            foreach (var emoji in optionEmojis)
            {
                await putReactOn.CreateReactionAsync(emoji); //Auf Nachricht "reagiern" 
            }

            var result = await interactvity.CollectReactionsAsync(putReactOn, timer); //Votes sammlen

            int count1 = 0; 
            int count2 = 0;
            int count3 = 0;
            int count4 = 0;

            foreach (var emoji in result) //Auszählen
            {
                if (emoji.Emoji == optionEmojis[0])
                {
                    count1++;
                }
                if (emoji.Emoji == optionEmojis[1])
                {
                    count2++;
                }
                if (emoji.Emoji == optionEmojis[2])
                {
                    count3++;
                }
                if (emoji.Emoji == optionEmojis[3])
                {
                    count4++;
                }
            }

            int totalVotes = count1 + count2 + count3 + count4; 

            string resultsString = optionEmojis[0] + ": " + count1 + " Votes \n" +
                                   optionEmojis[1] + ": " + count2 + " Votes \n" +
                                   optionEmojis[2] + ": " + count3 + " Votes \n" +
                                   optionEmojis[3] + ": " + count4 + " Votes \n\n" +
                                   "Insgesamt wurde " + totalVotes + " mal abgestimmt"; //Ergebnis wird in String gespeichert

            var resultsMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Green)
                    .WithTitle("Ergebnise der Umfrage")
                    .WithDescription(resultsString));

            await ctx.Channel.SendMessageAsync(resultsMessage); //Embed erstellen und anzeigen            
        }

        [SlashCommand("bildEmbed", "ueberschrift")]
        public async Task CaptionCommand(InteractionContext ctx, [Option("Überschrift", "Überschrift für die Embed Nachricht")] string caption,
                                                                 [Option("Bild", "Bild hochladen")] DiscordAttachment picture)
        {
            await ctx.DeferAsync();

            var captionMessage = new DiscordMessageBuilder()
                .AddEmbed(new DiscordEmbedBuilder()
                    .WithColor(DiscordColor.Azure)
                    .WithFooter(caption)
                    .WithImageUrl(picture.Url)
                    );

            await ctx.EditResponseAsync(new DiscordWebhookBuilder().AddEmbed(captionMessage.Embed));
        }
    }


}

