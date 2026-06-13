using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;

public class ChatGPTManager : MonoBehaviour
{
    [Header("Model Settings")]
    [SerializeField] private string modelName = "gpt-5-nano";

    [Header("Debug")]
    [SerializeField] private bool debugRequests = true;
    [SerializeField] private bool debugResponses = true;

    private OpenAIApi openAI = new OpenAIApi();

    public async Task<string> SendMessagesAsync(List<ChatMessage> messages)
    {
        if (debugRequests)
        {
            Debug.Log(BuildDebugRequestText(messages));
        }

        CreateChatCompletionRequest request = new CreateChatCompletionRequest
        {
            Model = modelName,
            Messages = messages
        };

        var response = await openAI.CreateChatCompletion(request);

        if (response.Choices != null && response.Choices.Count > 0)
        {
            string answer = response.Choices[0].Message.Content;

            if (debugResponses)
            {
                Debug.Log("[CHATGPT RESPONSE]\n" + answer);
            }

            return answer;
        }

        Debug.LogError("ChatGPTManager: Brak odpowiedzi z API.");
        return string.Empty;
    }

    private string BuildDebugRequestText(List<ChatMessage> messages)
    {
        StringBuilder sb = new StringBuilder();

        sb.AppendLine("========== CHATGPT REQUEST ==========");
        sb.AppendLine("Model: " + modelName);
        sb.AppendLine("Messages count: " + messages.Count);

        for (int i = 0; i < messages.Count; i++)
        {
            sb.AppendLine("-------------------------------------");
            sb.AppendLine("Message " + i);
            sb.AppendLine("Role: " + messages[i].Role);
            sb.AppendLine("Content:");
            sb.AppendLine(messages[i].Content);
        }

        sb.AppendLine("=====================================");

        return sb.ToString();
    }
}