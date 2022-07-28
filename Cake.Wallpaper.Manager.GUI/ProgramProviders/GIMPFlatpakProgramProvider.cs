using System;
using System.Diagnostics;
using System.IO;

namespace Cake.Wallpaper.Manager.GUI.ProgramProviders;

public class GIMPFlatpakProgramProvider : ProgramProvider {
    private readonly string _basePath;
    public override string DisplayName => "GIMP";

    public GIMPFlatpakProgramProvider(string basePath) {
        this._basePath = basePath;
    }

    public override void OpenWallpaperInProgram(Core.Models.Wallpaper wallpaper) {
        if (wallpaper?.FilePath is null) {
            throw new ArgumentNullException(nameof(wallpaper));
        }

        this.OpenFlatPak("org.gimp.GIMP", new string[] {Path.Combine(this._basePath, wallpaper.FilePath)});
    }
}