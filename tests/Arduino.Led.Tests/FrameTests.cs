using Arduino.Led;
using Xunit;

namespace Arduino.Led.Tests;

public class FrameTests
{
    [Fact]
    public void SetAndGetPixel_RoundTrips()
    {
        var frame = new Frame();
        frame.SetPixel(0, 0, true);
        frame.SetPixel(12, 7, true);
        frame.SetPixel(5, 3, true);

        Assert.True(frame.GetPixel(0, 0));
        Assert.True(frame.GetPixel(12, 7));
        Assert.True(frame.GetPixel(5, 3));
        Assert.False(frame.GetPixel(1, 0));
        Assert.False(frame.GetPixel(0, 1));
    }

    [Fact]
    public void OutOfRange_Pixels_AreIgnored()
    {
        var frame = new Frame();
        frame.SetPixel(-1, 0, true);
        frame.SetPixel(13, 0, true);
        frame.SetPixel(0, -1, true);
        frame.SetPixel(0, 8, true);

        Assert.False(frame.GetPixel(-1, 0));
        Assert.Equal(new string('.', LedMatrix.Width), frame.ToAscii().Split('\n')[0]);
    }

    [Fact]
    public void Clear_RemovesAllPixels()
    {
        var frame = new Frame();
        frame.SetPixel(6, 4, true);
        frame.Clear();
        Assert.False(frame.GetPixel(6, 4));
        Assert.Equal(0, frame.Bits.ToArray().Sum(b => b));
    }

    [Fact]
    public void Bits_Are_Packed_RowMajor()
    {
        var frame = new Frame();
        frame.SetPixel(0, 0, true); // bit 0
        frame.SetPixel(12, 0, true); // bit 12
        frame.SetPixel(0, 1, true); // bit 13
        frame.SetPixel(12, 7, true); // bit 103

        Assert.Equal(0x01, frame.Bits[0]); // bit 0
        Assert.Equal(0x30, frame.Bits[1]); // bits 12 and 13
        Assert.Equal(0x80, frame.Bits[12]); // bit 103
        Assert.True(frame.GetPixel(12, 7));
        Assert.False(frame.GetPixel(11, 7));
    }

    [Fact]
    public void CopyFrom_Rejects_WrongLength()
    {
        var frame = new Frame();
        Assert.Throws<ArgumentException>(() => frame.CopyFrom(new byte[12]));
    }

    [Fact]
    public void GrayFrame_Clamps_Brightness()
    {
        var gray = new GrayFrame();
        gray.SetPixel(0, 0, 99);
        gray.SetPixel(1, 0, -5);
        Assert.Equal(LedMatrix.MaxBrightness, gray.GetPixel(0, 0));
        Assert.Equal(0, gray.GetPixel(1, 0));
    }

    [Fact]
    public void GrayFrame_FromFrame_Maps_On_To_FullBrightness()
    {
        var frame = new Frame();
        frame.SetPixel(3, 3, true);
        GrayFrame gray = GrayFrame.FromFrame(frame);
        Assert.Equal(7, gray.GetPixel(3, 3));
        Assert.Equal(0, gray.GetPixel(4, 3));
    }
}
