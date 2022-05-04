using System;
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
            const char Space = ' ';

            var spanText = text.AsSpan().Trim();

            var estimatedLines = (float)Math.Ceiling(paint.MeasureText(spanText) / lineLengthLimit);

            ReadOnlySpan<char> substring;

            var @start = 0;
            var @break = 0;

            var ty = y - (paint.FontSpacing * (estimatedLines / 2)) + (paint.FontSpacing / 2);

            var lastIndex = spanText.Length - 1;

            for (int i = 0; i <= lastIndex; i++)
            {
                if (spanText[i] == Space)
                {
                    if (@start == @break)
                    {
                        @break = i;
                    }

                    substring = spanText[@start..i].Trim();

                    if (paint.MeasureText(substring) > lineLengthLimit)
                    {
                        canvas.DrawText(spanText[@start..@break].Trim().ToString(), x, ty, paint);
                        ty += paint.FontSpacing;
                        @start = @break;
                    }

                    @break = i;
                }

                if (i == lastIndex)
                {
                    substring = spanText[@start..].Trim();

                    if (paint.MeasureText(substring) > lineLengthLimit && @start != @break)
                    {
                        canvas.DrawText(spanText[@start..@break].Trim().ToString(), x, ty, paint);
                        ty += paint.FontSpacing;
                        canvas.DrawText(spanText[@break..].Trim().ToString(), x, ty, paint);
                    }
                    else
                    {
                        canvas.DrawText(substring.ToString(), x, ty, paint);
                    }
                }
            }
        }
    }
}
