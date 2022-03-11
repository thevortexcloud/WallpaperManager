namespace Cake.Wallpaper.Manager.Core.Models;

public record Person() {
    private Franchise? _primaryFranchise;
    public string Name { get; set; }
    public int ID { get; init; }
    public HashSet<Franchise> Franchises { get; init; } = new HashSet<Franchise>();

    public Franchise? PrimaryFranchise {
        get => this._primaryFranchise ?? Franchises.FirstOrDefault();
        set => this._primaryFranchise = value;
    }


    public override string ToString() {
        return $"{this.Name} ({this.PrimaryFranchise?.Name})";
    }
}