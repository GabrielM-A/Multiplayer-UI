using UnityEngine;

namespace UI.SFX
{
    public class UISoundEffectsManager : MonoBehaviour
    {
        public static UISoundEffectsManager instance;
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip hoverSFX;
        [SerializeField] private AudioClip boomSFX;
        /** Only one instance allowed at one time - destroy otherwise and log it **/
        private void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else
            {
                Debug.Log("Duplicate instance of this class");
                Destroy(gameObject);
            }
        }
        
        public void PlayHoverSFX()
        {
            instance._audioSource.PlayOneShot(instance.hoverSFX);
        }
        
        public void PlayBoomSFX()
        {
            instance._audioSource.PlayOneShot(instance.boomSFX);
        }
    }
}