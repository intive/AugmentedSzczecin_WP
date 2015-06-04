using System.Collections.Generic;

namespace AugmentedSzczecin.Models
{
    public static class SubcategoryList
    {
        private static readonly List<Subcategory> _subcategoryList = new List<Subcategory>()
                                                    {
                                                        new Subcategory() {Text = "Szkoła", EnumSubcategory = SubcategoryType.SCHOOL},
                                                        new Subcategory() {Text = "Szpital", EnumSubcategory = SubcategoryType.HOSPITAL},
                                                        new Subcategory() {Text = "Park", EnumSubcategory = SubcategoryType.PARK},
                                                        new Subcategory() {Text = "Pomnik", EnumSubcategory = SubcategoryType.MONUMENT},
                                                        new Subcategory() {Text = "Muzeum", EnumSubcategory = SubcategoryType.MUSEUM},
                                                        new Subcategory() {Text = "Urząd", EnumSubcategory = SubcategoryType.OFFICE},
                                                        new Subcategory() {Text = "Dworzec autobusowy", EnumSubcategory = SubcategoryType.BUS_STATION},
                                                        new Subcategory() {Text = "Dworzec kolejowy", EnumSubcategory = SubcategoryType.TRAIN_STATION},
                                                        new Subcategory() {Text = "Urząd pocztowy", EnumSubcategory = SubcategoryType.POST_OFFICE},
                                                        new Subcategory() {Text = "Kościół", EnumSubcategory = SubcategoryType.CHURCH},
                                                    };

        public static List<Subcategory> GetSubcategoryList()
        {
            return _subcategoryList;
        }
    }
}
