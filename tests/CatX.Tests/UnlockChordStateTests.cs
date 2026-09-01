using CatX.Models;
using CatX.Services;

namespace CatX.Tests;

[TestClass]
public sealed class UnlockChordStateTests
{
    [TestMethod]
    public void CtrlAltK_UnlocksWhileSuppressedModifiersAreHeld()
    {
        var state = new UnlockChordState();

        Assert.IsFalse(state.Process(UnlockChordState.VkLControl, true, UnlockChord.CtrlAltK));
        Assert.IsFalse(state.Process(UnlockChordState.VkRMenu, true, UnlockChord.CtrlAltK));

        Assert.IsTrue(state.Process(UnlockChordState.VkK, true, UnlockChord.CtrlAltK));
    }

    [TestMethod]
    public void CtrlShiftF12_AcceptsRightAndLeftModifierKeys()
    {
        var state = new UnlockChordState();

        state.Process(UnlockChordState.VkRControl, true, UnlockChord.CtrlShiftF12);
        state.Process(UnlockChordState.VkLShift, true, UnlockChord.CtrlShiftF12);

        Assert.IsTrue(state.Process(UnlockChordState.VkF12, true, UnlockChord.CtrlShiftF12));
    }

    [TestMethod]
    public void AltShiftPause_UnlocksWhileModifiersAreHeld()
    {
        var state = new UnlockChordState();

        state.Process(UnlockChordState.VkLMenu, true, UnlockChord.AltShiftPause);
        state.Process(UnlockChordState.VkRShift, true, UnlockChord.AltShiftPause);

        Assert.IsTrue(state.Process(UnlockChordState.VkPause, true, UnlockChord.AltShiftPause));
    }

    [TestMethod]
    public void ReleasedModifier_DoesNotUnlock()
    {
        var state = new UnlockChordState();

        state.Process(UnlockChordState.VkLControl, true, UnlockChord.CtrlAltK);
        state.Process(UnlockChordState.VkLMenu, true, UnlockChord.CtrlAltK);
        state.Process(UnlockChordState.VkLMenu, false, UnlockChord.CtrlAltK);

        Assert.IsFalse(state.Process(UnlockChordState.VkK, true, UnlockChord.CtrlAltK));
    }

    [TestMethod]
    public void Reset_ClearsTrackedModifiers()
    {
        var state = new UnlockChordState();

        state.Process(UnlockChordState.VkLControl, true, UnlockChord.CtrlAltK);
        state.Process(UnlockChordState.VkLMenu, true, UnlockChord.CtrlAltK);
        state.Reset();

        Assert.IsFalse(state.Process(UnlockChordState.VkK, true, UnlockChord.CtrlAltK));
    }
}
