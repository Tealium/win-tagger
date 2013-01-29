using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    public class TrackPropertyAttribute : TrackBaseAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class TrackPageViewAttribute : TrackBaseAttribute
    {
        public TrackPageViewAttribute(string pageName)
        {
            this.Value = pageName;
        }
        public string Value { get; set; }
    }

    public class TrackBaseAttribute : Attribute
    {

    }

}
