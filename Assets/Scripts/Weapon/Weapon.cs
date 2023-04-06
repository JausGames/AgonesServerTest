using System.Collections;
using System.Collections.Generic;
using UnityEngine;

abstract public class Weapon : Item
{
    [Header("Stats")]
    [SerializeField] protected WeaponStat stats;

    protected float nextShot;

    [Header("Components")]
    [SerializeField] public List<ParticleSystem> shootParticles;
    Hitable owner;
    public Hitable Owner { get => owner; set => owner = value; }
    private int ammo;

    public WeaponStat Stats { get => stats; set => stats = value; }

    public override bool Use(CombatController owner)
    {
        Debug.Log("Weapon, Use : Time = " + Time.time);
        Debug.Log("Weapon, Use : nextShot = " + nextShot);
        if (Time.time < nextShot || (ammo == 0)) return false; // check cooldown & ammunition
        nextShot = Time.time + Stats.cooldown;

        //owner.CameraFollow.RotationOffset = rndRecoil;
        //VFX
        owner.ShootBullet();
        return true;
    }
    private void Awake()
    {
        for(int i = 0; i < 9; i++)
        {
            Debug.Log("Awake, Weapon : layermask = " + LayerMask.LayerToName(i));
        }
    }

    protected RaycastHit[] ShootRaycast(Camera camera, LayerMask mask)
    {
        return Physics.RaycastAll(camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0), Camera.MonoOrStereoscopicEye.Mono), Mathf.Infinity, mask);
    }
    protected void FindRayVictims(CombatController owner, RaycastHit2D[] hits)
    {
        /*List<Player> players = new List<Player>();
        List<RaycastHit2D> rays = new List<RaycastHit2D>();
        List<int> layers = new List<int>();

        foreach (var hit in hits)
        {
            switch (hit.collider.gameObject.layer)
            {
                
                case 3: // Hit player body
                case 6: // Hit kart body
                case 8: // Hit horse body
                    var ennemy = hit.collider.GetComponent<Player>() ? hit.collider.GetComponent<Player>() : hit.collider.GetComponentInParent<Player>();
                    //if (players.Contains(ennemy)) break;
                    rays.Add(hit);
                    players.Add(ennemy);
                    layers.Add(hit.collider.gameObject.layer);
                    break;
                default:
                    rays.Add(hit);
                    players.Add(null);
                    layers.Add(hit.collider.gameObject.layer);
                    break;
            }
        }

        var dist = Mathf.Infinity;
        var closest = -1;
        for (var i = 0; i < rays.Count; i++)
        {
            var rayDist = ((Vector2)canonEnd.position - rays[i].point).sqrMagnitude;
            if (rayDist < dist)
            {
                closest = i;
                dist = rayDist;
            }
        }

        if (closest != -1)
        {
            if(players[closest])
            {
                Debug.Log("CombatController, Shoot : #" + owner.NetworkObjectId + " shot #" + players[closest].NetworkObjectId);
                //owner.GetHitCreditsEarnEvent.Invoke(Mathf.Min(players[closest].Health.Value, damage));
                owner.SummitGetHitServerRpc(players[closest].NetworkObjectId, stats.damage, stats.knockback, stats.knockTime, owner.NetworkObjectId);
                Debug.DrawLine(canonEnd.position, hits[closest].point, Color.red);
            }
            var layer = layers[closest];
            var origin = hits[closest].point;
            var direction = hits[closest].normal;
            owner.SubmitShotContactParticleServerRpc(layer, origin, direction);
            Debug.Log("Weapon, FindRayVictim : layer = " + layer);
        }*/

    }

    
    public void InstantiateContactParticles(int layer, Vector3 origin, Vector3 direction)
    {
        //var prtcl = layer == 6 ? woodParticles : layer == 0 ? sandParticles : bloodParticles;
        //var prtclInstance = Instantiate(prtcl, origin, Quaternion.identity);
        //prtclInstance.transform.LookAt(direction + prtclInstance.transform.position);
        //Destroy(prtclInstance.gameObject, 5f);
        //Destroy(prtclInstance.gameObject, 5f);
    }

}
[System.Serializable]
public class WeaponStat
{
    public float damage = 10f;
    public float range = 10f;
    public float cooldown = .7f;
    public float knockback = 1f;
    public float knockTime = .5f;

    public WeaponStat(WeaponStat stats)
    {
        this.damage = stats.damage;
        this.range = stats.range;
        this.cooldown = stats.cooldown;
        this.knockback = stats.knockback;
        this.knockTime = stats.knockTime;
    }
}
