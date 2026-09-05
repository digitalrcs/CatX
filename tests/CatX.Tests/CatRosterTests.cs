using CatX.Models;
using CatX.Services;
using System.Text.Json;
using System.Windows;

namespace CatX.Tests;

[TestClass]
public sealed class CatRosterTests
{
    [TestMethod]
    public void OldSettingsKeepOneCatAndNoCatStillDisablesAll()
    {
        var settings = JsonSerializer.Deserialize<AppSettings>("{\"CatStyle\":\"Calico\"}")!;
        Assert.HasCount(1, CatRoster.Resolve(settings));
        settings.CatCount = 8;
        settings.CatStyle = "No cat";
        Assert.IsEmpty(CatRoster.Resolve(settings));
    }

    [TestMethod]
    public void MultipleDifferentCatsRoundTripAndReceiveIndependentPreferences()
    {
        var settings = new AppSettings { CatCount = 3, CatStyle = "Realistic Tabby",
            AdditionalCatStyles = ["Calico", "Midnight"], PlayfulMouse = false, ChaseCursor = false };
        var saved = JsonSerializer.Deserialize<AppSettings>(JsonSerializer.Serialize(settings))!;
        var roster = CatRoster.Resolve(saved);
        CollectionAssert.AreEqual(new[] { "Realistic Tabby", "Calico", "Midnight" }, roster.Select(s => s.CatStyle).ToArray());
        Assert.IsTrue(roster.All(s => !s.PlayfulMouse && !s.ChaseCursor));
        roster[0].ChaseCursor = true;
        Assert.IsFalse(roster[1].ChaseCursor);
    }

    [TestMethod]
    public void InvalidSelectionsAreBoundedAndCatsStartInDifferentPlaces()
    {
        var settings = new AppSettings { CatCount = 99, CatStyle = "missing", AdditionalCatStyles = null! };
        var roster = CatRoster.Resolve(settings);
        Assert.HasCount(CatRoster.MaximumCats, roster);
        Assert.IsTrue(roster.All(s => CatRoster.Styles.Contains(s.CatStyle)));
        var area = new Rect(-1920, 0, 1920, 1080);
        var cats = Enumerable.Range(0, 8).Select(i => new CatBehavior(area, 42+i, i, 8)).ToArray();
        Assert.AreEqual(8, cats.Select(c => c.Position).Distinct().Count());
        Assert.IsTrue(cats.All(c => area.Contains(new Rect(c.Position, new Size(CatBehavior.Width, CatBehavior.Height)))));
    }
}
