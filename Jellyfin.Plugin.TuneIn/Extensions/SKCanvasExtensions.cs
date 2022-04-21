using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkiaSharp;

namespace Jellyfin.Plugin.TuneIn.Extensions
{
    /// <summary>
    /// Extension methods for SKCanvas.
    /// </summary>
    public static class SKCanvasExtensions
    {
        /// <summary>
        /// Draws Text on Canvas and wraps lines with length bigger than lineLengthLimit.
        /// </summary>
        /// <param name="canvas"><see cref="SKCanvas"/> instance.</param>
        /// <param name="text">Text to write.</param>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="lineLengthLimit">Max line length.</param>
        /// <param name="paint"><see cref="SKPaint"/> instance.</param>
        public static void DrawText(this SKCanvas canvas, string text, float x, float y, float lineLengthLimit, SKPaint paint)
        {
            const char space = ' ';

            var spanText = text.AsSpan().Trim();

            var estimatedLines = (float)Math.Ceiling(paint.MeasureText(spanText) / lineLengthLimit);

            ReadOnlySpan<char> substring;

            var start = 0;
            var end = 0;

            var ty = y - (paint.FontSpacing * (estimatedLines / 2)) + (paint.FontSpacing / 2);

            for (int i = 1; i < spanText.Length; i++)
            {
                if (spanText[i] == space)
                {
                    substring = spanText.Slice(start, i - start - 1);

                    if (paint.MeasureText(substring) > lineLengthLimit)
                    {
                        canvas.DrawText(spanText[start..end].Trim().ToString(), x, ty, paint);
                        ty += paint.FontSpacing;
                        start = i;
                    }

                    end = i;
                }
                else
                {
                    if (spanText[start] == space)
                    {
                        start = i;
                        end = i;
                    }
                }
            }

            canvas.DrawText(spanText[start..].Trim().ToString(), x, ty, paint);
        }
    }
}
