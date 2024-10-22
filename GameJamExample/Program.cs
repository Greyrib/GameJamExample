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

        static Thread? musicThread; // Thread so music can run simultaneously with gameplay loop
        static bool stopMusic = false; // For stopping music

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

                new Event ("Den pinende alarm så vækkede dig larmer stadig. Hvad vil du gøre herfra?", 
                    new List<string>{"1. Snooze i 5 min.", "2. Sov igennem alarmen ved hjælp af viljestyrke.", "3. Stå op med det samme."},
                    new List<string>(){"SnoozeDie", "SnowWhite", "Shower" },
                    "WakeUp")
                ,
                new Event ("Du skal beslutte dig for om du vil i bad eller ej. Hvad vil du gøre?",
                new List<string>{ "1. Gå i bad og brug shampoo og shower gel.",
                    "2. Gå i bad og brug håndsæbe til hele kroppen.",
                    "3. Spring badet over."},
                new List<string>{ "ShampooDie", "Brekfus" /*"SoapDie"*/, "NoShower" },
                "Shower", newPretext: "Du lever, du er frisk og klar til dagen")
                ,
                // NOTE Special case here, this event's choice affects future outcome in BoulderingHall
                new Event ("Tid til morgenmad. Nom nom.",
                new List<string>{"1. Drik en lille monne 160 mg koffein", "2. Drik en flaske vand", "3. Spis müsli" },
                new List<string>{ "Transport", "Transport", "Transport"},
                "Brekfus")
                ,
                new Event ("Du skal over på campus nu. Hvordan?",
                new List<string>{ "1. Gå i skole.", "2. Cykel i skole", "3. Offentlig transport" },
                new List<string> { "RunOver", "Skolevalg", "PublicTranspDie"}, 
                "Transport")
                ,
                new Event ("Hvad vil du lave i skolen?",
                new List<string>{ "1. Systemudvikling", "2. Programmering", "3. Spille Slope på Y8"},
                new List<string>{ "SysDie", "ProDie", "Bouldering"},
                "Skolevalg")
                ,
                new Event("Du skal til lidt eftermiddagsbouldering i bouldering hallen.",
                new List<string>{ "Rute 1: under broen", "Rute 2: igennem gyden"},
                new List<string>{ "", "BridgeDie"},
                "Bouldering")
                ,
                // No options for this event, since it's a special-case handled in 'Loop()'
                new Event("Bouldering med Orhan.",
                new List<string>{},
                new List<string>{},
                "Bouldering")
                ,
                new Event ("Du skal til at tage i byen nu. Hvor tager du hen?",
                new List<string> {"1. Man tager på Den brølende and", "2. Man tager på a-bar", "3. Man tager på old irish", "4. Man tager på Proud"},
                new List<string> {"Chica", "AbarDie", "OldIrishDie", "ProudDie"},
                "CityTrip")
                ,
                new Event ("Dame med hjem fra byen kl. 05:00 ?",
                new List<string>{ "1. Ja!", "2. Nej!"},
                new List<string>{ "Ladykiller", "CHICKEN"},
                "Chica")
            };

            // Create associated die-events
            // NOTE Disregard that VS states they aren't used; they're sought & triggered when an event points to a followup-event
            events.Add (new Event ("Du glider i shampoo-flasken, slaar hovedet og blöder ud!", new List<string> (), new List<string> (), "ShampooDie", newSpecial: EventSpecialCondition.Death));
            //events.Add (new Event ("Du lever en bar af håndsæbe er den bedste sæbe til krop og sjæl", new List<string> (), new List<string> (), "SoapDie"));
            events.Add (new Event ("Din nabo snakker højt om at du lugter hele vejen ind i hans lejlighed. Du dør af skam.", new List<string> (), new List<string> (), "NoShower", newSpecial: EventSpecialCondition.Death));

            events.Add (new Event ("Du får en blodprop og er dieded!", new List<string> (), new List<string> (), "SnoozeDie", newSpecial: EventSpecialCondition.Death));
            events.Add (new Event ("Evig søvn, snehvide style - RIP!", new List<string> (), new List<string> (), "SnowWhite", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("Du bliver næsten kørt ned af en bil, tror du er sikker, og så bliver du kørt ned af en cykel og dør", new List<string>(), new List<string>(), "RunOver"));
            events.Add(new Event("Du bliver blændet af solskin igennem vinduet og taber din kaffe på buschaufføren, så han kører ned i en å og du dør", new List<string>(), new List<string>(), "PublicTranspDie"));

            events.Add(new Event("J-bro dræber dig for at lave en objektmodel forkert", new List<string>(), new List<string>(), "SysDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Du kommer til at programmere en AI der dræber hele menneskeheden ups", new List<string>(), new List<string>(), "ProDie", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("Du dør. Leif er sur, hvorfor fanden er du i Leifs gyde?! Han kaster en pokeball aka en håndgranat og du eksploderer", new List<string>(), new List<string>(), "BridgeDie", newSpecial: EventSpecialCondition.Death));

            events.Add(new Event("A-bar findes ikke længere. Orhan er skuffet og henter en gruppe mænd til at tæske dig, og du dør efterfølgende.", new List<string>(), new List<string>(), "AbarDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Der er så dødt og kedeligt at du dræber dig selv.", new List<string>(), new List<string>(), "OldIrishDie", newSpecial: EventSpecialCondition.Death));
            events.Add(new Event("Du bliver trampet ihjel fordi der er for mange mennesker på floor.", new List<string>(), new List<string>(), "ProudDie", newSpecial: EventSpecialCondition.Death));

            events.Add (new Event ("Du valgte at tage en dame med hjem kl. 05, er du dum? Hun stjæler dine organer og efterlader dig i badekaret til at dø.", new List<string>(), new List<string>(), "Ladykiller", newSpecial: EventSpecialCondition.Death));

            // Set the very first event
            curEvent = events[0];

            // Create the winning-event
            Event winEvent = new Event ("Du bliver vækket af solen der skinner ind i dine øjne." +
                "\nDu kigger over på din kalender, og der står ikke fredag for første gang i mange dage…." +
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
            Console.WriteLine("\r\n   ___             _                __                         _       _   " +
                              "\r\n  / __\\ __ ___  __| | __ _  __ _   / _| ___  _ __    _____   _(_) __ _| |_ " +
                              "\r\n / _\\| '__/ _ \\/ _` |/ _` |/ _` | | |_ / _ \\| '__|  / _ \\ \\ / / |/ _` | __|" +
                              "\r\n/ /  | | |  __/ (_| | (_| | (_| | |  _| (_) | |    |  __/\\ V /| | (_| | |_ " +
                              "\r\n\\/   |_|  \\___|\\__,_|\\__,_|\\__, | |_|  \\___/|_|     \\___| \\_/ |_|\\__, |\\__|" +
                              "\r\n                           |___/                                 |___/     " +
                              "\r\n");
            Console.WriteLine("\nDu vågner op fredag morgen kl. 06:45, og solen skinner ind af vinduet." +
                "\nDet en dag som enhver anden, men der er noget der føles bekendt. " +
                "\nHmmm. Der er ingen grund til bekymring. Der er en travl dag foran dig" +
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
                            Console.WriteLine("Du har energi fra monneren du drak til morgenmad. Du flyver op af ruter i alle sværhedsgrader, og Orhan indser at han aldrig bliver lige så god til bouldering som dig.");
                        } else if (brekfus == 1)
                        {
                            Console.WriteLine("Du prøver at klatre op af den nemmeste rute, men vand er for svagelige mennesker og giver ingen energi. Du falder ned fra væggen og lander lige akkurat udenfor madrassen under dig og dør.");
                        } else if (brekfus == 2)
                        {
                            Console.WriteLine("Dagen går relativt okay og du klatrer som du normalt ville.\r\nSiden du spiste müsli til morgenmad føler du dig fyldt af energi, og du går efter en sværere rute end normalt. Halvvejs oppe løber müsliens energi tør. Du falder og forstrækker en muskel, som er lig med død for en svag IT studerende.");
                        }
                        curEvent = events.FirstOrDefault(x => x.eventId == "CityTrip");
                    }

                    

                    if (curEvent.evSpCon != Event.EventSpecialCondition.None) {
                        string specialDebugMessage = curEvent.evSpCon == EventSpecialCondition.Death ? " - LOSE CONDITION - " : " - WIN CONDITION - "; // Ternary conditional operator
                        //Console.WriteLine (specialDebugMessage);
                        stopMusic = true;
                        dontloop = true;
                        //musicThread.
                        Console.WriteLine ("\nPress en Buttongs for at Exitificére Applikasjionaellen");
                        //Console.ReadLine (); // Not needed; console requires input at the end of everything
                    }

                    playerInput = Console.ReadLine ();

                    // Special dev exit check, with v8 nullable-check
                    if (playerInput?.ToLower() ==  "qq") {
                        stopMusic = true;
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
                    //Console.WriteLine($"\n[DEBUG] Brekfus is now: {brekfus}\n");




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
            WaveFileReader waveFileReader = new WaveFileReader("piano loop.wav");

            // Create a WaveOutEvent instance
            WaveOutEvent waveOutEvent = new WaveOutEvent();

            // Play the WAV file
            waveOutEvent.Init(waveFileReader);
            waveOutEvent.Play();

            // Continuously check if playback is finished or stop is requested
            while (waveOutEvent.PlaybackState == PlaybackState.Playing && !stopMusic)
            {
                Thread.Sleep(100); // Sleep briefly to prevent busy-waiting
            }

            // Stop the playback if requested
            if (waveOutEvent.PlaybackState == PlaybackState.Playing)
            {
                waveOutEvent.Stop();
            }

            // Clean up
            waveOutEvent.Dispose();
            waveFileReader.Dispose();
        }

    }
}
