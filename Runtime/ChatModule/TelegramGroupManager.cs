/*
using System;
using UnityEngine;
using WTelegram;
using TL;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using LSCore;
using LSCore.Extensions.Unity;
using Sirenix.OdinInspector;

public class TelegramGroupManager : MonoBehaviour
{
    public long chatId = 2301292992;
    Client client;
    private Chat globalChat;
    [SerializeField] private RectTransform messagePrefab;
    [SerializeField] private RectTransform content;
    private string sessionFilePath;
    
    [Button]
    async void Begin()
    {
        client?.Dispose();
        sessionFilePath = StreamingAssetsUtils.GetOrCreateCopiedFile("telegram.session");
        client = new Client(Config);
        
        try
        {
            await client.LoginUserIfNeeded();
            client.OnUpdates += HandleUpdate;
            
            string[] friendUsernames = { "malvislight", "malvis_light" };
            globalChat = await GetExistingBasicChatById(chatId);
            globalChat ??= await CreateGroup($"{Application.productName}", friendUsernames);
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

    string Config(string what)
    {
        string verificationCode = "";
        string password = "";
        switch (what)
        {
            case "api_id": return "29124809";
            case "api_hash": return "33553afbccfe2882dcbcc9fe5450fdf2";
            case "phone_number": return "+19706041152";
            case "session_pathname": return sessionFilePath;
            case "verification_code": return verificationCode;
            case "password": return password;
            default: return null;
        }
    }
    
    async Task<Chat> CreateGroup(string groupTitle, string[] friendUsernames)
    {
        List<InputUser> inputUsers = new List<InputUser>();

        foreach (var username in friendUsernames)
        {
            var resolved = await client.Contacts_ResolveUsername(username);
            if (resolved != null && resolved.peer is PeerUser peerUser)
            {
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

        var updates = await client.Messages_CreateChat(inputUsers.ToArray(), groupTitle);
        if (updates.updates.Chats != null)
        {
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
            var fullChat = await client.Messages_GetFullChat(chatId);

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


    Task HandleUpdate(IObject update)
    {
        if (update is UpdateNewMessage newMsg)
        {
            if (newMsg.message.Peer is PeerChat || newMsg.message.Peer is PeerChannel)
            {
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
                        if (nm.message is Message message)
                        {
                        }
                    }
                }
            }
        }
        
        return Task.CompletedTask;
    }

    
    [Button]
    public async void GetMessages(int count)
    {
        try
        {
            var messages = await GetChatMessages(chatId, count);

            foreach (var m in messages)
            {
                if (m is Message mm)
                {
                    Debug.Log(mm.message);
                    var emojies = mm.entities?.OfType<MessageEntityCustomEmoji>().Select(x => x.document_id).ToArray();
                    var messageObj = Instantiate(messagePrefab, content);
                    
                    if (emojies?.Length > 0)
                    {
                        HandleCustomEmoji(client, emojies, messageObj);
                    }

                    if (mm.media is MessageMediaDocument mediaDocument)
                    {
                        SaveCustomEmoji(client, (Document)mediaDocument.document, messageObj);
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
        }
    }
    
    private async Task HandleCustomEmoji(WTelegram.Client client, long[] customEmojiIds, RectTransform messageObj)
    {
        var documents = await client.Messages_GetCustomEmojiDocuments(customEmojiIds);

        foreach (var documentBase in documents)
        {
            var doc = (Document)documentBase;
            Debug.Log($"Found custom emoji doc: ID={doc.ID}, mime={doc.mime_type}");

            await SaveCustomEmoji(client, doc, messageObj);
        }
    }

    public string decompressedAnimatedStickersPath = "AnimatedStickers";
    private async Task SaveCustomEmoji(WTelegram.Client client, Document doc, RectTransform messageObj)
    {
        string extension;
        switch (doc.mime_type)
        {
            case "application/x-tgsticker":
                extension = ".tgs";
                break;
            case "video/webm":
                extension = ".webm";
                break;
            case "video/mp4":
                extension = ".mp4";
                break;
            default:
                extension = ".bin";
                break;
        }

        var att = (DocumentAttributeSticker)doc.attributes.First(x => x is DocumentAttributeSticker);
        string basePath = $"{Application.persistentDataPath}/{doc.dc_id}_{doc.id}";
        
        string localPath = $"{basePath}{extension}";

        try
        {
            using (var fs = new FileStream(localPath, FileMode.Create, FileAccess.Write))
            {
                await client.DownloadFileAsync(doc, fs);
            }
        }
        catch (Exception)
        {
            return;
        }
        
        if (extension == ".tgs")
        {
            var dirPath = Path.Combine(Application.persistentDataPath, decompressedAnimatedStickersPath);
            Directory.CreateDirectory(dirPath);
            
            var outPath = Path.Combine(dirPath, $"{att.alt}.tgs");
            var fName = $"{att.alt}";
            int index = 0;
            while (File.Exists(outPath))
            {
                fName = $"{att.alt}_{++index}";
                outPath = Path.Combine(dirPath, $"{fName}.tgs");
            }
            File.Copy(localPath, outPath, true);
            
            if (doc.video_thumbs?.OfType<VideoSize>().Any(v => v.type == "f") == true)
            {
                var loc = new InputDocumentFileLocation
                {
                    id = doc.id,
                    access_hash = doc.access_hash,
                    file_reference = doc.file_reference,
                    thumb_size = "f" // <- ключ
                };

                var locPath = $"{basePath}_onclick{extension}";
                using (var fs = new FileStream(locPath, FileMode.Create, FileAccess.Write))
                {
                    await client.DownloadFileAsync(loc, fs);
                }
                var outPat = Path.Combine(dirPath, $"{fName}_onclick.tgs");
                File.Copy(locPath, outPat, true);
            }
            
            var asset = TelegramLottieAsset.Create(await File.ReadAllBytesAsync(localPath));
            LottieImage.Create(asset, messageObj);
        }
        else if (extension == ".mp4")
        {
            /*var videoPlayer = VideoPlayerCreator.Create(localPath, vp =>
            {
                var rawImage = vp.gameObject.AddComponent<LSRawImage>();
                rawImage.texture = vp.texture;
                rawImage.PreserveAspectRatio = true;
                rawImage.transform.SetParent(content);
            });
            
            videoPlayer.Play();#1#
        }
        
        Debug.Log($"Custom emoji saved to {localPath}");
    }

    public async Task<MessageBase> GetChatLastMessage(long chatId)
    {
        var messages = await GetChatMessages(chatId, 1);
        return messages.Length > 0 ? messages[0] : null;
    }
    
    
    public async Task<MessageBase[]> GetChatMessages(
        long chatId,
        int total = 100)
    {
        var peer = new InputPeerChat(chatId);
        var all = new List<MessageBase>(total);

        int offsetId = 0;
        while (all.Count < total)
        {
            int batch = Math.Min(100, total - all.Count);
            var history = await client.Messages_GetHistory(
                peer: peer,
                limit: batch,
                offset_id: offsetId);

            var msgs = history.Messages;
            if (msgs == null || msgs.Length == 0)
                break;

            all.AddRange(msgs);

            int oldestId = msgs.Min(m => m.ID);
            offsetId = oldestId;

            if (msgs.Length < batch)
                break;
        }

        return all.Take(total).ToArray();
    }


    void OnDestroy()
    {
        client?.Dispose();
    }
}
*/
