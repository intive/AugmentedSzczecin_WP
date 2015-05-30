namespace AugmentedSzczecin.Models
{
    public class Category
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

        private CategoryType _enumCategory;
        public CategoryType EnumCategory
        {
            get
            {
                return _enumCategory;
            }
            set
            {
                if (value != _enumCategory)
                {
                    _enumCategory = value;
                }
            }
        }

        public override string ToString()
        {
            return _text;
        }
    }
}