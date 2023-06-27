using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text.Json;
using Image = Lib.Draw.Image;
using Color = Lib.Draw.Color;

namespace Lib
{
    public static class LocalFile
    {
        private static readonly Cache<string, string> pathCache = new(FindDirectory);
        private static readonly Cache<string, int> countCache = new(FindCount);

        public static int FileCount(string directoryName)
        {
            return countCache.Get(directoryName);
        }

        public static string LoadTextFile(string directoryName, string fileName)
        {
            return File.ReadAllText(GetPath(directoryName, fileName));
        }

        public static T? LoadJsonFile<T>(string directoryName, string fileName)
        {
            string text = LoadTextFile(directoryName, fileName);
            return JsonSerializer.Deserialize<T>(text);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA1416: Only supported on Windows")]
        public static Image LoadPNGFile(string directoryName, string fileName)
        {
            string path = GetPath(directoryName, fileName);

            Bitmap original = new(path);
            int width = original.Width;
            int height = original.Height;

            PixelFormat format = PixelFormat.Format32bppArgb;
            int depth = System.Drawing.Image.GetPixelFormatSize(format) / 8; // Return size in bytes

            byte[] imageBytes = new byte[original.Width * original.Height * depth];

            BitmapData bmpData = original.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, format);
            Marshal.Copy(bmpData.Scan0, imageBytes, 0, bmpData.Stride * height);

            original.UnlockBits(bmpData);

            Color[,] pixels = new Color[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int baseIndex = (x + y * height) * 4;
                    // ARGB -> RGBA requires offsetting by 1, 2, 3, and then 0
                    pixels[x, height - 1 - y] = new Color(imageBytes[baseIndex + 2], imageBytes[baseIndex + 1], imageBytes[baseIndex], imageBytes[baseIndex + 3]);
                }
            }

            return new Image(pixels);
        }

        private static string GetPath(string directoryName, string fileName)
        {
            string path = Path.Join(pathCache.Get(directoryName), fileName);

            if (!File.Exists(path))
            {
                throw new Exception($"Path {path} does not exist");
            }

            return path;
        }

        private static int FindCount(string directoryName)
        {
            return Directory.GetFiles(pathCache.Get(directoryName)).Length;
        }

        private static string FindDirectory(string directoryName)
        {
            string parentDir = ".";
            while (!Directory.GetDirectories(parentDir)
                    .Any(dir => Path.GetFileName(dir).Contains(directoryName)))
            {
                parentDir = Path.Join(parentDir, "..");

                if (parentDir.Length > 100)
                {
                    throw new Exception($"Couldn't find directory {directoryName}");
                }
            }

            return Path.Join(parentDir, directoryName);
        }
    }
}
