//-----------------------------------------------------------------------
// <copyright file="RequiredInPrefabInstancesAttribute.cs" company="Sirenix ApS">
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

    using System;
    using System.ComponentModel;

    [AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Use [RequiredIn(PrefabKind.PrefabInstance)] instead.", true)]
    public sealed class RequiredInPrefabInstancesAttribute : Attribute
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
        public RequiredInPrefabInstancesAttribute()
        {
            this.MessageType = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="errorMessage">The message to display in the error box.</param>
        /// <param name="messageType">The type of info box to draw.</param>
        public RequiredInPrefabInstancesAttribute(string errorMessage, InfoMessageType messageType)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType  = messageType;
        }

        /// <summary>
        /// Adds an error box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="errorMessage">The message to display in the error box.</param>
        public RequiredInPrefabInstancesAttribute(string errorMessage)
        {
            this.ErrorMessage = errorMessage;
            this.MessageType  = InfoMessageType.Error;
        }

        /// <summary>
        /// Adds an info box to the inspector, if the property is missing.
        /// </summary>
        /// <param name="messageType">The type of info box to draw.</param>
        public RequiredInPrefabInstancesAttribute(InfoMessageType messageType)
        {
            this.MessageType = messageType;
        }
    }
}