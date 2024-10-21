using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameJamExample
{
    public class Event
    {
        public string eventId = "-1"; // Default value to indicate one wasn't set
        public string originText;
        public List<string> options;
        public List<string> optionsNextEvents; // What events we'll look for, to happen when given options are chosen

        public enum EventSpecialCondition { None, Death, Win }; // Required for the property below
        public EventSpecialCondition evSpCon; // Used to determine whether we've won or lost, when events with no options & this property set (to other than 'None') occurs

        // NOTE Last parameters with 'paramName = value' are optional parameters ; they must be last in param-list
        public Event (string newOrigin, List<string> newOptions, List<string> newNextEvents, string newId = "-1", EventSpecialCondition newSpecial = EventSpecialCondition.None)
        {
            //if (newNextEvents.Count != newOptions.Count)
            //    Console.Write("ERROR Array lengths don't match");

            eventId = newId;

            originText = newOrigin;
            options = newOptions;
            optionsNextEvents = newNextEvents;

            evSpCon = newSpecial;
        }
    }
}
