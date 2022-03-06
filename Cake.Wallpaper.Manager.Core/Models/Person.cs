namespace Cake.Wallpaper.Manager.Core.Models;

public record Person() {
    public string Name { get; init; }
    public int ID { get; init; }
    public int? Franchise { get; init; }
}