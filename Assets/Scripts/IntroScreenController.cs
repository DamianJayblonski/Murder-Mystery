using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IntroScreenController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private CanvasGroup introCanvasGroup;
    [SerializeField] private TMP_Text introText;

    [Header("Intro Texts")]
    [TextArea(5, 15)]
    [SerializeField] private string firstText =
        "Doszło do morderstwa.\n\nKażdy z obecnych coś ukrywa.";

    [TextArea(5, 15)]
    [SerializeField] private string secondText =
        "Informacje o sprawie:\n\n" +
        "Ofiara została znaleziona w warsztacie.\n" +
        "Na miejscu znajdowało się kilku podejrzanych.\n" +
        "Twoim zadaniem jest przesłuchać świadków i wskazać sprawcę.";

    [Header("Timing")]
    [SerializeField] private float typeSpeed = 0.04f;
    [SerializeField] private float waitAfterFirstText = 2f;
    [SerializeField] private float waitAfterSecondText = 3f;
    [SerializeField] private float fadeBetweenTextsDuration = 1f;
    [SerializeField] private float finalFadeDuration = 2f;

    private void Start()
    {
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        introCanvasGroup.gameObject.SetActive(true);
        introCanvasGroup.alpha = 1f;
        introCanvasGroup.blocksRaycasts = true;
        introCanvasGroup.interactable = true;

        yield return TypeText(firstText);

        yield return new WaitForSeconds(waitAfterFirstText);

        yield return FadeTextOut();

        introText.text = "";

        yield return FadeTextIn();

        yield return TypeText(secondText);

        yield return new WaitForSeconds(waitAfterSecondText);

        yield return FadePanelOut();

        introCanvasGroup.blocksRaycasts = false;
        introCanvasGroup.interactable = false;
        introCanvasGroup.gameObject.SetActive(false);
    }

    private IEnumerator TypeText(string text)
    {
        introText.text = "";

        foreach (char letter in text)
        {
            introText.text += letter;
            yield return new WaitForSeconds(typeSpeed);
        }
    }

    private IEnumerator FadeTextOut()
    {
        Color color = introText.color;
        float timer = 0f;

        while (timer < fadeBetweenTextsDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(1f, 0f, timer / fadeBetweenTextsDuration);
            introText.color = color;
            yield return null;
        }

        color.a = 0f;
        introText.color = color;
    }

    private IEnumerator FadeTextIn()
    {
        Color color = introText.color;
        float timer = 0f;

        while (timer < fadeBetweenTextsDuration)
        {
            timer += Time.deltaTime;
            color.a = Mathf.Lerp(0f, 1f, timer / fadeBetweenTextsDuration);
            introText.color = color;
            yield return null;
        }

        color.a = 1f;
        introText.color = color;
    }

    private IEnumerator FadePanelOut()
    {
        float timer = 0f;

        while (timer < finalFadeDuration)
        {
            timer += Time.deltaTime;
            introCanvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / finalFadeDuration);
            yield return null;
        }

        introCanvasGroup.alpha = 0f;
    }
}