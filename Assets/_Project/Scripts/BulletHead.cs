using Game.Other;
using UnityEngine;

namespace Game.Enemys
{
    [RequireComponent(typeof(Collider))]
    public class BulletHead : MonoBehaviour, IUpdate
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask playerLayer;

        private Vector3 targetPosition;
        private bool isInitialized;

        public void Init(Vector3 targetPos)
        {
            targetPosition = targetPos;
            isInitialized = true;
            Updater.Instance.Add(this);
            Destroy(gameObject, lifetime);
        }

        private void OnDestroy()
        {
            Updater.Instance.Remove(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) != 0)
            {
                Destroy(gameObject);
            }
        }

        public void OnUpdate()
        {
            if (!isInitialized) return;
            
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            
            if (Vector3.Distance(transform.position, targetPosition) < 0.05f)
            {
                // Замените на нужную логику, если требуется убрать сразу после достижения точки
            }
        }
    }
}