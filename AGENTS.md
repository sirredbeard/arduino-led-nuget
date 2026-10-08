# Agent instructions

Start with [`README.md`](README.md) for the public project description and
package usage.

Conventions:

- Target `net10.0` and `net11.0`. Nullable and implicit usings are on;
  warnings are errors.
- This is a pure rendering library: framebuffers, font, text effects.
  No I/O, no serial port, no hardware calls. The caller decides how frames
  reach the board.
- Keep it Native AOT and trim compatible: no reflection, no dynamic code.
- `src/Arduino.Led/Font5x7.cs` is generated from the upstream Muse Gadgets
  SDK font; do not hand-edit glyphs, keep the attribution header.
- Do not add co-author metadata to commits.
