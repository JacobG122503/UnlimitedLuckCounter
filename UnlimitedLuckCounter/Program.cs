using System.Diagnostics;


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

Console.CursorVisible = false;
Console.Clear();

while (true)
{
    Console.SetCursorPosition(0, 0);
    var guessedNumber = Random.Shared.Next(maxNumber + 1);
    guessesThisLevel++;

    var totalTime = stopwatch.Elapsed;
    var sinceLastMatch = totalTime - lastMatchTime;

    var printString = $"| Lucky Number: {luckyNumber} | Level: {level} | Range: (0-{maxNumber:N0}) | Level Guesses: {guessesThisLevel:N0} | Total Time: {FormatTime(totalTime)} | Since Last Match: {FormatTime(sinceLastMatch)} |";
    Console.WriteLine(printString.PadRight(Console.WindowWidth));
    Console.WriteLine(new string('-', printString.Length).PadRight(Console.WindowWidth));
    Console.WriteLine($"Guessed Number: {guessedNumber:N0}".PadRight(Console.WindowWidth));

    if (guessedNumber == luckyNumber)
    {
        var levelTime = totalTime.TotalMinutes - lastMatchTime.TotalMinutes;
        highscoreManager.TryUpdateHighscore(level, levelTime, maxNumber);

        maxNumber *= 10;
        level++;
        guessesThisLevel = 0;
        lastMatchTime = stopwatch.Elapsed;
    }
    //Thread.Sleep(100);
}

string FormatTime(TimeSpan timeSpan)
{
    return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
}