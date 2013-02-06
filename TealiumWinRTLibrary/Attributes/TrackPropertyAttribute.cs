﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tealium
{
    /// <summary>
    /// Associates a name/value pair for any reported events on this page.
    /// </summary>
    public class TrackPropertyAttribute : TrackBaseAttribute
    {
        public TrackPropertyAttribute(string name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        /// <summary>
        /// The name of the Tealium tracking variable to report.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The value of the property to report to Tealium.
        /// </summary>
        public string Value { get; set; }
    }
        
}
