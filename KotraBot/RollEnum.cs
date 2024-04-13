using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KotraBot
{
    public struct DicePool
    {
        public int d12;
        public int d8;
        public int difficulty;
        public int traits;
    }

    public struct DiceResult
    {
        public int index;
        public int size;
        public int result;
    }

    public struct DiceRoll
    {
        public DicePool pool;
        public DiceResult[] results;
        public int triumph;
        public int disaster;
        public bool success1;
        public bool success2;
    }


}
