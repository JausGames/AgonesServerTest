using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : Hitable
{
    public NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("no name", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    [Header("Component")]
    [SerializeField] GameObject ui;
    [SerializeField] Inventory inventory;
    [SerializeField] HealthBar healthbar;
    [SerializeField] CreditsUi creditsUi;
    [SerializeField] PlayerController controller;
    [SerializeField] CombatController combat;

    Interactable currentInteractable;
    private bool inInteraction;

    public float MaxHealth { get => maxHealth; set => maxHealth = value; }
    public bool InInteraction
    {
        get => inInteraction;
        set
        {
            combat.CanRotate = !value;
            inInteraction = value;
        }
    }
    public string Name { get => playerName.Value.ToString(); set => playerName.Value = new FixedString64Bytes(value); }


    public void Start()
    {
        combat = GetComponent<CombatController>();
        ui.SetActive(false);
        if (IsServer)
        {
            health.Value = maxHealth;
        }
        if (IsOwner)
        {
            ui.SetActive(true);
            healthbar.SetMaxHealth(maxHealth);
            healthbar.SetHealth(health.Value);
            health.OnValueChanged += UpdateHealthBar;
            Credits.OnValueChanged += UpdateCreditsUi;
        }
    }

    internal void UseOrSwitchQuickItem(int value)
    {
        var item = inventory.QuickItems[value].Item;

        if(item is OneUseItem)
        {
            combat.UseItemOneShot((OneUseItem)item);
            inventory.DestroyOneShotItem((OneUseItem)item);
        }
        else if(item is Weapon)
        {
            inventory.CurrentItem.Item = item;
            SetHoldItem(item);
        }
    }

    internal void SetHoldItem(Item item)
    {
        if (item == null)
        {
            combat.DestroyCurrentItem();
            inventory.DestroyCurrentItem();
        }
        else
            combat.SwitchItem(item);
    }

    internal void OpenCloseInventory()
    {
        inventory.OpenInventory();
    }

    internal void TryInteract()
    {
        if (currentInteractable)
        {
            if (InInteraction)
            {
                currentInteractable.CloseInteract(this);
                InInteraction = false;
            }
            else
            {
                currentInteractable.Interact(this);
                InInteraction = true;
            }
        }
    }

    internal void AddObjectToInventory(Item item)
    {
        inventory.AddItem(item);
    }

    private void Update()
    {
        if (!IsOwner) return;
        var col = Physics2D.OverlapCircle(transform.position + transform.up, .2f, LayerMask.GetMask("Interactable"));
        if (col)
        {
            var interactable = col.GetComponent<Interactable>() != null ? col.GetComponent<Interactable>() : col.GetComponentInChildren<Interactable>();
            if (interactable && currentInteractable != interactable)
            {
                Debug.Log("Player, Update : Switch");
                if (currentInteractable)
                    currentInteractable.HideInteractionText();
                currentInteractable = interactable;
                interactable.ShowInteractionText();
            }
        }
        else if (currentInteractable)
        {

            Debug.Log("Player, Update : Stop");
            currentInteractable.HideInteractionText();
            if (InInteraction)
                currentInteractable.CloseInteract(this);
            currentInteractable = null;
            InInteraction = false;

        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.up, .2f);
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (IsOwner) Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    #region Health / get hit / die

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
        combat.Die();
        controller.Die();
        GetComponent<Collider2D>().isTrigger = true;
        GetComponent<Collider2D>().enabled = false;

        StartCoroutine(Respawn());

    }
    /*protected override void EarnCredits(int creditBonus)
    {
        base.EarnCredits(creditBonus);
        //UpdateCreditsUi(wallet.Amount - creditBonwallet.Amount);
    }*/
    internal void UpdateCreditsUi(int previousValue, int newValue)
    {
        if (creditsUi)
            creditsUi.Amount = newValue;
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
        combat.Alive = true;
        GetComponent<Collider2D>().isTrigger = false;
        GetComponent<Collider2D>().enabled = true;

        SetAliveClientRpc();
    }
    [ClientRpc]
    public void SetAliveClientRpc()
    {
        GetComponent<Collider2D>().isTrigger = false;
        GetComponent<Collider2D>().enabled = true;
        combat.SetAlive();
        controller.SetAlive();
    }

    #endregion
}
