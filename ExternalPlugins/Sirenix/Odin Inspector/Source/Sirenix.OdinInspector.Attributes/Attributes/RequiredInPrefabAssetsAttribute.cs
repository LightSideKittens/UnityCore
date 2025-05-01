//-----------------------------------------------------------------------
// <copyright file="RequiredInPrefabAssetsAttribute.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector
{
#pragma warning disable

    using System.ComponentModel;
    using System;

    [Obsolete("Use [RequiredIn(PrefabKind.PrefabAsset)] instead.", true)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public sealed class RequiredInPrefabAssetsAttribute : Attribute
    {
        /// <summary>
        /// The message of the info box.
        /// </summary>
        public string ErrorMessage;

        /// <summary>
        /// The type of the info box.
        /// </summary>
        public InfoMessageType MessageType;

        /// <summary>
        /// Adds an error box to the inspector, if the property is missing.
        /// </summary>
        public RequiredInPrefabAssetsAttribute()
        {
            this.MessageType = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="errorMessage">The message to display in the error box.</param>
        /// <param name="messageType">The type of info box to draw.</param>
        public RequiredInPrefabAssetsAttribute(string errorMessage, InfoMessageType messageType)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType  = messageType;
        }

        /// <summary>
        /// Adds an error box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="errorMessage">The message to display in the error box.</param>
        public RequiredInPrefabAssetsAttribute(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType  = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="messageType">The type of info box to draw.</param>
        public RequiredInPrefabAssetsAttribute(InfoMessageType messageType)
        {
            this.MessageType = messageType;
        }
    }
}