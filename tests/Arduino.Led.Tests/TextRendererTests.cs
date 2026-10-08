using Arduino.Led;
using Xunit;

namespace Arduino.Led.Tests;

public class TextRendererTests
{
    [Fact]
    public void RenderColumns_Produces_GlyphPlusSpacing_PerChar()
    {
        byte[] columns = TextRenderer.RenderColumns("AB");
        Assert.Equal(12, columns.Length); // 2 * (5 + 1)

        // 'A' glyph from the embedded font (first 5 columns).
        Assert.Equal(new byte[] { 0x7C, 0x12, 0x11, 0x12, 0x7C }, columns[..5]);
        Assert.Equal(0x00, columns[5]); // spacing
    }

    [Fact]
    public void RenderStatic_Centers_ShortText()
    {
        Frame frame = TextRenderer.RenderStatic("I"); // 6 columns wide
        // Centered: starts at (13-6)/2 = 3. 'I' glyph middle column is 0x7F -> x=5 lit rows 0-6.
        Assert.True(frame.GetPixel(5, 0));
        Assert.True(frame.GetPixel(5, 6));
        Assert.False(frame.GetPixel(0, 0));
        Assert.False(frame.GetPixel(12, 7));
    }

    [Fact]
    public void RenderScroll_FrameCount_Covers_FullSweep()
    {
        byte[] columns = TextRenderer.RenderColumns("HI");
        List<Frame> frames = TextRenderer.RenderScroll("HI");

        // Offsets from Width down to -columns.Length inclusive.
        Assert.Equal(columns.Length + LedMatrix.Width + 1, frames.Count);

        // First frame: text fully off-screen right.
        Assert.Equal(new string('.', LedMatrix.Width), frames[0].ToAscii().Split('\n')[0]);
    }

    [Fact]
    public void RenderBlink_Alternates_OnOff()
    {
        List<Frame> frames = TextRenderer.RenderBlink("X", blinks: 2);
        Assert.Equal(4, frames.Count);

        bool onHasPixels = frames[0].Bits.ToArray().Any(b => b != 0);
        bool offIsEmpty = frames[1].Bits.ToArray().All(b => b == 0);
        Assert.True(onHasPixels);
        Assert.True(offIsEmpty);
    }

    [Fact]
    public void RenderPulse_Uses_GrayscaleRamp()
    {
        List<GrayFrame> frames = TextRenderer.RenderPulse("X", pulses: 1);

        // 1..7 then 6..0 = 14 frames.
        Assert.Equal(14, frames.Count);

        int maxFirst = frames[0].Pixels.ToArray().Max();
        int maxPeak = frames[6].Pixels.ToArray().Max();
        int maxLast = frames[^1].Pixels.ToArray().Max();
        Assert.Equal(1, maxFirst);
        Assert.Equal(7, maxPeak);
        Assert.Equal(0, maxLast);
    }

    [Fact]
    public void BlitColumns_Handles_NegativeOffset()
    {
        var frame = new Frame();
        byte[] columns = TextRenderer.RenderColumns("I");
        TextRenderer.BlitColumns(frame, columns, -2); // first two columns clipped

        // 'I' glyph is 0x00,0x00,0x5F,0x00,0x00 -> visible part starts at column 2 of the glyph.
        Assert.True(frame.GetPixel(0, 0)); // glyph column 2 (0x5F) lands at x=0
    }
}
