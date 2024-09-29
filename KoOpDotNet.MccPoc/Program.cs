/*-----------------------------------------------------------------------
  Author: Luke Tomkus
  This POC demonostrates a complete implementation of split screen for
  Halo: The Master Chief Collection. This code is directly adapted from
  the work of PoundlandBacon at Splitscreen.me. See:
  https://hub.splitscreen.me/handler/xjBSwFNj3ndRntsZh
---------------------------------------------------------------------*/

using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .AddCommandLine(args)
            .Build();

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
        //var LaunchAsDifferentUsers = false; //Launch each instance from a different user account | must run Nucleus as admin | will temporary create user accounts "nucleusplayerx" and delete them when closing Nucleus.
        var TransferNucleusUserAccountProfiles = true; // Will backup and restore Nucleus user account user profile's on windows between sessions (when user accounts are not kept).
                                                       // TODO: Is this necessary? Not clear why they've set this when LaunchAsDifferentUsers is false.
        // var UseCurrentUserEnvironment = false; //Force the game to use the current user's environment (useful for some games that may require different Window user accounts).
        var HandlerInterval = 100; // This seems to be a tickrate for launching apps and moving windows. Don't know if we care about this.
        //var SymlinkExe = false;
        var SymlinkGame = true;
        var SymlinkFolders = true;
        //var HardlinkGame = false;
        var ExecutableName = "MCC-Win64-Shipping.exe";
        var SteamID = "976730";
        //var GUID = "Halo MCC"; // that's not a guid pal.
        var GameName = "Halo: The Master Chief Collection";
        var BinariesFolder = "mcc\\binaries\\win64";
        //var MaxPlayers = 16; // maybe someday
        var MaxPlayers = 6;
        //var MaxPlayersOneMonitor = 16; // maybe someday
        var MaxPlayersOneMonitor = 6;
        var LauncherTitle = ""; //The name of the launcher's window title. Some games need to go through a launcher to open. This is needed or else the application will lose the game's window.
        //var Hook.ForceFocus = false; // Used by nucleus to hook the game with x360ce to fake window events, as some games need focus to allow input.
        //var Hook.ForceFocusWindowName = "Halo: The Master Chief Collection"; // Needed for the above.
        var ResetWindows = true; //After each new instance opens, resize, reposition and remove borders of the previous instance.
        var RefreshWindowAfterStart = true; //Should each game window be minimized and restored once all instances are opened?.
                                            // TODO: Is this necessary?
        //var Hook.DInputEnabled = false; //If the game supports direct input joystick
        //var UseDInputBlocker = false;
        //var Hook.XInputEnabled = false; //If the game supports xinput joysticks 
        //var Hook.XInputReroute = false; //If xinput is enabled, if rerouting should be enabled (basically is we'll reroute directinput back to xinput, so we can track more than 4 gamepads on xinput at once). 
        //var Hook.CustomDllEnabled = false; //If the game should be run using Nucleus custom version of x360ce for gamepad control and ForceFocus. Enabled by default as the majority of our games need it. Set it to false if you are using xinput plus dlls for x64 games. 
        var UseForceBindIP = true; //Set up game instances with ForceBindIP; each instance having their own IP. 
        var ForceBindIPNoDummy = true; //ForceBindIP will be used without the "dummy" launch argument, the argument prevents crashes but it causes issues in other games.
        var UserProfileSavePath = "AppData\\Roaming\\Goldberg SteamEmu Saves\\976730\\remote";
        var UserProfileConfigPath = "AppData\\LocalLow\\MCC";
        var UserProfileSavePathNoCopy = true;
        var UserProfileConfigPathNoCopy = true;
        var Description = ""; // Description of the game, used for the NucleusCoop UI.
        var KeepSymLinkOnExit = false;
        var PauseBetweenStarts = 30;
        var ProtoInput.InjectStartup = false;
        var ProtoInput.InjectRuntime_RemoteLoadMethod = false;
        var ProtoInput.InjectRuntime_EasyHookMethod = true;
        var ProtoInput.InjectRuntime_EasyHookStealthMethod = false;


        var LockInputAtStart = false;
        var LockInputSuspendsExplorer = true;
        var ProtoInput.FreezeExternalInputWhenInputNotLocked = false;
        var LockInputToggleKey = 0x23;


        var ProtoInput.RenameHandlesHook = false;
        string[] ProtoInputRenameHandles = [];
        string[] ProtoInputRenameNamedPipes = [];

        var ProtoInput.RegisterRawInputHook = true;
        var ProtoInput.GetRawInputDataHook = false;
        var ProtoInput.MessageFilterHook = true;
        var ProtoInput.GetCursorPosHook = false;
        var ProtoInput.SetCursorPosHook = false;
        var ProtoInput.GetKeyStateHook = false;
        var ProtoInput.GetAsyncKeyStateHook = false;
        var ProtoInput.GetKeyboardStateHook = false;
        var ProtoInput.CursorVisibilityHook = false;
        var ProtoInput.ClipCursorHook = true;
        var ProtoInput.FocusHooks = false;
        var ProtoInput.DrawFakeCursor = false;
        var ProtoInput.FindWindowHook = false;


        var ProtoInput.RawInputFilter = false;
        var ProtoInput.MouseMoveFilter = false;
        var ProtoInput.MouseActivateFilter = false;
        var ProtoInput.WindowActivateFilter = false;
        var ProtoInput.WindowActvateAppFilter = false;
        var ProtoInput.MouseWheelFilter = false;
        var ProtoInput.MouseButtonFilter = false;
        var ProtoInput.KeyboardButtonFilter = false;


        var ProtoInput.SendMouseWheelMessages = true;
        var ProtoInput.SendMouseButtonMessages = true;
        var ProtoInput.SendMouseMovementMessages = true;
        var ProtoInput.SendKeyboardButtonMessages = true;
        var ProtoInput.XinputHook = true;
        var ProtoInput.UseOpenXinput = true;
        var ProtoInput.UseDinputRedirection = false;
        var ProtoInput.DinputDeviceHook = false;
        var ProtoInput.DinputHookAlsoHooksGetDeviceState = false;

    }
}