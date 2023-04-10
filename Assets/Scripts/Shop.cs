using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Shop : MonoBehaviour
{
    [SerializeField] GameObject canvas;
    [SerializeField] RectTransform buttonContainer;
    [SerializeField] GameObject buttonPrefab;
    List<Button> buttons;

    [SerializeField] List<GameObject> items;

    private void Awake()
    {
        canvas.SetActive(false);

        foreach(var item in items)
        {
            var weapon = item.GetComponent<Weapon>();
            var btn = Instantiate(buttonPrefab, buttonContainer);
            var shopbtn = btn.GetComponentInChildren<ItemShopButton>();
            shopbtn.Name = weapon.name;
            shopbtn.Price = weapon.Price + "$";
            shopbtn.Sprite = weapon.Sprite;
        }
    }

    void OpenShop()
    {
        canvas.SetActive(true);
    }
    void CloseShop()
    {
        canvas.SetActive(false);
    }
}
