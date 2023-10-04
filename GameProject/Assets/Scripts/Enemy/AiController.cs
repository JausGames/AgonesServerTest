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
    private bool tryToActivate;
    private bool activationValue;

    public Vector3 Destination
    {
        get => agent.destination;
        set
        {
            currentTravelTime = Time.time;
            if (agent.isOnNavMesh)
                agent.SetDestination(value);
        }
    }

    public bool IsActive
    {
        get
        {
            if (agent.isOnNavMesh && agent.isActiveAndEnabled)
                return !agent.isStopped;
            else
                return false;
        }
        set
        {
            EnableAgent(value);
        }
    }

    public UnityEvent DestinationReachedOrUnreachable { get => destinationReachedOrUnreachable; set => destinationReachedOrUnreachable = value; }
    public float Speed { get => agent.velocity.magnitude / agent.speed; }

    public void StopAgent()
    {
        agent.velocity = Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (tryToActivate)
            EnableAgent(activationValue);

        // Check if we've reached the destination
        if (!agent.pathPending && agent.isActiveAndEnabled && agent.isOnNavMesh)
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

        if (!agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            tryToActivate = true;
            activationValue = v;
        }
        else 
            agent.isStopped = !v;


    }

}