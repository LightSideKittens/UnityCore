using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Sendbird.Chat;

public class SendbirdChatManager : MonoBehaviour
{
    public static SendbirdChatManager Instance { get; private set; }

    [Header("Sendbird")]
    [SerializeField] string applicationId;
    [SerializeField] SbLogLevel logLevel = SbLogLevel.Warning;
    [SerializeField] bool autoInitialize = true;

    bool isInitialized;
    string connectedUserId;
    string connectedAuthToken;

    readonly Queue<Action> mainThreadActions = new Queue<Action>();

    const string ConnectionHandlerId = "SendbirdChatManager_Connection";
    const string UserHandlerId = "SendbirdChatManager_User";
    const string OpenChannelHandlerId = "SendbirdChatManager_OpenChannel";
    const string GroupChannelHandlerId = "SendbirdChatManager_GroupChannel";

    SbConnectionHandler connectionHandler;
    SbUserEventHandler userEventHandler;
    SbOpenChannelHandler openChannelHandler;
    SbGroupChannelHandler groupChannelHandler;

    public bool IsInitialized => isInitialized;
    public bool IsConnected => SendbirdChat.GetConnectionState() == SbConnectionState.Open;
    public string CurrentUserId => connectedUserId;
    public SbUser CurrentUser => SendbirdChat.CurrentUser;

    public SbGroupChannelModule GroupChannelModule => SendbirdChat.GroupChannel;
    public SbOpenChannelModule OpenChannelModule => SendbirdChat.OpenChannel;
    public SbMessageModule MessageModule => SendbirdChat.Message;

    public event Action<SbConnectionState> ConnectionStateChanged;
    public event Action<string> Connected;
    public event Action<string> Disconnected;
    public event Action<SbUnreadMessageCount> TotalUnreadMessageCountChanged;

    public event Action<SbBaseChannel, SbBaseMessage> MessageReceived;
    public event Action<SbBaseChannel, SbBaseMessage> MessageUpdated;
    public event Action<SbBaseChannel, long> MessageDeleted;
    public event Action<SbBaseChannel, SbReactionEvent> ReactionUpdated;
    public event Action<SbBaseChannel> ChannelChanged;
    public event Action<string, SbChannelType> ChannelDeleted;

    public event Action<SbOpenChannel, SbUser> OpenChannelUserEntered;
    public event Action<SbOpenChannel, SbUser> OpenChannelUserExited;
    public event Action<IReadOnlyList<SbOpenChannel>> OpenChannelParticipantCountChanged;

    public event Action<SbGroupChannel, SbUser> GroupChannelUserJoined;
    public event Action<SbGroupChannel, SbUser> GroupChannelUserLeft;
    public event Action<SbGroupChannel> GroupChannelTypingStatusUpdated;
    public event Action<SbGroupChannel> GroupChannelReadStatusUpdated;
    public event Action<SbGroupChannel> GroupChannelDeliveryStatusUpdated;
    public event Action<SbGroupChannel> GroupChannelPinnedMessageUpdated;
    public event Action<IReadOnlyList<SbGroupChannel>> GroupChannelMemberCountChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (autoInitialize)
        {
            Initialize();
        }
    }

    void Update()
    {
        ProcessMainThreadActions();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            TearDownHandlers();
            Instance = null;
        }
    }

    public void Initialize(string overrideApplicationId = null, SbLogLevel? overrideLogLevel = null, string appVersion = null)
    {
        if (isInitialized)
            return;

        string appId = string.IsNullOrEmpty(overrideApplicationId) ? applicationId : overrideApplicationId;
        if (string.IsNullOrEmpty(appId))
            throw new InvalidOperationException("SendbirdChatManager: Application ID is empty.");

        if (overrideLogLevel.HasValue)
            logLevel = overrideLogLevel.Value;

        SbInitParams initParams = new SbInitParams(appId, logLevel, string.IsNullOrEmpty(appVersion) ? Application.version : appVersion);
        SendbirdChat.Init(initParams);

        SetupHandlers();

        isInitialized = true;
    }

    void SetupHandlers()
    {
        connectionHandler = new SbConnectionHandler
        {
            OnConnected = userId =>
            {
                connectedUserId = userId;
                EnqueueMainThread(() =>
                {
                    ConnectionStateChanged?.Invoke(SendbirdChat.GetConnectionState());
                    Connected?.Invoke(userId);
                });
            },
            OnDisconnected = userId =>
            {
                EnqueueMainThread(() =>
                {
                    ConnectionStateChanged?.Invoke(SendbirdChat.GetConnectionState());
                    Disconnected?.Invoke(userId);
                });
            },
            OnReconnectStarted = () =>
            {
                EnqueueMainThread(() =>
                {
                    ConnectionStateChanged?.Invoke(SendbirdChat.GetConnectionState());
                });
            },
            OnReconnectSucceeded = () =>
            {
                EnqueueMainThread(() =>
                {
                    ConnectionStateChanged?.Invoke(SendbirdChat.GetConnectionState());
                });
            },
            OnReconnectFailed = () =>
            {
                EnqueueMainThread(() =>
                {
                    ConnectionStateChanged?.Invoke(SendbirdChat.GetConnectionState());
                });
            }
        };
        SendbirdChat.AddConnectionHandler(ConnectionHandlerId, connectionHandler);

        userEventHandler = new SbUserEventHandler
        {
            OnTotalUnreadMessageCountChanged = unreadCount =>
            {
                EnqueueMainThread(() =>
                {
                    TotalUnreadMessageCountChanged?.Invoke(unreadCount);
                });
            }
        };
        SendbirdChat.AddUserEventHandler(UserHandlerId, userEventHandler);

        openChannelHandler = new SbOpenChannelHandler
        {
            OnMessageReceived = (channel, message) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageReceived?.Invoke(channel, message);
                });
            },
            OnMessageUpdated = (channel, message) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageUpdated?.Invoke(channel, message);
                });
            },
            OnMessageDeleted = (channel, messageId) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageDeleted?.Invoke(channel, messageId);
                });
            },
            OnChannelChanged = channel =>
            {
                EnqueueMainThread(() =>
                {
                    ChannelChanged?.Invoke(channel);
                });
            },
            OnChannelDeleted = (channelUrl, channelType) =>
            {
                EnqueueMainThread(() =>
                {
                    ChannelDeleted?.Invoke(channelUrl, channelType);
                });
            },
            OnReactionUpdated = (channel, reactionEvent) =>
            {
                EnqueueMainThread(() =>
                {
                    ReactionUpdated?.Invoke(channel, reactionEvent);
                });
            },
            OnUserEntered = (channel, user) =>
            {
                EnqueueMainThread(() =>
                {
                    OpenChannelUserEntered?.Invoke(channel, user);
                });
            },
            OnUserExited = (channel, user) =>
            {
                EnqueueMainThread(() =>
                {
                    OpenChannelUserExited?.Invoke(channel, user);
                });
            },
            OnChannelParticipantCountChanged = channels =>
            {
                EnqueueMainThread(() =>
                {
                    OpenChannelParticipantCountChanged?.Invoke(channels);
                });
            }
        };
        SendbirdChat.OpenChannel.AddOpenChannelHandler(OpenChannelHandlerId, openChannelHandler);

        groupChannelHandler = new SbGroupChannelHandler
        {
            OnMessageReceived = (channel, message) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageReceived?.Invoke(channel, message);
                });
            },
            OnMessageUpdated = (channel, message) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageUpdated?.Invoke(channel, message);
                });
            },
            OnMessageDeleted = (channel, messageId) =>
            {
                EnqueueMainThread(() =>
                {
                    MessageDeleted?.Invoke(channel, messageId);
                });
            },
            OnChannelChanged = channel =>
            {
                EnqueueMainThread(() =>
                {
                    ChannelChanged?.Invoke(channel);
                });
            },
            OnChannelDeleted = (channelUrl, channelType) =>
            {
                EnqueueMainThread(() =>
                {
                    ChannelDeleted?.Invoke(channelUrl, channelType);
                });
            },
            OnReactionUpdated = (channel, reactionEvent) =>
            {
                EnqueueMainThread(() =>
                {
                    ReactionUpdated?.Invoke(channel, reactionEvent);
                });
            },
            OnUserJoined = (channel, member) =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelUserJoined?.Invoke(channel, member);
                });
            },
            OnUserLeft = (channel, member) =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelUserLeft?.Invoke(channel, member);
                });
            },
            OnTypingStatusUpdated = channel =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelTypingStatusUpdated?.Invoke(channel);
                });
            },
            OnReadStatusUpdated = channel =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelReadStatusUpdated?.Invoke(channel);
                });
            },
            OnDeliveryStatusUpdated = channel =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelDeliveryStatusUpdated?.Invoke(channel);
                });
            },
            OnPinnedMessageUpdated = channel =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelPinnedMessageUpdated?.Invoke(channel);
                });
            },
            OnChannelMemberCountChanged = channels =>
            {
                EnqueueMainThread(() =>
                {
                    GroupChannelMemberCountChanged?.Invoke(channels);
                });
            }
        };
        SendbirdChat.GroupChannel.AddGroupChannelHandler(GroupChannelHandlerId, groupChannelHandler);
    }

    void TearDownHandlers()
    {
        SendbirdChat.RemoveConnectionHandler(ConnectionHandlerId);
        SendbirdChat.RemoveUserEventHandler(UserHandlerId);
        SendbirdChat.OpenChannel.RemoveOpenChannelHandler(OpenChannelHandlerId);
        SendbirdChat.GroupChannel.RemoveGroupChannelHandler(GroupChannelHandlerId);
        SendbirdChat.RemoveAllConnectionHandlers();
        SendbirdChat.RemoveAllUserEventHandlers();
        SendbirdChat.OpenChannel.RemoveAllOpenChannelHandlers();
        SendbirdChat.GroupChannel.RemoveAllGroupChannelHandlers();
    }

    void EnqueueMainThread(Action action)
    {
        if (action == null)
            return;

        lock (mainThreadActions)
        {
            mainThreadActions.Enqueue(action);
        }
    }

    void ProcessMainThreadActions()
    {
        while (true)
        {
            Action action = null;
            lock (mainThreadActions)
            {
                if (mainThreadActions.Count == 0)
                    break;

                action = mainThreadActions.Dequeue();
            }

            action?.Invoke();
        }
    }

    public Task<SbUser> ConnectAsync(string userId, string authToken = null)
    {
        if (!isInitialized)
            Initialize();

        var tcs = new TaskCompletionSource<SbUser>();

        SbUserHandler callback = (user, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            connectedUserId = userId;
            connectedAuthToken = authToken;
            tcs.TrySetResult(user);
        };

        if (string.IsNullOrEmpty(authToken))
        {
            SendbirdChat.Connect(userId, callback);
        }
        else
        {
            SendbirdChat.Connect(userId, authToken, callback);
        }

        return tcs.Task;
    }

    public Task DisconnectAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.Disconnect(() =>
        {
            connectedUserId = null;
            connectedAuthToken = null;
            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task<SbGroupChannel> CreateGroupChannelAsync(SbGroupChannelCreateParams createParams)
    {
        var tcs = new TaskCompletionSource<SbGroupChannel>();

        GroupChannelModule.CreateChannel(createParams, (channel, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }
            
            tcs.TrySetResult(channel);
        });

        return tcs.Task;
    }

    public Task<SbOpenChannel> CreateOpenChannelAsync(SbOpenChannelCreateParams createParams)
    {
        var tcs = new TaskCompletionSource<SbOpenChannel>();

        OpenChannelModule.CreateChannel(createParams, (channel, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(channel);
        });

        return tcs.Task;
    }

    public Task<SbGroupChannel> GetGroupChannelAsync(string channelUrl)
    {
        var tcs = new TaskCompletionSource<SbGroupChannel>();

        GroupChannelModule.GetChannel(channelUrl, (channel, inIsFromCache, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(channel);
        });

        return tcs.Task;
    }

    public Task<SbOpenChannel> GetOpenChannelAsync(string channelUrl)
    {
        var tcs = new TaskCompletionSource<SbOpenChannel>();

        OpenChannelModule.GetChannel(channelUrl, (channel, inIsFromCache, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(channel);
        });

        return tcs.Task;
    }

    public Task<SbUserMessage> SendUserMessageAsync(SbBaseChannel channel, string message, string data = null, string customType = null)
    {
        var tcs = new TaskCompletionSource<SbUserMessage>();

        SbUserMessageCreateParams messageParams = new SbUserMessageCreateParams(message)
        {
            Data = data,
            CustomType = customType
        };

        channel.SendUserMessage(messageParams, (userMessage, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(userMessage);
        });

        return tcs.Task;
    }

    public Task<SbFileMessage> SendFileMessageAsync(SbBaseChannel channel, SbFileInfo fileInfo, string data = null, string customType = null)
    {
        var tcs = new TaskCompletionSource<SbFileMessage>();

        SbFileMessageCreateParams fileParams = new SbFileMessageCreateParams(fileInfo)
        {
            Data = data,
            CustomType = customType
        };

        channel.SendFileMessage(fileParams, 
            (inRequestId, inBytesSent, inTotalBytesSent, inTotalBytesExpectedToSend) =>
        {

        }, (inFileMessage, inError) =>
        {
            if (inError != null)
            {
                tcs.TrySetException(new Exception(inError.ErrorMessage));
                return;
            }

            tcs.TrySetResult(inFileMessage);
        });

        return tcs.Task;
    }

    public Task DeleteMessageAsync(SbBaseChannel channel, long inMessageId)
    {
        var tcs = new TaskCompletionSource<bool>();

        channel.DeleteMessage(inMessageId, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task UpdateUserMessageAsync(SbBaseChannel channel, long messageId, SbUserMessageUpdateParams updateParams)
    {
        var tcs = new TaskCompletionSource<bool>();

        channel.UpdateUserMessage(messageId, updateParams, (userMessage, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task AddReactionAsync(SbBaseChannel channel, SbBaseMessage message, string key)
    {
        var tcs = new TaskCompletionSource<bool>();

        channel.AddReaction(message, key, (reactionEvent, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task DeleteReactionAsync(SbBaseChannel channel, SbBaseMessage message, string key)
    {
        var tcs = new TaskCompletionSource<bool>();

        channel.DeleteReaction(message, key, (reactionEvent, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public SbGroupChannelListQuery CreateMyGroupChannelListQuery(SbGroupChannelListQueryParams queryParams = null)
    {
        return GroupChannelModule.CreateMyGroupChannelListQuery(queryParams);
    }

    public SbOpenChannelListQuery CreateOpenChannelListQuery(SbOpenChannelListQueryParams queryParams = null)
    {
        return OpenChannelModule.CreateOpenChannelListQuery(queryParams);
    }

    public SbMessageSearchQuery CreateMessageSearchQuery(SbMessageSearchQueryParams queryParams = null)
    {
        return SendbirdChat.CreateMessageSearchQuery(queryParams);
    }

    public Task BlockUserAsync(string userId)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.BlockUser(userId, (user, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task UnblockUserAsync(string userId)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.UnblockUser(userId, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task UpdateCurrentUserInfoAsync(SbUserUpdateParams updateParams)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.UpdateCurrentUserInfo(updateParams, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task RegisterPushTokenAsync(SbPushTokenType tokenType, string token, bool unique)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.RegisterPushToken(tokenType, token, unique, (status, error) =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task UnregisterPushTokenAsync(SbPushTokenType tokenType, string token)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.UnregisterPushToken(tokenType, token, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task UnregisterAllPushTokensAsync()
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.UnregisterAllPushToken(error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task SetPushTriggerOptionAsync(SbPushTriggerOption option)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.SetPushTriggerOption(option, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task SetDoNotDisturbAsync(bool enabled, int startHour, int startMin, int endHour, int endMin, string timezone)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.SetDoNotDisturb(enabled, startHour, startMin, endHour, endMin, timezone, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task SetSnoozePeriodAsync(bool enabled, long startTimestamp, long endTimestamp)
    {
        var tcs = new TaskCompletionSource<bool>();

        SendbirdChat.SetSnoozePeriod(enabled, startTimestamp, endTimestamp, error =>
        {
            if (error != null)
            {
                tcs.TrySetException(new Exception(error.ErrorMessage));
                return;
            }

            tcs.TrySetResult(true);
        });

        return tcs.Task;
    }

    public Task<SbUnreadMessageCount> GetSubscribedUnreadMessageCountAsync()
    {
        var tcs = new TaskCompletionSource<SbUnreadMessageCount>();

        Task.Run(() =>
        {
            SbUnreadMessageCount count = GroupChannelModule.GetUnreadMessageCount();
            tcs.TrySetResult(count);
        });

        return tcs.Task;
    }
}
