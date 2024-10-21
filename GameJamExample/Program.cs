using System.Numerics;
using static GameJamExample.Event;

namespace GameJamExample
{
    internal class Program
    {
        public static List<Event> events = new List<Event> (); // A list of all events, so we can find them via their 'eventId' property
        static Event? curEvent; // The current event, with all associated properties
        static string? playerInput; // For when player chooses an option

        static bool dontloop = false; // To break out the repeating while-loop in 'Loop()'

        static void Main (string[] args)
        {
            // Game Intro Text
            Console.WriteLine("Du vågner op fredag morgen, solen skinner ind af vinduet." +
                "\nDet en dag som enhver anden, men der er noget, der føles bekendt. " +
                "\nHmmm. Der er ingen grund til bekymring, der er en travl dag foran dig" +
                "\nog du skal endda ud og klatre og drikke i byen med Orhan!\n");

            // TODO Could extend here with ...
            // Menu Options (Play Game, Settings, Exit)
            // Instructions
            // Clear screen & Console.Readline() await
            // ... Possibly by doing all this in a Startup() with submethods for structure & readability

            // Set special events
            new Event ("You died", new List<string> { "X to Quit!" }, new List<string> (), "YouDied");

            // Set regular events
            events = new List<Event> ()
            {
                // Reference Event Constructor Params : eventOriginText, optionTextStringList, optionNextEventIdStringList, eventIdString

                new Event ("Vågner i sin seng  klokken er 06:45", 
                    new List<string>{"1. Snooze i 5 min.", "2. Sov igennem alarmen", "Staa op med det samme"},
                    new List<string>(){"SnoozeDie", "SnowWhite", "Shower" },
                    "WakeUp")
                ,
                new Event ("I bad",
                new List<string>{ "Gå i bad og brug shampoo og shower gel.",
                    "Gå i bad og brug håndsæbe til hele kroppen.",
                    "Spring badet over."},
                new List<string>{ "ShampooDie", "CHICKEN" /*"SoapDie"*/, "NoShower" },
                "Shower")
            };

            // Create associated die-events
            events.Add (new Event ("Du glider i shampoo-flasken, slaar hovedet og blöder ud!", new List<string> (), new List<string> (), "ShampooDie", newSpecial: EventSpecialCondition.Death));
            //events.Add (new Event ("Du lever en bar af håndsæbe er den bedste sæbe til krop og sjæl", new List<string> (), new List<string> (), "SoapDie"));
            events.Add (new Event ("Din nabo snakker højt om at du lugter hele vejen ind i hans lejlighed, du dør af skam.", new List<string> (), new List<string> (), "NoShower", newSpecial: EventSpecialCondition.Death));

            events.Add (new Event ("Du faar en blodprop og er dieded!", new List<string> (), new List<string> (), "SnoozeDie", newSpecial: EventSpecialCondition.Death));
            events.Add (new Event ("Evig søvn, snehvide style - RIP!", new List<string> (), new List<string> (), "SnowWhite", newSpecial: EventSpecialCondition.Death));

            // Set the very first event
            curEvent = events[0];

            // Create the winning-event
            Event winEvent = new Event ("Du bliver vækket af solen, der skinner ind i dine øjne." +
                "\nDu kigger over på din kalender, der står ikke fredag for første gang i mange dage…." +
                "\nDET ENDELIG BLEVET WEEKEND!", new List<string> (), new List<string> (), "CHICKEN");
            winEvent.evSpCon = Event.EventSpecialCondition.Win;
            events.Add (winEvent);

            // Start stuff!
            Loop ();
        }

        static void Loop ()
        {
            while (dontloop == false) {
                // Debug stuff
                //Console.WriteLine ($"[DEBUG] Current event is presently \"{curEvent.originText}\" with {curEvent.options.Count} options & {curEvent.optionsNextEvents.Count} nextEvents | {curEvent.evSpCon}");

                if (curEvent != null) {
                    Console.WriteLine (curEvent.originText);
                    foreach (string option in curEvent.options)
                        Console.WriteLine (option);

                    if (curEvent.evSpCon != Event.EventSpecialCondition.None) {
                        string specialDebugMessage = curEvent.evSpCon == EventSpecialCondition.Death ? " - LOSE CONDITION - " : " - WIN CONDITION - "; // Ternary conditional operator
                        //Console.WriteLine (specialDebugMessage);
                        dontloop = true;
                        Console.WriteLine ("\nPress en Buttongs for at Exitificére Applikasjionaellen");
                        //Console.ReadLine (); // Not needed; console requires input at the end of everything
                    }

                    playerInput = Console.ReadLine ();

                    // Special dev exit check, with v8 nullable-check
                    if (playerInput?.ToLower() ==  "qq") {
                        Console.Clear ();
                        dontloop = true; // Set exit-condition so while-loop ceases occurring
                        break; // Break out of the while-loop we're in
                    }
                        

                    int parsedPlayerChoice = -1;
                    bool parseSuccess = int.TryParse (playerInput, out parsedPlayerChoice);
                    parsedPlayerChoice -= 1; // Player chooses 1-*, but option-array starts at 0



                    // Look through all events, to find whatever is the next event
                    if (curEvent.optionsNextEvents.Count > 0) {
                        for (int e = 0; e < events.Count; e++) {
                            if (parsedPlayerChoice >= 0 && parsedPlayerChoice < curEvent.optionsNextEvents.Count && curEvent.optionsNextEvents[parsedPlayerChoice] == events[e].eventId) {
                                curEvent = events[e];
                                break;
                            }
                        }
                    }

                    //Console.Clear ();
                    Console.WriteLine ("\n");
                }
            }
        }

    }
}
