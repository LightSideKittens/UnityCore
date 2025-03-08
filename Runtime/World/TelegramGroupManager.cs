using System.Collections;
using UnityEngine;
using WTelegram;
using TL;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using LSCore;
using Sirenix.OdinInspector;

public class TelegramGroupManager : MonoBehaviour
{
    public long chatId = 2301292992;
    Client client;
    private ChatMessagesManager chatManager;
    private Chat globalChat;
    
    [Button]
    async void Begin()
    {
        client?.Dispose();
        client = new Client(Config);
        
        try
        {
            await client.LoginUserIfNeeded();
            client.OnUpdates += HandleUpdate;
            
            string[] friendUsernames = { "malvislight", "malvis_light" };
            globalChat = await GetExistingBasicChatById(chatId);
            globalChat ??= await CreateGroup($"{Application.productName}", friendUsernames);
            chatManager = new ChatMessagesManager(client, chatId);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("Telegram Error: " + ex.Message);
        }
    }

    [Button]
    public async void SendMessage(string message)
    {
        if (globalChat != null)
        {
            await SendMessageToGroup(globalChat, message);
        }
    }
    
    // Функция-конфигурация для WTelegramClient
    string Config(string what)
    {
        switch (what)
        {
            case "api_id": return "29124809"; // Ваш API_ID
            case "api_hash": return "33553afbccfe2882dcbcc9fe5450fdf2";
            case "phone_number": return "+19706041152";
            case "session_pathname": return "C:/C-UnityProjects/LightSide/StarSavers/AdditionalResources/telegram.session";
            default: return null;
        }
    }

    // Метод для создания группы (базовой группы, не супер-группы)
    async Task<Chat> CreateGroup(string groupTitle, string[] friendUsernames)
    {
        List<InputUser> inputUsers = new List<InputUser>();

        foreach (var username in friendUsernames)
        {
            var resolved = await client.Contacts_ResolveUsername(username);
            if (resolved != null && resolved.peer is PeerUser peerUser)
            {
                // Ищем объект пользователя в списке resolved.users по идентификатору
                var user = resolved.users.Values.FirstOrDefault(u => u.id == peerUser.user_id);
                if (user != null)
                {
                    inputUsers.Add(new InputUser(user.ID, user.access_hash));
                }
                else
                {
                    Debug.LogWarning($"Пользователь {username} не найден или отсутствует access_hash.");
                }
            }
            else
            {
                Debug.LogWarning($"Не удалось получить данные для пользователя {username}.");
            }
        }

        if (inputUsers.Count == 0)
        {
            Debug.LogError("Не найдено ни одного валидного пользователя для создания группы.");
            return null;
        }

        // Создаем группу. Метод возвращает объект Updates, содержащий информацию о созданном чате.
        var updates = await client.Messages_CreateChat(inputUsers.ToArray(), groupTitle);
        if (updates.updates.Chats != null)
        {
            // Ищем среди созданных чатов объект типа Chat (базовая группа)
            foreach (var chatObj in updates.updates.Chats.Values)
            {
                if (chatObj is Chat basicChat)
                {
                    Debug.Log($"Группа \"{basicChat.title}\" успешно создана.");
                    return basicChat;
                }
            }
        }
        Debug.LogError("Не удалось создать группу.");
        return null;
    }
    
    /// <summary>
    /// Ищет существующую БАЗОВУЮ группу (Chat) по её числовому идентификатору.
    /// </summary>
    /// <param name="chatId">Идентификатор обычной (базовой) группы.</param>
    /// <returns>Объект Chat, если группа найдена; иначе null.</returns>
    private async Task<Chat> GetExistingBasicChatById(long chatId)
    {
        try
        {
            // Запрашиваем подробную информацию о чате
            var fullChat = await client.Messages_GetFullChat(chatId);

            // В fullChat.chats хранится список всех чатов, упомянутых в ответе
            if (fullChat.chats.TryGetValue(chatId, out ChatBase chatBase))
            {
                if (chatBase is Chat basicChat)
                {
                    Debug.Log($"Найдена базовая группа: {basicChat.title} (ID = {basicChat.id})");
                    return basicChat;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Ошибка при получении базовой группы c ID={chatId}: {ex.Message}");
        }

        return null;
    }
    
    async Task SendMessageToGroup(Chat chat, string message)
    {
        InputPeer inputPeer = null;
        
        if (chat is { } basicChat)
        {
            inputPeer = new InputPeerChat(basicChat.id);
        }

        await client.SendMessageAsync(inputPeer, message);
        Debug.Log("Сообщение отправлено в группу.");
    }

    // Обработчик входящих обновлений (для получения сообщений)
    Task HandleUpdate(IObject update)
    {
        // Проверяем, является ли обновление объектом UpdateNewMessage
        if (update is UpdateNewMessage newMsg)
        {
            // Определяем, что сообщение пришло из группы (базовая группа или канал)
            if (newMsg.message.Peer is PeerChat || newMsg.message.Peer is PeerChannel)
            {
                Debug.Log("Получено новое сообщение из группы: " + newMsg.message.ID);
            }
        }
        else if (update is UpdatesBase updatesBase && updatesBase.UpdateList != null)
        {
            foreach (var upd in updatesBase.UpdateList)
            {
                if (upd is UpdateNewMessage nm)
                {
                    if (nm.message.Peer is PeerChat || nm.message.Peer is PeerChannel)
                    {
                        Debug.Log("Получено новое сообщение из группы: " + nm.message.ID);

                        if (nm.message is Message message)
                        {
                            Debug.Log("Получено новое сообщение из группы: " + message.message);
                        }
                    }
                }
            }
        }
        
        return Task.CompletedTask;
    }

    [Button]
    public async void GetMessages()
    {
        var message = await GetChatLastMessage(chatId);
        var messages = await GetChatMessages(chatId, message?.ID ?? 0);

        foreach (var m in messages)
        {
            if (m is Message mm)
            {
                Debug.Log(mm.message);
            }
        }
    }

    public async Task<MessageBase> GetChatLastMessage(long chatId)
    {
        var messages = await GetChatMessages(chatId, 0, 1);
        return messages.Length > 0 ? messages[0] : null;
    }
    
    
    public async Task<MessageBase[]> GetChatMessages(
        long chatId,
        int offsetId,
        int limit = 100,
        bool needNewest = false)
    {
        var inputPeer = new InputPeerChat(chatId);
        
        Messages_MessagesBase history;

        if (needNewest)
        {
            history = await client.Messages_GetHistory(
                peer: inputPeer,
                limit: limit,
                min_id: offsetId + 1);
        }
        else
        {
            history = await client.Messages_GetHistory(
                peer: inputPeer,
                offset_id: offsetId,
                limit: limit);
        }

        return history.Messages;
    }

    void OnDestroy()
    {
        client?.Dispose();
    }
}
