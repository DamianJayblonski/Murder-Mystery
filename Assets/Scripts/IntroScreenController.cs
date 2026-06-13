using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TMP_Text introText;

    [Header("Intro Text")]
    [TextArea(5, 15)]
    [SerializeField] private string fullText =
        "Doszło do morderstwa.\n\n" +
        "Każdy z obecnych coś ukrywa.\n" +
        "Twoim zadaniem jest odkryć prawdę.";

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float waitAfterText = 2f;
    [SerializeField] private float fadeDuration = 2f;

    private void Start()
    {
        StartCoroutine(PlayIntro());
    }

    private IEnumerator PlayIntro()
    {
        introCanvasGroup.alpha = 1f;
        introCanvasGroup.blocksRaycasts = true;
        introCanvasGroup.interactable = true;

        introText.text = "";

        foreach (char letter in fullText)
        {
            introText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }

        yield return new WaitForSeconds(waitAfterText);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }

        introCanvasGroup.alpha = 0f;
        introCanvasGroup.blocksRaycasts = false;
        introCanvasGroup.interactable = false;

        gameObject.SetActive(false);
    }
}