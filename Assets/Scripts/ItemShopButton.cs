using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemShopButton : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI lbl_name;
    [SerializeField] TMPro.TextMeshProUGUI lbl_price;
    [SerializeField] Image image;

    public string Name { get => lbl_name.text; set => lbl_name.text = value; }
    public string Price { get => lbl_price.text; set => lbl_price.text = value; }
    public Sprite Sprite { get => image.sprite; set => image.sprite = value; }
}
