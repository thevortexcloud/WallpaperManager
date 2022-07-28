using System;
using System.Diagnostics;
using DynamicData;

namespace Cake.Wallpaper.Manager.GUI.ProgramProviders;

public abstract class ProgramProvider : IProgramProvider {
    public abstract string DisplayName { get; }
    public abstract void OpenWallpaperInProgram(Core.Models.Wallpaper wallpaper);

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
}