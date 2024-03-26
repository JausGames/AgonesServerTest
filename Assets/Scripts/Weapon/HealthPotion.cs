using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class OneUseItem : Item
{

    [SerializeField] protected List<ParticleSystem> useParticles;
    private void Awake()
    {
        Owner = GetComponentInParent<Hitable>();
    }
    public override bool Use()
    {
        SubmitBonusServerRpc();

        Owner.SoundManager.PlayClip(clipUse);

        ((Player)Owner).SetHoldItem(null);
        return true;
    }

    virtual public bool UseNotHold()
    {
        SubmitBonusServerRpc();

        Owner.SoundManager.PlayClip(clipUse);

        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitBonusServerRpc()
    {
        SubmitBonusClientRpc();
    }
    [ClientRpc]
    void SubmitBonusClientRpc()
    {
        foreach (var prtcl in useParticles)
        {
            prtcl.transform.parent = transform.parent;
            prtcl.Play();
            Destroy(prtcl.gameObject, 1f);
        }
    }
}

public class HealthPotion : OneUseItem
{
    [SerializeField] float bonus = 50f;

    public override bool Use()
    {
        var success = base.Use();

        if(success) Owner.AddHealth(bonus);

        return success;
    }

    public override bool UseNotHold()
    {
        var success = base.UseNotHold();

        if (success) Owner.AddHealth(bonus);

        return success;
    }

}
