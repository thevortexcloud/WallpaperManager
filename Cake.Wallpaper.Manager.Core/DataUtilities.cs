using Cake.Wallpaper.Manager.Core.Models;

namespace Cake.Wallpaper.Manager.Core;

public static class DataUtilities {
    /// <summary>
    /// Flattens a list of franchises into a single list
    /// </summary>
    /// <param name="franchises">The list of franchises to flatten</param>
    /// <returns>The flattened list of franchises</returns>
    public static IEnumerable<Franchise> FlattenFranchiseList(IEnumerable<Franchise> franchises) {
        var result = new List<Franchise>();
        foreach (var franchise in franchises) {
            if (franchise.ChildFranchises.Any()) {
                result.AddRange(FlattenFranchiseList(franchise.ChildFranchises));
                franchise.ChildFranchises.Clear();
            }

            result.Add(franchise);
        }

        return result.OrderBy(o => o.ParentID);
    }
}