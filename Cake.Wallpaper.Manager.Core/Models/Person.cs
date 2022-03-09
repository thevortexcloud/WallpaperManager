namespace Cake.Wallpaper.Manager.Core.Models;

public record Person() {
    public string Name { get; set; }
    public int ID { get; init; }
    public HashSet<Franchise> Franchises { get; init; } = new HashSet<Franchise>();
    public Franchise? PrimaryFranchise => Franchises?.FirstOrDefault();

    public override string ToString() {
        return $"{this.Name} ({this.PrimaryFranchise?.Name})";
    }
}