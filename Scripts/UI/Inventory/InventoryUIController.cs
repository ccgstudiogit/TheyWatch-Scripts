using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(CanvasGroup))]
public class InventoryUIController : MonoBehaviour
{
    // Lets HedgeMazeController know when the player's inventory is full for the first time
    public static event Action OnFirstTimeInventoryFull;

    private bool firstTimeFull;

    [Header("- References -")]
    [Header("Text")]
    [Tooltip("This reference is used to turn off the number texts when full (and display \"full\" text instead)")]
    [SerializeField] private GameObject count;
    [SerializeField] private TextMeshProUGUI currentCountText;
    [SerializeField] private TextMeshProUGUI maxSizeText;
    [SerializeField] private GameObject fullText;

    [Header("Fills")]
    [Tooltip("The order of the fills matters: when a runestone is collected, the fills will fill up in the order of this array. " +
        "When runestones are deposited into the portal, the fills will start at the last element and go to the 0th element")]
    [SerializeField] private Image[] images;
    [Tooltip("How long the fill animation should be when a runestone is collected")]
    [SerializeField] private float collectFillAnimationLength = 0.3f;
    [Tooltip("How long the fill animation should be when runestones are deposited into the portal (the animations do not happen) " +
        "concurrently, but rather one at a time starting from the fills[fills.Length - 1] to fills[0]")]
    [SerializeField] private float depositFillAnimationLength = 0.125f;
    [Tooltip("The fills image fill amount will be set to this once a runestone is collected")]
    [SerializeField] private float targetFill = 0.24f;

    private CanvasGroup canvasGroup;
    private PlayerInventory playerInventory;

    private int currentCount;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (currentCountText == null)
        {
#if UNITY_EDITOR
            Debug.LogWarning($"{name}'s currentCountText null. Disabling InventoryUIController.cs and hiding inventory");
#endif
            HideInventory();
            enabled = false;
            return;
        }

#if UNITY_EDITOR
        if (maxSizeText == null)
        {
            Debug.LogWarning($"{name}'s maxSizeText null.");
        }
#endif
    }

    private void Start()
    {
        StartCoroutine(GetPlayerInventoryRef());

        firstTimeFull = false;
        fullText.gameObject.SetActive(false);

        // Make sure the fill game objects' fill amount is 0 on start
        for (int i = 0; i < images.Length; i++)
        {
            images[i].fillAmount = 0;
        }
    }

    private void Update()
    {
        MonitorInventorySize();
    }

    private void MonitorInventorySize()
    {
        if (playerInventory == null)
        {
            return;
        }

        if (currentCount != playerInventory.collectablesInInventoryCount)
        {
            // Used for the fill game objects
            int difference = playerInventory.collectablesInInventoryCount - currentCount;

            // If the difference is greater than 0, fill the next fill image. Otherwise, reduce the fill to 0
            if (difference > 0)
            {
                // Loop through fills array and find the first one that has a close-to-zero fill amount
                for (int i = 0; i < images.Length; i++)
                {
                    if (images[i].fillAmount < 0.05f)
                    {
                        StartCoroutine(FillAmountLerp(images[i], targetFill, collectFillAnimationLength));
                        break;
                    }
                }
            }
            else
            {
                // If the difference is less than 0, that means the player deposited the runestones in the portal. Loop through the
                // fills array and set the fillAmounts to be 0 one after the other
                StartCoroutine(ResetFillAmounts(images));
            }

            currentCount = playerInventory.collectablesInInventoryCount;

            // Update text: if the inventory is full, activate "Full" text. Otherwise, update the numbers. Also invoke OnFirstTimeInventoryFull
            // to let HedgeMazeController send a generic device tip letting the player know to head back to the portal
            if (currentCount >= playerInventory.GetInventorySize())
            {
                if (!fullText.activeSelf)
                {
                    fullText.SetActive(true);
                    count.SetActive(false);
                }

                if (!firstTimeFull)
                {
                    firstTimeFull = true;
                    OnFirstTimeInventoryFull?.Invoke();
                }
            }
            else
            {
                if (!count.activeSelf)
                {
                    fullText.SetActive(false);
                    count.SetActive(true);
                }

                currentCountText.text = currentCount.ToString();
            }
        }
    }

    /// <summary>
    ///     Starts a fill animation for an image's fillAmount.
    /// </summary>
    /// <param name="image">Image reference.</param>
    /// <param name="targetFill">This is what the image's fillAmount should be set to.</param>
    /// <param name="duration">How long the animation will be.</param>
    private IEnumerator FillAmountLerp(Image image, float targetFill, float duration)
    {
        float lerp = 0f;
        float startFill = image.fillAmount;

        if (duration <= 0)
        {
            image.fillAmount = targetFill;
            yield break;
        }

        while (lerp < 1)
        {
            lerp = Mathf.MoveTowards(lerp, 1, Time.deltaTime / duration);
            image.fillAmount = Mathf.Lerp(startFill, targetFill, lerp);

            yield return null;
        }

        image.fillAmount = targetFill;
    }

    /// <summary>
    ///     Used to reduce the images' fillAmounts to 0 one by one.
    /// </summary>
    private IEnumerator ResetFillAmounts(Image[] images)
    {
        for (int i = images.Length - 1; i >= 0; i--)
        {
            if (images[i].fillAmount > 0.05f)
            {
                // Make sure to wait for this image's fillAmount to become 0 before starting the next one
                yield return StartCoroutine(FillAmountLerp(images[i], 0, depositFillAnimationLength));
            }
        }
    }

    private IEnumerator GetPlayerInventoryRef()
    {
        // The max time this coroutine will run before giving up on finding the player (if this number is reached,
        // then a serious issue occured as the player hasn't loaded in for this set amount of time OR due to testing)
        float maxSearchTime = 15f;
        float elapsedTime = 0f;

        while (playerInventory == null)
        {
            elapsedTime += Time.deltaTime;

            GameObject player = GameObject.FindWithTag("Player");

            if (player != null)
            {
                if (player.TryGetComponent(out PlayerInventory playerInventory))
                {
                    this.playerInventory = playerInventory;
                    currentCount = playerInventory.collectablesInInventoryCount;

                    if (maxSizeText != null)
                    {
                        maxSizeText.text = playerInventory.GetInventorySize().ToString();
                    }
                }
                else
                {
#if UNITY_EDITOR
                    Debug.LogWarning($"{gameObject.name} found Player, but Player does not have PlayerInventory.");
#endif
                    HideInventory();
                    enabled = false;
                    yield break;
                }
            }
            else if (elapsedTime > maxSearchTime)
            {
#if UNITY_EDITOR
                Debug.LogWarning($"{gameObject.name} could not find player after {maxSearchTime} seconds. Disabling InventoryUIController.cs");
#endif
                HideInventory();
                enabled = false;
                yield break;
            }

            yield return null;
        }
    }

    private void HideInventory()
    {
        canvasGroup.alpha = 0;
    }
}
