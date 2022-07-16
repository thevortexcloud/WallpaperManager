using System.Collections.Generic;
using System.Linq;
using Cake.Wallpaper.Manager.Core.Models;
using Cake.Wallpaper.Manager.GUI.ViewModels;
using Xunit;

namespace Cake.Wallpaper.Manager.Tests.Core.GUI.Viewmodels;

public sealed class FranchiseSelectListItemViewModelTests {
    [Fact]
    public void CanFindSelectedChildFranchise() {
        //Create dummy object with single selected element
        var model = new FranchiseSelectListItemViewModel(new Franchise() {
                Name = "Blah",
                ID = 1,
                ChildFranchises = new HashSet<Franchise>() {
                    new Franchise() {
                        Name = "Blah 2",
                        ID = 2,
                        ParentID = 1,
                    }
                }
            },
            true);

        var result = model.FindSelectedChildFranchises();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.True(result.All(o => o.Selected));
        Assert.Empty(result.SelectMany(o => o.ChildFranchises));
    }

    [Fact]
    public void CanFindSelectedChildFranchises() {
        //Create dummy object with multiple selected elements
        var model = new FranchiseSelectListItemViewModel(new Franchise() {
                Name = "Blah",
                ID = 1,
                ChildFranchises = new HashSet<Franchise>() {
                    new Franchise() {
                        Name = "Blah 2",
                        ID = 2,
                        ParentID = 1,
                    },
                    new Franchise() {
                        Name = "Blah blah",
                        ID = 3,
                        ParentID = 1,
                    }
                },
            },
            true);

        var result = model.FindSelectedChildFranchises();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Equal(2, result.Count);
        Assert.True(result.All(o => o.Selected));
        Assert.Empty(result.SelectMany(o => o.ChildFranchises));
    }

    [Fact]
    public void CanFindPartiallySelectedChildFranchises() {
        //Create dummy object with multiple selected elements
        var model = new FranchiseSelectListItemViewModel(new Franchise() {
                Name = "Blah",
                ID = 1,
                ChildFranchises = new HashSet<Franchise>() {
                    new Franchise() {
                        Name = "Blah 2",
                        ID = 2,
                        ParentID = 1,
                    },
                    new Franchise() {
                        Name = "Blah blah",
                        ID = 3,
                        ParentID = 1,
                    }
                },
            },
            true);
        //Set this to false to simulate the user only picking some options
        model.ChildFranchises[0].Selected = false;


        var result = model.FindSelectedChildFranchises();
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.Single(result);
        Assert.Contains(result, o => o.Selected);
        Assert.Empty(result.SelectMany(o => o.ChildFranchises));
    }
}