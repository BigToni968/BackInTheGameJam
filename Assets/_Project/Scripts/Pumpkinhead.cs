using System.Collections;
using Akila.FPSFramework;
using Pathfinding;
using UnityEngine;
using Game.Other;
using UnityHFSM;

namespace Game.Enemys
{
    public class Pumpkinhead : MonoBehaviour, IDamageable, IUpdate
    {
        [SerializeField] private Actor actor;
        [Header("Health Settings")]
        [SerializeField] private float health = 100f;

        [Header("Detection & Movement")]
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float detectionRadius = 10f;
        [SerializeField] private float offsetZ = 2.5f;
        [SerializeField] private float circleSpeed = 2f;

        [Header("Attack Settings")]
        [SerializeField] private float attackCooldown = 2f;
        [SerializeField] private BulletHead bulletHeadPrefab;
        [SerializeField] private Transform spawnPoint;

        [SerializeField] private AIPath ai;
        [SerializeField] private AIDestinationSetter destinationSetter;
        [SerializeField] private Rigidbody rb;
        
        private StateMachine fsm;
        private Transform playerTransform;
        private Vector3 lastKnownPosition;
        private bool isAttacking;
        private float currentCircleAngle;

        private void Start()
        {
            MaxHealth = health;
            Updater.Instance.Add(this);
            InitFSM();
        }

        private void InitFSM()
        {
            fsm = new StateMachine();
            
            fsm.AddState("Idle", onLogic: state => 
            {
                ai.isStopped = true;
                destinationSetter.target = null;
            });

            fsm.AddState("Chase", onLogic: state => 
            {
                if (playerTransform == null) return;
                
                ai.isStopped = false;
                destinationSetter.target = playerTransform;
                lastKnownPosition = playerTransform.position;
            });

            fsm.AddState("Investigate", onLogic: state =>
            {
                ai.isStopped = false;
                destinationSetter.target = null;
                ai.destination = lastKnownPosition;
            });

            fsm.AddTransition("Idle", "Chase", t => playerTransform != null);
            fsm.AddTransition("Investigate", "Chase", t => playerTransform != null);

            fsm.AddTransition("Chase", "Investigate", t => playerTransform == null);

            fsm.AddTransition("Investigate", "Idle", t => 
                playerTransform == null);

            fsm.Init();
        }

        private void DetectPlayer()
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);
            
            Transform previousTarget = playerTransform;
            Transform foundPlayer = null;

            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].transform != transform && !hits[i].transform.IsChildOf(transform))
                {
                    foundPlayer = hits[i].transform;
                    break;
                }
            }

            playerTransform = foundPlayer;

            if (previousTarget == null && playerTransform != null)
            {
                destinationSetter.target = playerTransform;
                lastKnownPosition = playerTransform.position;
                ai.SearchPath();
            }
        }

        private void LookAtPlayer()
        {
            if (playerTransform == null) return;

            Vector3 direction = playerTransform.position - transform.position;
            direction.y = 0;
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
            }
        }

        private IEnumerator AttackCoroutine()
        {
            isAttacking = true;

            if (playerTransform != null && bulletHeadPrefab != null)
            {
                LookAtPlayer();

                Vector3 targetPos = playerTransform.position;
                Vector3 spawnPos = spawnPoint != null ? spawnPoint.position : transform.position;
                Vector3 dir = (targetPos - spawnPos).normalized;

                Quaternion bulletRotation = dir != Vector3.zero ? Quaternion.LookRotation(dir) : Quaternion.identity;
                var bulletObj = Instantiate(bulletHeadPrefab, spawnPos, bulletRotation);
                bulletObj.Init(targetPos);
            }

            yield return new WaitForSeconds(attackCooldown);
            isAttacking = false;
        }

        private void Die()
        {
            StopAllCoroutines();
            Updater.Instance.Remove(this);

            destinationSetter.target = null;
            ai.isStopped = true;
            rb.isKinematic = false;
            rb.constraints = RigidbodyConstraints.None;

            enabled = false;
        }

        public void OnUpdate()
        {
            if (IsDead()) return;

            DetectPlayer();
            Debug.Log(fsm.ActiveStateName);
            fsm.OnLogic();
            
            if (playerTransform != null && !isAttacking)
            {
                float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= detectionRadius)
                {
                    StartCoroutine(AttackCoroutine());
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(transform.position, offsetZ);
        }

        public Actor GetActor() => actor;
        public float GetHealth() => health;

        public void Damage(float amount, Actor damageSource)
        {
            if (IsDead()) return;

            health -= amount;
            if (health <= 0)
                Die();
        }

        public bool IsDead() => health <= 0f;

        public bool deadConfirmed { get; set; } = false;
        public Vector3 deathForce { get; set; }
        public float MaxHealth { get; set; }
        public int GetGroupsCount() => 0;
        public Ragdoll GetRagdoll() => null;
    }
}