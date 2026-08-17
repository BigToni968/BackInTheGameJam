using System.Collections.Generic;
using UnityEngine;

namespace Game.Other
{
    public interface IUpdate
    {
        public void OnUpdate();
    }
    
    public class Updater : MonoBehaviour
    {
        public static Updater Instance { get; private set; }

        private List<IUpdate> _updates = new(10000);

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            Instance = this;
        }

        private void OnDestroy()
        {
            _updates?.Clear();
        }

        public void Add(IUpdate update)
        {
            _updates.Add(update);
        }

        public void Remove(IUpdate update)
        {
            _updates.Remove(update);
        }

        private void Update()
        {
            for (var i = _updates.Count - 1; i >= 0; i--)
                _updates[i]?.OnUpdate();
        }
    }
}