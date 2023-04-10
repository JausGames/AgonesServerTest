using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawner : NetworkBehaviour
{
    [SerializeField] GameObject prefab;

    private void OnServerInitialized()
    {
        Debug.Log("OnServerInitialized");
        /*GameObject go = Instantiate(prefab, new Vector3(-5f, 3f, 0f), Quaternion.identity);
        go.GetComponent<NetworkObject>().Spawn();*/
    }
   IEnumerator SpawnStuff()
    {
        while (true)
        {
            var rdnX = Random.Range(-6f, -4f);
            var rdnY = Random.Range(2.5f, 4f);
            GameObject go = Instantiate(prefab, new Vector3(-5f, 3f, 0f), Quaternion.identity);
            go.GetComponent<NetworkObject>().Spawn();
            yield return new WaitForSeconds(2f);
        }
    }

    private void Start()
    {
        StartCoroutine(SpawnStuff());
    }
}
