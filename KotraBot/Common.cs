using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot
{
    public static class Common
    {

        public static bool ToBool(this object o)
        {
            try
            {
                if (o is null)
                    return false;

                if (o is string)
                {
                    return bool.Parse(o.ToString().ToLower());
                }
                else if (o is bool)
                {
                    return (bool)o;
                }
                else if (o is int)
                {
                    return (int)o != 0;
                }
                else if (o is long)
                {
                    return (long)o != 0;
                }
                else if (o is double)
                {
                    return (double)o != 0;
                }
                else if (o is float)
                {
                    return (float)o != 0;
                }
                else if (o is decimal)
                {
                    return (decimal)o != 0;
                }
                else
                {
                    throw new Exception($"Cannot convert {o.GetType().Name} to bool");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
       
    }
}
