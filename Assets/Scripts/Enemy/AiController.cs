using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class AiController : MonoBehaviour
{
    UnityEvent destinationReachedOrUnreachable = new UnityEvent();
    [SerializeField] float currentTravelTime = 0f;
    [SerializeField] bool isActive = true;
    [SerializeField] NavMeshAgent agent;

    public Vector3 Destination
    {
        get => agent.destination;
        set
        {
            currentTravelTime = Time.time;
            agent.SetDestination(value);
        }
    }

    public bool IsActive
    {
        get => !agent.isStopped;
        set
        {
            agent.isStopped = !value;
        }
    }

    public UnityEvent DestinationReachedOrUnreachable { get => destinationReachedOrUnreachable; set => destinationReachedOrUnreachable = value; }
    public float Speed { get => agent.velocity.magnitude / agent.speed; }


    // Update is called once per frame
    void FixedUpdate()
    {
        // Check if we've reached the destination
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    DestinationReachedOrUnreachable.Invoke();
                }
            }
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(agent.destination, .25f);
    }

    internal void EnableAgent(bool v)
    {
        agent.isStopped = !v;
    }
}