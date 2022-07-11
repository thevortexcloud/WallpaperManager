using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using MessageBox.Avalonia.Enums;

namespace Cake.Wallpaper.Manager.GUI;

internal static class Common {
    public static async Task ShowExceptionMessageBoxAsync(this Window owner, string message, Exception exception) {
        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Error encountered", $"{message}{Environment.NewLine}{Environment.NewLine}Would you like to know more?", ButtonEnum.YesNo);

        if ((await messageBoxStandardWindow.ShowDialog(owner)) == ButtonResult.Yes) {
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error encountered", exception.ToString()).ShowDialog(owner);
        }
    }

    public static async Task ShowExceptionMessageBoxAsync(string message, Exception exception) {
        var messageBoxStandardWindow = MessageBox.Avalonia.MessageBoxManager
            .GetMessageBoxStandardWindow("Error encountered", $"{message}{Environment.NewLine}{Environment.NewLine}Would you like to know more?", ButtonEnum.YesNo);

        if ((await messageBoxStandardWindow.Show()) == ButtonResult.Yes) {
            await MessageBox.Avalonia.MessageBoxManager.GetMessageBoxStandardWindow("Error encountered", exception.ToString()).Show();
        }
    }
}