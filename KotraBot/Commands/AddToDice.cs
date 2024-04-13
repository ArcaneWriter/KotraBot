using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot.Commands
{

    public class AddToDice : ModuleBase<SocketCommandContext>
    {

        [Command("add")]
        public async Task ExecuteAsync([Remainder][Discord.Commands.Summary("pool")] string line)
        {
            var rollResult =  Cache.GetCache(Context.Message.Author.Id);

            if (rollResult is null)
            {
                await ReplyAsync("No valid last roll found to add bonus\n roll a new pool");
                return;
            }

            string[] args = line.Split('|'); // fist part bonus to add, second part index to add to

            if (args.Length != 2)
            {
                await ReplyAsync("Invalid input, please use the format !add bonus|indexes");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[0]))
            {
                await ReplyAsync("Invalid bonus input, please use the format !add bonus|indexes");
                return;
            }

            if (string.IsNullOrWhiteSpace(args[1]))
            {
                await ReplyAsync("Invalid index input, please use the format !add bonus|indexes");
                return;
            }



            try
            {
                args[0] = args[0].Trim();
                args[1] = args[1].Trim();

                string[] tempInd = args[1].Split(' ');
                int[] indexes = new int[tempInd.Length];

                for (int i = 0; i < tempInd.Length; i++)
                {
                    if (!int.TryParse(tempInd[i], out indexes[i]))
                    {
                        await ReplyAsync("Invalid index input, please use the format !add bonus|indexes");
                        return;
                    }
                }

                int bonus = 0;
                if (!int.TryParse(args[0], out bonus))
                {
                    await ReplyAsync("Invalid bonus input, please use the format !add bonus|indexes");
                    return;
                }

                for (int i = 0; i < rollResult.Value.results.Length; ++i)
                {
                    int index = rollResult.Value.results[i].index;
                    if (indexes.Contains(index)) rollResult.Value.results[i].result += bonus;
                }

                var res = rollResult.Value;

                int[] d12Results = res.results.Where(x => x.size == 12).Select(x => x.result).ToArray();
                int[] d8Results = res.results.Where(x => x.size == 8).Select(x => x.result).ToArray();
                int[] d6Results = res.results.Where(x => x.size == 6).Select(x => x.result).ToArray();
                int[] d4Results = res.results.Where(x => x.size == 4).Select(x => x.result).ToArray();
                DiceResult[] results = res.results;
                DicePool pool = res.pool;

                var diceRoll = RollBase.CalculateResults(ref d12Results, ref d8Results, ref d6Results, ref d4Results, ref results, ref pool, pool.@override);
                string message = RollBase.ElaborateResults(ref diceRoll);
                rollResult = diceRoll;
                await ReplyAsync(message);

                Cache.SetCache(Context.Message.Author.Id, diceRoll); // update cache
            }
            catch (Exception e)
            {
                await ReplyAsync("unexpected error");
                Console.WriteLine(e.Message);
            }
        }

    }
}
