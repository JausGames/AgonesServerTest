using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Player : Hitable
{
    [Header("Component")]
    [SerializeField] GameObject ui;
    [SerializeField] HealthBar healthbar;
    [SerializeField] PlayerController controller;
    [SerializeField] CombatController shooter;

    public NetworkVariable<float> Health { get => health; set => health = value; }
    public float MaxHealth { get => maxHealth; set => maxHealth = value; }

    public void Start()
    {
        shooter = GetComponent<CombatController>();
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
    void LateUpdate()
    {
        if (IsOwner) Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
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

    public override void TakeDamage(float damage, float knockback, float knocktime, Vector3 direction, ulong killerId)
    {
        Debug.Log("Player, GetHit : damage = " + damage);
        SetHealth(Mathf.Max(0f, health.Value - damage));
        if (health.Value == 0)
        {
            DieClientRpc(NetworkObjectId);
        }
    }
    private void UpdateHealthBar(float previous, float current)
    {
        healthbar.SetHealth(current);
    }

    public override void Die()
    {
        shooter.Die();
        controller.Die();
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(Respawn());

    }
    public IEnumerator Respawn()
    {
        yield return new WaitForSeconds(5f);

        if (IsOwner)
            SubmitRespawnServerRpc();
    }

    [ServerRpc]
    private void SubmitRespawnServerRpc()
    {
        Health.Value = MaxHealth;
        shooter.Alive = true;
        SetAliveClientRpc();
        shooter.SetAliveClientRpc();
        controller.SetAliveClientRpc();
    }
    [ClientRpc]
    public void SetAliveClientRpc()
    {
        GetComponent<Collider2D>().isTrigger = false;
        GetComponent<Collider2D>().enabled = true;
    }

    #endregion
}
