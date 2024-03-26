using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventory : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] GameObject canvas;
    [SerializeField] RectTransform buttonContainer;
    [SerializeField] GameObject buttonPrefab;
    List<GameObject> buttons = new List<GameObject>();

    [SerializeField] List<Item> items = new List<Item>();

    [SerializeField] Item selectedItem;

    [SerializeField] InventoryItemButton currentItem;
    [SerializeField] List<InventoryItemButton> quickItems;

    public InventoryItemButton CurrentItem { get => currentItem; set => currentItem = value; }
    public List<InventoryItemButton> QuickItems { get => quickItems; set => quickItems = value; }

    private void Awake()
    {
        canvas.SetActive(false);

        currentItem.Button.onClick.AddListener(delegate
        {
            currentItem.Item = selectedItem;
            player.SetHoldItem(selectedItem);
        });
        quickItems[0].Button.onClick.AddListener(delegate { quickItems[0].Item = selectedItem; });
        quickItems[1].Button.onClick.AddListener(delegate { quickItems[1].Item = selectedItem; });
        quickItems[2].Button.onClick.AddListener(delegate { quickItems[2].Item = selectedItem; });
        quickItems[3].Button.onClick.AddListener(delegate { quickItems[3].Item = selectedItem; });

        SetUiUp();
    }

    private void SetUiUp()
    {
        foreach (var button in buttons) Destroy(button);
        buttons.Clear();

        var oneShotItems = new List<Item>();
        Dictionary<Type, int> dict = new Dictionary<Type, int>();

        foreach (var item in items)
        {
            if (item is OneUseItem)
            {
                var contain = dict.ContainsKey(item.GetType());
                if (!contain)
                    dict.Add(item.GetType(), 1);
                else
                {
                    var val = dict.GetValueOrDefault(item.GetType());
                    dict.Remove(item.GetType());
                    dict.Add(item.GetType(), val + 1);
                }
            }
            else
            {
                var btn = Instantiate(buttonPrefab, buttonContainer);
                buttons.Add(btn);
                var inventorybtn = btn.GetComponentInChildren<InventoryItemButton>();
                inventorybtn.Item = item;
                inventorybtn.Button.onClick.AddListener(delegate { SelectItem(inventorybtn.Item); });
            }
        }

        foreach (var key in dict.Keys)
        {
            Item item = null;
            foreach (var it in items)
                if (it.GetType() == key) item = it;

            var btn = Instantiate(buttonPrefab, buttonContainer);
            buttons.Add(btn);
            var shopbtn = btn.GetComponentInChildren<InventoryItemButton>();
            shopbtn.Item = item;
            shopbtn.Nb = dict.GetValueOrDefault(key).ToString();
            shopbtn.Button.onClick.AddListener(delegate { SelectItem(shopbtn.Item); });
        }
    }

    internal void DestroyOneShotItem(OneUseItem item)
    {
        items.Remove(item);
        var count = 0;
        Item firstFound = null;

        foreach (var it in items)
        {
            if (it.GetType() == item.GetType())
            {
                count++;
                firstFound = it;
            }
        }
        foreach (var quick in quickItems)
        {
            if (quick.Item && quick.Item.GetType() == item.GetType())
                quick.Item = firstFound;
        }

        SetUiUp();
    }

    internal void DestroyCurrentItem()
    {
        items.Remove(currentItem.Item);
        CurrentItem.Item = null;
        SetUiUp();
    }

    void SelectItem(Item item)
    {
        selectedItem = item;
    }

    public void OpenInventory()
    {
        canvas.SetActive(!canvas.activeSelf);
    }

    internal void AddItem(Item item)
    {
        items.Add(item);
        if (currentItem.Item == null)
        {
            selectedItem = item;
            currentItem.Item = selectedItem;
            player.SetHoldItem(selectedItem);
        }
        SetUiUp();
    }
}
