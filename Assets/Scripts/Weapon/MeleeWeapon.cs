using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField] float swingTime = .1f;
    WeaponTrigger trigger;
    [SerializeField] private float startSwingTime;
    private float startPosition;
    [Header("Swing melee")]
    [SerializeField] float swingAngle = 60f;
    [SerializeField] NetworkVariable<bool> isOnRightSide = new NetworkVariable<bool>(true, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);



    public override bool Use()
    {
        Debug.Log("Weapon, Use : Time = " + Time.time);
        Debug.Log("Weapon, Use : nextShot = " + nextShot);
        if (Time.time < nextShot) return false; // check cooldown & ammunition
        nextShot = Time.time + Stats.cooldown;
        SubmitSwingServerRpc();
        return true;
    }
    private void Awake()
    {
        FindOwner();
        trigger = GetComponentInChildren<WeaponTrigger>();
        trigger.Weapon = this;
        trigger.enabled = false;
        if (Owner)
            transform.localRotation = Quaternion.Euler(0, 0, isOnRightSide.Value ? swingAngle : -swingAngle);
        isOnRightSide.OnValueChanged += OnWeaponSideValueChange;
    }
    private void OnWeaponSideValueChange(bool previous, bool current)
    {
        startSwingTime = Time.time;
        startPosition = transform.localEulerAngles.z;
        trigger.ActivateTrigger();
        Owner.SoundManager.PlayClip(clipUse);
        foreach (var prtcl in shootParticles)
        {
            if (prtcl.isPaused || prtcl.isStopped) prtcl.Play();
            var em = prtcl.emission;
            em.enabled = true;
        }
    }

    private void Update()
    {
        var desiredAngle = isOnRightSide.Value ? swingAngle : -swingAngle;
        if (transform.localEulerAngles.z != desiredAngle)
        {
            var time = Mathf.Min((Time.time - startSwingTime) / swingTime, 1f);
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.LerpAngle(startPosition, desiredAngle, time));

            if (time == 1f)
            {
                trigger.DeactivateTrigger();

                foreach (var prtcl in shootParticles)
                {
                    var em = prtcl.emission;
                    em.enabled = false;
                }
            }
        }
    }
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSwingServerRpc()
    {
        SwingWeapon();
        SwingClientRpc();
    }
    [ClientRpc]
    void SwingClientRpc()
    {
        SwingWeapon();
    }

    internal void SwingWeapon()
    {
        if(IsOwner)
            isOnRightSide.Value = !isOnRightSide.Value;
    }
}
