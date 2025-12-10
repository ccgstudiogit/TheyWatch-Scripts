using System.Collections.Generic;
using UnityEngine;

public class PauseAudioSourceOnGamePause : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource[] audioSources;

    // Keeps track of what audio sources were playing when the game was paused. Only start playing the audio source when
    // game resumes if that audio source was previously playing
    private Dictionary<AudioSource, bool> previouslyPaused = new Dictionary<AudioSource, bool>();

    private void OnEnable()
    {
        PauseHandler.OnGamePause += Pause;
        PauseHandler.OnGameResume += Resume;
    }

    private void OnDisable()
    {
        PauseHandler.OnGamePause -= Pause;
        PauseHandler.OnGameResume -= Resume;
    }

    private void Pause()
    {
        previouslyPaused.Clear();

        for (int i = 0; i < audioSources.Length; i++)
        {
            if (audioSources[i] == null)
            {
                continue;
            }
            
            previouslyPaused.TryAdd(audioSources[i], audioSources[i].isPlaying);
            audioSources[i].Pause();
        }
    }

    private void Resume()
    {   
        // Makes sure to only play the audio source if it was playing when the game was paused
        foreach (KeyValuePair<AudioSource, bool> kvp in previouslyPaused)
        {
            // Only start playing the audio source again if it was previously playing when the game was paused
            if (kvp.Value)
            {
                kvp.Key.UnPause();
            }
        }
    }
}
