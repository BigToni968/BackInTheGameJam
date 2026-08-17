using Akila.FPSFramework;
using UnityEngine;
using Game.Other;

namespace Game.Enemys
{
    [RequireComponent(typeof(Collider))]
    public class BulletHead : MonoBehaviour, IUpdate
    {
        [SerializeField] private float speed = 15f;
        [SerializeField] private float lifetime = 5f;
        [SerializeField] private LayerMask playerLayer;
        [SerializeField] private float damage = 1f;

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
                other.TryGetComponent(out HealthSystem healthSystem);
                healthSystem?.Damage(damage, null);
                Destroy(gameObject);
            }
        }

        public void OnUpdate()
        {
            if (!isInitialized) return;
            
            transform.position += transform.forward * speed * Time.deltaTime;
        }
    }
}