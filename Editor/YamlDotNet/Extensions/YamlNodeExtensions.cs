using System.Collections.Generic;
using System.IO;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;

namespace LSCore.Editor.YamlDotNet.Extensions
{
    public static class YamlNodeExtensions
    {
        public static YamlNode FindNodeByPath(this YamlNode node, string path)
        {
            return node.FindNodeByPath(SplitPath(path));
        }

        private static YamlNode FindNodeByPath(this YamlNode node, Queue<string> pathSegments)
        {
            if (pathSegments.Count == 0)
            {
                return node;
            }

            string segment = pathSegments.Dequeue();
            if (node is YamlMappingNode mappingNode)
            {
                if (mappingNode.Children.TryGetValue(segment, out var child))
                {
                    return child.FindNodeByPath(pathSegments);
                }
            }
            else if (node is YamlSequenceNode sequenceNode)
            {
                if (int.TryParse(segment, out int index) && index >= 0 && index < sequenceNode.Children.Count)
                {
                    return sequenceNode.Children[index].FindNodeByPath(pathSegments);
                }
            }

            return null;
        }

        private static Queue<string> SplitPath(string path)
        {
            var segments = new Queue<string>(path.Split('.'));
            return segments;
        }
        
        public static YamlNode RemoveNode(this YamlNode parentNode, string nodeName)
        {
            YamlNode node = null;
            if (parentNode is YamlMappingNode mappingNode)
            {
                if (mappingNode.Children.TryGetValue(nodeName, out node))
                {
                    mappingNode.Children.Remove(nodeName);
                }
            }
            else if (parentNode is YamlSequenceNode sequenceNode)
            {
                if (int.TryParse(nodeName, out int index) && index >= 0 && index < sequenceNode.Children.Count)
                {
                    node = sequenceNode.Children[index];
                    sequenceNode.Children.RemoveAt(index);
                    return node;
                }
            }

            return node;
        }
        
        public static string ToUnityFormatString(this YamlNode node)
        {
            var serializer = new SerializerBuilder()
                .WithIndentedSequences()
                .Build();

            using var writer = new StringWriter();

            serializer.Serialize(writer, node);
            return writer.ToString();
        }
    }
}