using Akila.FPSFramework;
using UnityEngine;

namespace Game.OtherObject
{
    public class AllusionObject : HealthSystem
    {
        
        private void Awake()
        {
            OnDeath.AddListener(Effect);
        }

        private void OnDestroy()
        {
            OnDeath.RemoveListener(Effect);
        }

        private void Effect()
        {
            
        }
    }
}