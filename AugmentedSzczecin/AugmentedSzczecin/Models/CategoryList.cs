using System.Collections.Generic;

namespace AugmentedSzczecin.Models
{
    public static class CategoryList
    {
        private static readonly List<Category> _categoryList = new List<Category>()
                                                    {
                                                        new Category() {Text = "Miejsca publiczne", EnumCategory = CategoryType.PLACE},
                                                        new Category() {Text = "Firmy i usługi", EnumCategory = CategoryType.POI},
                                                        new Category() {Text = "Wydarzenia", EnumCategory = CategoryType.EVENT},
                                                        new Category() {Text = "Znajomi", EnumCategory = CategoryType.PERSON},
                                                        new Category() {Text = "Wszystkie", EnumCategory = CategoryType.ALL},
                                                    };

        public static List<Category> GetCategoryList()
        {
            return _categoryList;
        }
    }
}
