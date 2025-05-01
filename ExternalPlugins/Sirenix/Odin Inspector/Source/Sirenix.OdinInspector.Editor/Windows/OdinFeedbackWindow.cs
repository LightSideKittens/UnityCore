//-----------------------------------------------------------------------
// <copyright file="OdinFeedbackWindow.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor.Windows
{
#pragma warning disable

    using Sirenix.OdinInspector;
    using UnityEngine;
    using UnityEditor;
    using System;
    using UnityEngine.Networking;
    using Sirenix.Utilities;
    using Sirenix.Utilities.Editor;
    using Sirenix.OdinInspector.Editor.Internal;
    using System.Linq;
    using Sirenix.OdinInspector.Editor.Validation;

    public class OdinFeedbackWindow : OdinEditorWindow
    {
        private const string MessageGroupName = "MESSAGE-GROUP";
        private const string WaitingGroupName = "WAITING-GROUP";
        private const string SuccessGroupName = "SUCCESS-GROUP";

        [NonSerialized]
        private State state = State.Message;

        [NonSerialized]
        private string errorMessage;

        [NonSerialized]
        private DateTime validatorInstallDate;

        [NonSerialized]
        private DateTime inspectorInstallDate;

        //private DateTime ValidatorInstallDate
        //{
        //    get
        //    {
        //        if (validatorInstallDate == default)
        //        {
        //            var path = DefaultSirenixPluginPath + "Odin Inspector/Assets/Editor/" + LookupAssetName;

        //            validatorInstallDate = SirenixAssetPaths.LookupAssetName
        //        }
        //    }
        //}

        [InfoBox("@errorMessage", InfoMessageType.Error, "@string.IsNullOrEmpty(errorMessage) == false")]
        [ShowIfGroup(MessageGroupName, Condition = "@state == State.Message")]
        [BoxGroup(MessageGroupName + "/Author", LabelText = "Optional"), LabelWidth(80), LabelText("Name"), ShowInInspector]
        private static EditorPrefString authorName = new EditorPrefString(nameof(authorName), null);

        [HorizontalGroup(MessageGroupName + "/Author/Split"), LabelWidth(80), LabelText("Email"), ShowInInspector]
        private static EditorPrefString authorEmail = new EditorPrefString(nameof(authorEmail), null);

        [HorizontalGroup(MessageGroupName + "/Author/Split"), LabelWidth(80), LabelText("Company"), ShowInInspector]
        private static EditorPrefString authorCompany = new EditorPrefString(nameof(authorCompany), null);

        [BoxGroup(MessageGroupName + "/Message", ShowLabel = false), SerializeField, LabelWidth(80), LabelText("Title")]
        private static string messageTitle;

        [BoxGroup(MessageGroupName + "/Author"), OnInspectorGUI]
        private static void Space() => GUILayout.Space(5);

        [BoxGroup(MessageGroupName + "/Message"), SerializeField, CustomValueDrawer("DrawMessageBox")]
        private string messageText;

        [SerializeField, HideInInspector]
        private string product;

        [SerializeField, HideInInspector]
        private OdinFeedbackUtility.FeedbackMetaData[] metadata;

        public static void Open(string product, params OdinFeedbackUtility.FeedbackMetaData[] metadata)
        {
            var size = new Vector2(475, 430);
            var rect = GUIHelper.GetEditorWindowRect().AlignCenter(size.x, size.y);

            var w = GetWindowWithRect<OdinFeedbackWindow>(rect, true, "Send Feedback");
            w.product = product;
            w.metadata = metadata?.Union(new OdinFeedbackUtility.FeedbackMetaData[]
            {
                new OdinFeedbackUtility.FeedbackMetaData("Unity Version", Application.unityVersion.ToString()),
                new OdinFeedbackUtility.FeedbackMetaData("Odin Version", OdinInspectorVersion.Version.ToString()),
                new OdinFeedbackUtility.FeedbackMetaData("Odin Build Name", OdinInspectorVersion.BuildName.ToString()),
                new OdinFeedbackUtility.FeedbackMetaData("Validator Install Date", OdinInspectorVersion.Version.ToString()),
                new OdinFeedbackUtility.FeedbackMetaData("Inspector Install Date", OdinInspectorVersion.Version.ToString()),
            }).ToArray();

            w.position = rect;
            w.minSize = size;
            w.maxSize = size;
            w.ShowUtility();
        }

        [BoxGroup(MessageGroupName + "/Message"), Button(ButtonSizes.Large)]
        public void Send()
        {
            string odinVersion = OdinInspectorVersion.Version + " " + OdinInspectorVersion.BuildName;
            if (OdinInspectorVersion.IsEnterprise)
            {
                odinVersion += " enterprise";
            }

            var message = new OdinFeedbackUtility.FeedbackMessage
            {
                AuthorName = authorName,
                AuthorEmail = authorEmail,
                AuthorCompany = authorCompany,
                Title = messageTitle,
                MessageText = messageText,
                Product = product,
                UnityVersion = Application.unityVersion,
                OdinVersion =  odinVersion,
                MetaData = metadata,
            };

            this.state = State.Waiting;

            OdinFeedbackUtility.SendFeedback(message, ReceiveReply);

        }
        
        private void ReceiveReply(OdinFeedbackUtility.FeedbackReply reply)
        {
            if (reply.Succeeded)
            {
                this.state = State.Success;
                this.errorMessage = null;
            }
            else
            {
                this.state = State.Message;
                this.errorMessage = reply.Message;
            }
        }

        private string DrawMessageBox(string value)
        {
            var rect = GUILayoutUtility.GetRect(0, 0, GUILayoutOptions.ExpandWidth().ExpandHeight());
            return EditorGUI.TextArea(rect, value);
        }

        [ShowIfGroup(WaitingGroupName, Condition = "@state == State.Waiting"), OnInspectorGUI]
        private void DrawWaitingForReply()
        {
            var rect = new Rect(Vector2.zero, this.position.size);

            var t = (long)(EditorApplication.timeSinceStartup * 3);

            GUI.Label(rect.AlignCenterY(20), "Hold on" + new string('.', (int)(t % 4)), SirenixGUIStyles.LabelCentered);

            GUIHelper.RequestRepaint();
        }
        [ShowIfGroup(SuccessGroupName, Condition = "@state == State.Success"), OnInspectorGUI]
        private void DrawSuccess()
        {
            var rect = new Rect(Vector2.zero, this.position.size);

            rect = rect.AlignCenterY(40);

            GUI.Label(rect, "Thank you for sharing your thoughts with us.", SirenixGUIStyles.LabelCentered);

            rect = rect.AlignCenterX(120).AddY(40);

            GUIHelper.PushColor(Color.green);
            if (SirenixEditorGUI.IconButton(rect, EditorIcons.Checkmark))
            {
                this.Close();
            }
            GUIHelper.PopColor();
        }

        //[ShowInInspector] private Vector2 size { get { return this.position.size; } }

        private enum State
        {
            Message,
            Waiting,
            Success
        }
    }

    public static class OdinFeedbackUtility
    {
        private const string Uri = "https://odininspector.com/api/feedback/send";

        private const int StatusCode_Offline = 0;
        private const int StatusCode_Success = 200;
        private const int StatusCode_PleaseWait = 400;
        private const int StatusCode_NotAccepted = 406;
        private const int StatusCode_FeatureDisabled = -1; // TODO

        private const int MinMessageLength = 10;
        private const int MaxMessageLength = 100_000;

        public static void SendFeedback(FeedbackMessage message, Action<FeedbackReply> onCompleted)
        {
            if (string.IsNullOrEmpty(message.MessageText) || message.MessageText.Length < MinMessageLength)
            {
                onCompleted(FeedbackReply.Fail("Please enter a message atleast " + MinMessageLength + " characters long."));
                return;
            }
            else if (message.MessageText.Length > MaxMessageLength)
            {
                onCompleted(FeedbackReply.Fail("Please reduce the message to at most " + MaxMessageLength + " characters."));
                return;
            }

            var json = JsonUtility.ToJson(message);

            // Apparently, you cannot just set the body of UnityWebRequest without it being encoded, unless you go through stuff like this...
            var request = new UnityWebRequest(
                Uri,
                "POST",
                new DownloadHandlerBuffer(),
                new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(json)) { contentType = "application/json" } );

            try
            {
                request.SetRequestHeader("User-Agent", "odin-inspector-unity-editor/1.0");
            }
            catch { }

            var op = OdinEditorWebUtility.SendWebRequest(request);

            OdinEditorWebUtility.SubscribeOnCompleted(op, a =>
            {
                try
                {
                    var returnCode = request.responseCode;
                    var returnMessage = request.downloadHandler.text;

                    FeedbackReply reply;

                    if (returnCode == StatusCode_Offline)
                    {
                        reply = FeedbackReply.Fail("Server offline. Please try again later.");
                    }
                    else if (returnCode == StatusCode_Success)
                    {
                        reply = FeedbackReply.Success();
                    }
                    else if (returnCode == StatusCode_PleaseWait)
                    {
                        reply = FeedbackReply.Fail(returnMessage);
                    }
                    else if (returnCode == StatusCode_FeatureDisabled)
                    {
                        reply = FeedbackReply.Fail("We currently do not accept any more feedback.");
                    }
                    else if (returnCode == StatusCode_NotAccepted)
                    {
                        reply = FeedbackReply.Fail("Not accepted: " + returnMessage);
                    }
                    else
                    {
                        reply = FeedbackReply.Fail("We ran into an unexpected error. Please try again later.");
                    }

                    onCompleted(reply);
                }
                finally
                {
                    request.Dispose();
                }
            });
        }
        
        public struct FeedbackReply
        {
            public bool Succeeded;
            public string Message;

            public static FeedbackReply Success()
            {
                return new FeedbackReply
                {
                    Succeeded = true,
                    Message = null,
                };
            }
            public static FeedbackReply Fail(string message)
            {
                return new FeedbackReply
                {
                    Succeeded = false,
                    Message = message,
                };
            }
        }
        public struct FeedbackMessage
        {
            public string AuthorName;
            public string AuthorEmail;
            public string AuthorCompany;
            public string Title;
            public string MessageText;
            public string UnityVersion;
            public string OdinVersion;
            public string Product;

            public FeedbackMetaData[] MetaData;
        }

        [Serializable]
        public struct FeedbackMetaData
        {
            public string Key;
            public string Value;

            public FeedbackMetaData(string key, string value)
            {
                this.Key = key;
                this.Value = value;
            }
        }
    }
}
#endif