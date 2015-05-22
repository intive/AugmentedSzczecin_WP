using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AugmentedSzczecin.Models
{
    public enum Category
    {
        PLACE,
        POI,
        EVENT,
        PERSON,
        ALL
    }

    public class Categories
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

        private Category _enumCategory;
        public Category EnumCategory
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
