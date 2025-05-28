using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

class SpawnManager : NetworkBehaviour
{
        [SerializeField] private EnemySpawner spawner;

    internal void Init()
    {
        StartCoroutine(spawner.SpawnStuff());
    }
}
