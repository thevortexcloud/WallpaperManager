using Tmds.DBus;

namespace Cake.Wallpaper.Manager.Core;

public class TestService : ITestService {
    public ObjectPath ObjectPath => Path;
    public static readonly ObjectPath Path = new ObjectPath("/cake/test");

    public Task<string> SayHelloAsync() {
        //Console.WriteLine("Hello!");
        return Task.FromResult("Hello");
    }
}