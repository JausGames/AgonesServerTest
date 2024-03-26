using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Inputs")]
    [SerializeField] NetworkVariable<Vector2> move = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private float moveSpeed = 5f;
    private bool alive = true;

    public Vector2 Move { get => move.Value; set => move.Value = value; }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner || !alive) return;

        transform.position += (Vector3)move.Value * moveSpeed * Time.deltaTime;
    }

    internal void Die()
    {
        alive = false;
    }
    internal void SetAlive()
    {
        alive = true;
    }
}
