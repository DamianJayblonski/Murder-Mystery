using UnityEngine;
using TMPro;

public class CharacterConversationSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private ChatUIController chatUIController;

    [Header("Characters")]
    [SerializeField] private string[] characterIds;

    [Header("Optional UI")]
    [SerializeField] private TMP_Text characterNameText;

    private int currentIndex = 0;

    private void Start()
    {
        if (characterIds == null || characterIds.Length == 0)
        {
            Debug.LogError("CharacterConversationSwitcher: Brak postaci na liście.");
            return;
        }

        SwitchToCharacter(currentIndex);
    }

    public async void NextCharacter()
    {
        if (characterIds.Length == 0) return;

        await conversationManager.SaveCurrentConversationSummaryIfNeeded();

        currentIndex++;

        if (currentIndex >= characterIds.Length)
            currentIndex = 0;

        SwitchToCharacter(currentIndex);
    }

    public async void PreviousCharacter()
    {
        if (characterIds.Length == 0) return;

        await conversationManager.SaveCurrentConversationSummaryIfNeeded();

        currentIndex--;

        if (currentIndex < 0)
            currentIndex = characterIds.Length - 1;

        SwitchToCharacter(currentIndex);
    }

    private void SwitchToCharacter(int index)
    {
        string characterId = characterIds[index];

        conversationManager.StartConversationWithoutSendingSummary(characterId);

        if (chatUIController != null)
        {
            chatUIController.ClearDialogueText();
        }

        if (characterNameText != null)
        {
            characterNameText.text = characterId;
        }

        Debug.Log("Przełączono rozmowę na postać: " + characterId);
    }
}