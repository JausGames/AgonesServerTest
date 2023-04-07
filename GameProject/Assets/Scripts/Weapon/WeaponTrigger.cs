using System.Collections;
using System.Collections.Generic;
using UnityEngine;

internal class WeaponTrigger : MonoBehaviour
{
    [SerializeField] Collider2D collider;
    [SerializeField] Weapon weapon;

    public Weapon Weapon { get => weapon; set => weapon = value; }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        //if (!owner.IsOwner) return;
        if (!enabled) return;
        if (collision.GetComponent<Hitable>())
        {
            Debug.Log("OnTriggerEnter2D");
            var hitable = collision.GetComponent<Hitable>();
            hitable.SummitGetHitServerRpc(hitable.NetworkObjectId, weapon.Stats.damage, weapon.Stats.knockback, weapon.Stats.knockTime, weapon.Owner.NetworkObjectId);

        }
    }
}
