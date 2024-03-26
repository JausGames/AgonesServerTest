using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class CombatController : NetworkBehaviour
{
    [SerializeField] private float offset = 45f;
    [Header("Inputs")]
    [SerializeField] NetworkVariable<Vector3> look = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] GameObject ui;

    [Space]
    [Header("Item")]
    [SerializeField] Item currentItem;
    [SerializeField] List<Item> allItems = new List<Item>();

    [Space]
    [Header("Components")]
    [SerializeField] private LayerMask hitablemask;


    [SerializeField] Player player;
    [SerializeField] Transform cameraContainer;
    [SerializeField] float cameraSpeed;

    [HideInInspector] UnityEvent<float> getHitCreditsEarnEvent = new UnityEvent<float>();

    [SerializeField] Transform rightHand;
    [SerializeField] AudioListener audioListener;
    [SerializeField] Transform target;
    [SerializeField] Quaternion rotationOffset;


    private bool reloading;
    private bool alive = true;
    private bool canRotate = true;
    private float lookSmoothVelocity = 2f;

    public Transform CameraContainer { get => cameraContainer; set => cameraContainer = value; }


    public LayerMask Hitablemask { get => hitablemask; set => hitablemask = value; }

    public Item CurrentItem
    {
        get => currentItem;
        set
        {
            currentItem = value;
        }
    }


    public bool Alive { get => alive; set => alive = value; }
    public bool CanRotate { get => canRotate; set => canRotate = value; }


    // Update is called once per frame
    private void Start()
    {
        //hitablemask = (1 << 3);
        if (IsOwner && IsLocalPlayer)
        {
            Cursor.lockState = CursorLockMode.Confined;
            //body.isKinematic = false;
            //cameraContainer.GetComponent<CameraFollow>().GetComponentInChildren<Camera>().enabled = true;
            //ui.SetActive(true);
            //audioListener.enabled = true;
        }

        var guids = AssetDatabase.FindAssets("t:prefab", new string[] { "Assets/Prefabs/Weapons" });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            allItems.Add(go.GetComponent<Item>());
        }
    }


    private void Update()
    {
        if(currentItem is Weapon)
        {
            foreach (var prtcl in ((Weapon)currentItem).shootParticles)
            {
                //prtcl.transform.position = ((Weapon)currentItem).canonEnd.position;
                prtcl.transform.LookAt(target);
            }
        }
    }

    private void LateUpdate()
    {
        if (IsOwner)
        {
            RotatePlayer();
            PlayAnimationServerRpc();
        }

    }


    [ServerRpc]
    internal void SubmitShotContactParticleServerRpc(int layer, Vector3 origin, Vector3 direction)
    {
        InstantiateShotContactParticlesClientRpc(layer, origin, direction);
    }

    [ClientRpc]
    private void InstantiateShotContactParticlesClientRpc(int layer, Vector3 origin, Vector3 direction)
    {
        ((Weapon)currentItem).InstantiateContactParticles(layer, origin, direction);
    }


    internal void SwitchItem(Item item)
    {
        SubmitChangeItemServerRpc(item.Id, OwnerClientId);
    }
    [ServerRpc]
    private void SubmitChangeItemServerRpc(int id, ulong owner)
    {
        if (currentItem != null)
            Destroy(CurrentItem.gameObject);
        var obj = Instantiate(FindItemById(id).Prefab, transform);
        var net = obj.GetComponent<NetworkObject>();
        net.SpawnWithOwnership(owner, true);
        obj.transform.parent = transform;
        CurrentItem = obj.GetComponent<Item>();
        CurrentItem.FindOwner();
        SubmitChangeItemClientRpc(net.NetworkObjectId, CurrentItem.Owner.NetworkObjectId);

    }

    [ClientRpc]
    private void SubmitChangeItemClientRpc(ulong objectId, ulong ownerObjId)
    {
        CurrentItem = GetNetworkObject(objectId).GetComponent<Item>();
        CurrentItem.Owner = GetNetworkObject(ownerObjId).GetComponent<Hitable>();
    }

    private Item FindItemById(int id)
    {
        foreach(var item in allItems)
        {
            if (item.Id == id) return item;
        }
        return null;
    }

    internal void UseItemOneShot(OneUseItem item)
    {
        if (!IsOwner || item == null || !alive) return;

        SubmitOneShotServerRpc(item.Id, OwnerClientId);
    }
    [ServerRpc]
    private void SubmitOneShotServerRpc(int id, ulong owner)
    {
        var obj = Instantiate(FindItemById(id).Prefab, transform);
        var net = obj.GetComponent<NetworkObject>();
        net.SpawnWithOwnership(owner, true);
        obj.transform.parent = transform;
        SubmitOneShotClientRpc(net.NetworkObjectId, CurrentItem.Owner.NetworkObjectId);
    }
    [ClientRpc]
    private void SubmitOneShotClientRpc(ulong objectId, ulong ownerObjId)
    {
        if (IsOwner)
        {
            var item = GetNetworkObject(objectId).GetComponent<OneUseItem>();
            item.Owner = GetNetworkObject(ownerObjId).GetComponent<Hitable>();
            item.UseNotHold();
            SubmitDestroyItemServerRpc(objectId);
        }
    }
    [ServerRpc]
    private void SubmitDestroyItemServerRpc(ulong objectId)
    {
        var item = GetNetworkObject(objectId).GetComponent<OneUseItem>();
        Destroy(item.gameObject);
    }

    internal void DestroyCurrentItem()
    {
        SubmitDestroyCurrentItemServerRpc();
    }
    [ServerRpc]
    private void SubmitDestroyCurrentItemServerRpc()
    {
        if (currentItem != null)
            Destroy(CurrentItem.gameObject);
        CurrentItem = null;
        SubmitDestroyCurrentItemClientRpc();

    }

    [ClientRpc]
    private void SubmitDestroyCurrentItemClientRpc()
    {
        CurrentItem = null;
    }


    [ServerRpc(RequireOwnership = false)]
    void PlayAnimationServerRpc()
    {
        PlayAnimationClientRpc();
    }

    [ClientRpc]
    void PlayAnimationClientRpc()
    {
        if (!IsOwner && canRotate)
        {
            RotatePlayer();
        }
    }

    [ServerRpc]
    void RotatePlayerServerRpc()
    {
        RotatePlayerClientRpc();
    }

    [ClientRpc]
    void RotatePlayerClientRpc()
    {
        if (!IsOwner)
        {
            RotatePlayer();
        }
    }

    private void RotatePlayer()
    {
        if (!canRotate) return;
        var angle = Mathf.Atan2(look.Value.y, look.Value.x) * Mathf.Rad2Deg + offset;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        RotatePlayerServerRpc();

    }
    internal void Attack(bool context)
    {
        if (!IsOwner || !context || currentItem == null || !alive) return;

        currentItem.Use();
    }


    internal void ShootBullet()
    {
        foreach (var prtcl in ((Weapon)currentItem).shootParticles)
        {
            prtcl.Play();
        }
    }
    [ServerRpc]
    public void SubmitShootServerRpc(Vector3 position, Quaternion rotation)
    {
        PlayShootParticleClientRpc(position, rotation);
    }

    [ClientRpc]
    void PlayShootParticleClientRpc(Vector3 position, Quaternion rotation)
    {
        if (IsOwner) return;
        foreach (var prtcl in ((Weapon)currentItem).shootParticles)
        {
            prtcl.transform.position = position;
            prtcl.transform.rotation = rotation;
            prtcl.Play();
        }
    }





    internal void Look(Vector2 vector2)
    {
        look.Value = (vector2 - (Vector2)transform.position).normalized;
    }
    //Called on client rpc
    public void Die()
    {
        if (!alive) return;

        alive = false;
    }
    public void SetAlive()
    {
        alive = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(look.Value, .2f);
    }
}
