// GENERATED FILE - DO NOT MODIFY MANUALLY

using System;
using Telegram.Bot.Serialization;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.Passport;
using Telegram.Bot.Types.Payments;
using Telegram.Bot.Types.ReplyMarkups;

namespace Telegram.Bot.Types
{
    /// <summary>This object represents a message.</summary>
    public partial class Message
    {
        /// <summary>Unique message identifier inside this chat. In specific instances (e.g., message containing a video sent to a big chat), the server might automatically schedule a message instead of sending it immediately. In such cases, this field will be 0 and the relevant message will be unusable until it is actually sent</summary>
        [Newtonsoft.Json.JsonProperty("message_id")]
        
        public int Id { get; set; }

        /// <summary><em>Optional</em>. Unique identifier of a message thread to which the message belongs; for supergroups only</summary>
        [Newtonsoft.Json.JsonProperty("message_thread_id")]
        public int? MessageThreadId { get; set; }

        /// <summary><em>Optional</em>. Sender of the message; may be empty for messages sent to channels. For backward compatibility, if the message was sent on behalf of a chat, the field contains a fake sender user in non-channel chats</summary>
        public User? From { get; set; }

        /// <summary><em>Optional</em>. Sender of the message when sent on behalf of a chat. For example, the supergroup itself for messages sent by its anonymous administrators or a linked channel for messages automatically forwarded to the channel's discussion group. For backward compatibility, if the message was sent on behalf of a chat, the field <see cref="From">From</see> contains a fake sender user in non-channel chats.</summary>
        [Newtonsoft.Json.JsonProperty("sender_chat")]
        public Chat? SenderChat { get; set; }

        /// <summary><em>Optional</em>. If the sender of the message boosted the chat, the number of boosts added by the user</summary>
        [Newtonsoft.Json.JsonProperty("sender_boost_count")]
        public int? SenderBoostCount { get; set; }

        /// <summary><em>Optional</em>. The bot that actually sent the message on behalf of the business account. Available only for outgoing messages sent on behalf of the connected business account.</summary>
        [Newtonsoft.Json.JsonProperty("sender_business_bot")]
        public User? SenderBusinessBot { get; set; }

        /// <summary>Date the message was sent. It is always a valid date.</summary>
        
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime Date { get; set; }

        /// <summary><em>Optional</em>. Unique identifier of the business connection from which the message was received. If non-empty, the message belongs to a chat of the corresponding business account that is independent from any potential bot chat which might share the same identifier.</summary>
        [Newtonsoft.Json.JsonProperty("business_connection_id")]
        public string? BusinessConnectionId { get; set; }

        /// <summary>Chat the message belongs to</summary>
        
        public Chat Chat { get; set; } = default!;

        /// <summary><em>Optional</em>. Information about the original message for forwarded messages</summary>
        [Newtonsoft.Json.JsonProperty("forward_origin")]
        public MessageOrigin? ForwardOrigin { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the message is sent to a forum topic</summary>
        [Newtonsoft.Json.JsonProperty("is_topic_message")]
        public bool IsTopicMessage { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the message is a channel post that was automatically forwarded to the connected discussion group</summary>
        [Newtonsoft.Json.JsonProperty("is_automatic_forward")]
        public bool IsAutomaticForward { get; set; }

        /// <summary><em>Optional</em>. For replies in the same chat and message thread, the original message. Note that the Message object in this field will not contain further <see cref="ReplyToMessage">ReplyToMessage</see> fields even if it itself is a reply.</summary>
        [Newtonsoft.Json.JsonProperty("reply_to_message")]
        public Message? ReplyToMessage { get; set; }

        /// <summary><em>Optional</em>. Information about the message that is being replied to, which may come from another chat or forum topic</summary>
        [Newtonsoft.Json.JsonProperty("external_reply")]
        public ExternalReplyInfo? ExternalReply { get; set; }

        /// <summary><em>Optional</em>. For replies that quote part of the original message, the quoted part of the message</summary>
        public TextQuote? Quote { get; set; }

        /// <summary><em>Optional</em>. For replies to a story, the original story</summary>
        [Newtonsoft.Json.JsonProperty("reply_to_story")]
        public Story? ReplyToStory { get; set; }

        /// <summary><em>Optional</em>. Bot through which the message was sent</summary>
        [Newtonsoft.Json.JsonProperty("via_bot")]
        public User? ViaBot { get; set; }

        /// <summary><em>Optional</em>. Date the message was last edited</summary>
        [Newtonsoft.Json.JsonProperty("edit_date")]
        [Newtonsoft.Json.JsonConverter(typeof(UnixDateTimeConverter))]
        public DateTime? EditDate { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the message can't be forwarded</summary>
        [Newtonsoft.Json.JsonProperty("has_protected_content")]
        public bool HasProtectedContent { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the message was sent by an implicit action, for example, as an away or a greeting business message, or as a scheduled message</summary>
        [Newtonsoft.Json.JsonProperty("is_from_offline")]
        public bool IsFromOffline { get; set; }

        /// <summary><em>Optional</em>. The unique identifier of a media message group this message belongs to</summary>
        [Newtonsoft.Json.JsonProperty("media_group_id")]
        public string? MediaGroupId { get; set; }

        /// <summary><em>Optional</em>. Signature of the post author for messages in channels, or the custom title of an anonymous group administrator</summary>
        [Newtonsoft.Json.JsonProperty("author_signature")]
        public string? AuthorSignature { get; set; }

        /// <summary><em>Optional</em>. For text messages, the actual text of the message</summary>
        public string? Text { get; set; }

        /// <summary><em>Optional</em>. For text messages, special entities like usernames, URLs, bot commands, etc. that appear in the text</summary>
        public MessageEntity[]? Entities { get; set; }

        /// <summary><em>Optional</em>. Options used for link preview generation for the message, if it is a text message and link preview options were changed</summary>
        [Newtonsoft.Json.JsonProperty("link_preview_options")]
        public LinkPreviewOptions? LinkPreviewOptions { get; set; }

        /// <summary><em>Optional</em>. Unique identifier of the message effect added to the message</summary>
        [Newtonsoft.Json.JsonProperty("effect_id")]
        public string? EffectId { get; set; }

        /// <summary><em>Optional</em>. Message is an animation, information about the animation. For backward compatibility, when this field is set, the <see cref="Document">Document</see> field will also be set</summary>
        public Animation? Animation { get; set; }

        /// <summary><em>Optional</em>. Message is an audio file, information about the file</summary>
        public Audio? Audio { get; set; }

        /// <summary><em>Optional</em>. Message is a general file, information about the file</summary>
        public Document? Document { get; set; }

        /// <summary><em>Optional</em>. Message contains paid media; information about the paid media</summary>
        [Newtonsoft.Json.JsonProperty("paid_media")]
        public PaidMediaInfo? PaidMedia { get; set; }

        /// <summary><em>Optional</em>. Message is a photo, available sizes of the photo</summary>
        public PhotoSize[]? Photo { get; set; }

        /// <summary><em>Optional</em>. Message is a sticker, information about the sticker</summary>
        public Sticker? Sticker { get; set; }

        /// <summary><em>Optional</em>. Message is a forwarded story</summary>
        public Story? Story { get; set; }

        /// <summary><em>Optional</em>. Message is a video, information about the video</summary>
        public Video? Video { get; set; }

        /// <summary><em>Optional</em>. Message is a <a href="https://telegram.org/blog/video-messages-and-telescope">video note</a>, information about the video message</summary>
        [Newtonsoft.Json.JsonProperty("video_note")]
        public VideoNote? VideoNote { get; set; }

        /// <summary><em>Optional</em>. Message is a voice message, information about the file</summary>
        public Voice? Voice { get; set; }

        /// <summary><em>Optional</em>. Caption for the animation, audio, document, paid media, photo, video or voice</summary>
        public string? Caption { get; set; }

        /// <summary><em>Optional</em>. For messages with a caption, special entities like usernames, URLs, bot commands, etc. that appear in the caption</summary>
        [Newtonsoft.Json.JsonProperty("caption_entities")]
        public MessageEntity[]? CaptionEntities { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the caption must be shown above the message media</summary>
        [Newtonsoft.Json.JsonProperty("show_caption_above_media")]
        public bool ShowCaptionAboveMedia { get; set; }

        /// <summary><em>Optional</em>. <see langword="true"/>, if the message media is covered by a spoiler animation</summary>
        [Newtonsoft.Json.JsonProperty("has_media_spoiler")]
        public bool HasMediaSpoiler { get; set; }

        /// <summary><em>Optional</em>. Message is a shared contact, information about the contact</summary>
        public Contact? Contact { get; set; }

        /// <summary><em>Optional</em>. Message is a dice with random value</summary>
        public Dice? Dice { get; set; }

        /// <summary><em>Optional</em>. Message is a game, information about the game. <a href="https://core.telegram.org/bots/api#games">More about games »</a></summary>
        public Game? Game { get; set; }

        /// <summary><em>Optional</em>. Message is a native poll, information about the poll</summary>
        public Poll? Poll { get; set; }

        /// <summary><em>Optional</em>. Message is a venue, information about the venue. For backward compatibility, when this field is set, the <see cref="Location">Location</see> field will also be set</summary>
        public Venue? Venue { get; set; }

        /// <summary><em>Optional</em>. Message is a shared location, information about the location</summary>
        public Location? Location { get; set; }

        /// <summary><em>Optional</em>. New members that were added to the group or supergroup and information about them (the bot itself may be one of these members)</summary>
        [Newtonsoft.Json.JsonProperty("new_chat_members")]
        public User[]? NewChatMembers { get; set; }

        /// <summary><em>Optional</em>. A member was removed from the group, information about them (this member may be the bot itself)</summary>
        [Newtonsoft.Json.JsonProperty("left_chat_member")]
        public User? LeftChatMember { get; set; }

        /// <summary><em>Optional</em>. A chat title was changed to this value</summary>
        [Newtonsoft.Json.JsonProperty("new_chat_title")]
        public string? NewChatTitle { get; set; }

        /// <summary><em>Optional</em>. A chat photo was change to this value</summary>
        [Newtonsoft.Json.JsonProperty("new_chat_photo")]
        public PhotoSize[]? NewChatPhoto { get; set; }

        /// <summary><em>Optional</em>. Service message: the chat photo was deleted</summary>
        [Newtonsoft.Json.JsonProperty("delete_chat_photo")]
        public bool? DeleteChatPhoto { get; set; }

        /// <summary><em>Optional</em>. Service message: the group has been created</summary>
        [Newtonsoft.Json.JsonProperty("group_chat_created")]
        public bool? GroupChatCreated { get; set; }

        /// <summary><em>Optional</em>. Service message: the supergroup has been created. This field can't be received in a message coming through updates, because bot can't be a member of a supergroup when it is created. It can only be found in <see cref="ReplyToMessage">ReplyToMessage</see> if someone replies to a very first message in a directly created supergroup.</summary>
        [Newtonsoft.Json.JsonProperty("supergroup_chat_created")]
        public bool? SupergroupChatCreated { get; set; }

        /// <summary><em>Optional</em>. Service message: the channel has been created. This field can't be received in a message coming through updates, because bot can't be a member of a channel when it is created. It can only be found in <see cref="ReplyToMessage">ReplyToMessage</see> if someone replies to a very first message in a channel.</summary>
        [Newtonsoft.Json.JsonProperty("channel_chat_created")]
        public bool? ChannelChatCreated { get; set; }

        /// <summary><em>Optional</em>. Service message: auto-delete timer settings changed in the chat</summary>
        [Newtonsoft.Json.JsonProperty("message_auto_delete_timer_changed")]
        public MessageAutoDeleteTimerChanged? MessageAutoDeleteTimerChanged { get; set; }

        /// <summary><em>Optional</em>. The group has been migrated to a supergroup with the specified identifier.</summary>
        [Newtonsoft.Json.JsonProperty("migrate_to_chat_id")]
        public long? MigrateToChatId { get; set; }

        /// <summary><em>Optional</em>. The supergroup has been migrated from a group with the specified identifier.</summary>
        [Newtonsoft.Json.JsonProperty("migrate_from_chat_id")]
        public long? MigrateFromChatId { get; set; }

        /// <summary><em>Optional</em>. Specified message was pinned. Note that the Message object in this field will not contain further <see cref="ReplyToMessage">ReplyToMessage</see> fields even if it itself is a reply.</summary>
        [Newtonsoft.Json.JsonProperty("pinned_message")]
        public Message? PinnedMessage { get; set; }

        /// <summary><em>Optional</em>. Message is an invoice for a <a href="https://core.telegram.org/bots/api#payments">payment</a>, information about the invoice. <a href="https://core.telegram.org/bots/api#payments">More about payments »</a></summary>
        public Invoice? Invoice { get; set; }

        /// <summary><em>Optional</em>. Message is a service message about a successful payment, information about the payment. <a href="https://core.telegram.org/bots/api#payments">More about payments »</a></summary>
        [Newtonsoft.Json.JsonProperty("successful_payment")]
        public SuccessfulPayment? SuccessfulPayment { get; set; }

        /// <summary><em>Optional</em>. Message is a service message about a refunded payment, information about the payment. <a href="https://core.telegram.org/bots/api#payments">More about payments »</a></summary>
        [Newtonsoft.Json.JsonProperty("refunded_payment")]
        public RefundedPayment? RefundedPayment { get; set; }

        /// <summary><em>Optional</em>. Service message: users were shared with the bot</summary>
        [Newtonsoft.Json.JsonProperty("users_shared")]
        public UsersShared? UsersShared { get; set; }

        /// <summary><em>Optional</em>. Service message: a chat was shared with the bot</summary>
        [Newtonsoft.Json.JsonProperty("chat_shared")]
        public ChatShared? ChatShared { get; set; }

        /// <summary><em>Optional</em>. The domain name of the website on which the user has logged in. <a href="https://core.telegram.org/widgets/login">More about Telegram Login »</a></summary>
        [Newtonsoft.Json.JsonProperty("connected_website")]
        public string? ConnectedWebsite { get; set; }

        /// <summary><em>Optional</em>. Service message: the user allowed the bot to write messages after adding it to the attachment or side menu, launching a Web App from a link, or accepting an explicit request from a Web App sent by the method <a href="https://core.telegram.org/bots/webapps#initializing-mini-apps">requestWriteAccess</a></summary>
        [Newtonsoft.Json.JsonProperty("write_access_allowed")]
        public WriteAccessAllowed? WriteAccessAllowed { get; set; }

        /// <summary><em>Optional</em>. Telegram Passport data</summary>
        [Newtonsoft.Json.JsonProperty("passport_data")]
        public PassportData? PassportData { get; set; }

        /// <summary><em>Optional</em>. Service message. A user in the chat triggered another user's proximity alert while sharing Live Location.</summary>
        [Newtonsoft.Json.JsonProperty("proximity_alert_triggered")]
        public ProximityAlertTriggered? ProximityAlertTriggered { get; set; }

        /// <summary><em>Optional</em>. Service message: user boosted the chat</summary>
        [Newtonsoft.Json.JsonProperty("boost_added")]
        public ChatBoostAdded? BoostAdded { get; set; }

        /// <summary><em>Optional</em>. Service message: chat background set</summary>
        [Newtonsoft.Json.JsonProperty("chat_background_set")]
        public ChatBackground? ChatBackgroundSet { get; set; }

        /// <summary><em>Optional</em>. Service message: forum topic created</summary>
        [Newtonsoft.Json.JsonProperty("forum_topic_created")]
        public ForumTopicCreated? ForumTopicCreated { get; set; }

        /// <summary><em>Optional</em>. Service message: forum topic edited</summary>
        [Newtonsoft.Json.JsonProperty("forum_topic_edited")]
        public ForumTopicEdited? ForumTopicEdited { get; set; }

        /// <summary><em>Optional</em>. Service message: forum topic closed</summary>
        [Newtonsoft.Json.JsonProperty("forum_topic_closed")]
        public ForumTopicClosed? ForumTopicClosed { get; set; }

        /// <summary><em>Optional</em>. Service message: forum topic reopened</summary>
        [Newtonsoft.Json.JsonProperty("forum_topic_reopened")]
        public ForumTopicReopened? ForumTopicReopened { get; set; }

        /// <summary><em>Optional</em>. Service message: the 'General' forum topic hidden</summary>
        [Newtonsoft.Json.JsonProperty("general_forum_topic_hidden")]
        public GeneralForumTopicHidden? GeneralForumTopicHidden { get; set; }

        /// <summary><em>Optional</em>. Service message: the 'General' forum topic unhidden</summary>
        [Newtonsoft.Json.JsonProperty("general_forum_topic_unhidden")]
        public GeneralForumTopicUnhidden? GeneralForumTopicUnhidden { get; set; }

        /// <summary><em>Optional</em>. Service message: a scheduled giveaway was created</summary>
        [Newtonsoft.Json.JsonProperty("giveaway_created")]
        public GiveawayCreated? GiveawayCreated { get; set; }

        /// <summary><em>Optional</em>. The message is a scheduled giveaway message</summary>
        public Giveaway? Giveaway { get; set; }

        /// <summary><em>Optional</em>. A giveaway with public winners was completed</summary>
        [Newtonsoft.Json.JsonProperty("giveaway_winners")]
        public GiveawayWinners? GiveawayWinners { get; set; }

        /// <summary><em>Optional</em>. Service message: a giveaway without public winners was completed</summary>
        [Newtonsoft.Json.JsonProperty("giveaway_completed")]
        public GiveawayCompleted? GiveawayCompleted { get; set; }

        /// <summary><em>Optional</em>. Service message: video chat scheduled</summary>
        [Newtonsoft.Json.JsonProperty("video_chat_scheduled")]
        public VideoChatScheduled? VideoChatScheduled { get; set; }

        /// <summary><em>Optional</em>. Service message: video chat started</summary>
        [Newtonsoft.Json.JsonProperty("video_chat_started")]
        public VideoChatStarted? VideoChatStarted { get; set; }

        /// <summary><em>Optional</em>. Service message: video chat ended</summary>
        [Newtonsoft.Json.JsonProperty("video_chat_ended")]
        public VideoChatEnded? VideoChatEnded { get; set; }

        /// <summary><em>Optional</em>. Service message: new participants invited to a video chat</summary>
        [Newtonsoft.Json.JsonProperty("video_chat_participants_invited")]
        public VideoChatParticipantsInvited? VideoChatParticipantsInvited { get; set; }

        /// <summary><em>Optional</em>. Service message: data sent by a Web App</summary>
        [Newtonsoft.Json.JsonProperty("web_app_data")]
        public WebAppData? WebAppData { get; set; }

        /// <summary><em>Optional</em>. Inline keyboard attached to the message. <c>LoginUrl</c> buttons are represented as ordinary <c>url</c> buttons.</summary>
        [Newtonsoft.Json.JsonProperty("reply_markup")]
        public InlineKeyboardMarkup? ReplyMarkup { get; set; }

        /// <summary>Gets the <see cref="MessageType">type</see> of the <see cref="Message"/></summary>
        /// <value>The <see cref="MessageType">type</see> of the <see cref="Message"/></value>
        [Newtonsoft.Json.JsonIgnore]
        public MessageType Type => this switch
        {
            { Text: not null }                              => MessageType.Text,
            { Animation: not null }                         => MessageType.Animation,
            { Audio: not null }                             => MessageType.Audio,
            { Document: not null }                          => MessageType.Document,
            { PaidMedia: not null }                         => MessageType.PaidMedia,
            { Photo: not null }                             => MessageType.Photo,
            { Sticker: not null }                           => MessageType.Sticker,
            { Story: not null }                             => MessageType.Story,
            { Video: not null }                             => MessageType.Video,
            { VideoNote: not null }                         => MessageType.VideoNote,
            { Voice: not null }                             => MessageType.Voice,
            { Contact: not null }                           => MessageType.Contact,
            { Dice: not null }                              => MessageType.Dice,
            { Game: not null }                              => MessageType.Game,
            { Poll: not null }                              => MessageType.Poll,
            { Venue: not null }                             => MessageType.Venue,
            { Location: not null }                          => MessageType.Location,
            { NewChatMembers: not null }                    => MessageType.NewChatMembers,
            { LeftChatMember: not null }                    => MessageType.LeftChatMember,
            { NewChatTitle: not null }                      => MessageType.NewChatTitle,
            { NewChatPhoto: not null }                      => MessageType.NewChatPhoto,
            { DeleteChatPhoto: not null }                   => MessageType.DeleteChatPhoto,
            { GroupChatCreated: not null }                  => MessageType.GroupChatCreated,
            { SupergroupChatCreated: not null }             => MessageType.SupergroupChatCreated,
            { ChannelChatCreated: not null }                => MessageType.ChannelChatCreated,
            { MessageAutoDeleteTimerChanged: not null }     => MessageType.MessageAutoDeleteTimerChanged,
            { MigrateToChatId: not null }                   => MessageType.MigrateToChatId,
            { MigrateFromChatId: not null }                 => MessageType.MigrateFromChatId,
            { PinnedMessage: not null }                     => MessageType.PinnedMessage,
            { Invoice: not null }                           => MessageType.Invoice,
            { SuccessfulPayment: not null }                 => MessageType.SuccessfulPayment,
            { RefundedPayment: not null }                   => MessageType.RefundedPayment,
            { UsersShared: not null }                       => MessageType.UsersShared,
            { ChatShared: not null }                        => MessageType.ChatShared,
            { ConnectedWebsite: not null }                  => MessageType.ConnectedWebsite,
            { WriteAccessAllowed: not null }                => MessageType.WriteAccessAllowed,
            { PassportData: not null }                      => MessageType.PassportData,
            { ProximityAlertTriggered: not null }           => MessageType.ProximityAlertTriggered,
            { BoostAdded: not null }                        => MessageType.BoostAdded,
            { ChatBackgroundSet: not null }                 => MessageType.ChatBackgroundSet,
            { ForumTopicCreated: not null }                 => MessageType.ForumTopicCreated,
            { ForumTopicEdited: not null }                  => MessageType.ForumTopicEdited,
            { ForumTopicClosed: not null }                  => MessageType.ForumTopicClosed,
            { ForumTopicReopened: not null }                => MessageType.ForumTopicReopened,
            { GeneralForumTopicHidden: not null }           => MessageType.GeneralForumTopicHidden,
            { GeneralForumTopicUnhidden: not null }         => MessageType.GeneralForumTopicUnhidden,
            { GiveawayCreated: not null }                   => MessageType.GiveawayCreated,
            { Giveaway: not null }                          => MessageType.Giveaway,
            { GiveawayWinners: not null }                   => MessageType.GiveawayWinners,
            { GiveawayCompleted: not null }                 => MessageType.GiveawayCompleted,
            { VideoChatScheduled: not null }                => MessageType.VideoChatScheduled,
            { VideoChatStarted: not null }                  => MessageType.VideoChatStarted,
            { VideoChatEnded: not null }                    => MessageType.VideoChatEnded,
            { VideoChatParticipantsInvited: not null }      => MessageType.VideoChatParticipantsInvited,
            { WebAppData: not null }                        => MessageType.WebAppData,
            _                                               => MessageType.Unknown
        };
    }
}
