using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FPSTracker : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI vsyncText;

    private Dictionary<int, string> cachedNumberStrings = new Dictionary<int, string>();

    private int[] frameRateSamples;
    private int cachedNumberAmounts = 1500;
    private int averageFromAmount = 150;
    private int averageCounter = 0;
    private int currentAveraged;

    private void Awake()
    {
        // Cache strings and create array
        for (int i = 0; i < cachedNumberAmounts; i++)
        {
            cachedNumberStrings[i] = i.ToString();
        }

        frameRateSamples = new int[averageFromAmount];
    }

    private void Update()
    {
        // Sample
        var currentFrame = (int)Mathf.Round(1f / Time.unscaledDeltaTime);
        frameRateSamples[averageCounter] = currentFrame;

        // Average
        var average = 0f;

        foreach (var frameRate in frameRateSamples)
        {
            average += frameRate;
        }

        currentAveraged = (int)Mathf.Round(average / averageFromAmount);
        averageCounter = (averageCounter + 1) % averageFromAmount;

        // Assign to UI
        text.text = currentAveraged switch
        {
            var x when x >= 0 && x < cachedNumberAmounts => cachedNumberStrings[x],
            var x when x >= cachedNumberAmounts => $"> {cachedNumberAmounts}",
            var x when x < 0 => "< 0",
            _ => "?"
        };

        vsyncText.text = $"Vsync : {SettingsManager.instance.IsVSyncEnabled()}";
    }
}
