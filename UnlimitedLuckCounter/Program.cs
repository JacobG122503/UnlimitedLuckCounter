

var typedCorrectly = false;

var luckyNumber = 0;

while (!typedCorrectly)
{
    Console.Write("Enter a lucky number 0-9 (inclusive): ");
    var input = Console.ReadLine();

    if (int.TryParse(input, out var number) && !(number < 0 || number > 9))
    {
        luckyNumber = number;
        typedCorrectly = true;
    }
    else
    {
        Console.WriteLine("That's not a valid number, try again.");
    }
}

var maxNumber = 10;
var level = 1;
var guessesThisLevel = 0;

Console.CursorVisible = false;
Console.Clear();
while (true)
{
    Console.SetCursorPosition(0, 0);
    var guessedNumber = Random.Shared.Next(maxNumber + 1);
    guessesThisLevel++;
    var printString = $"| Lucky Number: {luckyNumber} | Level: {level} | Range (0-{maxNumber}) | Level Guesses: {guessesThisLevel}|";
    Console.WriteLine(printString);
    for (var i = 0; i < printString.Length; i++) Console.Write("-");
    Console.WriteLine();
    Console.WriteLine($"Guessed Number: {guessedNumber}".PadRight(Console.WindowWidth));
    if (guessedNumber == luckyNumber)
    {
        maxNumber *= 10;
        level++;
        guessesThisLevel = 0;
    }
    //Thread.Sleep(100);
}