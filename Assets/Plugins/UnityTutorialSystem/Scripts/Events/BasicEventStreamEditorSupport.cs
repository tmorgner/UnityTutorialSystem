using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace UnityTutorialSystem.Events
{
    public static class BasicEventStreamEditorSupport
    {
        /// <summary>
        ///     Removes all nodes that are no longer part of the graph's set of defined event types.
        ///     The list returned contains all retained nodes that continue to be valid.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="retainedNodes"></param>
        /// <returns></returns>
        public static Dictionary<string, BasicEventStreamMessage> CleanGraph(BasicEventStream graph,
                                                                             Dictionary<string, BasicEventStreamMessage> retainedNodes = null)
        {
            if (retainedNodes == null)
            {
                retainedNodes = new Dictionary<string, BasicEventStreamMessage>();
            }

#if UNITY_EDITOR
            if (graph == null)
            {
                Debug.Log("Not a graph");
                return retainedNodes;
            }

            var assetPath = AssetDatabase.GetAssetPath(graph);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null)
                {
                    continue;
                }

                if (asset == graph)
                {
                    continue;
                }


                var node = asset as BasicEventStreamMessage;
                if (node != null)
                {
                    if (graph.IsValidMessage(node))
                    {
                        Debug.Log("Retained asset " + asset + " of type " + asset.GetType());
                        retainedNodes.Add(node.name, node);
                    }
                    else
                    {
                        Debug.LogWarning("Destroyed asset " + node);
                        Object.DestroyImmediate(node, true);
                    }
                }
                else
                {
                    Debug.LogWarning("Destroyed foreign asset " + asset);
                    Object.DestroyImmediate(asset, true);
                }
            }
#endif
            return retainedNodes;
        }

        public static void GenerateNodes(BasicEventStream basicEventStream,
                                         List<BasicEventStreamMessage> generatedNodes)
        {
#if UNITY_EDITOR
            foreach (var n in generatedNodes)
            {
                EditorUtility.SetDirty(n);
                AssetDatabase.AddObjectToAsset(n, basicEventStream);
            }

            EditorUtility.SetDirty(basicEventStream);
#endif
        }
    }
}