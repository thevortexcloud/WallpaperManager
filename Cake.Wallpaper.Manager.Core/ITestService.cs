using Tmds.DBus;

namespace Cake.Wallpaper.Manager.Core;
[DBusInterface("cake.test")]
public interface ITestService : IDBusObject {
    Task<string> SayHelloAsync();
}