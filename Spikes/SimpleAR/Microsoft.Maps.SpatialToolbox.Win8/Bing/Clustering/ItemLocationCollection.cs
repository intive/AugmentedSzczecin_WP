using System.Collections.Generic;

#if WINDOWS_APP
using Bing.Maps;
#elif WPF
using Microsoft.Maps.MapControl.WPF;
#elif WINDOWS_PHONE_APP
using Windows.Devices.Geolocation;
#elif WINDOWS_PHONE
using System.Device.Location;
#endif

namespace Microsoft.Maps.SpatialToolbox.Bing.Clustering
{
    /// <summary>
    /// A collection of location based items.
    /// </summary>
    public class ItemLocationCollection : List<ItemLocation>
    {
        #region Public Events

        public delegate void CollectionChangedEvent();

        /// <summary>
        /// Event that is triggered when the collection changes. 
        /// </summary>
        public event CollectionChangedEvent CollectionChanged;

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds an item to the collection for a location.
        /// </summary>
        /// <param name="item">Item to add to the collection.</param>
        /// <param name="location">Location that the item is linked to.</param>
#if WINDOWS_PHONE_APP
        public void Add(object item, Geopoint location)
#elif WINDOWS_PHONE
        public void Add(object item, GeoCoordinate location)
#else
        public void Add(object item, Location location)
#endif
        {
            base.Add(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Adds a range of ItemLocation's to the collection.
        /// This is more effienciet that adding item's individually 
        /// as the CollectionChanged event will only be fired once.
        /// </summary>
        /// <param name="items"></param>
        public void AddRange(ItemLocationCollection items)
        {
            base.AddRange(items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Clears all item locations from the collection.
        /// </summary>
        public new void Clear()
        {
            base.Clear();

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Gets an item by index in the collection.
        /// </summary>
        /// <param name="index">Index of item.</param>
        /// <returns>Item in specified index.</returns>
        public ItemLocation GetItemByIndex(int index)
        {
            if (index < this.Count)
            {
                return this[index];
            }

            return null;
        }

        /// <summary>
        /// Gets a collection of items for a list of indicies. 
        /// </summary>
        /// <param name="index">List of indicies.</param>
        /// <returns>Collection of items.</returns>
        public ItemLocationCollection GetItemsByIndex(List<int> index)
        {
            var items = new ItemLocationCollection();

            foreach (var i in index)
            {
                if (i < this.Count)
                {
                    items.Add(this[i]);
                }
            }

            return items;
        }

        /// <summary>
        /// Inserts an item into the collection at a specified index. 
        /// </summary>
        /// <param name="index">Index to add item to.</param>
        /// <param name="item">Item to add to collection.</param>
        /// <param name="location">Location that item is for.</param>
#if WINDOWS_PHONE_APP
        public void Insert(int index, object item, Geopoint location)
#elif WINDOWS_PHONE
        public void Insert(int index, object item, GeoCoordinate location)
#else
        public void Insert(int index, object item, Location location)
#endif
        {
            base.Insert(index, new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Inserts a collection of locations at a specified index.
        /// </summary>
        /// <param name="index">Index to insert items at.</param>
        /// <param name="items">Collection of items to add.</param>
        public void InsertRange(int index, ItemLocationCollection items)
        {
            base.InsertRange(index, items);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Removes an item from the collection.
        /// </summary>
        /// <param name="item">Item to remove.</param>
        /// <param name="location">Location of item to remove.</param>
#if WINDOWS_PHONE_APP
        public void Remove(object item, Geopoint location)
#elif WINDOWS_PHONE
        public void Remove(object item, GeoCoordinate location)
#else
        public void Remove(object item, Location location)
#endif
        {
            base.Remove(new ItemLocation(item, location));

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Removes an item at a specified index. 
        /// </summary>
        /// <param name="index">Index of item to remove.</param>
        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);

            if (CollectionChanged != null)
            {
                CollectionChanged();
            }
        }

        /// <summary>
        /// Removes a range of items from the collection.
        /// </summary>
        /// <param name="index">Index of where to start removing items.</param>
        /// <param name="count">Number of items to remove from collection.</param>
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
