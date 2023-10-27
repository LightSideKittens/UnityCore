using System.Collections.Generic;
using System.IO;
using LSCore.Editor.YamlDotNet.Extensions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using YamlDotNet.RepresentationModel;

namespace LSCore.Editor.YamlDotNet.Unity
{
    internal class YamlEditor : OdinEditorWindow
    {
        [MenuItem(LSPaths.Windows.YamlEditor)]
        private static void OpenWindow()
        {
            GetWindow<YamlEditor>().Show();
        }
        
        [MultiLineProperty(30)]
        [SerializeField] private string inputYaml;

        [SerializeField] private string rootNodePath;
        [SerializeField] private List<string> nodesToWrap;
        [SerializeField] private string wrapNodeName;

        [Button]
        private void Wrap()
        {
            var yaml = new YamlStream();
            yaml.Load(new StringReader(inputYaml));
            var yamlNode = yaml.Documents[0].RootNode;
            var root = yamlNode.FindNodeByPath(rootNodePath) as YamlSequenceNode;
            
            foreach (YamlMappingNode node in root)
            {
                var wrapNode = new YamlMappingNode();
                
                foreach (var nodeToWrap in nodesToWrap)
                {
                    wrapNode.Add(nodeToWrap, node.RemoveNode(nodeToWrap));
                }

                node.Add(wrapNodeName, wrapNode);
            }

            outputYaml = yamlNode.ToUnityFormatString();
        }

        [MultiLineProperty(30)]
        [SerializeField] private string outputYaml;
    }
}