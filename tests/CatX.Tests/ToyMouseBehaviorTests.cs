using CatX.Services;
using System.Windows;

namespace CatX.Tests;

[TestClass]
public sealed class ToyMouseBehaviorTests
{
    [TestMethod]
    public void MouseFindsCheeseCarriesItAndDeliversThroughSameHole()
    {
        foreach (var seed in new[] { 7, 42, 1234 })
        {
            var area = new Rect(-1920, -120, 1920, 1080);
            var mouse = new ToyMouseBehavior(seed);
            mouse.RequestVisit();
            var points = new List<Point>();
            var hole = new Point();
            Point? previous = null;
            var visited = false;
            var returned = false;
            var carried = false;
            var reachedCheese = false;
            for (var i = 0; i < 150 * 60; i++)
            {
                mouse.Step(1d / 60, area, [], true);
                if (mouse.Visible)
                {
                    if (previous is Point last)
                    {
                        Assert.AreEqual(hole, mouse.Hole);
                        Assert.IsLessThanOrEqualTo(ToyMouseBehavior.WalkSpeed / 60 + .001, (mouse.Position - last).Length);
                    }
                    else hole = mouse.Hole;
                    Assert.IsTrue(area.Contains(new Rect(mouse.Position.X - 29, mouse.Position.Y - 17, 58, 34)));
                    carried |= mouse.CarryingCheese;
                    reachedCheese |= (mouse.Position - mouse.CheesePosition).Length < 3;
                    points.Add(mouse.Position);
                    previous = mouse.Position;
                    visited |= mouse.Activity == MouseActivity.SeekingCheese;
                }
                else if (previous.HasValue)
                {
                    Assert.IsLessThanOrEqualTo(3d, (mouse.Position - (hole - new Vector(mouse.Outward * 40, 0))).Length);
                    returned = true;
                    break;
                }
            }
            Assert.IsTrue(visited && returned, $"Seed {seed}: {mouse.Activity}, position {mouse.Position}, hole {mouse.Hole}, cheese {mouse.CheesePosition}, delivered {mouse.Deliveries}");
            Assert.IsTrue(carried && reachedCheese);
            Assert.AreEqual(1, mouse.Deliveries);
            Assert.AreEqual(0, mouse.Catches);
            Assert.IsGreaterThan(500d, points.Max(p => (p - hole).Length));
        }
    }

    [TestMethod]
    public void EightCatsFollowOneMouseWithoutDelayingNaps()
    {
        var area = new Rect(0, 0, 1920, 1080);
        var cats = Enumerable.Range(0, 8).Select(i => new CatBehavior(area, i, i, 8) { ChaseCursor = false }).ToArray();
        var mouse = new ToyMouseBehavior(42);
        var returns = 0;
        var followers = new HashSet<CatBehavior>();
        var sleepers = new HashSet<CatBehavior>();
        for (var i = 0; i < 180 * 60; i++)
        {
            var previous = mouse.Position;
            var wasVisible = mouse.Visible;
            mouse.Step(1d / 60, area, cats, true);
            if (wasVisible && !mouse.Visible) returns++;
            if (wasVisible && mouse.Visible)
                Assert.IsLessThanOrEqualTo(ToyMouseBehavior.EscapeSpeed / 60 + .001, (mouse.Position - previous).Length);
            foreach (var cat in cats)
            {
                cat.Step(1d / 60, area, new Point(), cats, mouse);
                if (cat.Mood == CatMood.MouseChase) followers.Add(cat);
                if (cat.Mood == CatMood.Sleeping) sleepers.Add(cat);
                Assert.IsTrue(area.Contains(new Rect(cat.Position, new Size(CatBehavior.Width, CatBehavior.Height))));
            }
        }
        Assert.IsGreaterThan(0, returns, "The shared mouse must be able to return even with eight cats.");
        Assert.HasCount(8, followers);
        Assert.HasCount(8, sleepers);
    }

    [TestMethod]
    public void CatsCanCatchMouseAndMouseCanWinCheeseTrips()
    {
        var area = new Rect(0, 0, 1600, 1000);
        var catches = 0;
        var deliveries = 0;
        var ran = false;
        for (var seed = 0; seed < 8; seed++)
        {
            var cats = Enumerable.Range(0, 3).Select(i => new CatBehavior(area, seed * 10 + i, i, 3) { ChaseCursor = false }).ToArray();
            var mouse = new ToyMouseBehavior(seed);
            mouse.RequestVisit();
            for (var i = 0; i < 240 * 20; i++)
            {
                mouse.Step(.05, area, cats, true);
                foreach (var cat in cats)
                {
                    cat.Step(.05, area, new Point(), cats, mouse);
                    ran |= cat.Mood == CatMood.MouseChase && cat.Speed > 170;
                }
                if (mouse.Activity == MouseActivity.Caught)
                {
                    Assert.IsFalse(mouse.CarryingCheese);
                    var count = mouse.Catches;
                    foreach (var cat in cats) Assert.IsFalse(mouse.TryCatch(cat));
                    Assert.AreEqual(count, mouse.Catches, "Only one cat can win a catch.");
                }
            }
            catches += mouse.Catches;
            deliveries += mouse.Deliveries;
        }
        Assert.IsTrue(ran);
        Assert.IsGreaterThan(0, catches);
        Assert.IsGreaterThan(0, deliveries);
    }

    [TestMethod]
    public void DisableDisplayChangeAndLongPauseAreHandled()
    {
        var area = new Rect(0, 0, 1280, 800);
        var mouse = new ToyMouseBehavior(42);
        mouse.RequestVisit();
        mouse.Step(.016, area, [], true);
        Assert.IsTrue(mouse.Visible);
        var before = mouse.Position;
        mouse.Step(60, area, [], true);
        Assert.IsLessThanOrEqualTo(ToyMouseBehavior.WalkSpeed * .05 + .001, (mouse.Position - before).Length);
        mouse.Step(.016, new Rect(-1280, 0, 1280, 800), [], true);
        Assert.IsFalse(mouse.Visible);
        mouse.RequestVisit();
        mouse.Step(.016, area, [], true);
        mouse.Step(.016, area, [], false);
        Assert.IsFalse(mouse.Visible);
        mouse.RequestVisit();
        mouse.Step(.016, new Rect(-100, 0, 100, 100), [], true);
        Assert.IsFalse(mouse.Visible);
    }
}
