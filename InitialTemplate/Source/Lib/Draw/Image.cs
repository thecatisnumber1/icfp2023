namespace Lib.Draw
{
    public class Image
    {
        private readonly Color[,] pixels;

        public Image(int width, int height)
        {
            pixels = new Color[width, height];
            pixels.Initialize();
        }

        public Image(Color[,] pixels)
        {
            this.pixels = pixels;
        }

        public Color this[Point p] => pixels[p.X, p.Y];

        public Color this[int x, int y] => pixels[x, y];

        public int Width => pixels.GetLength(0);

        public int Height => pixels.GetLength(1);

        public Image Clone()
        {
            return Extract(new Point(), new Point(Width, Height));
        }

        public Image Extract(Point bottomLeft, Point topRight)
        {
            int width = topRight.X - bottomLeft.X;
            int height = topRight.Y - bottomLeft.Y;
            Color[,] cut = new Color[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cut[x, y] = pixels[bottomLeft.X + x, bottomLeft.Y + y].Clone();
                }
            }

            return new Image(cut);
        }
    }
}
