using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace iHawkGlyphTypefaceLibrary
{
    public class GlyphTypefaceManager
    {
        #region constructor

        public GlyphTypefaceManager(string fontFileName)
        {
            _glyphTypeface = new System.Windows.Media.GlyphTypeface(new Uri(fontFileName));
        }

        #endregion

        #region property

        private readonly System.Windows.Media.GlyphTypeface _glyphTypeface;
        private const float Dpi = 96;

        public int GlyphCount
        {
            get
            {
                try
                {
                    return _glyphTypeface.GlyphCount;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return 0;
                }
            }
        }

        public IDictionary<int, ushort> CharacterToGlyphMap
        {
            get
            {
                try
                {
                    return _glyphTypeface.CharacterToGlyphMap;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public IDictionary<CultureInfo, string> FamilyNames
        {
            get
            {
                try
                {
                    return _glyphTypeface.FamilyNames;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        public IDictionary<CultureInfo, string> VersionStrings
        {
            get
            {
                try
                {
                    return _glyphTypeface.VersionStrings;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
        }

        #endregion

        #region method

        public System.Drawing.Bitmap RenderSingleCharacter(ushort glyphIndex, System.Drawing.Color backColor, System.Drawing.Color foreColor, int fontSize)
        {
            try
            {
                var geometry = _glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 1);
                var advanceWidth = _glyphTypeface.AdvanceWidths[glyphIndex];
                var cellSize = new Size(fontSize, fontSize * _glyphTypeface.Height);
                var offsetX = advanceWidth * cellSize.Width;
                var offsetY = fontSize * _glyphTypeface.Baseline;
                var drawingVisual = new System.Windows.Media.DrawingVisual();
                var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(foreColor.A, foreColor.R, foreColor.G, foreColor.B));
                var pen = new System.Windows.Media.Pen();
                using (var dc = drawingVisual.RenderOpen())
                {
                    dc.PushTransform(new System.Windows.Media.TranslateTransform(0, offsetY));
                    dc.DrawGeometry(brush, null, geometry);
                    dc.Pop(); // get rid of the transform
                }

                var bitWidth = (int) Math.Ceiling(offsetX);
                var bitHeight = (int) Math.Ceiling(cellSize.Height);
                var targetImage = new System.Windows.Media.Imaging.RenderTargetBitmap(bitWidth, bitHeight, Dpi, Dpi, System.Windows.Media.PixelFormats.Pbgra32);
                targetImage.Render(drawingVisual);

                var bmpEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                bmpEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(targetImage));

                var bmp = new System.Drawing.Bitmap(bitWidth, bitHeight);
                bmp.SetResolution(Dpi, Dpi);
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.Clear(backColor);

                    using (var ms = new MemoryStream())
                    {
                        bmpEncoder.Save(ms);
                        graphics.DrawImage(System.Drawing.Image.FromStream(ms), new System.Drawing.PointF(0, 0));
                    }

                    graphics.Save();
                }

                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 渲染字符串
        /// </summary>
        /// <param name="previewText"></param>
        /// <param name="backColor">背景色</param>
        /// <param name="foreColor">前景色</param>
        /// <param name="fontSize">font size in pixel</param>
        /// <returns></returns>
        public System.Drawing.Bitmap RenderString(string previewText, System.Drawing.Color backColor, System.Drawing.Color foreColor, int fontSize)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(previewText))
                {
                    if (_glyphTypeface.FamilyNames.Count == 0) previewText = "no name";
                    else if (_glyphTypeface.FamilyNames.ContainsKey(CultureInfo.CurrentCulture)) previewText = _glyphTypeface.FamilyNames[CultureInfo.CurrentCulture];
                    else previewText = _glyphTypeface.FamilyNames[_glyphTypeface.FamilyNames.Keys.First()];
                }

                var glyphIndexList = GetGlyphIndexList(previewText);
                if (glyphIndexList.Count <= 0) return null;
                var geometryList = glyphIndexList.Select(glyphIndex => _glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 1d)).ToList();
                var advanceWidthList = glyphIndexList.Select(glyphIndex => _glyphTypeface.AdvanceWidths[glyphIndex]).ToList();

                var cellSize = new Size(fontSize, fontSize * _glyphTypeface.Height);

                var offsetX = 0d;
                var offsetY = fontSize * _glyphTypeface.Baseline;
                var drawingVisual = new System.Windows.Media.DrawingVisual();
                var brush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(foreColor.R, foreColor.G, foreColor.B));
                //var pen = new System.Windows.Media.Pen(brush, 0);
                using (var dc = drawingVisual.RenderOpen())
                {
                    for (var i = 0; i < geometryList.Count; i++)
                    {
                        var geometry = geometryList[i];
                        var advanceWidth = advanceWidthList[i];
                        //if (geometry.IsEmpty()) continue;
                        dc.PushTransform(new System.Windows.Media.TranslateTransform(offsetX, offsetY));
                        dc.DrawGeometry(brush, null, geometry);
                        dc.Pop(); // get rid of the transform
                        offsetX += advanceWidth * cellSize.Width;
                    }
                }

                var bitWidth = (int) Math.Ceiling(offsetX);
                var bitHeight = (int) Math.Ceiling(cellSize.Height);
                var targetImage = new System.Windows.Media.Imaging.RenderTargetBitmap(bitWidth, bitHeight, Dpi, Dpi, System.Windows.Media.PixelFormats.Pbgra32);
                targetImage.Render(drawingVisual);

                var bmpEncoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
                bmpEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(targetImage));

                //redraw to Bitmap
                var bmp = new System.Drawing.Bitmap(bitWidth, bitHeight);
                bmp.SetResolution(Dpi, Dpi);
                using (var graphics = System.Drawing.Graphics.FromImage(bmp))
                {
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    graphics.Clear(backColor);

                    using (var ms = new MemoryStream())
                    {
                        bmpEncoder.Save(ms);
                        graphics.DrawImage(System.Drawing.Image.FromStream(ms), new System.Drawing.PointF(0, 0));
                    }

                    graphics.Save();
                }

                return bmp;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        /// <summary>
        /// 获取字形索引列表
        /// </summary>
        private List<ushort> GetGlyphIndexList(string glyphText)
        {
            try
            {
                var glyphIndexList = new List<ushort>();
                var glyphTextChars = Encoding.Unicode.GetChars(Encoding.Unicode.GetBytes(glyphText));
                foreach (var glyphTextChar in glyphTextChars)
                {
                    var glyphTextCharUnicodeDec = Convert.ToInt32(glyphTextChar);
                    if (!_glyphTypeface.CharacterToGlyphMap.ContainsKey(glyphTextCharUnicodeDec)) continue;
                    var glyphIndex = _glyphTypeface.CharacterToGlyphMap[glyphTextCharUnicodeDec];
                    glyphIndexList.Add(glyphIndex);
                }

                return glyphIndexList;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return null;
            }
        }

        public void DrawStringTest()
        {
            const int ColumnCount = 10;
            const int MaxDrawCount = 30; // use int.MaxValue to draw them all            
            const double fontSize = 50d;
            // the height of each cell has to include over/under hanging glyphs
            var cellSize = new Size(fontSize, fontSize * _glyphTypeface.Height);

            var glyphs = from glyphIndex in _glyphTypeface.CharacterToGlyphMap.Values select _glyphTypeface.GetGlyphOutline(glyphIndex, fontSize, 1d);

            // now create the visual we'll draw them to
            var dv = new System.Windows.Media.DrawingVisual();
            var drawCount = -1;
            using (var dc = dv.RenderOpen())
            {
                foreach (var g in glyphs)
                {
                    drawCount++;
                    if (drawCount >= MaxDrawCount) break; // don't draw more than you want
                    if (g.IsEmpty()) continue; // don't draw the blank ones
                    // center horizontally in the cell
                    var xOffset = (drawCount % ColumnCount) * cellSize.Width + (cellSize.Width - g.Bounds.Width) / 2d;
                    // place the character on the baseline of the cell
                    var yOffset = (drawCount / ColumnCount) * cellSize.Height + fontSize * _glyphTypeface.Baseline;
                    dc.PushTransform(new System.Windows.Media.TranslateTransform(xOffset, yOffset));
                    dc.DrawGeometry(System.Windows.Media.Brushes.Red, null, g);
                    dc.Pop(); // get rid of the transform
                }
            }

            var rowCount = drawCount / ColumnCount;
            if (drawCount % ColumnCount != 0) rowCount++; // to include partial rows
            var bitWidth = (int) Math.Ceiling(cellSize.Width * ColumnCount);
            var bitHeight = (int) Math.Ceiling(cellSize.Height * rowCount);
            var bmp = new System.Windows.Media.Imaging.RenderTargetBitmap(bitWidth, bitHeight, 96, 96, System.Windows.Media.PixelFormats.Pbgra32);
            bmp.Render(dv);

            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(bmp));
            using (var file = new FileStream("FontTable.png", FileMode.Create)) encoder.Save(file);
        }

        #endregion
    }
}
