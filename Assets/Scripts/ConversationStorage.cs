using System.Collections.Generic;
using UnityEngine;

public class ConversationStorage : MonoBehaviour
{
    [SerializeField] private List<CharacterConversationData> characters = new List<CharacterConversationData>();

    private Dictionary<string, CharacterConversationData> characterDictionary = new Dictionary<string, CharacterConversationData>();

    private void Awake()
    {
        characterDictionary.Clear();

        foreach (var character in characters)
        {
            if (character != null && !string.IsNullOrEmpty(character.characterId))
            {
                if (!characterDictionary.ContainsKey(character.characterId))
                {
                    if (character.currentConversationMessages == null)
                        character.currentConversationMessages = new List<OpenAI.ChatMessage>();

                    characterDictionary.Add(character.characterId, character);
                }
                else
                {
                    Debug.LogWarning($"ConversationStorage: Duplikat characterId: {character.characterId}");
                }
            }
        }
    }

    public CharacterConversationData GetCharacterData(string characterId)
    {
        if (characterDictionary.TryGetValue(characterId, out var data))
        {
            return data;
        }

        Debug.LogError($"ConversationStorage: Nie znaleziono postaci o ID: {characterId}");
        return null;
    }

    public void UpdateSummary(string characterId, string summary)
    {
        CharacterConversationData data = GetCharacterData(characterId);
        if (data != null)
        {
            data.lastConversationSummary = summary;
        }
    }

    public void ClearCurrentConversation(string characterId)
    {
        CharacterConversationData data = GetCharacterData(characterId);
        if (data != null)
        {
            data.currentConversationMessages.Clear();
        }
    }
}