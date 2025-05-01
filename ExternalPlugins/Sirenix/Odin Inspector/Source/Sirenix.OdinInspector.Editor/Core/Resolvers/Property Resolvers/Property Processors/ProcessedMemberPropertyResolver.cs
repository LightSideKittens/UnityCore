//-----------------------------------------------------------------------
// <copyright file="ProcessedMemberPropertyResolver.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
namespace Sirenix.OdinInspector.Editor
{
#pragma warning disable

    using System;
    using System.Collections.Generic;

    [ResolverPriority(-5)]
    public class ProcessedMemberPropertyResolver<T> : BaseMemberPropertyResolver<T>, IDisposable
    {
        private List<OdinPropertyProcessor> processors;

        public virtual void Dispose()
        {
            if (this.processors != null)
            {
                for (int i = 0; i < this.processors.Count; i++)
                {
                    var disposable = this.processors[i] as IDisposable;

                    if (disposable != null)
                    {
                        disposable.Dispose();
                    }
                }
            }
        }

        protected override InspectorPropertyInfo[] GetPropertyInfos()
        {
            if (this.processors == null)
            {
                this.processors = OdinPropertyProcessorLocator.GetMemberProcessors(this.Property);
            }

            var includeSpeciallySerializedMembers = !this.Property.ValueEntry.SerializationBackend.IsUnity;
            var infos = InspectorPropertyInfoUtility.CreateMemberProperties(this.Property, typeof(T), includeSpeciallySerializedMembers);

            for (int i = 0; i < this.processors.Count; i++)
            {
                ProcessedMemberPropertyResolverExtensions.ProcessingOwnerType = typeof(T);
                try
                {
                    this.processors[i].ProcessMemberProperties(infos);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogException(ex);
                }
            }

            return InspectorPropertyInfoUtility.BuildPropertyGroupsAndFinalize(this.Property, typeof(T), infos, includeSpeciallySerializedMembers);
        }
    }
}
#endif