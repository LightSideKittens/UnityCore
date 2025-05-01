//-----------------------------------------------------------------------
// <copyright file="FileFolderPathValidator.cs" company="Sirenix ApS">
// Copyright (c) Sirenix ApS. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------
#if UNITY_EDITOR
#define ODIN_INSPECTOR
#define ODIN_INSPECTOR_3
#define ODIN_INSPECTOR_3_1
#define ODIN_INSPECTOR_3_2
#define ODIN_INSPECTOR_3_3
using System.Linq;
using UnityEngine;

[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.FilePathValidator))]
[assembly: Sirenix.OdinInspector.Editor.Validation.RegisterValidator(typeof(Sirenix.OdinInspector.Editor.Validation.FolderPathValidator))]

namespace Sirenix.OdinInspector.Editor.Validation
{
#pragma warning disable

    using Sirenix.OdinInspector.Editor.ValueResolvers;
    using System.IO;

    public sealed class FilePathValidator : AttributeValidator<FilePathAttribute, string>
    {
        private bool requireExistingPath;
        private ValueResolver<string> parentPathProvider;

        protected override void Initialize()
        {
            this.requireExistingPath = this.Attribute.RequireExistingPath;

            if (this.requireExistingPath)
            {
                this.parentPathProvider = ValueResolver.GetForString(this.Property, this.Attribute.ParentFolder);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.requireExistingPath)
            {
                string path = this.ValueEntry.SmartValue ?? string.Empty;
                string parent = this.parentPathProvider.GetValue() ?? string.Empty;

                if (string.IsNullOrEmpty(parent) == false)
                {
                    path = Path.Combine(parent, path);
                }

                if (!this.Attribute.IncludeFileExtension)
                {
                    var directoryPath = Path.GetDirectoryName(path);

                    if (directoryPath != null)
                    {
                        var fileName = Path.GetFileName(path);
                        var files = Directory.GetFiles(directoryPath, $"{fileName}.*");

                        // We don't have any specific extensions to check for so it's a valid path if 'any' file exists with the given name.
                        if (string.IsNullOrEmpty(this.Attribute.Extensions))
                        {
                            if (files.Length > 0)
                            {
                                result.ResultType = ValidationResultType.Valid;
                            }
                            else
                            {
                                result.ResultType = ValidationResultType.Error;
                                result.Message = "The path does not exist.";
                            }
                        }
                        else
                        {
                            var splitExtensions = this.Attribute.Extensions
                                .Replace(" ", "")
                                .Split(',')
                                .Select(e => e.StartsWith(".") ? e : $".{e}");

                            var foundFile = false;
                            foreach (var file in files)
                            {
                                var fileExtension = Path.GetExtension(file);
                                
                                if (!splitExtensions.Contains(fileExtension)) continue;
                                
                                result.ResultType = ValidationResultType.Valid;
                                foundFile = true;
                                break;
                            }

                            if (foundFile) return;
                            
                            result.ResultType = ValidationResultType.Error;
                            result.Message = $"The path does not exist. These are the valid paths:\n\n{string.Join("\n", splitExtensions.Select(e => $"{path}{e}"))}";
                             
                        }
                    }
                }
                else if (File.Exists(path))
                {
                    result.ResultType = ValidationResultType.Valid;
                }
                else
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "The path does not exist.";
                }
            }
            else
            {
                result.ResultType = ValidationResultType.IgnoreResult;
            }
        }
    }

    public sealed class FolderPathValidator : AttributeValidator<FolderPathAttribute, string>
    {
        private bool requireExistingPath;
        private ValueResolver<string> parentPathProvider;

        protected override void Initialize()
        {
            this.requireExistingPath = this.Attribute.RequireExistingPath;

            if (this.requireExistingPath)
            {
                this.parentPathProvider = ValueResolver.GetForString(this.Property, this.Attribute.ParentFolder);
            }
        }

        protected override void Validate(ValidationResult result)
        {
            if (this.requireExistingPath)
            {
                string path = this.ValueEntry.SmartValue ?? string.Empty;
                string parent = this.parentPathProvider.GetValue() ?? string.Empty;

                if (string.IsNullOrEmpty(parent) == false)
                {
                    path = Path.Combine(parent, path);
                }

                if (Directory.Exists(path.TrimEnd('/', '\\') + "/"))
                {
                    result.ResultType = ValidationResultType.Valid;
                }
                else
                {
                    result.ResultType = ValidationResultType.Error;
                    result.Message = "The path '" + path + "' does not exist.";
                }
            }
            else
            {
                result.ResultType = ValidationResultType.IgnoreResult;
            }
        }
    }
}
#endif