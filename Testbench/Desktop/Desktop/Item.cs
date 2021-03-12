using System;
using System.IO;

namespace VooDo.Testbench.Desktop
{

    internal sealed class Item
    {

        private static readonly string[] s_shapes = { "icosahedron", "pyramid", "torus", "cylinder", "cone", "cube", "sphere" };
        private static readonly int[] s_basePrices = { 29, 12, 22, 8, 10, 3, 5 };
        private static readonly string[] s_colors = { "Scarlet", "Pumpkin", "Lemon", "Chartreuse", "Malachite", "Pastel green", "Aqua", "Sky", "Neon", "Lavender", "Rose" };
        private static readonly float[] s_fractPrices = { .0f, .5f, .75f, .69f, .89f, .9f, .99f };
        private static readonly Random s_random = new Random();
        private const string c_imagePath = "Images/Items";
        private const float c_maxPriceDeviance = 1.5f;

        public static Item[] CreateItems(int _count)
        {
            Item[] items = new Item[_count];
            while (_count-- > 0)
            {
                items[_count] = CreateItem();
            }
            return items;
        }

        public static Item CreateItem()
        {
            int shape = s_random.Next(s_shapes.Length);
            int color = s_random.Next(s_colors.Length);
            string imageUrl = Path.Combine(c_imagePath, $"{shape}_{color}.png");
            string title = $"{s_colors[color]} {s_shapes[shape]}";
            float price = s_basePrices[shape];
            price += (int)Math.Round(s_random.NextDouble() * (1 - c_maxPriceDeviance) * price);
            price += s_fractPrices[s_random.Next(s_fractPrices.Length)];
            return new Item(imageUrl, title, price);
        }

        private Item(string _imageUrl, string _title, float _price)
        {
            ImageUrl = _imageUrl;
            Title = _title;
            Price = _price;
        }

        internal string ImageUrl { get; }
        internal string Title { get; }
        internal float Price { get; }
        internal string FormattedPrice => Price.ToString("F2");

    }

}
