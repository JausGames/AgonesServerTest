using System;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using System.Collections;

public class ShooterController : NetworkBehaviour
{
    [SerializeField] private float offset = 45f;
    [Header("Inputs")]
    [SerializeField] NetworkVariable<Vector3> look = new NetworkVariable<Vector3>(Vector3.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] GameObject ui;

    [Space]
    [Header("Item")]
    [SerializeField] Weapon currentWeapon;

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
    private float lookSmoothVelocity = 2f;

    public Transform CameraContainer { get => cameraContainer; set => cameraContainer = value; }


    public LayerMask Hitablemask { get => hitablemask; set => hitablemask = value; }
 
    public Weapon CurrentWeapon
    {
        get => currentWeapon;
        set
        {

            if (currentWeapon == null  || value.Id != currentWeapon.Id)
            {
                currentWeapon = value;
            }


        }
    }

    public bool Alive { get => alive; set => alive = value; }


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

        
    }


    private void Update()
    {
        foreach (var prtcl in ((Weapon)currentWeapon).shootParticles)
        {
            prtcl.transform.position = ((Weapon)currentWeapon).canonEnd.position;
            prtcl.transform.LookAt(target);
        }
        


    }
    private void LateUpdate()
    {
        RotatePlayer();

        PlayAnimationServerRpc();
        
    }


    [ServerRpc]
    internal void SubmitShotContactParticleServerRpc(int layer, Vector3 origin, Vector3 direction)
    {
        InstantiateShotContactParticlesClientRpc(layer, origin, direction);
    }

    [ClientRpc]
    private void InstantiateShotContactParticlesClientRpc(int layer, Vector3 origin, Vector3 direction)
    {
        ((Weapon)currentWeapon).InstantiateContactParticles(layer, origin, direction);
    }

    private void SetItemUp(Weapon value)
    {

        if (currentWeapon != null)
            Destroy(currentWeapon.gameObject); ;
        currentWeapon = Instantiate(value, rightHand);
        //if (item.GetComponent<Weapon>()) bulletStart = item.GetComponent<Weapon>().canonEnd;
        //else bulletStart = null;
        currentWeapon.Id = value.GetComponent<Item>().Id;
        currentWeapon.Prefab = value.GetComponent<Item>().Prefab;

        /*if (heldItem.GetComponent<Weapon>()) bulletStart = heldItem.GetComponent<Weapon>().canonEnd;
        else bulletStart = null;*/
        //seat.Item = heldItem.gameObject;

        //bulletStart = value.GetComponent<Weapon>().canonEnd;

        //seat.Item = value.Prefab;
        //heldItem = seat.Item.GetComponent<Item>();

        /*if (heldItem is Weapon)
        {
            ((Weapon)currentWeapon).shootParticles[0].transform.parent.parent = seat.transform;
        }*/
    }

    [ServerRpc]
    private void SubmitChangeItemServerRpc(int id, bool isWeapon)
    {
        if(IsServer && !IsOwner)
        {
            //SetItemUp(FindItemById(id, isWeapon).Prefab.GetComponent<Item>());
        }
        ChangeItemClientRpc(id, isWeapon);
    }
    [ClientRpc]
    private void ChangeItemClientRpc(int id, bool isWeapon)
    {
        if (IsServer || IsOwner) return;
        //SetItemUp(FindItemById(id, isWeapon).Prefab.GetComponent<Item>());
    }

    internal void SwitchItem(bool destroyCurrent = false)
    {
        /*if (destroyCurrent)
        {
            if (heldItem is Weapon) currentWeapon = null;
            else currentTool = null;

            Destroy(heldItem.gameObject);
        }

        if ((heldItem is Tool || heldItem == null) && currentWeapon != null) HeldItem = currentWeapon;
        else if ((heldItem is Weapon || heldItem == null) && currentTool != null) HeldItem = currentTool;*/
    }


    [ServerRpc(RequireOwnership = false)]
    void PlayAnimationServerRpc()
    {
        PlayAnimationClientRpc();
    }

    [ClientRpc]
    void PlayAnimationClientRpc()
    {
        if(!IsOwner)
        {
            RotatePlayer();
        }
        //animatorController.Direction = (x * Vector2.right + y * Vector2.up).normalized;
    }

    private void RotatePlayer()
    {
        /*var mouseScreenPos = look.Value;
        var startingScreenPos = Camera.main.WorldToScreenPoint(transform.position);
        mouseScreenPos.x -= startingScreenPos.x;
        mouseScreenPos.y -= startingScreenPos.y;*/
        var angle = Mathf.Atan2(look.Value.y - transform.position.y, look.Value.x - transform.position.x) * Mathf.Rad2Deg + offset;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

    }

    internal void StartReloading()
    {
        if (reloading || ((Weapon)currentWeapon).RemainingAmmo == 0) return;
        reloading = true;
        SubmitReloadServerRpc();
    }
    internal void Reload()
    {
        ((Weapon)currentWeapon).Reload();
        //ik.enabled = true;
        reloading = false;
    }
    [ServerRpc]
    public void SubmitReloadServerRpc()
    {
        PlayReloadClientRpc();
    }

    [ClientRpc]
    void PlayReloadClientRpc()
    {
        StartCoroutine(WaitToReload());
        //Reload();
        //ik.enabled = false;
        //animatorController.Reload();
    }
    IEnumerator WaitToReload()
    {
        yield return new WaitForSeconds(.7f);

        Reload();
    }
    internal void Shoot(bool context)
    {
        if (!IsOwner || !context || currentWeapon == null || !alive) return;

        currentWeapon.Use(this);
    }

    internal void ShootBullet()
    {
        foreach (var prtcl in ((Weapon)currentWeapon).shootParticles)
        {
            prtcl.Play();
        }
        SubmitShootServerRpc(((Weapon)currentWeapon).canonEnd.position, ((Weapon)currentWeapon).canonEnd.rotation);
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
        foreach (var prtcl in ((Weapon)currentWeapon).shootParticles)
        {
            prtcl.transform.position = position;
            prtcl.transform.rotation = rotation;
            prtcl.Play();
        }
    }


    [ServerRpc]
    public void SummitGetHitServerRpc(ulong playerid, float damage, ulong originId)
    {
        Debug.Log("ShooterController, SummitGetHitServerRpc : touched player = #" + playerid);
        GetNetworkObject(playerid).GetComponentInChildren<Player>().GetHit(damage, originId);
    }



    internal void Look(Vector2 vector2)
    {
        look.Value = vector2;
    }
    //Called on client rpc
    public void Die()
    {
        if (!alive) return;
        
        alive = false;
        //animatorController.Die();
    }
    [ClientRpc]
    public void SetAliveClientRpc()
    {
        alive = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(look.Value, .2f);
    }
}
