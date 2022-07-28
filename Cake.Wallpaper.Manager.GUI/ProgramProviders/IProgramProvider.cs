namespace Cake.Wallpaper.Manager.GUI.ProgramProviders {
    public interface IProgramProvider {
        public string DisplayName { get; }
        public void OpenWallpaperInProgram(Core.Models.Wallpaper wallpaper);
    }
}