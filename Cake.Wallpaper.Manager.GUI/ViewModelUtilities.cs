using System.Collections.Generic;
using System.Linq;
using Cake.Wallpaper.Manager.GUI.ViewModels;

namespace Cake.Wallpaper.Manager.GUI;

public static class ViewModelUtilities {
    public static IEnumerable<FranchiseSelectListItemViewModel> FlattenFranchiseList(IEnumerable<FranchiseSelectListItemViewModel> franchiseSelectListItemViewModel) {
        var result = new List<FranchiseSelectListItemViewModel>();
        foreach (var franchisse in franchiseSelectListItemViewModel) {
            result.Add(franchisse);
            if (franchisse?.ChildFranchises?.Any() ?? false) {
                result.AddRange(FlattenFranchiseList(franchisse.ChildFranchises));
            }

            //franchisse?.ChildFranchises?.Clear();
        }

        return result;
    }
}