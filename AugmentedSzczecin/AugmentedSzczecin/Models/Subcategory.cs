namespace AugmentedSzczecin.Models
{
    public class Subcategory
    {
        private string _text;
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                if (value != _text)
                {
                    _text = value;
                }
            }
        }

        private SubcategoryType _enumSubcategory;
        public SubcategoryType EnumSubcategory
        {
            get
            {
                return _enumSubcategory;
            }
            set
            {
                if (value != _enumSubcategory)
                {
                    _enumSubcategory = value;
                }
            }
        }

        public override string ToString()
        {
            return _text;
        }
    }
}