using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Web;
using Jellyfin.Plugin.TuneIn.Extensions;
using Microsoft.AspNetCore.Mvc;
using SkiaSharp;

namespace Jellyfin.Plugin.TuneIn.Controllers
{
    /// <summary>
    /// Tune In image controller.
    /// </summary>
    [ApiController]
    [Route("api/v1/TuneIn/[controller]")]
    public class ImageController : Controller
    {
        /// <summary>
        /// Generate image based on <see cref="ImageAttributes"/>.
        /// </summary>
        /// <param name="imageAttributes">Image attributes.</param>
        /// <returns>Image Result.</returns>
        [HttpGet("generate/{Text}-w{Width}-h{Height}-fs{FontSize}.{Format=png}")]
        public IActionResult Generate([FromRoute, FromQuery, NotNull] ImageAttributes imageAttributes)
        {
            if (!Enum.TryParse<SKEncodedImageFormat>(imageAttributes.Format, true, out var imageFormat))
            {
                ModelState.AddModelError(nameof(imageAttributes.Format), $"Invalid {nameof(imageAttributes.Format)}: {imageAttributes.Format}");
            }

            if (!SKColor.TryParse(imageAttributes.FontColor, out var fontColor))
            {
                ModelState.AddModelError(nameof(imageAttributes.FontColor), $"Invalid {nameof(imageAttributes.FontColor)}: {imageAttributes.FontColor}");
            }

            if (!SKColor.TryParse(imageAttributes.BackgroundColor, out var backgroundColor))
            {
                ModelState.AddModelError(nameof(imageAttributes.BackgroundColor), $"Invalid {nameof(imageAttributes.BackgroundColor)}: {imageAttributes.BackgroundColor}");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            byte[] fileContent;

            using (var skBitmap = new SKBitmap(imageAttributes.Width, imageAttributes.Height))
            {
                using (var canvas = new SKCanvas(skBitmap))
                {
                    using var skTypeFace = SKTypeface.FromFamilyName(imageAttributes.FontFamilyName, SKFontStyle.Bold);
                    using var font = new SKFont(skTypeFace, size: imageAttributes.FontSize);
                    using var paint = new SKPaint(font)
                    {
                        Color = fontColor,
                        TextAlign = SKTextAlign.Center,
                        IsAntialias = true,
                        IsAutohinted = true,
                        SubpixelText = true,
                    };

                    canvas.DrawColor(backgroundColor);

                    var decodedText = HttpUtility.UrlDecode(imageAttributes.Text);
                    canvas.DrawText(decodedText!, imageAttributes.Width / 2f, imageAttributes.Height / 2f, imageAttributes.Width - (imageAttributes.TextPadding * 2), paint);
                }

                using var encoder = skBitmap.Encode(imageFormat, 100);
                fileContent = encoder.ToArray();
            }

            return File(fileContent, $"image/{imageFormat.ToString().ToLowerInvariant()}");
        }

        /// <summary>
        /// Returns Embeded Image File.
        /// </summary>
        /// <param name="imageFile"> Image file name.</param>
        /// <returns>Image Stream Content.</returns>
        [HttpGet("{imageFile}")]
        public IActionResult Get(string imageFile)
        {
            var imagePath = $"{typeof(Plugin).Namespace}.Images.{imageFile}";

            var imageStream = GetType().Assembly.GetManifestResourceStream(imagePath);

            if (imageStream == null)
            {
                return NotFound();
            }

            var extension = Path.GetExtension(imageFile)?[1..];

            return File(imageStream, $"image/{extension}");
        }
    }
}
