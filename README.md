# Arduino.Led

.NET 10 and 11 rendering library for the 8x13 LED matrix on the Arduino VENTUNO Q. Framebuffers, a 5x8 font, text effects. Pure managed code. No hardware calls.

The board's 104 blue LEDs are wired to the STM32H5 MCU. The MCU still does the pin driving. Arduino's `Arduino_LED_Matrix` library handles that part. See the [VENTUNO Q LED matrix guide](https://docs.arduino.cc/tutorials/ventuno-q/led-matrix/). This library does everything in .NET before the frames go over the wire.

## Contents

- [Install](#install)
- [Use it](#use-it)
- [API](#api)
- [Thanks](#thanks)
- [License](#license)

## Install

GitHub Packages needs a classic personal access token with `read:packages`. Even for public packages.

```bash
$ dotnet nuget add source https://nuget.pkg.github.com/sirredbeard/index.json \
    --name sirredbeard-github \
    --username <github-user> \
    --password <github-token> \
    --store-password-in-clear-text
$ dotnet add package Arduino.Led --version 0.1.0
```

Do not put the token in a repo. It lives in your user NuGet config.

## Use it

```csharp
using Arduino.Led;

// Scrolling text, one frame per step.
foreach (Frame frame in TextRenderer.RenderScroll("HELLO"))
{
    byte[] wire = frame.Bits.ToArray(); // 13 packed bytes, bit (y*13+x) = pixel (x, y)
    SendToBoard(wire);
}

// A grayscale pulse, using the matrix 3-bit brightness levels.
foreach (GrayFrame frame in TextRenderer.RenderPulse("MUSE", pulses: 2))
{
    byte[] pixels = frame.Pixels.ToArray(); // 104 bytes, row-major, 0-7
    SendToBoard(pixels);
}
```

## API

- `LedMatrix`: geometry. `Width` (13), `Height` (8), `MaxBrightness` (7), `RgbLedCount` (4).
- `Frame`: packed 1-bit framebuffer, 13 bytes. `SetPixel` and `GetPixel`. `ToAscii()` for debugging.
- `GrayFrame`: 3-bit grayscale framebuffer, 104 bytes row-major, values 0-7. Matches the `uint8_t[104]` layout in Arduino's LED matrix guide.
- `TextRenderer`: `RenderStatic`, `RenderScroll`, `RenderBlink`, `RenderPulse`. 5x8 font, printable ASCII.

## Thanks

- Arduino for the [VENTUNO Q LED matrix guide](https://docs.arduino.cc/tutorials/ventuno-q/led-matrix/).
- Adafruit Industries for the `glcdfont` 5x8 font, via the Muse Gadgets upstream SDK (`esp32/main/pixel_font.c`).

## License

[MIT](LICENSE)
