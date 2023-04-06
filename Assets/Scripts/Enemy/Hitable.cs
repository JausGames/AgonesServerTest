using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

    public class Hitable : NetworkBehaviour
    {
        [Header("Status")]
        [SerializeField] protected bool alive;

        [Header("Characteristics")]
        [SerializeField] protected float maxHealth = 100f;
        [Header("Stats")]
        [SerializeField] protected NetworkVariable<float> health = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    protected Rigidbody2D body;

        // Update player health
        virtual public void TakeDamage(float damage, float knockback, float knocktime, Vector3 direction, ulong killerid)
        {
            if (health.Value == 0f) return;
            health.Value -= damage;
            if (health.Value <= 0)
            {
                health.Value = 0;
                DieClientRpc(NetworkObjectId);
        }
            if(knockback > 0f)
            {
                body.velocity = knockback * direction;
                //GetComponent<Rigidbody>().AddForce(stats.knockback * direction, ForceMode.Impulse);
            }
    }
    [ClientRpc]
    virtual protected void DieClientRpc(ulong objecid)
    {
        Debug.Log("Player, KillPlayerClientRpc : playerid = " + objecid);
        GetNetworkObject(objecid).gameObject.GetComponent<Hitable>().Die();
    }

    virtual public void Die()
        {
            // Code for death event
            alive = false;
            body.velocity = Vector3.zero;
            body.isKinematic = true;
            //body.useGravity = false;
        }
    [ServerRpc(RequireOwnership = false)]
    public void SummitGetHitServerRpc(ulong playerid, float damage, float knockback, float knocktime, ulong originId)
    {
        Debug.Log("CombatController, SummitGetHitServerRpc : touched player = #" + playerid);
        var origin = GetNetworkObject(originId).GetComponent<Hitable>().transform.position;
        var victim = GetNetworkObject(playerid).GetComponent<Hitable>().transform.position;

        GetNetworkObject(playerid).GetComponent<Hitable>().TakeDamage(damage, knockback, knocktime, victim - origin, originId);
    }
}
