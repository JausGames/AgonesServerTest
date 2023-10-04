using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MeleeZoneWeapon : Weapon
{
    WeaponTrigger trigger;
    [SerializeField] float swingTime = .1f;
    [SerializeField] private float startSwingTime;
    //[Header("Zone melee")]

    private void Awake()
    {
        Owner = GetComponentInParent<Hitable>();
        trigger = GetComponentInChildren<WeaponTrigger>();
        trigger.Weapon = this;
        trigger.enabled = false;
    }
    public override bool Use()
    {
        Debug.Log("Weapon, Use : Time = " + Time.time);
        Debug.Log("Weapon, Use : nextShot = " + nextShot);
        if (Time.time < nextShot) return false; // check cooldown & ammunition
        nextShot = Time.time + Stats.cooldown;
        SubmitSwingServerRpc();
        return true;
    }
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSwingServerRpc()
    {
        SwingClientRpc();
    }
    [ClientRpc]
    void SwingClientRpc()
    {
        StartCoroutine(SwingCoroutine());
        foreach(var prtcl in shootParticles)
        {
            prtcl.Play();
        }
    }

    private IEnumerator SwingCoroutine()
    {
        trigger.ActivateTrigger();
        yield return new WaitForSeconds(swingTime);
        trigger.DeactivateTrigger();
    }
}
