using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    internal class TealiumStatusLog
    {
        internal static void Information(string message)
        {
            SendToDebug(message);
        }

        internal static void Warning(string message)
        {

            SendToDebug(message);
            if (Debugger.IsAttached)
                Debugger.Break(); //there was an issue, please investigate the message to resolve
        }

        internal static void Error(string message)
        {
            SendToDebug(message);
            if (Debugger.IsAttached)
                Debugger.Break();//there was an issue, please investigate the message to resolve
        }

        private static void SendToDebug(string message)
        {
            Debug.WriteLine("##TEALIUM TAGGER: {0} ##", message);
        }
    }
}
