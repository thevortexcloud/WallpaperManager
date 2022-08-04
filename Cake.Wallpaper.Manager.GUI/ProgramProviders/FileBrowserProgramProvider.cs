using System;
using System.IO;

namespace Cake.Wallpaper.Manager.GUI.ProgramProviders;

/// <summary>
/// A program provider that attempts to open the given wallpaper in a file browser (EG Dolphin on Linux and Windows Explorer on Windows)
/// </summary>
public class FileBrowserProgramProvider : ProgramProvider {
    #region Private readonly variables
    private readonly string _basePath;
    #endregion

    #region Public constructor
    public FileBrowserProgramProvider(string basePath) {
        this._basePath = basePath;
    }
    #endregion

    #region ProgramProvider implementation
    public override string DisplayName => "File Browser";

    public override void OpenWallpaperInProgram(Core.Models.Wallpaper wallpaper) {
        if (wallpaper?.FilePath == null) {
            throw new ArgumentNullException(nameof(wallpaper));
        }

        var filepath = Path.Combine(this._basePath, wallpaper.FilePath);
        switch (Environment.OSVersion.Platform) {
            case PlatformID.Win32NT:
                //TODO: Test this on Windows, I have no idea if this actually works as I don't use Windows. Stackoverflow says it should though
                //https://stackoverflow.com/questions/13680415/how-to-open-explorer-with-a-specific-file-selected
                this.OpenWindowsProgram("explorer.exe", new string[] {$"/select,{filepath}"});
                break;
            case PlatformID.Unix:
                //For now only handle Dolphin. Gnome is something we can worry about if anybody ever complains about it
                this.OpenLinuxProgram("dolphin", new[] {"--select", filepath});
                break;
            default: throw new ApplicationException("Unknown operating system");
        }
    }
    #endregion
}