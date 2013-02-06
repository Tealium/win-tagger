using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    public class TealiumTaggerLoadException : Exception
    {
        public TealiumTaggerLoadException(string message) : base(message)
        {
        }
    }

    public class TealiumTaggerLoggingException : Exception
    {
        public TealiumTaggerLoggingException(string message) : base(message)
        {
        }
    }

    public class TealiumConfigurationException : Exception
    {
        public TealiumConfigurationException(string message)
            : base(message)
        {
        }
    }

}
