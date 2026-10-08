namespace Arduino.Led;

/// <summary>
/// Geometry of the VENTUNO Q onboard LED matrix. 8 rows by 13 columns of
/// blue LEDs, per https://docs.arduino.cc/tutorials/ventuno-q/led-matrix/.
/// The matrix also does 8 grayscale brightness levels (3 bits) per LED.
/// </summary>
public static class LedMatrix
{
    /// <summary>Matrix width in pixels.</summary>
    public const int Width = 13;

    /// <summary>Matrix height in pixels.</summary>
    public const int Height = 8;

    /// <summary>Total pixels (Width * Height).</summary>
    public const int PixelCount = Width * Height;

    /// <summary>Bytes in a packed 1-bit frame: 104 bits, row-major, bit (y * 13 + x).</summary>
    public const int FrameByteCount = 13;

    /// <summary>Maximum per-pixel brightness (3-bit grayscale).</summary>
    public const int MaxBrightness = 7;

    /// <summary>Number of user RGB LEDs on the board.</summary>
    public const int RgbLedCount = 4;
}
