using UnityEngine;
using TMPro;

public class ChatUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private TMP_InputField playerInputField;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Test Character")]
    [SerializeField] private string characterId = "innkeeper_01";

    private string lastPlayerMessage = "";
    private string lastNpcMessage = "";

    private string currentCharacterName = "NPC";

    private void Start()
    {
        if (conversationManager != null)
        {
            conversationManager.OnNpcResponse.AddListener(ShowNpcResponse);
        }

        StartNpcConversation();

        if (playerInputField != null)
        {
            playerInputField.Select();
            playerInputField.ActivateInputField();
        }
    }

    public void StartNpcConversation()
    {
        if (conversationManager == null) return;

        dialogueText.text = "";
        conversationManager.StartConversation(characterId);
    }

    public void SendPlayerMessage()
    {
        if (conversationManager == null || playerInputField == null) return;

        string playerMessage = playerInputField.text;

        if (string.IsNullOrWhiteSpace(playerMessage))
            return;

        lastPlayerMessage = playerMessage;
        lastNpcMessage = "";
        UpdateDialogueText();

        conversationManager.SendPlayerMessage(playerMessage);

        playerInputField.text = "";
        playerInputField.Select();
        playerInputField.ActivateInputField();
    }

    public void EndConversation()
    {
        if (conversationManager == null) return;

        conversationManager.EndConversationAndSaveSummary();
        dialogueText.text = "";
    }

    private void ShowNpcResponse(string response)
    {
        if (dialogueText == null) return;

        lastNpcMessage = response;
        UpdateDialogueText();
    }

    private void UpdateDialogueText()
    {
        dialogueText.text = "";

        if (!string.IsNullOrWhiteSpace(lastPlayerMessage))
        {
            dialogueText.text += "Ty: " + lastPlayerMessage + "\n\n";
        }

        if (!string.IsNullOrWhiteSpace(lastNpcMessage))
        {
            dialogueText.text += currentCharacterName + ": " + lastNpcMessage;
        }
    }

    public void ClearDialogueText()
    {
        lastPlayerMessage = "";
        lastNpcMessage = "";

        if (dialogueText != null)
            dialogueText.text = "";
    }

    public void SetCharacterName(string characterName)
    {
        currentCharacterName = characterName;
    }
}