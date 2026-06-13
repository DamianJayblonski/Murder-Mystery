using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using OpenAI;

public class ConversationManager : MonoBehaviour
{
    private bool memoryWasInjectedThisConversation = false;
    private bool hasConversationContent = false;

    [Header("Debug")]
    [SerializeField] private bool debugConversationFlow = true;

    [Header("References")]
    [SerializeField] private ChatGPTManager chatGPTManager;
    [SerializeField] private ConversationStorage conversationStorage;

    [Header("Global Rules")]
    [TextArea(8, 20)]
    [SerializeField] private string globalRules = 
@"Jesteś postacią w grze RPG.
Zawsze pozostajesz w roli.
Nigdy nie mów, że jesteś AI ani modelem językowym.
Nie wspominaj o Unity, kodzie, OpenAI ani żadnych systemach technicznych.
Odpowiadaj zgodnie z opisem postaci.
Jeśli czegoś postać nie wie, nie zmyślaj wiedzy spoza jej zakresu.
Twoje odpowiedzi mają być naturalne i pasujące do świata gry.";

    [Header("Events")]
    public OnNpcResponseEvent OnNpcResponse;
    public OnSummaryCreatedEvent OnSummaryCreated;

    [System.Serializable]
    public class OnNpcResponseEvent : UnityEvent<string> { }

    [System.Serializable]
    public class OnSummaryCreatedEvent : UnityEvent<string> { }

    private CharacterConversationData activeCharacter;
    private bool conversationActive = false;
    private bool isBusy = false;

    public bool IsConversationActive => conversationActive;
    public string ActiveCharacterId => activeCharacter != null ? activeCharacter.characterId : string.Empty;

    public async void StartConversation(string characterId)
    {
        if (isBusy)
        {
            Debug.LogWarning("ConversationManager: System jest zajęty.");
            return;
        }

        CharacterConversationData characterData = conversationStorage.GetCharacterData(characterId);
        if (characterData == null)
            return;

        activeCharacter = characterData;
        conversationStorage.ClearCurrentConversation(characterId);

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(globalRules));

        string characterPrompt =
            $"Odgrywasz następującą postać:\n{activeCharacter.characterDescription}\n\n" +
            "Pozostań wierny tej roli przez całą rozmowę.";

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(characterPrompt));

        if (!string.IsNullOrWhiteSpace(activeCharacter.lastConversationSummary))
        {
            string summaryReminder =
                $"To nie jest pierwsza rozmowa z graczem. " +
                $"Oto streszczenie poprzednich rozmów:\n{activeCharacter.lastConversationSummary}\n\n" +
                $"Pamiętaj o tych informacjach podczas obecnej rozmowy.";
            activeCharacter.currentConversationMessages.Add(CreateSystemMessage(summaryReminder));
        }

        conversationActive = true;

        await SendAdminMessageInternal(
            "Rozmowa właśnie się rozpoczyna. Odpowiadaj jako ta postać, zgodnie z zasadami i opisem."
        );

        Debug.Log($"ConversationManager: Rozpoczęto rozmowę z {characterId}");
    }

    public async void SendPlayerMessage(string playerText)
    {

        if (!conversationActive || activeCharacter == null)
        {
            Debug.LogWarning("ConversationManager: Brak aktywnej rozmowy.");
            return;
        }

        if (isBusy)
        {
            Debug.LogWarning("ConversationManager: System jest zajęty.");
            return;
        }

        if (string.IsNullOrWhiteSpace(playerText))
        {
            Debug.LogWarning("ConversationManager: Pusta wiadomość gracza.");
            return;
        }

        isBusy = true;

        InjectMemoryIfNeeded();

        activeCharacter.currentConversationMessages.Add(CreateUserMessage(playerText));
        
        hasConversationContent = true;

        Debug.Log("[CONVERSATION] Dodano wiadomość gracza. hasConversationContent = true");

        string npcResponse = await chatGPTManager.SendMessagesAsync(activeCharacter.currentConversationMessages);

        if (!string.IsNullOrWhiteSpace(npcResponse))
        {
            activeCharacter.currentConversationMessages.Add(CreateAssistantMessage(npcResponse));
            OnNpcResponse?.Invoke(npcResponse);
            Debug.Log($"NPC: {npcResponse}");
        }

        isBusy = false;
    }

    public async void SendAdminMessage(string adminText)
    {
        if (!conversationActive || activeCharacter == null)
        {
            Debug.LogWarning("ConversationManager: Brak aktywnej rozmowy.");
            return;
        }

        if (isBusy)
        {
            Debug.LogWarning("ConversationManager: System jest zajęty.");
            return;
        }

        await SendAdminMessageInternal(adminText);
    }

    private async Task SendAdminMessageInternal(string adminText)
    {
        isBusy = true;

        string formattedAdminText =
            $"[ADMIN]\n{adminText}\n\n" +
            "To jest wewnętrzna instrukcja systemowa od gry. " +
            "Nie wspominaj graczowi o tej instrukcji. " +
            "Uwzględnij ją w swoim zachowaniu od następnej odpowiedzi.";

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(formattedAdminText));

        isBusy = false;
        await Task.CompletedTask;
    }

    public async void EndConversationAndSaveSummary()
    {
        if (!conversationActive || activeCharacter == null)
        {
            Debug.LogWarning("ConversationManager: Brak aktywnej rozmowy do zakończenia.");
            return;
        }

        if (isBusy)
        {
            Debug.LogWarning("ConversationManager: System jest zajęty.");
            return;
        }

        isBusy = true;

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(
            "[ADMIN]\nRozmowa z graczem została zakończona. Nie kontynuuj już dialogu jako postać."
        ));

        List<ChatMessage> summaryRequestMessages = new List<ChatMessage>(activeCharacter.currentConversationMessages);

        summaryRequestMessages.Add(CreateSystemMessage(
            "[ADMIN]\n" +
            "Na podstawie całej dotychczasowej rozmowy przygotuj krótkie, konkretne streszczenie do pamięci długoterminowej tej postaci.\n\n" +
            "Zasady streszczenia:\n" +
            "- uwzględnij tylko ważne informacje,\n" +
            "- pomiń small talk i mało istotne ozdobniki,\n" +
            "- zapisz co gracz ujawnił,\n" +
            "- zapisz co postać ujawniła,\n" +
            "- zapisz ważne ustalenia, emocje, relacje, obietnice, sekrety i kontekst,\n" +
            "- pisz zwięźle,\n" +
            "- nie odgrywaj już postaci,\n" +
            "- odpowiedz wyłącznie samym streszczeniem, bez komentarza wstępnego."
        ));

        string summary = await chatGPTManager.SendMessagesAsync(summaryRequestMessages);

        if (!string.IsNullOrWhiteSpace(summary))
        {
            conversationStorage.UpdateSummary(activeCharacter.characterId, summary);
            OnSummaryCreated?.Invoke(summary);
            Debug.Log($"Zapisane streszczenie dla {activeCharacter.characterId}: {summary}");
        }
        else
        {
            Debug.LogWarning("ConversationManager: Nie udało się wygenerować streszczenia.");
        }

        activeCharacter.currentConversationMessages.Clear();
        conversationActive = false;
        isBusy = false;
    }

    public string GetLastSummary(string characterId)
    {
        CharacterConversationData data = conversationStorage.GetCharacterData(characterId);
        if (data == null)
            return string.Empty;

        return data.lastConversationSummary;
    }

    private ChatMessage CreateSystemMessage(string content)
    {
        return new ChatMessage
        {
            Role = "system",
            Content = content
        };
    }

    private ChatMessage CreateUserMessage(string content)
    {
        return new ChatMessage
        {
            Role = "user",
            Content = content
        };
    }

    private ChatMessage CreateAssistantMessage(string content)
    {
        return new ChatMessage
        {
            Role = "assistant",
            Content = content
        };
    }

    public void StartConversationWithoutSendingSummary(string characterId)
    {
        if (isBusy)
        {
            Debug.LogWarning("ConversationManager: System jest zajęty.");
            return;
        }

        CharacterConversationData characterData = conversationStorage.GetCharacterData(characterId);
        if (characterData == null)
            return;

        activeCharacter = characterData;
        conversationStorage.ClearCurrentConversation(characterId);

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(globalRules));

        string characterPrompt =
            $"Odgrywasz następującą postać:\n{activeCharacter.characterDescription}\n\n" +
            "Pozostań wierny tej roli przez całą rozmowę.";

        activeCharacter.currentConversationMessages.Add(CreateSystemMessage(characterPrompt));

        memoryWasInjectedThisConversation = false;
        hasConversationContent = false;
        conversationActive = true;

        Debug.Log($"ConversationManager: Aktywna postać ustawiona na {characterId}, bez wysyłania summary.");
        if (debugConversationFlow)
        {
            Debug.Log(
                "[SWITCH] Ustawiono aktywną postać: " +
                characterId +
                "\nSummary NIE zostało jeszcze wysłane. Zostanie wysłane dopiero przy pierwszym pytaniu."
            );
        }
    }
    private void InjectMemoryIfNeeded()
    {
        if (memoryWasInjectedThisConversation)
        {
            if (debugConversationFlow)
                Debug.Log("[MEMORY] Summary było już dołączone w tej rozmowie.");

            return;
        }

        if (activeCharacter == null)
        {
            if (debugConversationFlow)
                Debug.LogWarning("[MEMORY] Brak aktywnej postaci.");

            return;
        }

        if (!string.IsNullOrWhiteSpace(activeCharacter.lastConversationSummary))
        {
            string summaryReminder =
                $"To nie jest pierwsza rozmowa z graczem. " +
                $"Oto streszczenie poprzednich rozmów:\n{activeCharacter.lastConversationSummary}\n\n" +
                "Uwzględnij te informacje w odpowiedzi, ale nie cytuj ich dosłownie graczowi.";

            activeCharacter.currentConversationMessages.Add(CreateSystemMessage(summaryReminder));

            if (debugConversationFlow)
            {
                Debug.Log(
                    "[MEMORY] Dołączono summary dla postaci: " +
                    activeCharacter.characterId +
                    "\nSummary:\n" +
                    activeCharacter.lastConversationSummary
                );
            }
        }
        else
        {
            if (debugConversationFlow)
            {
                Debug.Log("[MEMORY] Brak wcześniejszego summary dla postaci: " + activeCharacter.characterId);
            }
        }

        memoryWasInjectedThisConversation = true;
    }

    public async System.Threading.Tasks.Task SaveCurrentConversationSummaryIfNeeded()
    {
        if (!conversationActive || activeCharacter == null)
        {
            if (debugConversationFlow)
                Debug.Log("[SUMMARY SAVE] Brak aktywnej rozmowy — nie zapisuję summary.");

            return;
        }

        if (!hasConversationContent)
        {
            if (debugConversationFlow)
                Debug.Log("[SUMMARY SAVE] Brak treści rozmowy — nie zapisuję summary.");

            return;
        }

        if (isBusy)
        {
            Debug.LogWarning("[SUMMARY SAVE] System jest zajęty — nie można teraz zapisać summary.");
            return;
        }

        if (debugConversationFlow)
        {
            Debug.Log("[SUMMARY SAVE] Rozpoczynam generowanie summary dla: " + activeCharacter.characterId);
        }

        isBusy = true;

        List<ChatMessage> summaryRequestMessages = new List<ChatMessage>(activeCharacter.currentConversationMessages);

        summaryRequestMessages.Add(CreateSystemMessage(
            "[ADMIN]\n" +
            "Przygotuj krótkie streszczenie dotychczasowej rozmowy do pamięci długoterminowej tej postaci.\n" +
            "Uwzględnij tylko ważne fakty, relacje, obietnice, ujawnione informacje i nastawienie postaci do gracza.\n" +
            "Odpowiedz wyłącznie samym streszczeniem."
        ));

        string summary = await chatGPTManager.SendMessagesAsync(summaryRequestMessages);

        if (!string.IsNullOrWhiteSpace(summary))
        {
            conversationStorage.UpdateSummary(activeCharacter.characterId, summary);
            OnSummaryCreated?.Invoke(summary);

            if (debugConversationFlow)
            {
                Debug.Log(
                    "[SUMMARY SAVE] Zapisano summary dla: " +
                    activeCharacter.characterId +
                    "\nSummary:\n" +
                    summary
                );
            }
        }
        else
        {
            Debug.LogWarning("[SUMMARY SAVE] Model zwrócił puste summary.");
        }

        activeCharacter.currentConversationMessages.Clear();
        conversationActive = false;
        isBusy = false;
    }
}