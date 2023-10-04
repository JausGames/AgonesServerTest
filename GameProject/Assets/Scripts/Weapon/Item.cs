using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

abstract public class Item : NetworkBehaviour
{
    [SerializeField] int id;
    [SerializeField] GameObject prefab;
    [SerializeField] new string name;
    [SerializeField] int price;
    [SerializeField] Sprite sprite;
    [SerializeField] Hitable owner;


    [SerializeField] protected AudioClip clipUse;
    public Hitable Owner { get => owner; set => owner = value; }

    public GameObject Prefab { get => prefab; set => prefab = value; }
    public int Id { get => id; set => id = value; }
    public int Price { get => price; set => price = value; }
    public Sprite Sprite { get => sprite; set => sprite = value; }

    public abstract bool Use();

    public void FindOwner()
    {
        Debug.Log("Item, FindOwner : Test = " + GetComponentInParent<Hitable>());
        Owner = GetComponentInParent<Hitable>();
    }
}
