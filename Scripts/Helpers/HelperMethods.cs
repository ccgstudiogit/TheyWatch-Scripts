using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HelperMethods
{
    /// <summary>
    ///     Checks if the target game object is within the camera's viewport frustrum.
    /// </summary>
    public static bool IsVisible(Camera camera, GameObject target)
    {
        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(camera);
        return planes.All(plane => plane.GetDistanceToPoint(target.transform.position) >= 0);
    }

    /// <summary>
    ///     Remaps a value from one range to another.
    /// </summary>
    /// <param name="value">The value to be remapped</param>
    /// <param name="fromMin">The old range's minimum value</param>
    /// <param name="fromMax">The old range's maximum value</param>
    /// <param name="toMin">the new range's minimum value</param>
    /// <param name="toMax">the new range's maximum value</param>
    public static float Remap(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        // Prevents division by 0
        if (Mathf.Approximately(fromMax, fromMin))
        {
            return toMin;
        }

        float fromAbs = value - fromMin;
        float fromMaxAbs = fromMax - fromMin;

        float normal = fromAbs / fromMaxAbs;

        float toMaxAbs = toMax - toMin;
        float toAbs = toMaxAbs * normal;

        float to = toAbs + toMin;

        return to;
    }

    /// <summary>
    ///     Recursively sets the layer of the given transfrom and all of its child transform to the 
    ///     specified layer.
    /// </summary>
    /// <param name="root">The root transform whose layer (and children's layers) will be changed.</param>
    /// <param name="layer">The target layer to apply recursively to all transforms.</param>
    public static void SetLayerRecursive(Transform root, int layer)
    {
        if (root == null)
        {
            return;
        }

        root.gameObject.layer = layer;

        foreach (Transform child in root)
        {
            SetLayerRecursive(child, layer);
        }
    }

    /// <summary>
    ///     Shuffles the elements of a list in-place using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="listToBeShuffled">The list to shuffle.</param>
    public static void ShuffleList<T>(List<T> listToBeShuffled)
    {
        for (int i = listToBeShuffled.Count - 1; i > 0; i--)
        {
            int randomIndex = UnityEngine.Random.Range(0, i + 1);
            (listToBeShuffled[i], listToBeShuffled[randomIndex]) = (listToBeShuffled[randomIndex], listToBeShuffled[i]);
        }
    }

    /// <summary>
    ///     Shuffles the elements of an array using System.Random.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array.</typeparam>
    /// <param name="rng">System.Random</param>
    /// <param name="arrayToBeShuffled">The array to shuffle.</param>
    public static void ShuffleArray<T>(this System.Random rng, T[] arrayToBeShuffled)
    {
        int n = arrayToBeShuffled.Length;

        while (n > 1)
        {
            int k = rng.Next(n--);
            T temp = arrayToBeShuffled[n];
            arrayToBeShuffled[n] = arrayToBeShuffled[k];
            arrayToBeShuffled[k] = temp;
        }
    }

    /// <summary>
    ///     Adds a layer to the specified layer mask.
    /// </summary>
    /// <param name="layerMask">The layer mask to add to.</param>
    /// <param name="name">The layer's name that should be added to the layer mask.</param>
    public static void AddLayerToLayerMask(ref LayerMask layerMask, string name)
    {
        int layerNumber = LayerMask.NameToLayer(name);

        if (layerNumber != -1)
        {
            layerMask |= 1 << layerNumber;
        }
    }

    /// <summary>
    ///     Removes a layer from the specified layer mask.
    /// </summary>
    /// <param name="layerMask">The layer mask to remove from.</param>
    /// <param name="name">The layer's name that should be removed from the layer mask.</param>
    public static void RemoveLayerFromLayerMask(ref LayerMask layerMask, string name)
    {
        int layerNumber = LayerMask.NameToLayer(name);

        if (layerNumber != -1)
        {
            layerMask &= ~(1 << layerNumber);
        }
    }

    /// <summary>
    ///     Checks if the MonoBehaviour is not null, enabled, and if the game object is active.
    /// </summary>
    /// <returns>True if not null, is enabled, and the game object is active. Returns false if any of these conditions 
    ///     are not true.</returns>
    public static bool FullyActive<T>(T monoBehaviour) where T : MonoBehaviour
    {
        return monoBehaviour != null && monoBehaviour.enabled && monoBehaviour.gameObject.activeSelf;
    }

    /// <summary>
    ///     Checks if the MonoBehaviour is not null and enabled.
    /// </summary>
    /// <returns>True if not null and is enabled, false if otherwise.</returns>
    public static bool NotNullAndEnabled<T>(T monoBehaviour) where T : MonoBehaviour
    {
        return monoBehaviour != null && monoBehaviour.enabled;
    }

    /// <summary>
    ///     Evaluates and gets a random value based on the specified animation curve. A random x value is selected along the
    ///     curve's range, and the float that gets returned is the corresponding y value.
    /// </summary>
    /// <returns>A random float based on the animation curve. Returns 0 if the curve is null or the length is 0.</returns>
    public static float GetRandomValueFromAnimationCurve(AnimationCurve animationCurve)
    {
        if (animationCurve == null || animationCurve.length == 0)
        {
#if UNITY_EDITOR
            Debug.LogWarning("animationCurve || animationCurve.Length == 0 from HelperMethods.GetRandomValueFromAnimationCurve().");
#endif
            return 0f;
        }

        // Get the animation curve's start and end points
        float curveStart = animationCurve.keys[0].time;
        float curveEnd = animationCurve.keys[animationCurve.length - 1].time;

        // Return a random float that is somewhere in between the start and end values
        return animationCurve.Evaluate(Mathf.Lerp(curveStart, curveEnd, UnityEngine.Random.value));
    }

    /// <summary>
    ///     t = 0 is at the start, 1 = 1 is at the end (similar to lerp). Provides fast movement at first, and then smoothly
    ///     slows down as the target is approached.
    /// </summary>
    /// <param name="t">Normalized time.</param>
    /// <returns>Returns a float between 0 and 1 based on the normalized time input.</returns>
    public static float EaseOutCubic(float t)
    {
        t = Mathf.Clamp01(t);
        return 1f - Mathf.Pow(1f - t, 3);
    }

    /// <summary>
    ///     Checks if an integer is even.
    /// </summary>
    /// <returns>True if the integer is even, false if the integer is odd.</returns>
    public static bool IsEven(int num)
    {
        return num % 2 == 0;
    }

    /// <summary>
    ///     Invoke a method or lambda expression after a delay.
    /// </summary>
    /// <param name="function">The function to call after the delay.</param>
    /// <param name="delay">The delay in seconds.</param>
    public static void Invoke(this MonoBehaviour mb, Action function, float delay)
    {
        mb.StartCoroutine(InvokeRoutine(function, delay));
    }

    /// <summary>
    ///     A coroutine used for Invoke() to execute a method after a delay in seconds.
    /// </summary>
    private static IEnumerator InvokeRoutine(Action f, float d)
    {
        yield return new WaitForSeconds(d);
        f?.Invoke();
    }

    /// <summary>
    ///     Incoke a method or lambda expression after a certain amount of frames (Note: the minimum amount of frames
    ///     is 1, so if frames entered is <= 0, there will still be one frame delay).
    /// </summary>
    /// <param name="function">The function to call after the frame delay.</param>
    /// <param name="frames">The number of frames to wait before executing the function.</param>
    public static void InvokeAfterFrames(this MonoBehaviour mb, Action function, int frames)
    {
        if (frames < 1)
        {
            frames = 1;
        }

        mb.StartCoroutine(InvokeAfterXFramesRoutine(function, frames));
    }

    /// <summary>
    ///     A coroutine used for InvokeAfterFrames() to execute a method after X amount of frames.
    /// </summary>
    private static IEnumerator InvokeAfterXFramesRoutine(Action f, int frames)
    {
        for (int i = 0; i < frames; i++)
        {
            yield return null;
        }

        f?.Invoke();
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
            if (gameObjects[i] != null && gameObjects[i].activeSelf != setActive)
            {   
                gameObjects[i].SetActive(setActive);
            }
        }
    }
}
