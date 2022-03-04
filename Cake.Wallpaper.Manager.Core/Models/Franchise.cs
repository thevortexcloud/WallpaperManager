namespace Cake.Wallpaper.Manager.Core.Models;

public record Franchise {
    public int ID { get; init; }
    public string Name { get; init; }
    public List<ChildFranchise> ChildFranchises { get; init; } = new List<ChildFranchise>();

    public override string ToString() {
        return this.Name;
    }
}

public record ChildFranchise : Franchise {
    public int Parent { get; init; }
}