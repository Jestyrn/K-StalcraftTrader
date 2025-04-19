using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TraderForStalCraft.Scripts
{
    internal class StartingScript
    {
        private static bool _isStarted;
        public static void Start()
        {
            _isStarted = true;
        }

        public static void Stop()
        {
            _isStarted = false;

        }
    }
}
