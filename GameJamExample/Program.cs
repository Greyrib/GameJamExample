using System.Numerics;
using static GameJamExample.Event;
using NAudio.Wave;

namespace GameJamExample
{
    internal class Program
    {
        public static List<Event> events = new List<Event> (); // A list of all events, so we can find them via their 'eventId' property
        static Event? curEvent; // The current event, with all associated properties
        static string? playerInput; // For when player chooses an option

        static bool dontloop = false; // To break out the repeating while-loop in 'Loop()'

        static int brekfus = -1;

        static Thread musicThread;

        static void Main (string[] args)
        {
            // Create a new thread that runs the Play_Music method
            musicThread = new Thread(new ThreadStart(Play_Music));

            // Start the thread
            musicThread.Start();
            //Play_Music();
            

            Game_Intro();
            
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
                new List<string>{ "ShampooDie", "Brekfus" /*"SoapDie"*/, "NoShower" },
                "Shower")
                ,
                // NOTE Special case here, this event's choice affects future outcome in BoulderingHall
                new Event ("Morgenmad. Nom nom.",
                new List<string>{"Drikker monner", "Drik en flaske vand", "Spis müsli" },
                new List<string>{ "Transport", "Transport", "Transport"},
                "Brekfus")
                ,
                new Event ("Transport til skole.",
                new List<string>{ "Gaa i skole.", "Cykle i skole", "Offentlig transport" },
                new List<string> { "RunOver", "Skolevalg", "PublicTranspDie"}, 
                "Transport")
                ,
                new Event ("Skolevalg",
                new List<string>{ "Systemudvikling", "Programmering", "Spille slope på y8"},
                new List<string>{ "SysDie", "ProDie", "Bouldering"},
                "Skolevalg")
                ,
                new Event("Gå til bouldering hallen",
                new List<string>{ "Rute 1 under broen", "rute 2 igennem gyden"},
                new List<string>{ "", "BridgeDie"},
                "Bouldering")
                ,
                // No options for this event, since it's a special-case handled in 'Loop()'
                new Event("Bouldering med Orhan.",
                new List<string>{},
                new List<string>{},
                "Bouldering")
                ,
                new Event ("Tager i byen",
                new List<string> {"Man tager på Den brølende and", "Man tager på a-bar", "Man tager på old irish", "Man tager på Proud"},
                new List<string> {"Chica", "AbarDie", "OldIrishDie", "ProudDie"},
                "CityTrip")
                ,
                new Event ("Dame med hjem fra byen kl. 05:00 ?",
                new List<string>{ "Ja!", "Nej!"},
                new List<string>{ "Ladykiller", "CHICKEN"},
                "Chica")
            };

            // Create associated die-events
            // NOTE Disregard that VS states they aren't used; they're sought & triggered when an event points to a followup-event
            events.Add (new Event ("Du glider i shampoo-flasken, slaar hovedet og blöder ud!", new List<string> (), new List<string> (), "ShampooDie", newSpecial: EventSpecialCondition.Death));
            //events.Add (new Event ("Du lever en bar af håndsæbe er den bedste sæbe til krop og sjæl", new List<string> (), new List<string> (), "SoapDie"));
            events.Add (new Event ("Din nabo snakker højt om at du lugter hele vejen ind i hans lejlighed, du dør af skam.", new List<string> (), new List<string> (), "NoShower", newSpecial: EventSpecialCondition.Death));

            events.Add (new Event ("Du faar en blodprop og er dieded!", new List<string> (), new List<string> (), "SnoozeDie", newSpecial: EventSpecialCondition.Death));
            events.Add (new Event ("Evig søvn, snehvide style - RIP!", new List<string> (), new List<string> (), "SnowWhite", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("Du bliver næsten kørt ned af en bil, tror du er sikker og så bliver du kørt ned af en cykel og dør", new List<string>(), new List<string>(), "RunOver"));
            events.Add(new Event("Du bliver blændet af solskinnet igennem vinduet, taber din kaffe ind i buschaufføren og han kører ned i en å og du dør", new List<string>(), new List<string>(), "PublicTranspDie"));

            events.Add(new Event("J-bro dræber dig  for at lave en objekt-model forkert", new List<string>(), new List<string>(), "SysDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Du kommer til at programmere en AI der dræber hele menneskeligheden", new List<string>(), new List<string>(), "ProDie", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("du dør Leif er sur, hvorfor fanden er du i Leifs gyde?! Han kaster en pokeball aka en håndgranat og du eksploderer", new List<string>(), new List<string>(), "BridgeDie", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("A-bar findes ikke længere Orhan er skuffet og henter en gruppe mænd til at tæske dig, du dør efterfølgende.", new List<string>(), new List<string>(), "AbarDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Der er så dødt og kedeligt at du dræber dig selv.", new List<string>(), new List<string>(), "OldIrishDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Du bliver trampet ihjel, fordi der er for mange mennesker på floor.", new List<string>(), new List<string>(), "ProudDie", newSpecial: EventSpecialCondition.Death));

            events.Add (new Event ("Du valgte at tage en dame med hjem kl 05 er du dum? Hun stjæler dine organer og efterlader dig i badekaret til at dø.", new List<string>(), new List<string>(), "Ladykiller", newSpecial: EventSpecialCondition.Death));

            // Set the very first event
            curEvent = events[0];

            // Create the winning-event
            Event winEvent = new Event ("Du bliver vækket af solen, der skinner ind i dine øjne." +
                "\nDu kigger over på din kalender, der står ikke fredag for første gang i mange dage…." +
                "\n\nDET ENDELIG BLEVET WEEKEND!", new List<string> (), new List<string> (), "CHICKEN", foreground: "Green");
            winEvent.evSpCon = Event.EventSpecialCondition.Win;
            events.Add (winEvent);

            // Start stuff!
            Loop ();
        }

        static void Game_Intro ()
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;

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

            Console.ForegroundColor = ConsoleColor.White;
        }
        static void Loop ()
        {
            while (dontloop == false) {
                // Debug stuff
                //Console.WriteLine ($"[DEBUG] Current event is presently \"{curEvent.originText}\" with {curEvent.options.Count} options & {curEvent.optionsNextEvents.Count} nextEvents | {curEvent.evSpCon}");

                if (curEvent != null) {
                    curEvent.PreEventStuff();
                    Console.WriteLine (curEvent.originText);

                    if (curEvent.eventId != "Bouldering")
                    {
                        foreach (string option in curEvent.options)
                            Console.WriteLine (option);
                    } else
                    {
                        if (brekfus == 0)
                        {
                            Console.WriteLine("Du har energi fra monneren du drak til morgenmad. Du flyver op af ruter i alle sværhedsgrader, Orhan indser at han aldrig bliver lige så god til bouldering som dig.");
                        } else if (brekfus == 1)
                        {
                            Console.WriteLine("Du prøver at klatre op af den nemmeste rute, men vand er for svage mennesker og giver ingen energi. Du falder ned fra væggen og lander lige akkurat udenfor madrassen under dig og du dør.");
                        } else if (brekfus == 2)
                        {
                            Console.WriteLine("Dagen går relativt okay og du klatre som du normalt ville.\r\nSiden du spiste müsli til morgenmad føler du dig fyldt af energi, du går efter en sværere rute end normalt. Halvvejs oppe løber müsliens energi tør. Du falder og forstrækker en muskel, dette er lig med død for en svag IT studerende.");
                        }
                        curEvent = events.FirstOrDefault(x => x.eventId == "CityTrip");
                    }

                    

                    if (curEvent.evSpCon != Event.EventSpecialCondition.None) {
                        string specialDebugMessage = curEvent.evSpCon == EventSpecialCondition.Death ? " - LOSE CONDITION - " : " - WIN CONDITION - "; // Ternary conditional operator
                        //Console.WriteLine (specialDebugMessage);
                        dontloop = true;
                        //musicThread.
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
                    // Restart the game
                    else if (playerInput?.ToUpper() == "R")
                    {
                        Console.Clear();
                        Game_Intro();
                        curEvent = events[0];
                        continue;
                    }
                    // Cheatcode for winning
                    else if (playerInput?.ToUpper() == "CHICKEN")
                    {
                        curEvent = events.FirstOrDefault(x => x.eventId == "CHICKEN");
                        continue;
                    }
                        

                    int parsedPlayerChoice = -1;
                    bool parseSuccess = int.TryParse (playerInput, out parsedPlayerChoice);
                    parsedPlayerChoice -= 1; // Player chooses 1-*, but option-array starts at 0




                    // Set brekfus-variable (based off player's parsed choice), if it's the special brekfus event
                    if (curEvent.eventId == "Brekfus")
                        brekfus = parsedPlayerChoice;
                    Console.WriteLine($"\n[DEBUG] Brekfus is now: {brekfus}\n");




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

        static void Play_Music()
        {
            // Load the WAV file
            WaveFileReader waveFileReader = new WaveFileReader(@"C:\Archive S\UCL\3. Semester\Git\GameJam med Förste Semester\GameJamExample\GameJamExample\piano loop.wav");
            // Create a WaveOutEvent instance
            WaveOutEvent waveOutEvent = new WaveOutEvent();

            // Play the WAV file
            waveOutEvent.Init(waveFileReader);
            waveOutEvent.Play();

            // Wait for the playback to finish
            while (waveOutEvent.PlaybackState == PlaybackState.Playing)
            {
                // Do nothing
            }

            // Clean up
            waveOutEvent.Stop();
            waveOutEvent.Dispose();
        }

    }
}
