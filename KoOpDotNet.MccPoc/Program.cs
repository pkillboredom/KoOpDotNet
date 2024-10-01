/*-----------------------------------------------------------------------
  Author: Luke Tomkus
  This POC demonostrates a complete implementation of split screen for
  Halo: The Master Chief Collection. This code is directly adapted from
  the work of PoundlandBacon at Splitscreen.me. See:
  https://hub.splitscreen.me/handler/xjBSwFNj3ndRntsZh
---------------------------------------------------------------------*/

using System.Diagnostics;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

        // Begin Info from NucleusCoop/Splitscreen.me handler.
        string[] killMutexList = ["Chelankenstein"];
        string[] dirSymlinkExclusions = [
            "data\\ui",
            "halo1\\shaders",
            "mcc\\binaries\\win64",
            "mcc\\binaries\\win64\\steam_settings\\load_dlls",
            "mcc\\Content\\Movies"
        ];
        string[] fileSymlinkExclusions = [
            "steam_api64.dll",
            "steam_api.dll",
            "steam_appid.txt",
            "local_save.txt",
            "xlive64.dll",
            "xinput1_3.dll",
            "FMS_logo_microsoft_7_1_.bk2"
        ];
        string[] fileCopyList = [
            "MCC-Win64-Shipping.exe",
            "gametiplist.xml",
            "playercustomization.xml",
            "scoredb.xml",
            "texturepacks.xml",
            "unlockdb.xml",
            "fmod_event_net64.dll",
            "fmodex64.dll",
            "var cfg",
            "halo1.dll",
            "user.cfg",
            "fx.bin",
            "psh.bin",
            "vsh.bin",
            "app.config",
            "d3dcompiler_47.dll",
            "SC_DLL.dll",
            "SwitchSandbox.cmd",
            "bink2w64.dll",
            "cell_release_x64_4_0.dll",
            "halonetworklayer_ship.dll",
            "liblz4.dll",
            "PartyWin.dll",
            "PartyWin7.dll",
            "simplenetworklibrary-x64-release.dll",
            "xaudio2_9redist.dll",
            "xforgelibrary_release.dll"
        ];
        string[] CopyEnvFoldersToNucleusAccounts = ["AppData\\LocalLow\\MCC\\LocalFiles"];
        var UseNucleusEnvironment = true; //Use custom environment variables for games that use them, replaces some common paths (e.g. AppData) with C:\Users\<your username>\NucleusCoop.
        var UseGoldberg = true; //Use the built-in Goldberg features in Nucleus | default: false.
        var GoldbergExperimental = true; //Use the experimental branch of Goldberg | Requires `Game.UseGoldberg = true` | default: false.
        var GoldbergNoLocalSave = true; //Do not create a local_save.txt file for Goldberg, saves are to use default game save location.
        var SymlinkGame = true;
        var SymlinkFolders = true;
        var ExecutableName = "MCC-Win64-Shipping.exe";
        var SteamID = "976730";
        var GameName = "Halo: The Master Chief Collection";
        var BinariesFolder = "mcc\\binaries\\win64";
        var MaxPlayers = 6;
        var LauncherTitle = ""; //The name of the launcher's window title. Some games need to go through a launcher to open. This is needed or else the application will lose the game's window.
        var ResetWindows = true; //After each new instance opens, resize, reposition and remove borders of the previous instance.
        var UseForceBindIP = true; //Set up game instances with ForceBindIP; each instance having their own IP. 
        var ForceBindIPNoDummy = true; //ForceBindIP will be used without the "dummy" launch argument, the argument prevents crashes but it causes issues in other games.
        var UserProfileSavePath = "AppData\\Roaming\\Goldberg SteamEmu Saves\\976730\\remote";
        var UserProfileConfigPath = "AppData\\LocalLow\\MCC";
        var UserProfileSavePathNoCopy = true; //Do not copy files from original UserProfileSavePath if using Nucleus Environment.
        var UserProfileConfigPathNoCopy = true; //Do not copy files from original UserProfileConfigPath if using Nucleus Environment.
        var Description = ""; // Description of the game, used for the NucleusCoop UI.
        var KeepSymLinkOnExit = false; // Enable or disable symlink files from being deleted when Nucleus is closed | default: false.
                                       // Also deletes the symlink files when the game is picked (i.e. on startup), in case we crashed last time.
                                       // End Info from NucleusCoop/Splitscreen.me handler.
        var GameDirPath = "/home/luke/.local/share/Steam/steamapps/common/Halo The Master Chief Collection";
        var GameExePath = "/home/luke/.local/share/Steam/steamapps/common/Halo The Master Chief Collection/MCC/Binaries/Win64/MCC-Win64-Shipping.exe";

        // Get the player count from the command line.
        int playerCount = Convert.ToInt32(config["PlayerCount"]);
        if (playerCount < 2 || playerCount > MaxPlayers)
        {
            throw new Exception($"Player count must be between 2 and {MaxPlayers}");
        }
        // Get the resolution of the primary display.
        (int displayWidth, int displayHeight) = XrandrInterop.GetPrimaryMonitorResolution();
        // Decide the resolution of each player's window. Waste space rather than giving one player more space, if the number of players is odd.
        int playerLayoutCount = playerCount % 2 == 0 ? playerCount : playerCount + 1;

        // This forumla is not very smart and will not scale to more than 6 players, or allow vertical splits. Too bad.
        int playerWindowWidth = displayWidth / (playerLayoutCount / 2);
        int playerWindowHeight = displayHeight / 2;
        // Create a list of player window positions.
        List<(int x, int y)> playerWindowPositions = new List<(int, int)>();
        for (int i = 0; i < playerCount; i++)
        {
            int x = i % (playerLayoutCount / 2) * playerWindowWidth;
            int y = i / (playerLayoutCount / 2) * playerWindowHeight;
            playerWindowPositions.Add((x, y));
        }

        // Create a temporary directory for the game and user profiles. This must be deleted when the program exits.
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);
        string[] instanceDirs = new string[playerCount];
        try
        {
            ProcessStartInfo[] playerStartInfos = new ProcessStartInfo[playerCount];
            // For each player, create a subdirectory for the game and user profiles.
            for (int i = 0; i < playerCount; i++)
            {
                string instanceDir = Path.Combine(tempDir, $"Player{i}");
                Directory.CreateDirectory(instanceDir);
                instanceDirs[i] = instanceDir;
                string instanceGameDirPath = Path.Combine(instanceDir, "game");
                Directory.CreateDirectory(instanceGameDirPath);
                string instanceUserDirPath = Path.Combine(instanceDir, "user");
                Directory.CreateDirectory(instanceUserDirPath);
                // Symbolic Link all of the files and folders in the original game directory, EXCEPT those listed in fileSymlinkExclusions, dirSymlinkExclusions, and fileCopyList.
                // Copy the files in fileCopyList.
                string[] gameDirContents = Directory.GetFiles(GameDirPath, "*", SearchOption.AllDirectories);
                foreach (string file in gameDirContents)
                {
                    string relativePath = Path.GetRelativePath(GameDirPath, file);
                    bool fileIsInSymlinkDirExclusions = dirSymlinkExclusions.Any(exclusion => relativePath.StartsWith(exclusion));
                    bool fileIsInSymlinkFileExclusions = fileSymlinkExclusions.Contains(Path.GetFileName(file));
                    bool fileIsInCopyList = fileCopyList.Contains(Path.GetFileName(file));
                    if (!(fileIsInSymlinkDirExclusions ||
                        fileIsInSymlinkFileExclusions ||
                        fileIsInCopyList))
                    {
                        string symlinkPath = Path.Combine(instanceGameDirPath, relativePath);
                        string symlinkDir = Path.GetDirectoryName(symlinkPath)!;
                        if (!Directory.Exists(symlinkDir))
                        {
                            Directory.CreateDirectory(symlinkDir);
                        }
                        File.CreateSymbolicLink(symlinkPath, file);
                    }
                    else if (fileIsInCopyList)
                    {
                        string copyPath = Path.Combine(instanceGameDirPath, relativePath);
                        File.Copy(file, copyPath);
                    }
                    else
                    {
                        continue;
                    }
                }
                

                
                
                // Create Process Start Infos for each player.
                ProcessStartInfo startInfo = new ProcessStartInfo()
                {
                    FileName = "gamescope",
                    Arguments = $"-W {playerWindowWidth} -H {playerWindowHeight} -- /home/luke/.local/share/Steam/ubuntu12_32/reaper SteamLaunch AppId=976730 -- /home/luke/.local/share/Steam/ubuntu12_32/steam-launch-wrapper -- /home/luke/.local/share/Steam/steamapps/common/SteamLinuxRuntime_sniper/_v2-entry-point --verb=waitforexitandrun -- /home/luke/.local/share/Steam/steamapps/common/Proton - Experimental/proton waitforexitandrun {launchExePath} -no-eac",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                playerStartInfos[i] = startInfo;
            }

            Process[] playerProcesses = new Process[playerCount];
            // One at a time, start the processes, remove the borders, resize them, and move them to the correct position.
            // We have to resize them after removing the borders, because removing the borders expands the inner surface to the old whole window size.
            for (int i = 0; i < playerCount; i++)
            {
                playerProcesses[i] = Process.Start(playerStartInfos[i]);
                playerProcesses[i].WaitForInputIdle();
                string clientId = KdotoolInterop.GetKWinClientId(playerProcesses[i], GameName);
                KdotoolInterop.SetKWinWindowBorder(clientId, false);
                KdotoolInterop.ResizeKWinWindow(clientId, (uint)playerWindowWidth, (uint)playerWindowHeight);
                KdotoolInterop.MoveKWinWindow(clientId, (uint)playerWindowPositions[i].x, (uint)playerWindowPositions[i].y);
            }
        }
        finally
        {
            // Clean up the temporary directory.
            Directory.Delete(tempDir, true);
        }
    }
}