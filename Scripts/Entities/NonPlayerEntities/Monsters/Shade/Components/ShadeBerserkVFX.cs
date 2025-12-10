using UnityEngine;

public class ShadeBerserkVFX : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem darkMist;
    [SerializeField] private ParticleSystem redMist;

    public void PlayDarkMist(float duration = -1)
    {
        if (darkMist != null)
        {
            // Make sure that the duration of the dark mist VFX matches the stun time before Shade officially becomes berserk
            if (duration > 0)
            {
                ParticleSystem.MainModule mainModule = darkMist.main;
                mainModule.duration = duration;
            }

            darkMist.Play();
        }
    }

    public void PlayRedMist()
    {
        if (redMist != null)
        {
            redMist.Play();
        }
    }
}
