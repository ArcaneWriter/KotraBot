using Discord.Commands;
using Discord.Interactions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot.Commands
{


    public class RollBase : ModuleBase<SocketCommandContext>
    {
        protected void RemoveFromFirstThenSecond(ref int a, ref int b, int amountToRemove, out int bin)
        {
            a = a - amountToRemove;
            bin = 0;
            if (a < 0)
            {
                b = b + a;
                a = 0;
                if (b < 0)
                {
                    bin = Math.Abs(b); // how couldn't be removed
                    b = 0;
                }
            }
        }

        protected static void FormatResultTable(DiceResult[] results, ref string message)
        {
            var titleline = new StringBuilder();
            var middleline = new StringBuilder();
            var bottomline = new StringBuilder();

            if (results.Any(r => r.size == 12))
            {
                titleline.Append($"D12");
                foreach (var roll in results.Where(r => r.size == 12))
                {
                    middleline.Append($"{roll.result.ToString().PadRight(2)} ");
                    bottomline.Append($"{roll.index.ToString().PadRight(2)} ");
                }
                middleline.Append("    ");
                bottomline.Append("    ");
            }

            int tmp = middleline.Length - titleline.Length - 1;
            titleline.Append(new string(' ', tmp));

            if (results.Any(r => r.size == 8))
            {
                titleline.Append("|D8");
                foreach (var roll in results.Where(r => r.size == 8))
                {
                    middleline.Append($"{roll.result.ToString().PadRight(2)} ");
                    bottomline.Append($"{roll.index.ToString().PadRight(2)} ");
                }
                middleline.Append("    ");
                bottomline.Append("    ");
            }
            tmp = middleline.Length - titleline.Length - 1;
            titleline.Append(new string(' ', tmp));

            if (results.Any(r => r.size == 6))
            {
                titleline.Append("|D6");
                foreach (var roll in results.Where(r => r.size == 6))
                {
                    middleline.Append($"{roll.result.ToString().PadRight(2)} ");
                    bottomline.Append($"{roll.index.ToString().PadRight(2)} ");
                }
                middleline.Append("    ");
                bottomline.Append("    ");
            }
            tmp = middleline.Length - titleline.Length - 1;
            titleline.Append(new string(' ', tmp));

            if (results.Any(r => r.size == 4))
            {
                titleline.Append("|D4");
                foreach (var roll in results.Where(r => r.size == 4))
                {
                    middleline.Append($"{roll.result.ToString().PadRight(2)} ");
                    bottomline.Append($"{roll.index.ToString().PadRight(2)} ");
                }
                middleline.Append("    ");
                bottomline.Append("    ");
            }
            tmp = middleline.Length - titleline.Length - 1;
            titleline.Append(new string(' ', tmp));

            message += "```c#\n";
            message += titleline.ToString() + "\n";
            message += middleline.ToString() + " risultati \n";
            message += bottomline.ToString() + " indici \n";

            message += "```\n";
        }

        protected DiceRoll RollDice(DicePool pool)
        {
            int d12 = pool.d12;
            int d8 = pool.d8;
            int difficulty = pool.difficulty;
            int traits = pool.traits;
            bool _override = pool.@override;


            if (d12 > 6) d12 = 6; // d12 cap
            if (d8 > 6) d8 = 6; // d8 cap
            if (difficulty > 6) difficulty = 6; // difficulty cap

            //remove from d12 and d8 first
            RemoveFromFirstThenSecond(ref d8, ref d12, difficulty, out int remainder);

            //calculate d6 and d4 pools total max of dice is 6
            if (traits > 6) traits = 6; // traits cap

            int d6 = (difficulty - remainder) - traits; //todo get bin from Remove 
            int d4 = difficulty > traits ? traits : difficulty;

            if (d6 < 0) d6 = 0;
            if (d4 < 0) d4 = 0;

            while (d6 + d4 > 6)
            {
                if (d6 > 0)
                    d6--;
                else
                    d4--;
            }

            // roll dices
            var rnd = new Random();

            int[] d12Results = new int[d12];
            int[] d8Results = new int[d8];
            int[] d6Results = new int[d6];
            int[] d4Results = new int[d4];

            int i;
            var results = new DiceResult[d12 + d8 + d6 + d4];

            for (i = 0; i < d12; ++i)
            {
                d12Results[i] = rnd.Next(1, 13);
            }
            d12Results = d12Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

            int indexer = 0;
            foreach (int result in d12Results)
            {
                var roll = new DiceResult
                {
                    index = indexer,
                    size = 12,
                    result = result
                };
                results[indexer] = roll;
                indexer++;
            }

            for (i = 0; i < d8; ++i)
            {
                d8Results[i] = rnd.Next(1, 9);
            }
            d8Results = d8Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

            foreach (int result in d8Results)
            {
                var roll = new DiceResult
                {
                    index = indexer,
                    size = 8,
                    result = result
                };
                results[indexer] = roll;
                indexer++;
            }

            for (i = 0; i < d6; ++i)
            {
                d6Results[i] = rnd.Next(1, 7);
            }
            d6Results = d6Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

            foreach (int result in d6Results)
            {
                var roll = new DiceResult
                {
                    index = indexer,
                    size = 6,
                    result = result
                };
                results[indexer] = roll;
                indexer++;
            }

            for (i = 0; i < d4; ++i)
            {
                d4Results[i] = rnd.Next(1, 5);
            }
            d4Results = d4Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

            foreach (int result in d4Results)
            {
                var roll = new DiceResult
                {
                    index = indexer,
                    size = 4,
                    result = result
                };
                results[indexer] = roll;
                indexer++;
            }

            return CalculateResults(ref d12Results, ref d8Results, ref d6Results, ref d4Results, ref results, ref pool, pool.@override);
        }

        /// <summary>
        /// Calculate the results of the dice rolls
        /// </summary>
        /// <param name="d12Results"></param>
        /// <param name="d8Results"></param>
        /// <param name="d6Results"></param>
        /// <param name="d4Results"></param>
        /// <param name="results"></param>
        /// <param name="pool"></param>
        /// <returns></returns>
        public static DiceRoll CalculateResults(ref int[] d12Results, ref int[] d8Results, ref int[] d6Results, ref int[] d4Results, ref DiceResult[] results, ref DicePool pool, bool _override)
        {
            //detailed explanation of what happens to calculate results
            // 1. get the max value of the positive dices
            // 2. get the max value of the negative dices
            // 3. check if at least one dice is >= 6 for both the positive and negative dices
            // 4. if only one of the two is >= 6 then it's a partial success, if both are >= 6 then it's a full success, else it's a failure
            // 5. count the number of 12s in the positive dices, these are triumphs
            // 6. count the number of 1s in the negative dices, these are disasters

            //positive dices
            int d12Max = d12Results.Length > 0 ? d12Results.Max() : 0;
            int d8Max = d8Results.Length > 0 ? d8Results.Max() : 0;
            //negative dices
            int d6Max = d6Results.Length > 0 ? d6Results.Max() : 0;
            int d4Max = d4Results.Length > 0 ? d4Results.Max() : 0;


            bool d12higherThanD8 = d12Max >= d8Max;
            int positiveDiceMax = d12higherThanD8 ? d12Max : d8Max;

            bool d6higherThanD4 = d6Max >= d4Max;
            int negativeDiceMax = d6higherThanD4 ? d6Max : d4Max;

            bool success1 = positiveDiceMax >= 6;
            bool success2 = negativeDiceMax >= 6;

            int triumph = 0;
            int disaster = 0;

            if (!_override)
            {
                var positiveResults = d12Results.Concat(d8Results).ToArray();
                triumph = positiveResults.Count(x => x == 12);

                var negativeResults = d6Results.Concat(d4Results).ToArray();
                disaster = negativeResults.Count(x => x == 1);
            }
            else
            {
                //in override mode we calculate the triumphs and disasters from the results
                triumph = results.Where(r => r.result >= 6).Count();
                disaster = results.Where(r => r.result < 6).Count();
            }

            var diceRoll = new DiceRoll
            {
                pool = pool,
                results = results,
                triumph = triumph,
                disaster = disaster,
                success1 = success1,
                success2 = success2
            };

            return diceRoll;
        }

        public static string ElaborateResults(ref DiceRoll rollResult)
        {
            var results = rollResult.results;
            var success1 = rollResult.success1;
            var success2 = rollResult.success2;
            var triumph = rollResult.triumph;
            var disaster = rollResult.disaster;

            string message = "";
            if (success1 && success2)
            {
                message = "Risultato: Successo pieno!\n\n";
            }
            else if (success1 || success2)
            {
                message = "Risultato: Successo parziale!\n\n";
            }
            else
            {
                message = "Risultato: Fallimento!\n\n";
            }

            message += "Risultati\n";


            results = results.OrderByDescending(x => x.size).ThenByDescending(x => x.result).ToArray();

            FormatResultTable(results, ref message);

            message += $"Trionfi: {triumph}\n";
            message += $"Catastrofi: {disaster}\n";

            return message;
        }
    }

    public class Roll : RollBase
    {

        [Command("roll")]
        public async Task ExecuteAsync([Remainder][Discord.Commands.Summary("pool")] string line)
        {
            Cache.TryInvalidateCache(Context.Message.Author.Id);

            if (string.IsNullOrEmpty(line))
            {
                await ReplyAsync("not enough params");
                return;
            }

            //its time for some logic
            //i need d12 + d8 + difficulty + traits + special d12 to not scale, so 5  

            string[] strings = line.Split(' ');
            if (strings.Length < 2)
            {
                await ReplyAsync("not enoght params");
                return;
            }

            try
            {
                //get input
                int d12 = int.Parse(strings[0]);
                int d8 = int.Parse(strings[1]);
                int difficulty = int.Parse(strings.Length >= 3 ? strings[2] : "0");
                int traits = int.Parse(strings.Length >= 3 ? strings[3] : "0");
                
                string fithArg = strings.Length > 4 ? strings[4].Trim() : "false";
                bool _override = fithArg.ToBool();


                DicePool pool = new DicePool
                {
                    d12 = d12,
                    d8 = d8,
                    difficulty = difficulty,
                    traits = traits,
                    @override = _override
                };

                var rollResult = RollDice(pool);

                string message = ElaborateResults(ref rollResult);
                await ReplyAsync(message);

                var diceRoll = new DiceRoll
                {
                    pool = pool,
                    results = rollResult.results
                };

                Cache.SetCache(Context.Message.Author.Id ,diceRoll);
            }
            catch (Exception ex)
            {
                await ReplyAsync("Unexpected Error");
                Console.WriteLine(ex.Message);
            }
        }



    }

    public class Reroll : RollBase
    {

        [Command("reroll")]
        public async Task ExecuteAsync([Remainder][Discord.Commands.Summary("pool")] string line)
        {
            if (Cache.TryInvalidateCache(Context.Message.Author.Id))
            {
                await ReplyAsync("Last roll to old to be used");
                return;
            }

            var lastRoll = Cache.GetCache(Context.Message.Author.Id);
            if (lastRoll is null)
            {
                await ReplyAsync("No last roll found");
                return;
            }

            try
            {
                if (line.Trim().ToLower() == "all")
                {
                    var pool = lastRoll.Value.pool;
                    var rollResult = RollDice(pool);
                    string message = ElaborateResults(ref rollResult);
                    await ReplyAsync(message);

                    var diceRoll = new DiceRoll
                    {
                        pool = pool,
                        results = rollResult.results
                    };

                    Cache.SetCache(Context.Message.Author.Id, diceRoll);
                }
                else
                {
                    List<int> indexesToReroll = new List<int>();
                    string[] strings = line.Split(' ');
                    foreach (var str in strings)
                    {
                        if (int.TryParse(str, out int index))
                        {
                            indexesToReroll.Add(index);
                        }
                    }

                    if (indexesToReroll.Count == 0)
                    {
                        await ReplyAsync("No indexes to reroll");
                        return;
                    }
                    else
                    {
                        await ReplyAsync($"Rerolling indexes: {string.Join(", ", indexesToReroll)} ...");
                    }

                    var resultsToReroll = lastRoll.Value.results.Where(x => indexesToReroll.Contains(x.index))?.ToArray(); // get results to reroll
                    if (resultsToReroll is null || resultsToReroll.Count() == 0)
                    {
                        await ReplyAsync("No valid indexes to reroll");
                        return;
                    }

                    var validResults = lastRoll.Value.results.Where(x => !indexesToReroll.Contains(x.index)).ToArray(); // get results to keep

                    if (validResults is null)
                    {
                        validResults = new DiceResult[0];
                    }


                    var rnd = new Random();
                    for (int i = 0; i < resultsToReroll.Length; ++i)
                    {
                        var result = resultsToReroll[i];
                        switch (result.size)
                        {
                            case 12:
                                result.result = rnd.Next(1, 13);
                                break;
                            case 8:
                                result.result = rnd.Next(1, 9);
                                break;
                            case 6:
                                result.result = rnd.Next(1, 7);
                                break;
                            case 4:
                                result.result = rnd.Next(1, 5);
                                break;
                        }
                        resultsToReroll[i] = result;
                    }

                    var newResults = validResults.Concat(resultsToReroll).ToArray();
                    newResults = newResults.OrderByDescending(x => x.size).ThenByDescending(x => x.result).ToArray();

                    int indexer = 0;
                    for (int i = 0; i < newResults.Length; ++i)
                    {
                        newResults[i].index = indexer;
                        indexer++;
                    }

                    int[] d12results = newResults.Where(x => x.size == 12).Select(x => x.result).ToArray();
                    int[] d8results = newResults.Where(x => x.size == 8).Select(x => x.result).ToArray();
                    int[] d6results = newResults.Where(x => x.size == 6).Select(x => x.result).ToArray();
                    int[] d4results = newResults.Where(x => x.size == 4).Select(x => x.result).ToArray();

                    DiceRoll dRoll = lastRoll.Value;
                    var rollResult = CalculateResults(ref d12results, ref d8results, ref d6results, ref d4results, ref newResults, ref dRoll.pool, dRoll.pool.@override);

                    string message = ElaborateResults(ref rollResult);
                    await ReplyAsync(message);

                    Cache.SetCache(Context.Message.Author.Id, rollResult);
                }
            }
            catch (Exception ex)
            {
                await ReplyAsync("Unexpected Error");
                Console.WriteLine(ex.Message);
            }
        }
    }
}
