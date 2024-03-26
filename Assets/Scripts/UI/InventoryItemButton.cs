using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    [SerializeField] TMPro.TextMeshProUGUI lbl_name;
    [SerializeField] TMPro.TextMeshProUGUI lbl_nb;
    [SerializeField] Item item;
    [SerializeField] Image image;
    [SerializeField] Button button;

    private void Awake()
    {
        Item = item;
    }

    public string Name { get => lbl_name.text; set => lbl_name.text = value; }
    public string Nb { get => lbl_nb.text; set => lbl_nb.text = value; }
    public Sprite Sprite { get => image.sprite; set => image.sprite = value; }
    public Button Button { get => button; set => button = value; }
    public Item Item { get => item; 
        set
        {
            item = value;
            if(item)
            {
                image.enabled = true;
                Sprite = item.Sprite;
                Name = item.name;
            }
            else
            {
                image.enabled = false;
                Sprite = null;
                Name = "";
            }
        }
    }

}
