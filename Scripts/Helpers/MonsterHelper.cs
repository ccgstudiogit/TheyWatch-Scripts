using UnityEngine;

// This class contains useful methods for Monster scripts
public static class MonsterHelper
{
    /// <summary>
    ///     This method can be used to instantiate a prefab with an audio source and play a sound effect via that instantiated audio
    ///     source (useful for instances where a sound effect is needed after an audio source has left the area or been destroyed).
    /// </summary>
    /// <param name="audioSourcePrefab">The prefab with an audio source.</param>
    /// <param name="spawnPosition">Where the prefab should be spawned.</param>
    /// <param name="sfx">The sound effect that should be played.</param>
    public static void CreateAudioSourceAndPlaySFX(GameObject audioSourcePrefab, Vector3 spawnPosition, SoundEffectSO sfx)
    {
        if (audioSourcePrefab == null || sfx == null)
        {
            return;
        }

        GameObject obj = GameObject.Instantiate(audioSourcePrefab, spawnPosition, Quaternion.identity);
        AudioSource source = obj.GetComponent<AudioSource>();

        sfx.Play(source);

        // Destroy the instantiated object after the clip ends
        GameObject.Destroy(obj, source.clip.length / source.pitch);
    }   

    /// <summary>
    ///     Can be used to set an array of game objects to be active or inactive.
    /// </summary>
    /// <param name="gameObjects">The array of game objects.</param>
    /// <param name="setActive">Whether or not to set this array of game objects to be active.</param>
    public static void SetGameObjectsActive(GameObject[] gameObjects, bool setActive)
    {
        for (int i = 0; i < gameObjects.Length; i++)
        {
            if (gameObjects[i] != null)
            {   
                gameObjects[i].SetActive(setActive);
            }
        }
    }
}
