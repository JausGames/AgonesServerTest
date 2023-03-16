using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Header("Inputs")]
    [SerializeField] NetworkVariable<Vector2> move = new NetworkVariable<Vector2>(Vector2.zero, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    [SerializeField] private float moveSpeed = 5f;

    public Vector2 Move { get => move.Value; set => move.Value = value; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;
        //float horizontalInput = Input.GetAxis("Horizontal");
        //Get the value of the Horizontal input axis.

        //float verticalInput = Input.GetAxis("Vertical");
        //Get the value of the Vertical input axis.

        //move.Value = new Vector3(horizontalInput, verticalInput, 0);

        //transform.Translate(move.Value * moveSpeed * Time.deltaTime);
        transform.position += (Vector3)move.Value * moveSpeed * Time.deltaTime;
    }
}
