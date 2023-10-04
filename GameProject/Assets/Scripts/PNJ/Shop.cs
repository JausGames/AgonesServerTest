using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Shop : Interactable
{
    [SerializeField] Player player;
    [SerializeField] GameObject canvas;
    [SerializeField] RectTransform buttonContainer;
    [SerializeField] GameObject buttonPrefab;
    List<Button> buttons;

    [SerializeField] List<Item> items;

    private void Awake()
    {
        canvas.SetActive(false);


        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Prefabs/Weapons" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            items.Add(go.GetComponent<Item>());
        }

        foreach (var item in items)
        {
            var btn = Instantiate(buttonPrefab, buttonContainer);
            var shopbtn = btn.GetComponentInChildren<ItemShopButton>();
            shopbtn.Button.onClick.AddListener(delegate { BuyObject(shopbtn.Item); });
            shopbtn.Item = item;
        }
    }

    void BuyObject(Item item)
    {
        if (player == null || !this.player.RemoveCredits(item.Price)) return;

        player.AddObjectToInventory(item);

    }

    void CloseShop()
    {
        canvas.SetActive(false);
    }

    public override void Interact(Player player)
    {
        this.player = player;
        canvas.SetActive(true);
    }
    public override void CloseInteract(Player player)
    {
        this.player = player;
        canvas.SetActive(false);
    }
}
