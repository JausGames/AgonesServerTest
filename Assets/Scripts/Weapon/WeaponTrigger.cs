using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class WeaponTrigger : MonoBehaviour
{
    [SerializeField] Collider2D collider;
    [SerializeField] Weapon weapon;
    [SerializeField] List<Hitable> victims = new List<Hitable>();

    public Weapon Weapon { get => weapon; set => weapon = value; }
    private void Update()
    {
        Debug.Log("enabled ? " + enabled);   
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!enabled) return;
        if (collision.GetComponent<Hitable>())
        {
            Debug.Log("OnTriggerEnter2D");
            var hitable = collision.GetComponent<Hitable>();
            if (hitable == weapon.Owner || victims.Contains(hitable)) return;
            victims.Add(hitable);
            hitable.SummitGetHitServerRpc(hitable.NetworkObjectId, weapon.Stats.damage, weapon.Stats.knockback, weapon.Stats.knockTime, weapon.Owner.NetworkObjectId);

        }
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!enabled) return;
        if (collision.GetComponent<Hitable>())
        {
            Debug.Log("OnTriggerEnter2D");
            var hitable = collision.GetComponent<Hitable>();
            if (hitable == weapon.Owner || victims.Contains(hitable)) return;
            victims.Add(hitable);
            hitable.SummitGetHitServerRpc(hitable.NetworkObjectId, weapon.Stats.damage, weapon.Stats.knockback, weapon.Stats.knockTime, weapon.Owner.NetworkObjectId);

        }
    }
    public void ActivateTrigger()
    {
        victims.Clear();
        enabled = true;
    }
    public void DeactivateTrigger()
    {
        enabled = false;
    }
}
