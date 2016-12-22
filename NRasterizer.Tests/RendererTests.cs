using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    [TestFixture]
    public class RendererTests
    {
        [Test]
        [TestCase("Hello World", 64, 350, 84)]
        public void MesureText(string text, int fontSize, int expectedWidth, int expectedHeight)
        {
            var mockRasterizer = new Mock<IGlyphRasterizer>();
            mockRasterizer.Setup(x => x.Resolution).Returns(72); // rasterizer is responsible for the resolution

            var typeface = TestFonts.OpenSans_Regular;

            var options = new RendererOptions()
            {
                FontSize = fontSize
            };

            var renderer = new Renderer(typeface, mockRasterizer.Object, options);

            var size = renderer.Measure(text);
            Assert.AreEqual(expectedWidth, size.Width);
            Assert.AreEqual(expectedHeight, size.Height);
        }
    }
}
