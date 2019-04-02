#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityTutorialSystem.Events
{
    /// <summary>
    ///   Support code for creating event message sub-objects on BasicEventStream assets.
    ///   This is an editor-only class containing helper methods used by the BasicEventStream
    ///   editor callbacks.
    /// </summary>
    public static class BasicEventStreamEditorSupport
    {
        /// <summary>
        ///     Removes all nodes that are no longer part of the stream's set of defined event types.
        ///     The list returned contains all retained nodes that continue to be valid.
        /// </summary>
        /// <param name="eventStream">The event stream being processed</param>
        /// <param name="retainedNodes">A buffer of retained message nodes keyed by their name</param>
        /// <returns></returns>
        public static Dictionary<string, BasicEventStreamMessage> CleanGraph(BasicEventStream eventStream,
                                                                             Dictionary<string, BasicEventStreamMessage> retainedNodes = null)
        {
            if (retainedNodes == null)
            {
                retainedNodes = new Dictionary<string, BasicEventStreamMessage>();
            }

            if (eventStream == null)
            {
                Debug.Log("Not a graph");
                return retainedNodes;
            }

            var assetPath = AssetDatabase.GetAssetPath(eventStream);
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(assetPath))
            {
                if (asset == null)
                {
                    continue;
                }

                if (asset == eventStream)
                {
                    continue;
                }


                var node = asset as BasicEventStreamMessage;
                if (node != null)
                {
                    if (eventStream.IsValidMessage(node))
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

            return retainedNodes;
        }

        /// <summary>
        ///   Adds the given list of generated nodes as sub-objects to the event stream asset.
        /// </summary>
        /// <param name="basicEventStream">The event stream</param>
        /// <param name="generatedNodes">The nodes that should be added to the stream asset</param>
        public static void GenerateNodes(BasicEventStream basicEventStream,
                                         List<BasicEventStreamMessage> generatedNodes)
        {
            foreach (var n in generatedNodes)
            {
                EditorUtility.SetDirty(n);
                AssetDatabase.AddObjectToAsset(n, basicEventStream);
            }

            EditorUtility.SetDirty(basicEventStream);
        }
    }
}
#endif
