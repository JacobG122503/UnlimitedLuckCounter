using System.Globalization;

public class HighscoreManager
{
    private readonly string _filePath;
    private readonly Dictionary<int, (DateTime timestamp, int guesses, double time, int range)> _levelStats = new();

    public HighscoreManager(string filePath)
    {
        _filePath = filePath;
        LoadHighscores();
    }

    private void LoadHighscores()
    {
        if (!File.Exists(_filePath)) return;

        _levelStats.Clear();

        var lines = File.ReadAllLines(_filePath);
        foreach (var line in lines.Skip(1))
        {
            if (!line.StartsWith("[")) continue;

            try
            {
                var firstBracketEnd = line.IndexOf(']');
                if (firstBracketEnd == -1) continue;
                var timestampStr = line.Substring(1, firstBracketEnd - 1);

                var levelText = "Level: ";
                var levelIndex = line.IndexOf(levelText);
                if (levelIndex == -1) continue;

                var dashIndex = line.IndexOf('-', levelIndex);
                if (dashIndex == -1) continue;

                var levelStr = line.Substring(levelIndex + levelText.Length, dashIndex - (levelIndex + levelText.Length)).Trim();

                var guessesText = "Guesses: ";
                var guessesStartIndex = line.IndexOf(guessesText, dashIndex);
                if (guessesStartIndex == -1) continue;

                var pipeIndex = line.IndexOf('|', guessesStartIndex);
                if (pipeIndex == -1) continue;

                var guessesStr = line.Substring(guessesStartIndex + guessesText.Length, pipeIndex - (guessesStartIndex + guessesText.Length)).Trim().Replace(",", "");

                var rangeStartIndex = line.IndexOf('[', pipeIndex);
                if (rangeStartIndex == -1) continue;

                var rangeEndIndex = line.IndexOf(']', rangeStartIndex);
                if (rangeEndIndex == -1) continue;

                var rangeStr = line.Substring(rangeStartIndex + 1, rangeEndIndex - rangeStartIndex - 1).Replace(",", "");

                var formattedTimeStr = line.Substring(rangeEndIndex + 1).Trim();
                double time = 0;

                if (DateTime.TryParse(timestampStr, out var timestamp) &&
                    int.TryParse(levelStr, out var level) &&
                    int.TryParse(guessesStr, out var guesses) &&
                    int.TryParse(rangeStr, out var range))
                {
                    if (!_levelStats.ContainsKey(level) || guesses < _levelStats[level].guesses)
                    {
                        _levelStats[level] = (timestamp, guesses, time, range);
                    }
                }
            }
            catch
            {
                continue;
            }
        }
    }

    public bool TryUpdateHighscore(int level, int guesses, double time, int range)
    {
        if (!_levelStats.ContainsKey(level) || guesses < _levelStats[level].guesses)
        {
            _levelStats[level] = (DateTime.Now, guesses, time, range);
            SaveHighscore(level);
            return true;
        }

        return false;
    }

    private void SaveHighscore(int level)
    {
        if (!_levelStats.ContainsKey(level))
            return;

        var entry = _levelStats[level];
        var formattedTime = FormatTime(entry.time);
        var rangeWithCommas = entry.range.ToString("N0", CultureInfo.InvariantCulture);
        var guessesWithCommas = entry.guesses.ToString("N0", CultureInfo.InvariantCulture);

        var lines = File.Exists(_filePath) ? File.ReadAllLines(_filePath).ToList() : new List<string>();

        if (lines.Count == 0 || !lines[0].Equals("HIGHSCORES"))
        {
            lines.Insert(0, "HIGHSCORES");
        }

        var levelPattern = $"Level: {level} -";
        var existingLineIndex = -1;

        for (var i = 0; i < lines.Count; i++)
        {
            if (lines[i].Contains(levelPattern))
            {
                existingLineIndex = i;
                break;
            }
        }

        var newLine = $"[{entry.timestamp:MM/dd/yyyy hh:mm:ss tt}] Level: {level} - Guesses: {guessesWithCommas} | [{rangeWithCommas}] {formattedTime}";

        if (existingLineIndex != -1)
        {
            lines[existingLineIndex] = newLine;
        }
        else
        {
            lines.Add(newLine);
        }

        File.WriteAllLines(_filePath, lines);
    }

    private string FormatTime(double seconds)
    {
        var ts = TimeSpan.FromSeconds(seconds);

        var parts = new List<string>();
        if (ts.Hours > 0) parts.Add($"{ts.Hours}h");
        if (ts.Minutes > 0 || ts.Hours > 0) parts.Add($"{ts.Minutes}m");
        if (ts.Seconds > 0 || ts.Minutes > 0 || ts.Hours > 0) parts.Add($"{ts.Seconds}s");
        parts.Add($"{ts.Milliseconds}ms");

        return string.Join(" ", parts);
    }
}