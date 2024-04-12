using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot.Commands
{
    
    public class Roll : ModuleBase<SocketCommandContext>
    {
        [Command("roll")]
        [Summary("Calculate pools dice from player pool + difficulty + enemy traits")]
        public async Task ExecuteAsync([Remainder][Summary("pool")] string line)
        {
            if (string.IsNullOrEmpty(line))
            {
                await ReplyAsync("not enough params");
                return;
            }

            //its time for some logic
            //i need d12 + d8 + difficulty + traits + special d12 to not scale, so 5  

            string[] strings = line.Split(' ');
            if (strings.Length < 4)
            {
                await ReplyAsync("not enoght params");
                return;
            }

            try
            {
                //get input
                int d12 = int.Parse(strings[0]);
                int d8 = int.Parse(strings[1]);
                int difficulty = int.Parse(strings[2]);
                int traits = int.Parse(strings[3]);

                if (d12 > 6) d12 = 6; // d12 cap
                if (d8 > 6) d8 = 6; // d8 cap

                //remove from d12 and d8 first
                RemoveFromFirstThenSecond(ref d8, ref d12, difficulty);

                //calculate d6 and d4 pools total max of dice is 6
                if (difficulty > 6) difficulty = 6; // difficulty cap
                if (traits > 6) traits = 6; // traits cap

                int d6 = difficulty - traits;
                int d4 = traits;

                while(d6 + d4 > 6)
                {
                    if (d6 > 0)
                        d6--;
                    else
                        d4--;
                }

                if (d6 < 0) d6 = 0;
                if (d4 < 0) d4 = 0;

                // roll dices
                var rnd = new Random();

                int[] d12Results = new int[d12];
                int[] d8Results = new int[d8];
                int[] d6Results = new int[d6];
                int[] d4Results = new int[d4];

                int i;
                for (i = 0; i < d12; ++i)
                {
                    d12Results[i] = rnd.Next(1, 13);
                }
                d12Results = d12Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

                for (i = 0; i < d8; ++i)
                {
                    d8Results[i] = rnd.Next(1, 9);
                }
                d8Results = d8Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

                for (i = 0; i < d6; ++i)
                {
                    d6Results[i] = rnd.Next(1, 7);
                }
                d6Results = d6Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

                for (i = 0; i < d4; ++i)
                {
                    d4Results[i] = rnd.Next(1, 5);
                }
                d4Results = d4Results.OrderByDescending(x => x).Where(x => x > 0).ToArray();

                
                //positive dices
                int d12Max = d12Results.Length > 0 ? d12Results.Max() : 0;
                int d8Max = d8Results.Length > 0 ? d8Results.Max() : 0;
                //negative dices
                int d6Max = d6Results.Length > 0 ? d6Results.Max() : 0;
                int d4Max = d4Results.Length > 0 ? d4Results.Max() : 0;


                bool d12higherThanD8 = d12 >= d8;
                int positiveDiceMax = d12higherThanD8 ? d12Max : d8Max;

                bool d6higherThanD4 = d6 >= d4;
                int negativeDiceMax = d6higherThanD4 ? d6Max : d4Max;

                bool success1 = positiveDiceMax >= 6;
                bool success2 = negativeDiceMax >= 6;

                int triumph = d12Results.Count(x => x == 12);
                int disaster = d4Results.Count(x => x == 1);


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

                message += "Risultati\n\n";
                message += $"D12: {string.Join(" ", d12Results)}\n";
                message += $"D8: {string.Join(" ", d8Results)}\n";
                message += $"D6: {string.Join(" ", d6Results)}\n";
                message += $"D4: {string.Join(" ", d4Results)}\n\n";

                message += $"Trionfi: {triumph}\n";
                message += $"Catastrofi: {disaster}\n";

                await ReplyAsync(message);
            }
            catch(Exception ex)
            {
                await ReplyAsync("Unexpected Error");
                Console.WriteLine(ex.Message);
            }         
        }


        private void RemoveFromFirstThenSecond(ref int a, ref int b, int amountToRemove)
        {
            a = a - amountToRemove;
            if (a < 0)
            {
                b = b + a;
                a = 0;
                if (b < 0) b = 0;
            }
        }
    }
}
