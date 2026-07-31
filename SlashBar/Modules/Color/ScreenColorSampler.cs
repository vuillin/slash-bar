using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using DrawingSize = System.Drawing.Size;

namespace SlashBar.Modules.Color;

/// <summary>
/// Captures a screen region and reads the center pixel. No WPF Window dependency.
/// </summary>
public sealed class ScreenColorSampler : IDisposable {

    private readonly int _size;
    private readonly Bitmap _bitmap;
    private readonly byte[] _bgraBuffer;
    private bool _disposed;

    public ScreenColorSampler(int sampleSize) {
        if (sampleSize < 1 || sampleSize % 2 == 0)
            throw new ArgumentOutOfRangeException(nameof(sampleSize), "Sample size must be a positive odd integer.");

        _size = sampleSize;
        _bitmap = new Bitmap(sampleSize, sampleSize, PixelFormat.Format32bppArgb);
        _bgraBuffer = new byte[sampleSize * sampleSize * 4];
    }

    public int SampleSize => _size;

    /// <summary>
    /// Fills <paramref name="bgraDestination"/> (BGRA, stride = size*4) and returns the center color.
    /// </summary>
    public (byte R, byte G, byte B) Sample(int centerX, int centerY, Span<byte> bgraDestination) {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (bgraDestination.Length < _bgraBuffer.Length)
            throw new ArgumentException("Destination buffer too small.", nameof(bgraDestination));

        var half = _size / 2;
        var srcX = centerX - half;
        var srcY = centerY - half;

        using (var g = Graphics.FromImage(_bitmap)) {
            g.CopyFromScreen(srcX, srcY, 0, 0, new DrawingSize(_size, _size));
        }

        var bmpData = _bitmap.LockBits(
            new Rectangle(0, 0, _size, _size),
            ImageLockMode.ReadOnly,
            PixelFormat.Format32bppArgb);

        try {
            var stride = bmpData.Stride;
            var expectedStride = _size * 4;

            if (stride == expectedStride) {
                Marshal.Copy(bmpData.Scan0, _bgraBuffer, 0, _bgraBuffer.Length);
            }
            else {
                for (var row = 0; row < _size; row++) {
                    Marshal.Copy(
                        bmpData.Scan0 + row * stride,
                        _bgraBuffer,
                        row * expectedStride,
                        expectedStride);
                }
            }
        }
        finally {
            _bitmap.UnlockBits(bmpData);
        }

        _bgraBuffer.AsSpan().CopyTo(bgraDestination);

        var centerOffset = (half * _size + half) * 4;
        var b = _bgraBuffer[centerOffset];
        var gChan = _bgraBuffer[centerOffset + 1];
        var r = _bgraBuffer[centerOffset + 2];
        return (r, gChan, b);
    }

    public void Dispose() {
        if (_disposed)
            return;
        _disposed = true;
        _bitmap.Dispose();
    }
}
