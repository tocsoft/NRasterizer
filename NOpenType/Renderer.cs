//Apache2, 2014-2016,   WinterDev
using System;

namespace NRasterizer
{
    public class Renderer
    {
        private const int PointsPerInch = 72;
        private readonly IGlyphRasterizer _rasterizer;
        private readonly Typeface _typeface;
        private readonly RendererOptions _options;
        private readonly double _multiplyer;
        private readonly double _divisor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Renderer"/> class.
        /// </summary>
        /// <param name="typeface">The typeface.</param>
        /// <param name="rasterizer">The rasterizer.</param>
        /// <param name="options">The options.</param>
        public Renderer(Typeface typeface, IGlyphRasterizer rasterizer, RendererOptions options)
        {
            _options = options;
            _typeface = typeface;
            _rasterizer = rasterizer;
            _multiplyer = rasterizer.Resolution / PointsPerInch;
            _divisor = EmSquare.Size / _options.FontSize;
        }

        /// <summary>
        /// Renders the specified glyph at the specified X and Y position.
        /// </summary>
        /// <param name="x">The x postion in pixels to draw the glyph.</param>
        /// <param name="y">The y postion in pixels to draw the glyph.</param>
        /// <param name="text">The glyph.</param>
        public void RenderGlyph(double x, double y, Glyph glyph)
        {
            x = x * _divisor / _multiplyer;
            y = y * _divisor / _multiplyer;
            RenderGlyph(x, y, _multiplyer, _divisor, glyph);
        }

        internal void RenderGlyph(double x, double y, double multiplier, double divisor, Glyph glyph)
        {
            var rasterizer = new ToPixelRasterizer(x, y, multiplier, divisor, _rasterizer);

            ushort[] contours = glyph.EndPoints;
            short[] xs = glyph.X;
            short[] ys = glyph.Y;
            bool[] onCurves = glyph.On;

            int npoints = xs.Length;
            int startContour = 0;
            int cpoint_index = 0;

            rasterizer.BeginRead(contours.Length);

            int lastMoveX = 0;
            int lastMoveY = 0;

            int controlPointCount = 0;
            for (int i = 0; i < contours.Length; i++)
            {
                int nextContour = contours[startContour] + 1;
                bool isFirstPoint = true;
                Point<int> secondControlPoint = new Point<int>();
                Point<int> thirdControlPoint = new Point<int>();
                bool justFromCurveMode = false;

                for (; cpoint_index < nextContour; ++cpoint_index)
                {

                    short vpoint_x = xs[cpoint_index];
                    short vpoint_y = ys[cpoint_index];
                    if (onCurves[cpoint_index])
                    {
                        //on curve
                        if (justFromCurveMode)
                        {
                            switch (controlPointCount)
                            {
                                case 1:
                                    {
                                        rasterizer.Curve3(
                                            secondControlPoint.x,
                                            secondControlPoint.y,
                                            vpoint_x,
                                            vpoint_y);
                                    }
                                    break;
                                case 2:
                                    {
                                        rasterizer.Curve4(
                                                secondControlPoint.x,  secondControlPoint.y,
                                                thirdControlPoint.x, thirdControlPoint.y,
                                                vpoint_x, vpoint_y);
                                    }
                                    break;
                                default:
                                    {
                                        throw new NotSupportedException();
                                    }
                            }
                            controlPointCount = 0;
                            justFromCurveMode = false;
                        }
                        else
                        {
                            if (isFirstPoint)
                            {
                                isFirstPoint = false;
                                lastMoveX = vpoint_x;
                                lastMoveY = vpoint_y;
                                rasterizer.MoveTo(lastMoveX, lastMoveY);
                            }
                            else
                            {
                                rasterizer.LineTo(vpoint_x, vpoint_y);
                            }
                        }
                    }
                    else
                    {
                        switch (controlPointCount)
                        {
                            case 0:
                                {
                                    secondControlPoint = new Point<int>(vpoint_x, vpoint_y);
                                }
                                break;
                            case 1:
                                {
                                    //we already have prev second control point
                                    //so auto calculate line to 
                                    //between 2 point
                                    Point<int> mid = GetMidPoint(secondControlPoint, vpoint_x, vpoint_y);
                                    //----------
                                    //generate curve3
                                    rasterizer.Curve3(
                                        secondControlPoint.x, secondControlPoint.y,
                                        mid.x, mid.y);
                                    //------------------------
                                    controlPointCount--;
                                    //------------------------
                                    //printf("[%d] bzc2nd,  x: %d,y:%d \n", mm, vpoint.x, vpoint.y);
                                    secondControlPoint = new Point<int>(vpoint_x, vpoint_y);
                                }
                                break;
                            default:
                                {
                                    throw new NotSupportedException("Too many control points");
                                }
                        }

                        controlPointCount++;
                        justFromCurveMode = true;
                    }
                }
                //--------
                //close figure
                //if in curve mode
                if (justFromCurveMode)
                {
                    switch (controlPointCount)
                    {
                        case 0: break;
                        case 1:
                            {
                                rasterizer.Curve3(
                                    secondControlPoint.x, secondControlPoint.y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        case 2:
                            {
                                rasterizer.Curve4(
                                    secondControlPoint.x, secondControlPoint.y,
                                    thirdControlPoint.x, thirdControlPoint.y,
                                    lastMoveX, lastMoveY);
                            }
                            break;
                        default:
                            { throw new NotSupportedException("Too many control points"); }
                    }
                    justFromCurveMode = false;
                    controlPointCount = 0;
                }
                rasterizer.CloseFigure();
                //--------                   
                startContour++;
            }
            rasterizer.EndRead();
        }

        private static Point<int> GetMidPoint(Point<int> v1, int v2x, int v2y)
        {
            return new Point<int>((v1.X + v2x) / 2, (v1.Y + v2y) / 2);
        }

        private static Point<int> GetMidPoint(Point<int> v1, Point<int> v2)
        {
            return new Point<int>((v1.x + v2.x) / 2, (v1.y + v2.y) / 2);
        }

        /// <summary>
        /// Renders the specified text at the specified X and Y position.
        /// </summary>
        /// <param name="x">The x postion in pixels to draw the text.</param>
        /// <param name="y">The y postion in pixels to draw the text.</param>
        /// <param name="text">The text.</param>
        /// <returns>The size of the rendered text in pixels.</returns>
        public Size Render(int x, int y, string text)
        {
            return Render(x, y, text, true);
        }

        /// <summary>
        /// Measures the specified text in pixels,
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns>The size of the text in pixels os though it was rendered.</returns>
        public Size Measure(string text)
        {
            return Render(0, 0, text, false);
        }

        private double GetAdvanceWidth(char character)
        {
            return (_typeface.GetAdvanceWidth(character) * _multiplyer / _divisor);
        }

        private double GetLineDrawHeight()
        {
            var drawheightEM = _typeface.Bounds.YMax - _typeface.Bounds.YMin;
            var drawheightPX = drawheightEM * _multiplyer / _divisor; // convert back to pixel space

            return drawheightPX;
        }

        private Size Render(int x, int y, string text, bool renderGlyph)
        {
            double xx = x;
            double yy = y;

            double width = 0;
            foreach (var character in text)
            {
                var glyph = _typeface.Lookup(character);

                if (renderGlyph)
                {
                    RenderGlyph(xx, yy, glyph);
                }

                xx += GetAdvanceWidth(character);

                if (xx > width)
                {
                    width = xx;
                }
            }

            if (renderGlyph)
            {
                _rasterizer.Flush();
            }

            var yDiff = (yy - y);

            return new Size()
            {
                Height = (int)(GetLineDrawHeight() + yDiff), //add the full draw height to the offset of the last draw position.
                Width = (int) width 
            };
        }
    }

}