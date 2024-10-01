using System.Diagnostics;

public static class KdotoolInterop
{
    public static string GetKWinClientId(Process process, string windowName, int timeoutMs = 5000)
    {
        var pid = process.Id;
        // use kdotool to get the client id
        var kdotool = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "kdotool",
                Arguments = $"search --pid {pid} --name {windowName} --all",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        kdotool.Start();
        kdotool.WaitForExit(timeoutMs);
        var error = kdotool.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Error while getting client id: {error}");
        }
        var output = kdotool.StandardOutput.ReadToEnd();
        if (string.IsNullOrEmpty(output))
        {
            throw new Exception("Could not find client id");
        }
        var clientIds = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (clientIds.Length != 1)
        {
            throw new Exception("Found more than one client id");
        }
        var clientId = clientIds[0];
        return clientId;
    }
    public static string MoveKWinWindow(string clientId, uint xPos, uint yPos, int timeoutMs = 5000)
    {
        // use kdotool to move the window
        var kdotool = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "kdotool",
                Arguments = $"windowmove {clientId} {xPos} {yPos}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        kdotool.Start();
        kdotool.WaitForExit(timeoutMs);
        var error = kdotool.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Error while moving window: {error}");
        }
        return kdotool.StandardOutput.ReadToEnd();
    }
    public static string SetKWinWindowBorder(string clientId, bool borderEnabled, int timeoutMs = 5000)
    {
        // use kdotool to set the window border
        var kdotool = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "kdotool",
                Arguments = $"windowstate --{(borderEnabled ? "add" : "remove")} no_border {clientId}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        kdotool.Start();
        kdotool.WaitForExit(timeoutMs);
        var error = kdotool.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Error while setting window border: {error}");
        }
        return kdotool.StandardOutput.ReadToEnd();
    }
    public static string ResizeKWinWindow(string clientId, uint width, uint height, int timeoutMs = 5000)
    {
        // use kdotool to resize the window
        var kdotool = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "kdotool",
                Arguments = $"windowsize {clientId} {width} {height}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            }
        };
        kdotool.Start();
        kdotool.WaitForExit(timeoutMs);
        var error = kdotool.StandardError.ReadToEnd();
        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception($"Error while resizing window: {error}");
        }
        return kdotool.StandardOutput.ReadToEnd();
    }
}