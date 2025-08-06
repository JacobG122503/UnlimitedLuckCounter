using System.Globalization;

public class HighscoreManager
{
    private readonly string _filePath;
    private readonly Dictionary<int, (DateTime timestamp, double time, int range)> _levelTimes = new();

    public HighscoreManager(string filePath)
    {
        _filePath = filePath;
        LoadHighscores();
    }

    private void LoadHighscores()
    {
        if (!File.Exists(_filePath)) return;

        var lines = File.ReadAllLines(_filePath);
        foreach (var line in lines.Skip(1))
        {
            if (!line.StartsWith("[")) continue;

            var firstBracketEnd = line.IndexOf(']');
            var timestampStr = line.Substring(1, firstBracketEnd - 1);

            var secondBracketStart = line.IndexOf('[', firstBracketEnd + 1);
            var secondBracketEnd = line.IndexOf(']', secondBracketStart + 1);
            var rangeStr = line.Substring(secondBracketStart + 1, secondBracketEnd - secondBracketStart - 1).Replace(",", "");

            var rest = line.Substring(secondBracketEnd + 1).Trim();

            var levelStart = rest.IndexOf("Level:");
            var dashIndex = rest.IndexOf('-');
            var parenIndex = rest.LastIndexOf('(');

            if (levelStart == -1 || dashIndex == -1 || parenIndex == -1) continue;

            var levelStr = rest.Substring(levelStart + 6, dashIndex - levelStart - 6).Trim();
            var rawTimeStr = rest.Substring(parenIndex + 1).Trim(')', ' ');

            if (DateTime.TryParse(timestampStr, out var timestamp) &&
                int.TryParse(rangeStr, out var range) &&
                int.TryParse(levelStr, out var level) &&
                double.TryParse(rawTimeStr, out var rawTime))
            {
                if (!_levelTimes.ContainsKey(level) || rawTime < _levelTimes[level].time)
                {
                    _levelTimes[level] = (timestamp, rawTime, range);
                }
            }
        }
    }

    public bool TryUpdateHighscore(int level, double time, int range)
    {
        if (!_levelTimes.ContainsKey(level) || time < _levelTimes[level].time)
        {
            _levelTimes[level] = (DateTime.Now, time, range);
            SaveHighscores();
            return true;
        }

        return false;
    }

    private void SaveHighscores()
    {
        using var writer = new StreamWriter(_filePath, false);
        writer.WriteLine("HIGHSCORES");

        foreach (var entry in _levelTimes.OrderBy(e => e.Key))
        {
            var formattedTime = FormatTime(entry.Value.time);
            var rangeWithCommas = entry.Value.range.ToString("N0", CultureInfo.InvariantCulture);
            writer.WriteLine($"[{entry.Value.timestamp:G}] [{rangeWithCommas}] Level: {entry.Key} - {formattedTime} ({entry.Value.time})");
        }
    }

    private string FormatTime(double minutes)
    {
        var ts = TimeSpan.FromMinutes(minutes);

        var parts = new List<string>();
        if (ts.Hours > 0) parts.Add($"{ts.Hours}h");
        if (ts.Minutes > 0 || ts.Hours > 0) parts.Add($"{ts.Minutes}m");
        if (ts.Seconds > 0 || ts.Minutes > 0 || ts.Hours > 0) parts.Add($"{ts.Seconds}s");
        parts.Add($"{ts.Milliseconds}ms");

        return string.Join(" ", parts);
    }
}