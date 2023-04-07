using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeWeapon : Weapon
{
    [SerializeField] bool isOnRight = true;
    [SerializeField] float swingSpeed = 5f;
    [SerializeField] float swingAngle = 66f;
    WeaponTrigger trigger;


    public override bool Use(CombatController owner)
    {
        Debug.Log("Weapon, Use : Time = " + Time.time);
        Debug.Log("Weapon, Use : nextShot = " + nextShot);
        if (Time.time < nextShot) return false; // check cooldown & ammunition
        nextShot = Time.time + Stats.cooldown;
        owner.Swing();
        return true;
    }
    private void Awake()
    {
        Owner = GetComponentInParent<Hitable>();
        trigger = GetComponentInChildren<WeaponTrigger>();
        trigger.Weapon = this;
    }

    private void Update()
    {
        if(isOnRight && transform.localEulerAngles.z != 66f)
        {
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, swingAngle, swingSpeed * Time.deltaTime));
            
        }
        
        if(!isOnRight && transform.localEulerAngles.z != -66f)
        {
            transform.localRotation = Quaternion.Euler(0, 0, Mathf.MoveTowardsAngle(transform.localEulerAngles.z, -swingAngle, swingSpeed * Time.deltaTime));
        }
    }

    internal void SwingWeapon()
    {
        isOnRight = !isOnRight;
    }
}
