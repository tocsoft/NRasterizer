//Apache2, 2014-2016,   WinterDev
using System;

namespace NRasterizer
{
    /// <summary>
    /// Configuration options for controlling how text is rendered.
    /// </summary>
    public class RendererOptions
    {
        /// <summary>
        /// Gets or sets the size of the font in point
        /// </summary>
        /// <value>
        /// The size of the font.
        /// </value>
        public float FontSize { get; set; } = 10;
   }
}