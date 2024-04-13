using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot
{
    /// <summary>
    /// wrapper around secrets.json admin list
    /// </summary>
    public static class Admin
    {
        private static ulong[] _adminIds;

        /// <summary>
        /// given a user id, check if they are an admin
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static bool IsAdmin(ulong id)
        {
            if (_adminIds is not null)
            {
                return _adminIds.Contains(id);
            }
            return false;
        }

        /// <summary>
        /// set the admin ids
        /// </summary>
        /// <param name="adminIds">IDs list</param>
        public static void Init(ulong[] adminIds)
        {
            Admin._adminIds = adminIds;
        }
    }
}
