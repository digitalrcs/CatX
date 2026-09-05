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

    [TestMethod]
    public void NapsHappenWithinFirstMinuteAndLastAtLeastThirtySecondsDespiteToyVisits()
    {
        var area = new Rect(0, 0, 1920, 1080);
        for (var seed = 0; seed < 20; seed++)
        {
            var cat = new CatBehavior(area, seed) { RoamSeconds = 60 };
            var sleepStarted = -1d;
            var sleptFor = 0d;
            for (var i = 0; i < 120 * 60; i++)
            {
                cat.Step(1d / 60, area, new Point(900, 300));
                if (cat.Mood == CatMood.Sleeping)
                {
                    if (sleepStarted < 0) sleepStarted = i / 60d;
                    sleptFor += 1d / 60;
                    Assert.IsFalse(cat.MouseVisible, "Toys must not interrupt a nap.");
                    Assert.AreEqual(area.Bottom - CatBehavior.Height - 8, cat.Position.Y, 3);
                }
                else if (sleepStarted >= 0) break;
            }
            Assert.IsTrue(sleepStarted is >= 0 and < 65, $"Seed {seed}: first nap at {sleepStarted} seconds");
            Assert.IsGreaterThanOrEqualTo(29.9, sleptFor, $"Seed {seed}: nap lasted only {sleptFor} seconds");
        }
    }

    [TestMethod]
    public void ToyMakesSlowContinuousRoundTripThroughSameHole()
    {
        foreach (var seed in new[] { 7, 42, 1234 })
        {
            var area = new Rect(-1920, -120, 1920, 1080);
            var cat = new CatBehavior(area, seed);
            Point? previous = null;
            var hole = new Point();
            var farthest = -40d;
            var visits = 0;
            for (var i = 0; i < 300 * 60; i++)
            {
                cat.Step(1d / 60, area, new Point(-900, 300));
                if (cat.MouseVisible)
                {
                    if (previous is Point last)
                    {
                        Assert.AreEqual(hole, cat.MouseHolePosition);
                        Assert.IsLessThanOrEqualTo(CatBehavior.MouseSpeed / 60 + .001, (cat.MousePosition - last).Length);
                    }
                    else
                    {
                        hole = cat.MouseHolePosition;
                        farthest = -40;
                        Assert.IsLessThanOrEqualTo(-39d, (cat.MousePosition.X - hole.X) * cat.MouseOutwardDirection);
                    }
                    farthest = Math.Max(farthest, (cat.MousePosition.X - hole.X) * cat.MouseOutwardDirection);
                    previous = cat.MousePosition;
                }
                else if (previous is Point last)
                {
                    Assert.IsGreaterThanOrEqualTo(99d, farthest, "Mouse should emerge fully before returning.");
                    Assert.AreEqual(-40, (last.X - hole.X) * cat.MouseOutwardDirection, .01, "Mouse must enter its hole before disappearing.");
                    previous = null;
                    visits++;
                }
            }
            Assert.IsGreaterThanOrEqualTo(2, visits);
        }
    }

    [TestMethod]
    public void MouseHolesVaryAcrossDesktopAndExcursionsExtendBeyondOldLimit()
    {
        var area = new Rect(0, 0, 1920, 1080);
        var cat = new CatBehavior(area, 42);
        var holes = new HashSet<Point>();
        var longest = 0d;
        for (var i = 0; i < 600*60; i++)
        {
            cat.Step(1d/60, area, new Point(900, 300));
            if (!cat.MouseVisible) continue;
            holes.Add(cat.MouseHolePosition);
            longest = Math.Max(longest, cat.MouseTravel);
            Assert.IsTrue(area.Contains(cat.MouseHolePosition));
            Assert.IsGreaterThan(CatBehavior.MouseEscapeDistance, (cat.MousePosition-cat.Center).Length);
        }
        Assert.IsGreaterThanOrEqualTo(3, holes.Count);
        Assert.IsGreaterThan(400d, longest);
        Assert.IsGreaterThan(200d, holes.Max(p => p.Y)-holes.Min(p => p.Y));
    }
}
