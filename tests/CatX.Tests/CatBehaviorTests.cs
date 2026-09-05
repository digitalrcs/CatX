using CatX.Services;
using System.Windows;

namespace CatX.Tests;

[TestClass]
public sealed class CatBehaviorTests
{
    [TestMethod]
    public void OrdinaryCursorTravelAndStationaryCursorDoNotExciteCat()
    {
        var detector = new CursorExcitement();
        for (var i = 0; i < 60; i++) Assert.IsFalse(detector.Observe(new Point(i * 8, 100), 1d / 60));
        for (var i = 0; i < 60; i++) Assert.IsFalse(detector.Observe(new Point(472, 100), 1d / 60));
    }

    [TestMethod]
    public void RapidReversalsTriggerChaseAndSettingDisablesIt()
    {
        var area = new Rect(0, 0, 1280, 800);
        var cat = new CatBehavior(area, 42) { PlayfulMouse = false };
        for (var i = 0; i < 12; i++) cat.Step(1d / 60, area, new Point(i % 2 == 0 ? 600 : 620, 300));
        Assert.AreEqual(CatMood.CursorChase, cat.Mood);
        cat.ChaseCursor = false;
        cat.Step(1d / 60, area, new Point(600, 300));
        Assert.AreNotEqual(CatMood.CursorChase, cat.Mood);
    }

    [TestMethod]
    public void AllBehaviorsStayOnMonitorAndToyEscapesBeforeContact()
    {
        var area = new Rect(-1920, -120, 1920, 1080);
        var cat = new CatBehavior(area, 1234);
        var observed = new HashSet<CatMood>();
        for (var i = 0; i < 600 * 60; i++)
        {
            var before = cat.Position;
            cat.Step(1d / 60, area, new Point(-900, 300));
            observed.Add(cat.Mood);
            Assert.IsTrue(area.Contains(new Rect(cat.Position, new Size(CatBehavior.Width, CatBehavior.Height))));
            Assert.IsLessThanOrEqualTo(285d / 60 + .001, (cat.Position - before).Length, "No teleporting during ordinary animation.");
            if (cat.MouseVisible) Assert.IsGreaterThan(CatBehavior.MouseEscapeDistance - .01, (cat.MousePosition - cat.Center).Length, "Toy must escape before physical contact.");
            if (Math.Abs(cat.Position.X - before.X) > .04) Assert.AreEqual(cat.Position.X > before.X ? -1d : 1d, cat.Facing);
        }
        foreach (var mood in new[] { CatMood.Walking, CatMood.Sitting, CatMood.Grooming, CatMood.LyingDown, CatMood.Sleeping, CatMood.Waking, CatMood.MouseChase })
            Assert.Contains(mood, observed, $"Did not exercise {mood}");
    }

    [TestMethod]
    public void SleepingCatWakesBeforeChasingAndDisablingToyRemovesIt()
    {
        var area = new Rect(0, 0, 1280, 800);
        var cat = new CatBehavior(area, 1234);
        var slept = false;
        var sawToy = false;
        for (var i = 0; i < 36000; i++)
        {
            cat.Step(1d / 60, area, new Point(600, 300));
            if (cat.MouseVisible && !sawToy)
            {
                sawToy = true;
                cat.PlayfulMouse = false;
                cat.Step(1d / 60, area, new Point(600, 300));
                Assert.IsFalse(cat.MouseVisible);
            }
            if (cat.Mood == CatMood.Sleeping) { slept = true; break; }
        }
        Assert.IsTrue(slept);
        Assert.IsTrue(sawToy);
        for (var i = 0; i < 12; i++) cat.Step(1d / 60, area, new Point(i % 2 == 0 ? 600 : 620, 300));
        Assert.AreEqual(CatMood.Waking, cat.Mood);
        for (var i = 0; i < 140; i++) cat.Step(1d / 60, area, new Point(i % 2 == 0 ? 600 : 620, 300));
        Assert.AreEqual(CatMood.CursorChase, cat.Mood);
    }

    [TestMethod]
    public void TinyDisplayAndLongUiPauseDoNotThrowOrJump()
    {
        var area = new Rect(-200, 0, 180, 150);
        var cat = new CatBehavior(area, 7);
        for (var i = 0; i < 100; i++) cat.Step(60, area, new Point(-100, 50));
        Assert.AreEqual(new Point(-200, 0), cat.Position);
    }
}
