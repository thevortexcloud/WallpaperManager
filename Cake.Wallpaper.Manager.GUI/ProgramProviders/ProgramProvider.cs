using System;
using System.Diagnostics;
using DynamicData;

namespace Cake.Wallpaper.Manager.GUI.ProgramProviders;

public abstract class ProgramProvider : IProgramProvider {
    #region Abstract properties
    /// <summary>
    /// Returns the display name of the program to open
    /// </summary>
    public abstract string DisplayName { get; }
    #endregion

    #region Abstract methods
    /// <summary>
    /// Attempts to open the given wallpaper in a program that this instance can handle
    /// </summary>
    /// <param name="wallpaper">The wallpaper to pass to the external program</param>
    public abstract void OpenWallpaperInProgram(Core.Models.Wallpaper wallpaper);
    #endregion

    #region Protected methods
    // <summary>
    /// Opens the given flatpak if the program is currently running in a Unix/Linux environment
    /// </summary>
    /// <param name="program">A path to the program, or the program name if it is available in the path</param>
    /// <param name="arguments">The arguments to pass to the program</param>
    protected void OpenFlatPak(string program, string[] arguments) {
        //Flatpaks are only supported on *nix
        if (Environment.OSVersion.Platform != PlatformID.Unix) {
            return;
        }

        using (var process = new Process()) {
            process.StartInfo.FileName = "flatpak";
            process.StartInfo.ArgumentList.Add("run");
            process.StartInfo.ArgumentList.Add(program);
            process.StartInfo.ArgumentList.AddRange(arguments);
            process.StartInfo.UseShellExecute = true;

            process.Start();
        }
    }

    /// <summary>
    /// Opens the given program if the program is currently running in a Unix/Linux environment
    /// </summary>
    /// <param name="program">A path to the program, or the program name if it is available in the path</param>
    /// <param name="arguments">The arguments to pass to the program</param>
    protected void OpenLinuxProgram(string program, string[] arguments) {
        if (Environment.OSVersion.Platform != PlatformID.Unix) {
            return;
        }

        this.OpenProgram(program, arguments);
    }

    /// <summary>
    /// Opens the given program if the program is currently running in a Windows environment
    /// </summary>
    /// <param name="program">A path to the program, or the program name if it is available in the path</param>
    /// <param name="arguments">The arguments to pass to the program</param>
    protected void OpenWindowsProgram(string program, string[] arguments) {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT) {
            return;
        }

        this.OpenProgram(program, arguments);
    }

    /// <summary>
    /// Opens the given program with the given arguments
    /// </summary>
    /// <param name="program">A path to the program, or the program name if it is available in the path</param>
    /// <param name="arguments">The arguments to pass to the program</param>
    protected void OpenProgram(string program, string[] arguments) {
        using (var process = new Process()) {
            process.StartInfo.FileName = program;
            process.StartInfo.ArgumentList.AddRange(arguments);
            process.StartInfo.UseShellExecute = true;

            process.Start();
        }
    }
    #endregion
}