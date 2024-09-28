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
        var UseNucleusEnvironment = true;
        var UseGoldberg = true;
        var GoldbergExperimental = true;
        var GoldbergNoLocalSave = true;
        //var LaunchAsDifferentUsers = false;
        var TransferNucleusUserAccountProfiles = true;
        var UseCurrentUserEnvironment = false;
        var HandlerInterval = 100;
        var SymlinkExe = false;
        var SymlinkGame = true;
        var SymlinkFolders = true;
        var HardlinkGame = false;
        var ExecutableName = "MCC-Win64-Shipping.exe";
        var SteamID = "976730";
        var GUID = "Halo MCC";
        var GameName = "Halo: The Master Chief Collection";
        var BinariesFolder = "mcc\\binaries\\win64";
        var MaxPlayers = 16;
        var MaxPlayersOneMonitor = 16;
        var LauncherTitle = "";             
        var Hook.ForceFocus = false;
        var Hook.ForceFocusWindowName = "Halo: The Master Chief Collection";
        var ResetWindows = true;
        var RefreshWindowAfterStart = true;
        var Hook.DInputEnabled = false;
        var UseDInputBlocker = false;
        var Hook.XInputEnabled = false;
        var Hook.XInputReroute = false;
        var Hook.CustomDllEnabled = false;
        var UseForceBindIP = true;
        var ForceBindIPNoDummy = true;
        var UserProfileSavePath = "AppData\\Roaming\\Goldberg SteamEmu Saves\\976730\\remote";
        var UserProfileConfigPath = "AppData\\LocalLow\\MCC";
        var UserProfileSavePathNoCopy = true;
        var UserProfileConfigPathNoCopy = true;
        var Description = "IMPORTANT: Do not place Nucleus inside the game files. Change network type to LAN in the game settings for all instances, then press X on your gamepad or click the roster icon to join the other players. If you are using keyboards/mice after all the instances have launched, resized and positioned correctly, press the END key once to lock the input for all instances to have their own working cursor, you need to left click each mouse to make the emulated cursors appear after locking the input. Press the END key again to unlock the input when you finish playing. You can also use CTRL+Q to close Nucleus and all its instances when the input is unlocked. If you get stretching in Halo 4 try locking the input until you are in-var  The new offline LAN option is the default now and the recommended one being a lot easier to use but if you still want to use the old Online connection method you have to select the Online mode UI option, if you select to enable this option Nucleus will launch an instance then close, open Nucleus again and the UI option will be enabled (check status), using the Online mode Nucleus will launch all instances under different temporary windows users so each instance can log-in using different accounts and can connect to each other, you need to launch Nucleus as admin for this, easy anticheat modes not supported. Log in using different microsoft accounts in each instance. To connect the instances open the roster and invite the other players. For this handler to work Nucleus needs to be in the root of your drive outside any users folders, C: is recommended, for example C:\\NucleusCoop, you can also try giving full permissions to the game and Nucleus folders to all users. You need internet connection if you are using the online option cause the instances connect via the xbox live servers.";
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