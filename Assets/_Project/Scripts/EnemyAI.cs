using System.Collections;
using UnityEngine;
using Pathfinding; // Требуется A* Pathfinding Project

namespace Game
{
    [RequireComponent(typeof(AIPath))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Target Settings")] [SerializeField]
        private Transform playerTransform;

        [SerializeField] private float stoppingDistance = 5f;
        [SerializeField] private float updatePathInterval = 0.2f;

        [Header("Dodge Settings")] [Range(0f, 1f)] [SerializeField]
        private float dodgeChance = 0.5f; // 50% шанс уклонения

        [SerializeField] private float dodgeDistance = 4f;
        [SerializeField] private float dodgeDuration = 0.8f;
        [SerializeField] private float dodgeCooldown = 1.5f;

        private IAstarAI ai;
        private bool isDodging = false;
        private bool canDodge = true;
        private float lastPathUpdate;

        private void Awake()
        {
            ai = GetComponent<IAstarAI>();
        }

        private void Start()
        {
            // Если игрок не назначен в инспекторе, ищем по тегу
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) playerTransform = player.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null || isDodging) return;

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            // Обновляем путь с небольшим интервалом для оптимизации
            if (Time.time > lastPathUpdate + updatePathInterval)
            {
                lastPathUpdate = Time.time;

                if (distanceToPlayer > stoppingDistance)
                {
                    ai.isStopped = false;
                    ai.destination = playerTransform.position;
                }
                else
                {
                    // Достигли нужной дистанции — останавливаем движение, но разворачиваемся к игроку
                    ai.isStopped = true;
                    RotateTowardsPlayer();
                }
            }
        }

        private void RotateTowardsPlayer()
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            direction.y = 0; // Исключаем наклоны по оси Y
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 8f);
            }
        }

        /// <summary>
        /// Вызывайте этот метод, когда игрок стреляет в сторону бота/в бота.
        /// </summary>
        public void OnPlayerShot()
        {
            if (!canDodge || isDodging) return;

            // Проверяем шанс уклонения
            if (Random.value <= dodgeChance)
            {
                StartCoroutine(DodgeRoutine());
            }
        }

        private IEnumerator DodgeRoutine()
        {
            isDodging = true;
            canDodge = false;

            // Выбираем случайное направление уклонения: влево (-1) или вправо (1)
            float sideDir = Random.value > 0.5f ? 1f : -1f;

            // Перпендикулярный вектор относительно взгляда бота
            Vector3 dodgeDirection = transform.right * sideDir;
            Vector3 targetDodgePosition = transform.position + dodgeDirection * dodgeDistance;

            // Задаем AI точку для стрейфа
            ai.isStopped = false;
            ai.destination = targetDodgePosition;

            // Ждем завершения уклонения
            yield return new WaitForSeconds(dodgeDuration);

            isDodging = false;

            // Кулдаун перед следующим возможным уклонением
            yield return new WaitForSeconds(dodgeCooldown);
            canDodge = true;
        }
    }
}