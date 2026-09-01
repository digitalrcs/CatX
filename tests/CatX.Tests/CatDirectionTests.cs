namespace CatX.Tests;

[TestClass]
public sealed class CatDirectionTests
{
    [TestMethod]
    public void RightwardTravel_MirrorsLeftFacingArtworkToFaceRight()
    {
        Assert.AreEqual(-1, CatOverlayWindow.ScaleForTravel(currentX: 100, targetX: 400));
    }

    [TestMethod]
    public void LeftwardTravel_KeepsLeftFacingArtworkFacingLeft()
    {
        Assert.AreEqual(1, CatOverlayWindow.ScaleForTravel(currentX: 400, targetX: 100));
    }
}
