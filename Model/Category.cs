using System.Collections.Generic;
using System.Windows;

namespace AhorcadoClient.Model
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public string ResourceKey { get; set; }

        public Category(int categoryId, string nameKeyBase)
        {
            CategoryID = categoryId;
            ResourceKey = $"Category_{nameKeyBase}";
            CategoryName = Application.Current.Resources[ResourceKey]?.ToString() ?? nameKeyBase;
        }

        public override string ToString()
        {
            return CategoryName;
        }

        public static List<Category> GetDefaultCategories()
        {
            return new List<Category>
            {
                new Category(1, "Animals"),
                new Category(2, "Fruits"),
                new Category(3, "Countries"),
                new Category(4, "Sports"),
                new Category(5, "Movies"),
                new Category(6, "Technology"),
                new Category(7, "Food"),
                new Category(8, "Colors"),
                new Category(9, "Clothing"),
                new Category(10, "Music")
            };
        }
    }
}