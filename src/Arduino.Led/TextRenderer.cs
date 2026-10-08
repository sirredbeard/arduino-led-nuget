namespace Arduino.Led;

/// <summary>
/// Renders text into matrix frames using the embedded 5x8 font. Pure managed
/// code, no hardware calls. The caller decides how frames reach the board.
/// </summary>
public static class TextRenderer
{
    private const int CharSpacing = 1;

    /// <summary>
    /// Renders <paramref name="text"/> to a column strip: one byte per column,
    /// bit 0 is the top pixel, matching the font layout. Each glyph contributes
    /// 5 columns plus one blank spacing column.
    /// </summary>
    public static byte[] RenderColumns(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var columns = new List<byte>(text.Length * (Font5x7.GlyphWidth + CharSpacing));
        foreach (char c in text)
        {
            ReadOnlySpan<byte> glyph = Font5x7.GetGlyph(c);
            foreach (byte column in glyph)
            {
                columns.Add(column);
            }

            columns.Add(0x00); // spacing
        }

        return columns.ToArray();
    }

    /// <summary>Renders <paramref name="text"/> as a single centered frame.</summary>
    public static Frame RenderStatic(string text)
    {
        byte[] columns = RenderColumns(text);
        var frame = new Frame();

        int startX = Math.Max(0, (LedMatrix.Width - columns.Length) / 2);
        BlitColumns(frame, columns, startX);
        return frame;
    }

    /// <summary>
    /// Renders <paramref name="text"/> as a scrolling marquee: the text enters
    /// from the right, crosses the matrix, and exits to the left.
    /// </summary>
    public static List<Frame> RenderScroll(string text)
    {
        byte[] columns = RenderColumns(text);
        var frames = new List<Frame>(columns.Length + LedMatrix.Width + 1);

        // Column 0 of the strip starts off-screen right (x = Width) and ends
        // off-screen left (x = -stripWidth).
        for (int offset = LedMatrix.Width; offset >= -columns.Length; offset--)
        {
            var frame = new Frame();
            BlitColumns(frame, columns, offset);
            frames.Add(frame);
        }

        return frames;
    }

    /// <summary>Renders <paramref name="text"/> as on/off blink frames.</summary>
    public static List<Frame> RenderBlink(string text, int blinks)
    {
        if (blinks < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(blinks), "Blink count must be at least 1.");
        }

        Frame on = RenderStatic(text);
        var off = new Frame();
        var frames = new List<Frame>(blinks * 2);
        for (int i = 0; i < blinks; i++)
        {
            frames.Add(on);
            frames.Add(off);
        }

        return frames;
    }

    /// <summary>
    /// Renders <paramref name="text"/> as grayscale pulse frames: brightness
    /// ramps 0-7 and back, using the matrix's 3-bit grayscale support.
    /// </summary>
    public static List<GrayFrame> RenderPulse(string text, int pulses)
    {
        if (pulses < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(pulses), "Pulse count must be at least 1.");
        }

        Frame mask = RenderStatic(text);
        var frames = new List<GrayFrame>(pulses * (LedMatrix.MaxBrightness * 2));
        for (int p = 0; p < pulses; p++)
        {
            for (int b = 1; b <= LedMatrix.MaxBrightness; b++)
            {
                frames.Add(ApplyBrightness(mask, b));
            }

            for (int b = LedMatrix.MaxBrightness - 1; b >= 0; b--)
            {
                frames.Add(ApplyBrightness(mask, b));
            }
        }

        return frames;
    }

    private static GrayFrame ApplyBrightness(Frame mask, int brightness)
    {
        var gray = new GrayFrame();
        for (int y = 0; y < LedMatrix.Height; y++)
        {
            for (int x = 0; x < LedMatrix.Width; x++)
            {
                if (mask.GetPixel(x, y))
                {
                    gray.SetPixel(x, y, brightness);
                }
            }
        }

        return gray;
    }

    /// <summary>
    /// Blits a column strip into <paramref name="frame"/> at horizontal position
    /// <paramref name="x0"/> (may be negative for off-screen columns).
    /// </summary>
    public static void BlitColumns(Frame frame, ReadOnlySpan<byte> columns, int x0)
    {
        ArgumentNullException.ThrowIfNull(frame);

        for (int i = 0; i < columns.Length; i++)
        {
            int x = x0 + i;
            if ((uint)x >= LedMatrix.Width)
            {
                continue;
            }

            byte column = columns[i];
            for (int y = 0; y < LedMatrix.Height; y++)
            {
                if ((column & (1 << y)) != 0)
                {
                    frame.SetPixel(x, y, true);
                }
            }
        }
    }
}
