using Discord.Commands;
using KotraBot.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace KotraBot
{
    public static class Cache
    {
        public static ConcurrentDictionary<ulong, DiceRoll> LastRolls = new ConcurrentDictionary<ulong, DiceRoll>(); // user id -> dice pool for caching
        public static ConcurrentDictionary<ulong, DateTime> LastRollsTime = new ConcurrentDictionary<ulong, DateTime>(); // user id -> last roll time for caching expiration

        /// <summary>
        /// this method is used to invalidate cache if it's older than 10 minutes
        /// returns true if cache was invalidated
        /// </summary>
        /// <returns></returns>
        public static bool TryInvalidateCache(ulong userId)
        {
            if (LastRollsTime.ContainsKey(userId))
            {
                if (DateTime.UtcNow - LastRollsTime[userId] > TimeSpan.FromMinutes(10))
                {
                    //cache expired
                    LastRollsTime.TryRemove(userId, out _);
                    LastRolls.TryRemove(userId, out _);
                    return true;
                }
            }
            return false;
        }

        public static bool InvalidateCache(ulong userId)
        {
            if (LastRollsTime.ContainsKey(userId))
            {
                bool @try = LastRollsTime.TryRemove(userId, out _);
                LastRolls.TryRemove(userId, out _);
                return @try;
            }
            return true;
        }

        public static void SetCache(ulong userId, DiceRoll diceRoll)
        {
            if (LastRolls.ContainsKey(userId))
            {
                LastRolls[userId] = diceRoll;
                LastRollsTime[userId] = DateTime.UtcNow;
            }
            else
            {
                LastRolls.TryAdd(userId, diceRoll);
                LastRollsTime.TryAdd(userId, DateTime.UtcNow);
            }
        }

        public static DiceRoll? GetCache(ulong userId)
        {
            TryInvalidateCache(userId);
            if (LastRolls.ContainsKey(userId))
            {
                return LastRolls[userId];
            }
            else
            {
                return null;
            }
        }

    }

    /// <summary>
    /// This class is used to control the cache
    /// its a command module so it can be used to invalidate cache 
    /// </summary>
    public class CacheController : ModuleBase<SocketCommandContext>
    {
        const string USER_ID_REGEX = @"<@!?(\d+)>";

        [Command("invalidate")]
        public async Task InvalidateCache([Remainder] string line)
        {
            if (!Admin.isAdmin(Context.Message.Author.Id))
            {
                await ReplyAsync("You are not authorized to use this command");
                return;
            }


            if (string.IsNullOrWhiteSpace(line))
            {
                await ReplyAsync("Please provide a user id to invalidate cache for");
                return;
            }

            Match? match = System.Text.RegularExpressions.Regex.Match(line, USER_ID_REGEX);

            if (match is null)
            {
                await ReplyAsync("Invalid user id provided");
                return;
            }

            for (int i = 1; i < match.Groups.Count; i++)
            {
                if (ulong.TryParse(match.Groups[i].Value, out ulong userId))
                {
                    if (Cache.InvalidateCache(userId))
                    {
                        await ReplyAsync($"Cache invalidated for user <@{userId}>");
                    }
                    else
                    {
                        await ReplyAsync($"Cache not found for user {userId}");
                    }
                }
            }
        }

    }

}
