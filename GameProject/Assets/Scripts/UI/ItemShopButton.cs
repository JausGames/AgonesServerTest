using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI lbl_name;
    [SerializeField] TMPro.TextMeshProUGUI lbl_price;
    [SerializeField] Item item;
    [SerializeField] Image image;
    [SerializeField] Button button;

    public string Name { get => lbl_name.text; set => lbl_name.text = value; }
    public string Price { get => lbl_price.text; set => lbl_price.text = value; }
    public Sprite Sprite { get => image.sprite; set => image.sprite = value; }
    public Button Button { get => button; set => button = value; }
    public Item Item
    {
        get => item;
        set
        {
            item = value;
            if (item)
            {
                image.enabled = true;
                Sprite = item.Sprite;
                Price = item.Price.ToString()+"$";
                Name = item.name;
            }
            else
            {
                image.enabled = false;
                Sprite = null;
                Name = "";
                Price = "";
            }
        }
    }
}
