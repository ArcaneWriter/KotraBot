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
        public static ConcurrentDictionary<ulong, DiceRoll> lastRolls = new ConcurrentDictionary<ulong, DiceRoll>(); // user id -> dice pool for caching
        public static ConcurrentDictionary<ulong, DateTime> lastRollsTime = new ConcurrentDictionary<ulong, DateTime>(); // user id -> last roll time for caching expiration

        /// <summary>
        /// this method is used to invalidate cache if it's older than 10 minutes
        /// returns true if cache was invalidated
        /// </summary>
        /// <returns></returns>
        public static bool TryInvalidateCache(ulong userId)
        {
            if (lastRollsTime.ContainsKey(userId))
            {
                if (DateTime.UtcNow - lastRollsTime[userId] > TimeSpan.FromMinutes(10))
                {
                    //cache expired
                    lastRollsTime.TryRemove(userId, out _);
                    lastRolls.TryRemove(userId, out _);
                    return true;
                }
            }
            return false;
        }

        public static bool InvalidateCache(ulong userId)
        {
            if (lastRollsTime.ContainsKey(userId))
            {
                bool @try = lastRollsTime.TryRemove(userId, out _);
                lastRolls.TryRemove(userId, out _);
                return @try;
            }
            return true;
        }

        public static void SetCache(ulong userId, DiceRoll diceRoll)
        {
            if (lastRolls.ContainsKey(userId))
            {
                lastRolls[userId] = diceRoll;
                lastRollsTime[userId] = DateTime.UtcNow;
            }
            else
            {
                lastRolls.TryAdd(userId, diceRoll);
                lastRollsTime.TryAdd(userId, DateTime.UtcNow);
            }
        }

        public static DiceRoll? GetCache(ulong userId)
        {
            TryInvalidateCache(userId);
            if (lastRolls.ContainsKey(userId))
            {
                return lastRolls[userId];
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
            if (!Admin.IsAdmin(Context.Message.Author.Id))
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

            if (match is null || match.Groups.Count <= 1) // no user id provided
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
