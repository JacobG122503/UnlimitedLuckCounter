using System.Diagnostics;

Console.WriteLine("UNLIMITED LUCK COUNTER");
Console.WriteLine("=====================");

var exitProgram = false;
while (!exitProgram)
{
    Console.Clear();
    Console.WriteLine("UNLIMITED LUCK COUNTER");
    Console.WriteLine("=====================");
    Console.WriteLine("1. Start Game");
    Console.WriteLine("2. Exit");
    Console.Write("Select option (1 or 2): ");

    var modeInput = Console.ReadLine();

    if (modeInput == "2")
    {
        exitProgram = true;
        continue;
    }

    var typedCorrectly = false;
    var luckyNumber = 0;

    while (!typedCorrectly)
    {
        Console.Write("Enter a lucky number 0-9 (inclusive): ");
        var input = Console.ReadLine();

        if (int.TryParse(input, out var number) && number >= 0 && number <= 9)
        {
            luckyNumber = number;
            typedCorrectly = true;
        }
        else
        {
            Console.WriteLine("That's not a valid number, try again.");
        }
    }

    var highscoreManager = new HighscoreManager(Path.Combine(Directory.GetCurrentDirectory(), "../../../highscores.txt"));

    var maxNumber = 10;
    var level = 1;
    var guessesThisLevel = 0;

    var stopwatch = Stopwatch.StartNew();
    var lastMatchTime = stopwatch.Elapsed;

    var guessRateWindow = new Queue<(DateTime time, int count)>();
    var lastRateUpdate = DateTime.Now;
    var guessesAtLastUpdate = 0;
    var currentGuessesPerSecond = 0.0;

    Console.CursorVisible = false;
    Console.Clear();

    try
    {
        while (true)
        {
            Console.SetCursorPosition(0, 0);
            var guessedNumber = Random.Shared.Next(maxNumber + 1);
            guessesThisLevel++;

            var totalTime = stopwatch.Elapsed;
            var sinceLastMatch = totalTime - lastMatchTime;

            var luckPercentage = maxNumber > 0 ? (guessesThisLevel / (double)maxNumber) * 100 : 0;

            var now = DateTime.Now;
            if ((now - lastRateUpdate).TotalMilliseconds >= 500)
            {
                var guessesAdded = guessesThisLevel - guessesAtLastUpdate;
                var secondsElapsed = (now - lastRateUpdate).TotalSeconds;

                if (secondsElapsed > 0)
                {
                    var windowRate = guessesAdded / secondsElapsed;

                    guessRateWindow.Enqueue((now, guessesThisLevel));
                    while (guessRateWindow.Count > 5)
                        guessRateWindow.Dequeue();

                    if (guessRateWindow.Count >= 2)
                    {
                        var oldest = guessRateWindow.Peek();
                        var totalGuessesInWindow = guessesThisLevel - oldest.count;
                        var totalTimeInWindow = (now - oldest.time).TotalSeconds;

                        if (totalTimeInWindow > 0)
                            currentGuessesPerSecond = totalGuessesInWindow / totalTimeInWindow;
                    }
                    else
                    {
                        currentGuessesPerSecond = windowRate;
                    }

                    lastRateUpdate = now;
                    guessesAtLastUpdate = guessesThisLevel;
                }
            }

            var printString = $"| Lucky Number: {luckyNumber} | Level: {level} | Range: (0-{maxNumber:N0}) | Level Guesses: {guessesThisLevel:N0} | Luck: {luckPercentage:F2}% | {currentGuessesPerSecond:N0} guesses/s | Total Time: {FormatTime(totalTime)} | Since Last Match: {FormatTime(sinceLastMatch)} |";
            Console.WriteLine(printString.PadRight(Console.WindowWidth));
            Console.WriteLine(new string('-', printString.Length).PadRight(Console.WindowWidth));
            Console.WriteLine($"Guessed Number: {guessedNumber:N0}".PadRight(Console.WindowWidth));

            if (guessedNumber == luckyNumber)
            {
                highscoreManager.TryUpdateHighscore(level, guessesThisLevel, sinceLastMatch.TotalSeconds, maxNumber);

                maxNumber *= 10;
                level++;
                guessesThisLevel = 0;
                lastMatchTime = stopwatch.Elapsed;

                guessRateWindow.Clear();
                lastRateUpdate = DateTime.Now;
                guessesAtLastUpdate = 0;
                currentGuessesPerSecond = 0;
            }

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true);
                if (key.Key == ConsoleKey.Escape)
                {
                    Console.WriteLine("\nGame interrupted. Press any key to return to menu...");
                    Console.ReadKey(true);
                    break;
                }
            }
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\nAn error occurred: {ex.Message}");
        Console.WriteLine("Press any key to continue...");
        Console.ReadKey();
    }
}

string FormatTime(TimeSpan timeSpan) => timeSpan.Hours > 0
    ? $"{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s"
    : $"{timeSpan.Minutes}m {timeSpan.Seconds}s";