using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot
{
    public static class Admin
    {
        private static ulong[] AdminIds;

        public static bool isAdmin(ulong id)
        {
            if (AdminIds is not null)
            {
                return AdminIds.Contains(id);
            }
            return false;
        }

        public static void Init(ulong[] adminIds)
        {
            Admin.AdminIds = adminIds;
        }
    }
}
