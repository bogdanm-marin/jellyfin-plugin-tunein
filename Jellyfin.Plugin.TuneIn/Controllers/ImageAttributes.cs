using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.TuneIn.Controllers
{
    /// <summary>
    /// Immage attributes.
    /// </summary>
    public class ImageAttributes
    {
        /// <summary>
        /// Gets or sets text to be written on image.
        /// </summary>
        [Required]
        [FromRoute]
        public string? Text { get; set; }

        /// <summary>
        /// Gets or Sets Image format.
        /// </summary>
        [Required]
        [DefaultValue("png")]
        [FromRoute]
        public string Format { get; set; } = "png";

        /// <summary>
        /// Gets or sets image width.
        /// </summary>
        [DefaultValue(405)]
        [FromRoute]
        public int Width { get; set; } = 405;

        /// <summary>
        /// Gets or sets image height.
        /// </summary>
        [DefaultValue(405)]
        [FromRoute]
        public int Height { get; set; } = 405;

        /// <summary>
        /// Gets or sets text font color.
        /// </summary>
        [DefaultValue("24B7A3")]
        [FromQuery]
        public string FontColor { get; set; } = "24B7A3";

        /// <summary>
        /// Gets or sets text font size.
        /// </summary>
        [DefaultValue(30)]
        [FromRoute]
        public float FontSize { get; set; } = 30;

        /// <summary>
        /// Gets or sets text font family name.
        /// </summary>
        [DefaultValue("Noto Sans, Noto Sans HK, Noto Sans JP, Noto Sans KR, Noto Sans SC, sans-serif")]
        [FromQuery]
        public string FontFamilyName { get; set; } = "Noto Sans, Noto Sans HK, Noto Sans JP, Noto Sans KR, Noto Sans SC, sans-serif";

        /// <summary>
        /// Gets or sets background color.
        /// </summary>
        [DefaultValue("FFFFFF")]
        [FromQuery]
        public string BackgroundColor { get; set; } = "FFFFFF";

        /// <summary>
        /// Gets or sets Text padding.
        /// </summary>
        [DefaultValue(15f)]
        [FromQuery]
        public float TextPadding { get; set; } = 15f;
    }
}
