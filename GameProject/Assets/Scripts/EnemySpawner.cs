using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] GameObject prefab;
    [SerializeField] List<GameObject> spawnedStuff;


    public IEnumerator SpawnStuff()
    {
        while (true)
        {
            if (spawnedStuff.Count < 5)
            {
                var rdnX = Random.Range(-6f, -4f);
                var rdnY = Random.Range(2.5f, 4f);
                GameObject go = Instantiate(prefab, new Vector3(-5f, 3f, 0f), Quaternion.identity);
                var enemy = go.GetComponent<BasicEnemy>();
                enemy.DieEvent.AddListener(delegate { spawnedStuff.Remove(go); });
                spawnedStuff.Add(go);
                go.GetComponent<NetworkObject>().Spawn();
            }
            yield return new WaitForSeconds(2f);
        }
    }

}
