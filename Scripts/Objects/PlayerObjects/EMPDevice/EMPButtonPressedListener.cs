using System;
using UnityEngine;

/// <summary>
///     This script should be attached to the player arms (same game object as the animator) and listens for the animator's
///     animation event OnButtonPressed, then fires off the OnEMPButtonPressed event to let EMPDevice.cs know when the actual
///     button was pressed in the animation.
/// </summary>
public class EMPButtonPressedListener : MonoBehaviour
{
    public static event Action OnEMPButtonPressed;

    /// <summary>
    ///     Listens for the OnButtonPressed event in the use EMP animation clip.
    /// </summary>
    public void OnButtonPressed()
    {
        OnEMPButtonPressed?.Invoke();
    }
}
