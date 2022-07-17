namespace Cake.Wallpaper.Manager.Core.Models;

public class Franchise {
    #region Public properties
    /// <summary>
    /// Returns the ID of the franchise, or 0 if this is a new franchise
    /// </summary>
    public int ID { get; init; }

    /// <summary>
    /// Gets/sets the display name of the franchise
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets/sets the depth of the wallpaper in the hierarchy
    /// </summary>
    public int Depth { get; set; }

    /// <summary>
    /// Gets/sets the parent ID of the franchise
    /// </summary>
    public int? ParentID { get; set; }
    #endregion

    #region Object overrides
    public override int GetHashCode() {
        return ID;
    }

    public override bool Equals(Object? obj) {
        //We can override the equality to make it compare by the ID of the franchise rather than by the hash
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
    #endregion
}