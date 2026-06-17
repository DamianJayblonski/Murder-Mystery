using UnityEngine;
using TMPro;

public class CharacterConversationSwitcher : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private ChatUIController chatUIController;
    [SerializeField] private string[] characterNames;

    [Header("Characters")]
    [SerializeField] private string[] characterIds;
    [SerializeField] private GameObject[] characterModels;

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

        if (characterModels == null || characterModels.Length != characterIds.Length)
        {
            Debug.LogWarning("CharacterConversationSwitcher: Liczba modeli nie zgadza się z liczbą ID postaci.");
        }

        SwitchToCharacter(currentIndex);
    }

    public async void NextCharacter()
    {
        await conversationManager.SaveCurrentConversationSummaryIfNeeded();

        currentIndex++;

        if (currentIndex >= characterIds.Length)
            currentIndex = 0;

        SwitchToCharacter(currentIndex);
    }

    public async void PreviousCharacter()
    {
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

        UpdateCharacterModel(index);

        if (chatUIController != null)
        {
            chatUIController.ClearDialogueText();
        }

        if (characterNameText != null)
        {
            characterNameText.text = characterId;
        }
        
        if (chatUIController != null && characterNames != null && characterNames.Length > index)
        {
            chatUIController.SetCharacterName(characterNames[index]);
        }

        Debug.Log("Przełączono rozmowę na postać: " + characterId);
    }

    private void UpdateCharacterModel(int activeIndex)
    {
        if (characterModels == null || characterModels.Length == 0)
            return;

        for (int i = 0; i < characterModels.Length; i++)
        {
            if (characterModels[i] != null)
            {
                characterModels[i].SetActive(i == activeIndex);
            }
        }
    }
}