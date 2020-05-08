using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GlyphTypefaceConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var files = new DirectoryInfo("..\\..\\..\\testfonts\\").GetFiles();
            foreach (var file in files)
            {
                Console.WriteLine(file.FullName);

                var gtm = new iHawkGlyphTypefaceLibrary.GlyphTypefaceManager(file.FullName);

                var ss = new List<string>
                {
                    "中华人民共和国",
                    "the quick brown fox jumps over the lazy dog",
                    "0123456789",
                    "永"
                };
                foreach (var s in ss)
                {
                    var bmp0 = gtm.RenderString(s, Color.Black, Color.White, 50);
                    bmp0?.Save($"{file.Name}-{s}.png", ImageFormat.Png);
                }

                gtm.DrawStringTest();

                /*
                var cgm = gtm.CharacterToGlyphMap;
                if (cgm != null)
                {
                    foreach (var item in cgm)
                    {
                        Console.WriteLine($"Code: {item.Key}, GID: {item.Value}");
                    }
                }
                */

                var fns = gtm.FamilyNames;
                if (fns != null)
                {
                    foreach (var item in fns)
                    {
                        Console.WriteLine($"Language: {item.Key}, FamilyName: {item.Value}");
                    }
                }

                var bmp = gtm.RenderSingleCharacter(3000, Color.Black, Color.White, 72);
                bmp?.Save($"{file.Name}-{3000}.png", ImageFormat.Png);
                /*
                var gc = gtm.GlyphCount;
                Console.WriteLine($"Glyph count: {gc}");
                for (ushort i = 0; i < gc; i++)
                {
                    var bmp = gtm.RenderSingleCharacter(i, Color.Black, Color.White, 72);
                    bmp?.Save($"{file.Name}-{i}.png", ImageFormat.Png);
                }
                */
            }
        }
    }
}
