using System.Collections.Generic;
using System.Linq;
using Windows.Devices.Geolocation;

namespace AugmentedSzczecin.PointBasedClustering
{
    public class ItemLocationCollection : List<ItemLocation>
    {
        #region Public Events

        public delegate void CollectionChangedEvent();
        public event CollectionChangedEvent CollectionChanged;

        #endregion

        #region Public Methods

        public void Add(object item, Geopoint location)
        {
            base.Add(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void AddRange(ItemLocationCollection items)
        {
            base.AddRange(items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void Clear()
        {
            base.Clear();

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public ItemLocation GetItemByIndex(int index)
        {
            if (index < Count)
            {
                return this[index];
            }

            return null;
        }

        public ItemLocationCollection GetItemsByIndex(List<int> index)
        {
            var items = new ItemLocationCollection();
            items.AddRange(from i in index where i < Count select this[i]);

            return items;
        }

        public void Insert(int index, object item, Geopoint location)
        {
            base.Insert(index, new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void InsertRange(int index, ItemLocationCollection items)
        {
            base.InsertRange(index, items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public void Remove(object item, Geopoint location)
        {
            base.Remove(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        #endregion
    }
}
