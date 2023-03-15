using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] NetworkVariable<float> health = new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [SerializeField] float maxHealth = 100f;

    [Space]
    [Header("Component")]
    [SerializeField] GameObject ui;
    [SerializeField] HealthBar healthbar;
    [SerializeField] PlayerController controller;


    public void Start()
    {
        if (IsServer)
        {
            health.Value = maxHealth;
        }
        if (IsOwner && healthbar)
        {
            ui.SetActive(true);
            healthbar.SetMaxHealth(maxHealth);
            healthbar.SetHealth(health.Value);
            health.OnValueChanged += UpdateHealthBar;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Health / get hit / die
    public void SetHealth(float health)
    {
        this.health.Value = health;
    }
    public void AddHealth(float regenValue)
    {
        SubmitAddHealthServerRpc(regenValue);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitAddHealthServerRpc(float regenValue)
    {
        this.health.Value = Mathf.Min(health.Value + regenValue, maxHealth);
    }

    public void GetHit(float damage, ulong killerId)
    {
        Debug.Log("Player, GetHit : damage = " + damage);
        SetHealth(Mathf.Max(0f, health.Value - damage));
        if (health.Value == 0)
        {
            KillPlayerClientRpc(NetworkObjectId);
        }
    }
    private void UpdateHealthBar(float previous, float current)
    {
        healthbar.SetHealth(current);
    }

    [ClientRpc]
    private void KillPlayerClientRpc(ulong playerid)
    {
        Debug.Log("Player, KillPlayerClientRpc : playerid = " + playerid);
        //GetNetworkObject(playerid).gameObject.GetComponentInChildren<Player>().controller.Die();
    }

    #endregion
}
