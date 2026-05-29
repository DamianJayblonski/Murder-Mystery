using System.Collections.Generic;
using UnityEngine;
using OpenAI;

[System.Serializable]
public class CharacterConversationData
{
    public string characterId;

    [TextArea(5, 20)]
    public string characterDescription;

    [TextArea(5, 20)]
    public string lastConversationSummary;

    [System.NonSerialized]
    public List<ChatMessage> currentConversationMessages = new List<ChatMessage>();

    public CharacterConversationData(string id, string description)
    {
        characterId = id;
        characterDescription = description;
        lastConversationSummary = string.Empty;
        currentConversationMessages = new List<ChatMessage>();
    }
}