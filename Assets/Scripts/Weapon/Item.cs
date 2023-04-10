using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

abstract public class Item : NetworkBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] new string name;
    [SerializeField] int price;
    [SerializeField] Sprite sprite;

    int id;
    public GameObject Prefab { get => prefab; set => prefab = value; }
    public int Id { get => id; set => id = value; }
    public int Price { get => price; set => price = value; }
    public Sprite Sprite { get => sprite; set => sprite = value; }

    public abstract bool Use();
}
