using System.Diagnostics;
using System.Text.RegularExpressions;

public static class XrandrInterop {
    public static (int, int) GetPrimaryMonitorResolution() {
        // use xrandr to get the resolution of the primary monitor
        var xrandr = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = "xrandr",
                Arguments = "--current",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        xrandr.Start();
        xrandr.WaitForExit();
        var error = xrandr.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error)) {
            throw new Exception($"Error while getting resolution: {error}");
        }
        var output = xrandr.StandardOutput.ReadToEnd();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var primaryLine = lines.FirstOrDefault(line => line.Contains("primary"));
        if (primaryLine == null) {
            throw new Exception("Could not find primary monitor");
        }
        var resolutionMatch = Regex.Match(primaryLine, @"current (\d+) x (\d+)");
        if (!resolutionMatch.Success) {
            throw new Exception("Could not find resolution");
        }
        var width = int.Parse(resolutionMatch.Groups[1].Value);
        var height = int.Parse(resolutionMatch.Groups[2].Value);
        return (width, height);
    }
}