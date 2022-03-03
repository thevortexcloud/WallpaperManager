using Cake.Wallpaper.Manager.Core;
using Tmds.DBus;

public static class Program {
    static async Task Main(string[] args) {
        // server
        var server = new ServerConnectionOptions();
        using (var connection = new Connection(server)) {
            await connection.RegisterObjectAsync(new TestService());
            var boundAddress = await server.StartAsync( "unix:path=/tmp/dbus-test");
            System.Console.WriteLine($"Server listening at {boundAddress}");
            Console.ReadLine();

            //Console.WriteLine(await connection.ListServicesAsync());
            // client
            /*using (var client = new Connection(boundAddress)) {
                await client.ConnectAsync();
                System.Console.WriteLine("Client connected");
                var proxy = client.CreateProxy<ITestService>("cake.test", TestService.Path);
                Console.WriteLine(await proxy.SayHelloAsync());
            }*/
        }
    }
}