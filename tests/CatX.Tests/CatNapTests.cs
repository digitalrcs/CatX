using CatX.Services;
using System.Windows;

namespace CatX.Tests;

[TestClass]
public sealed class CatNapTests
{
    [TestMethod]
    public void CatsReserveDistinctNearbyNapsAcrossTheDesktop()
    {
        var area = new Rect(-1600, -100, 1600, 1000);
        // Identical seeds and initial positions deliberately bring every cat to
        // the same preferred nap location at the same time.
        var cats = Enumerable.Range(0, 8).Select(_ => new CatBehavior(area, 42)
            { ChaseCursor = false, PlayfulMouse = false }).ToArray();
        var slept = new HashSet<CatBehavior>();
        var beds = new HashSet<Point>();
        var maximumTogether = 0;
        for (var frame = 0; frame < 120 * 20; frame++)
        {
            foreach (var cat in cats)
            {
                cat.Step(.05, area, new Point(), cats);
                if (cat.Mood == CatMood.Sleeping) { slept.Add(cat); beds.Add(cat.Position); }
            }
            var napping = cats.Where(cat => cat.Mood is CatMood.Sleeping or CatMood.LyingDown).ToArray();
            maximumTogether = Math.Max(maximumTogether, napping.Length);
            for (var i = 0; i < napping.Length; i++)
                for (var j = i + 1; j < napping.Length; j++)
                    Assert.IsGreaterThan(CatBehavior.NapSpacing - 6, (napping[i].Position - napping[j].Position).Length);
            Assert.IsTrue(cats.All(cat => area.Contains(new Rect(cat.Position, new Size(CatBehavior.Width, CatBehavior.Height)))));
        }
        Assert.HasCount(8, slept);
        Assert.AreEqual(8, maximumTogether);
        Assert.IsGreaterThan(100d, beds.Max(p => p.Y) - beds.Min(p => p.Y));
        Assert.IsTrue(beds.Any(p => p.Y > area.Top + 100 && p.Y < area.Bottom - CatBehavior.Height - 100));
    }

    [TestMethod]
    public void TinyDisplayWaitsForFreeNapSpaceInsteadOfStackingSleepers()
    {
        var area = new Rect(0, 0, 180, 150);
        var cats = Enumerable.Range(0, 3).Select(_ => new CatBehavior(area, 42)
            { ChaseCursor = false, PlayfulMouse = false }).ToArray();
        var hadNap = false;
        for (var i = 0; i < 120 * 20; i++)
        {
            foreach (var cat in cats) cat.Step(.05, area, new Point(), cats);
            var sleepers = cats.Count(cat => cat.Mood is CatMood.Sleeping or CatMood.LyingDown);
            hadNap |= sleepers > 0;
            Assert.IsLessThanOrEqualTo(1, sleepers);
        }
        Assert.IsTrue(hadNap);
    }
}
