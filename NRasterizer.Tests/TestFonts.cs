using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NRasterizer.Tests
{
    public static class TestFonts
    {
        private static readonly string _fontsFolder;

        static TestFonts()
        {
            var path = Path.GetDirectoryName(new Uri(typeof(TestFonts).Assembly.Location).LocalPath);
            _fontsFolder = $"{path}/TestFonts";
        }

        public static Typeface LoadFont(string path)
        {
            var reader = new OpenTypeReader();
            using (var fs = File.OpenRead(path))
            {
                return reader.Read(fs);
            }
        }
        
        public static Typeface OpenSans_Regular => LoadFont($"{_fontsFolder}/OpenSans-Regular.ttf");

        public static IEnumerable<string> AllFonts
        {
            get
            {
                var fontFiles = new[] { "OpenSans-Regular.ttf", "segoeui.ttf", "CompositeMS.ttf" };
                foreach (var fontFile in fontFiles)
                {
                    yield return $"{_fontsFolder}/{fontFile}";
                }
            }
        }
    }
}
