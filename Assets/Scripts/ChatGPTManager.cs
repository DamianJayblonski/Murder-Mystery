using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;

public class ChatGPTManager : MonoBehaviour
{
    [Header("Model Settings")]
    [SerializeField] private string modelName = "gpt-5-nano";

    private OpenAIApi openAI = new OpenAIApi();

    public async Task<string> SendMessagesAsync(List<ChatMessage> messages)
    {
        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Model = modelName,
            Messages = messages
        };

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            return response.Choices[0].Message.Content;
        }

        Debug.LogError("ChatGPTManager: Brak odpowiedzi z API.");
        return string.Empty;
    }
}