namespace CatX.Tests;

[TestClass]
public sealed class CatCoatPatternTests
{
    [TestMethod]
    [DataRow("Midnight")]
    [DataRow("Tuxedo")]
    public void SolidAndTuxedoCoats_DoNotShowStraySpots(string style)
    {
        var palette = CatOverlayWindow.PaletteFor(style);

        Assert.IsFalse(palette.ShowHeadPatch);
        Assert.IsFalse(palette.ShowBodyPatch);
    }

    [TestMethod]
    public void CalicoCoat_KeepsSeparateHeadAndBodyMarkings()
    {
        var palette = CatOverlayWindow.PaletteFor("Calico");

        Assert.IsTrue(palette.ShowHeadPatch);
        Assert.IsTrue(palette.ShowBodyPatch);
        Assert.AreNotEqual(palette.HeadPatch, palette.BodyPatch);
    }
}
