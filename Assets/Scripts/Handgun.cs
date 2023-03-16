using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Handgun : Weapon
{

    public override bool Use(ShooterController owner)
    {
        var canShoot = base.Use(owner);
        if (!canShoot) return false;
        Debug.DrawRay((Vector3)canonEnd.position, (Vector3)canonEnd.position + (Vector3)transform.forward, Color.red, 3f);
        var hits = Physics2D.RaycastAll((Vector2)canonEnd.position, (Vector2)canonEnd.position + (Vector2)transform.forward, 10f, owner.Hitablemask);
        FindRayVictims(owner, hits);
        return true;
    }

}
