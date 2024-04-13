﻿using Discord.Commands;
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
            var RollResult =  Cache.GetCache(Context.Message.Author.Id);

            if (RollResult is null)
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

                for (int i = 0; i < RollResult.Value.results.Length; ++i)
                {
                    int index = RollResult.Value.results[i].index;
                    if (indexes.Contains(index)) RollResult.Value.results[i].result += bonus;
                }

                var res = RollResult.Value;

                int[] d12Results = res.results.Where(x => x.size == 12).Select(x => x.result).ToArray();
                int[] d8Results = res.results.Where(x => x.size == 8).Select(x => x.result).ToArray();
                int[] d6Results = res.results.Where(x => x.size == 6).Select(x => x.result).ToArray();
                int[] d4Results = res.results.Where(x => x.size == 4).Select(x => x.result).ToArray();
                DiceResult[] results = res.results;
                DicePool pool = res.pool;

                var diceRoll = RollBase.CalculateResults(ref d12Results, ref d8Results, ref d6Results, ref d4Results, ref results, ref pool);
                string message = RollBase.ElaborateResults(ref diceRoll);
                RollResult = diceRoll;
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
