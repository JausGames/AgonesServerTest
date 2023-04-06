using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemyAnimatorEvent : MonoBehaviour
{
    UnityEvent isNotAttackingEvent = new UnityEvent();
    UnityEvent activateTriggerEvent = new UnityEvent();
    UnityEvent deactivateTriggerEvent = new UnityEvent();

    public UnityEvent IsNotAttackingEvent { get => isNotAttackingEvent; set => isNotAttackingEvent = value; }
    public UnityEvent ActivateTriggerEvent { get => activateTriggerEvent; set => activateTriggerEvent = value; }
    public UnityEvent DeactivateTriggerEvent { get => deactivateTriggerEvent; set => deactivateTriggerEvent = value; }
    public void OnIsNotAttacking() => isNotAttackingEvent.Invoke();
    public void OnActivateTrigger() => activateTriggerEvent.Invoke();
    public void OnDeactivateTrigger() => deactivateTriggerEvent.Invoke();
}
