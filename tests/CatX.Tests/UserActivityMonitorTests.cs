using CatX.Services;

namespace CatX.Tests;

[TestClass]
public sealed class UserActivityMonitorTests
{
    [TestMethod]
    public void ElapsedMilliseconds_HandlesWindowsTickCountRollover()
    {
        var elapsed = UserActivityMonitor.ElapsedMilliseconds(
            currentTick: 25,
            previousTick: uint.MaxValue - 24);

        Assert.AreEqual((uint)50, elapsed);
    }

    [TestMethod]
    public void EffectiveIdle_DoesNotIncludeIdleTimeBeforeMonitoringStarted()
    {
        var effectiveIdle = UserActivityMonitor.EffectiveIdleMilliseconds(
            desktopIdleMilliseconds: 60_000,
            monitoringElapsedMilliseconds: 5_000);

        Assert.AreEqual(5_000, effectiveIdle);
    }

    [TestMethod]
    public void EffectiveIdle_UsesRecentActivityToResetCountdown()
    {
        var effectiveIdle = UserActivityMonitor.EffectiveIdleMilliseconds(
            desktopIdleMilliseconds: 750,
            monitoringElapsedMilliseconds: 20_000);

        Assert.AreEqual(750, effectiveIdle);
    }
}
