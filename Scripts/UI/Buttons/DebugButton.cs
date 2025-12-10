#if UNITY_EDITOR
using UnityEngine;

public class DebugButton : MonoBehaviour
{
    public void Click()
    {
        Debug.Log("Click!");
    }

    public void Hover()
    {
        Debug.Log("Enter Hover");
    }

    public void Leave()
    {
        Debug.Log("Left");
    }

    public void DoubleClick()
    {
        Debug.Log("Double Click!");
    }
}
#endif
