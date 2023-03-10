using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BugsFarm.UI
{
    public class UIListComponent : MonoBehaviour
    {
        public event Action<object> OnItemClick;
        public event Action OnItemsClear;
        
        [SerializeField] private UIListItem _itemPrefab;
        [SerializeField] private Transform _content;
        
        private Dictionary<UIListItem, object> _items;
        
        public void SetInteractable(bool interactable)
        {
            if (_items.Count == 0) return;
            
            foreach (var item in _items.Keys)
            {
                item.SetInteractable(interactable);
            }
        }
        
        public void SetImage(object itemID, Sprite sprite)
        {
            var item = GetItem(itemID);
            if(item.IsNullOrDefault()) return;
            item.SetImage(sprite);
        }
        
        public void SetText(object itemID, string text)
        {
            var item = GetItem(itemID);
            if(item.IsNullOrDefault()) return;
            item.SetText(text);
        }
        
        public void FillList(IEnumerable<object> itemsData)
        {
            if (itemsData == null)
            {
                throw new NullReferenceException($"{this} : FillList :: ItemsData enumerable is null.");
            }

            Clear();
        }
        
        public IEnumerable<UIListItem> GetItems()
        {
            return _items?.Keys;
        }
        
        public UIListItem GetItem(object itemID)
        {
            return _items.ContainsValue(itemID) ?_items.FirstOrDefault(x=> x.Value == itemID).Key : default;
        }
        
        public void Clear()
        {
            if(_items == null) return;
            
            while (_items.Count > 0)
            {
                var item = _items.Keys.First();
                _items.Remove(item);
                Destroy(item.gameObject);
            }
            _items.Clear();
            OnItemsClear?.Invoke();
        }
        
        private void ItemOnClick(object sender, EventArgs e)
        {
            var itemData = _items[(UIListItem) sender];
            OnItemClick?.Invoke(itemData);
        }
    }
}