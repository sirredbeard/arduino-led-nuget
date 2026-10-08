using System.Text;

namespace Arduino.Led;

/// <summary>
/// A packed 1-bit framebuffer for the 8x13 matrix: 104 bits, row-major.
/// Bit (y * 13 + x) is the pixel at column <c>x</c>, row <c>y</c>.
/// </summary>
public sealed class Frame
{
    private readonly byte[] _bits = new byte[LedMatrix.FrameByteCount];

    /// <summary>Gets the packed bits.</summary>
    public ReadOnlySpan<byte> Bits => _bits;

    /// <summary>Clears all pixels.</summary>
    public void Clear() => Array.Clear(_bits);

    /// <summary>Sets the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public void SetPixel(int x, int y, bool on)
    {
        if ((uint)x >= LedMatrix.Width || (uint)y >= LedMatrix.Height)
        {
            return;
        }

        int bit = y * LedMatrix.Width + x;
        if (on)
        {
            _bits[bit / 8] |= (byte)(1 << (bit % 8));
        }
        else
        {
            _bits[bit / 8] &= (byte)~(1 << (bit % 8));
        }
    }

    /// <summary>Gets the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public bool GetPixel(int x, int y)
    {
        if ((uint)x >= LedMatrix.Width || (uint)y >= LedMatrix.Height)
        {
            return false;
        }

        int bit = y * LedMatrix.Width + x;
        return (_bits[bit / 8] & (1 << (bit % 8))) != 0;
    }

    /// <summary>ORs another frame into this one.</summary>
    public void Or(Frame other)
    {
        ArgumentNullException.ThrowIfNull(other);
        for (int i = 0; i < _bits.Length; i++)
        {
            _bits[i] |= other._bits[i];
        }
    }

    /// <summary>Copies packed bits into this frame.</summary>
    public void CopyFrom(ReadOnlySpan<byte> bits)
    {
        if (bits.Length != _bits.Length)
        {
            throw new ArgumentException(
                $"A frame needs exactly {LedMatrix.FrameByteCount} bytes.", nameof(bits));
        }

        bits.CopyTo(_bits);
    }

    /// <summary>Renders the frame as ASCII art ('#' on, '.' off).</summary>
    public string ToAscii()
    {
        var sb = new StringBuilder(LedMatrix.Height * (LedMatrix.Width + 1));
        for (int y = 0; y < LedMatrix.Height; y++)
        {
            for (int x = 0; x < LedMatrix.Width; x++)
            {
                sb.Append(GetPixel(x, y) ? '#' : '.');
            }

            if (y < LedMatrix.Height - 1)
            {
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }
}

/// <summary>
/// A 3-bit grayscale framebuffer: one byte per pixel (row-major), values 0-7.
/// Same layout as the <c>uint8_t[104]</c> array Arduino's LED matrix guide uses
/// with <c>setGrayscaleBits(3)</c>.
/// </summary>
public sealed class GrayFrame
{
    private readonly byte[] _pixels = new byte[LedMatrix.PixelCount];

    /// <summary>Gets the row-major pixel brightness values (0-7).</summary>
    public ReadOnlySpan<byte> Pixels => _pixels;

    /// <summary>Clears all pixels.</summary>
    public void Clear() => Array.Clear(_pixels);

    /// <summary>Sets the brightness (0-7) of the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public void SetPixel(int x, int y, int brightness)
    {
        if ((uint)x >= LedMatrix.Width || (uint)y >= LedMatrix.Height)
        {
            return;
        }

        _pixels[y * LedMatrix.Width + x] = (byte)Math.Clamp(brightness, 0, LedMatrix.MaxBrightness);
    }

    /// <summary>Gets the brightness (0-7) of the pixel at (<paramref name="x"/>, <paramref name="y"/>).</summary>
    public int GetPixel(int x, int y)
    {
        if ((uint)x >= LedMatrix.Width || (uint)y >= LedMatrix.Height)
        {
            return 0;
        }

        return _pixels[y * LedMatrix.Width + x];
    }

    /// <summary>Builds a grayscale frame from a 1-bit frame (on = full brightness).</summary>
    public static GrayFrame FromFrame(Frame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        var gray = new GrayFrame();
        for (int y = 0; y < LedMatrix.Height; y++)
        {
            for (int x = 0; x < LedMatrix.Width; x++)
            {
                if (frame.GetPixel(x, y))
                {
                    gray.SetPixel(x, y, LedMatrix.MaxBrightness);
                }
            }
        }

        return gray;
    }
}
