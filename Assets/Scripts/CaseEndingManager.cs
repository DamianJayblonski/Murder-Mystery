using System.Collections;
using UnityEngine;
using TMPro;

public class CaseEndingManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private CanvasGroup endingCanvasGroup;
    [SerializeField] private TMP_Text endingText;

    [Header("Case Settings")]
    [SerializeField] private string guiltyCharacterId = "suspect_02";

    [Header("Messages")]
    [TextArea(3, 10)]
    [SerializeField] private string correctAccusationText =
        "Miałeś rację.\n\nTo właśnie ta osoba była winna.";

    [TextArea(3, 10)]
    [SerializeField] private string wrongAccusationText =
        "To nie była właściwa osoba.\n\nPrawdziwy sprawca pozostał na wolności.";

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float waitBeforeFade = 3f;
    [SerializeField] private float fadeDuration = 2f;

    private bool endingStarted = false;

    public void AccuseCurrentCharacter()
    {
        if (endingStarted)
            return;

        if (conversationManager == null)
        {
            Debug.LogError("CaseEndingManager: Brak ConversationManager.");
            return;
        }

        string accusedId = conversationManager.ActiveCharacterId;

        if (string.IsNullOrWhiteSpace(accusedId))
        {
            Debug.LogWarning("CaseEndingManager: Brak aktywnej postaci do oskarżenia.");
            return;
        }

        bool isCorrect = accusedId == guiltyCharacterId;

        StartCoroutine(ShowEnding(isCorrect, accusedId));
    }

    private IEnumerator ShowEnding(bool isCorrect, string accusedId)
    {
        endingStarted = true;

        endingCanvasGroup.gameObject.SetActive(true);
        endingCanvasGroup.alpha = 1f;
        endingCanvasGroup.blocksRaycasts = true;
        endingCanvasGroup.interactable = true;

        endingText.text = "";

        string resultText = isCorrect ? correctAccusationText : wrongAccusationText;

        Debug.Log("[ENDING] Oskarżono: " + accusedId + " | Poprawnie: " + isCorrect);

        foreach (char letter in resultText)
        {
            endingText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(waitBeforeFade);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            endingCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        endingCanvasGroup.alpha = 0f;
        endingCanvasGroup.blocksRaycasts = false;
        endingCanvasGroup.interactable = false;
    }
}