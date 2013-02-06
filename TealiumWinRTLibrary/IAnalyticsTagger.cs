using System;
namespace Tealium
{
    interface IAnalyticsTagger
    {
        void SetVariable(string name, string value);
        void SetVariables(System.Collections.IDictionary variables);
        void TrackCustomEvent(string eventName, System.Collections.IDictionary variables);
        void TrackItemClicked(string itemName, System.Collections.IDictionary variables = null);
        void TrackScreenViewed(string viewName, System.Collections.IDictionary variables = null);
    }
}
