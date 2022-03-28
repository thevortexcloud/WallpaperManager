namespace Cake.Wallpaper.Manager.Core.Models;

public class Franchise {
    public int ID { get; init; }
    public string Name { get; set; }
    public HashSet<Franchise> ChildFranchises { get; init; } = new HashSet<Franchise>();

    public int? ParentID { get; set; }

    public override int GetHashCode() {
        return ID;
    }

    public override bool Equals(Object? obj) {
        if (obj is Franchise other) {
            if (other is null) {
                return false;
            }

            if (other.ID == this.ID) {
                return true;
            }
        }

        return false;
    }
}