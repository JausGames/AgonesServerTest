using System.Collections;
using System.Collections.Generic;
using UnityEngine;

    public class BasicEnemy : Hitable
    {
        [Header("Move settings")]
        [SerializeField] float checkRadius = 5f;
        [SerializeField] float checkForHitRadius = 1.5f;
        [SerializeField] float newDestinationRadius = .5f;
        [SerializeField] private float rotationSpeed = 150f;

        [Space]
        [Header("Hit")]
        [SerializeField] LayerMask enemyLayer;
        [SerializeField] private float knockoutTime = 0f;

        [Space]
        [Header("Componenent")]
        private EnemyAnimatorEvent animatorEvent;
        [SerializeField] private Weapon weapon;


        float deadTime = 0f;
        Hitable target;
        [SerializeField] FSM fsm = new FSM();
        Animator animator;
        AiController controller;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            controller = GetComponent<AiController>();
            animator = GetComponentInChildren<Animator>();
            //controller.DestinationReachedOrUnreachable.AddListener(delegate { Debug.Log("BasicEnemy, Event : " + gameObject.name + " reached destination"); });

            //animatorEvent = GetComponentInChildren<EnemyAnimatorEvent>();
            //animatorEvent.IsNotAttackingEvent.AddListener(delegate { if (fsm.currentState.type == StateType.Dead) return; fsm.currentState.type = StateType.CheckForTarget; });

            //animatorEvent.ActivateTriggerEvent.AddListener(delegate { ((MeleeWeapon)weapon).Trigger.IsActive = true; });
            //animatorEvent.DeactivateTriggerEvent.AddListener(delegate { ((MeleeWeapon)weapon).Trigger.IsActive = false; });
        }
        // Update is called once per frame
        void Update()
        {

            CheckNextState();

            animator.SetFloat("Speed", controller.Speed);

            switch (fsm.currentState.type)
            {
                case StateType.CheckForTarget:

                    break;

                case StateType.MoveToTarget:
                    if ((controller.Destination - target.transform.position).sqrMagnitude > newDestinationRadius)
                    {
                        controller.Destination = target.transform.position;
                    }
                    break;

                case StateType.HitTarget:

                    transform.rotation = GetNewRotation();
                    break;

                case StateType.Dead:

                    if (Time.time > deadTime + 4f && Time.time < deadTime + 1f)
                        transform.localScale -= Vector3.one * .2f * Time.deltaTime;
                    else if (Time.time > deadTime + 1f)
                        Destroy(gameObject);
                    break;

                case StateType.InAttack:

                    transform.rotation = GetNewRotation();
                    break;

                default:
                    break;
            }
        }

        private Quaternion GetNewRotation()
        {
            Vector3 aiToTarget = target.transform.position - transform.position;
            aiToTarget.y = 0f;
            Quaternion newRotatation = Quaternion.LookRotation(aiToTarget);
            var result = Quaternion.RotateTowards(transform.rotation, newRotatation, rotationSpeed * Time.deltaTime);
            return result;
        }

        private Hitable CheckEnemy()
        {
            var hits = Physics2D.OverlapCircleAll(transform.position, checkRadius, enemyLayer);

            foreach (var hit in hits)
            {
                var hitable = hit.GetComponent<Hitable>() ? hit.GetComponent<Hitable>() : hit.GetComponentInParent<Hitable>();
                if (hitable)
                    return hitable;
            }
            return null;
        }
        public override void TakeDamage(float damage, float knockback, float knocktime, Vector3 direction, ulong killerid)
        {
            base.TakeDamage(damage, knockback, knocktime, direction, killerid);

            if (fsm.currentState.type != StateType.Dead && knockback > 0f)
            {
                //body.velocity = Vector3.zero;
                knockoutTime = Time.time + knocktime;
                controller.IsActive = false;
                if(fsm.currentState.type != StateType.InAttack && fsm.currentState.type != StateType.KnockOut) animator.SetTrigger("GetHit");
                fsm.currentState.type = StateType.KnockOut;
            }
        }

        public override void Die()
        {
            base.Die();

            fsm.currentState.type = StateType.Dead;
            controller.Destination = transform.position;
            animator.SetTrigger("Die");
            deadTime = Time.time;
            controller.IsActive = false;
            GetComponent<Collider2D>().enabled = false;
            GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = false;
            controller.enabled = false;
        }

        public void CheckNextState()
        {
            switch (fsm.currentState.type)
            {
                case StateType.CheckForTarget:
                    target = CheckEnemy();
                    if (target != null)
                    {
                        if (!controller.IsActive) controller.IsActive = true;
                        fsm.currentState.type = StateType.MoveToTarget;
                        controller.Destination = target.transform.position;
                    }
                    break;
                case StateType.MoveToTarget:
                    if(!target) fsm.currentState.type = StateType.CheckForTarget;
                    // :(
                    if ((transform.position - target.transform.position).sqrMagnitude <= checkForHitRadius)
                    {
                        fsm.currentState.type = StateType.HitTarget;
                    }
                    break;
                case StateType.HitTarget:
                    if ((transform.position - target.transform.position).sqrMagnitude > checkForHitRadius)
                        fsm.currentState.type = StateType.MoveToTarget;
                   /* else
                    {
                        fsm.currentState.type = StateType.InAttack;
                        animator.SetTrigger("Attack");
                    }*/
                    break;
                case StateType.Dead:
                    break;
                case StateType.InAttack:
                    break;
                case StateType.KnockOut:
                    if(knockoutTime < Time.time)
                    {
                        fsm.currentState.type = StateType.CheckForTarget;
                        controller.IsActive = true;
                    }
                    break;
                default:
                    break;
            }
        }
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, checkForHitRadius);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, checkRadius);
        }

        [System.Serializable]
        public class FSM
        {

            public State currentState;

            public FSM()
            {
                currentState = new State(StateType.CheckForTarget);
            }


            [System.Serializable]
            public class State
            {
                public StateType type;

                public State(StateType type)
                {
                    this.type = type;
                }
            }
        }
        public enum StateType
        {
            CheckForTarget,
            MoveToTarget,
            HitTarget,
            InAttack,
            KnockOut,
            Dead
        }
    }


