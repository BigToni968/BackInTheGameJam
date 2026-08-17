using UnityEngine;

namespace Akila.FPSFramework
{
    [AddComponentMenu("Akila/FPS Framework/Managers/Game Manager")]
    public class GameManager : MonoBehaviour
    {
        [SerializeField] UIManager HUD;
        [SerializeField] DeathCamera deathCamera;
        
        public UIManager UIManager { get; set; }
        public DeathCamera DeathCamera { get; set; }
        
        public static GameManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null)
                Destroy(Instance.gameObject);

            Instance = this;
            
            if(HUD) UIManager = Instantiate(HUD);

            if(deathCamera) DeathCamera = Instantiate(deathCamera);
        }
    }
}