using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityTutorialSystem.Helpers
{
    /// <summary>
    ///     Copied from VRTK_SharedMethods class (MIT License).
    /// </summary>
    public static class SharedMethods
    {
        /// <summary>
        ///     Finds all components of a given type.
        /// </summary>
        /// <remarks>
        ///     This method returns components from active as well as inactive GameObjects in all scenes. It doesn't return assets.
        ///     For performance reasons it is recommended to not use this function every frame. Cache the result in a member
        ///     variable at startup instead.
        /// </remarks>
        /// <typeparam name="T">The component type to search for. Must be a subclass of `Component`.</typeparam>
        /// <param name="searchAllScenes">
        ///     If this is true, all loaded scenes will be searched. If this is false, only the active
        ///     scene will be searched.
        /// </param>
        /// <returns>All the found components. If no component is found an empty array is returned.</returns>
        public static T[] FindEvenInactiveComponents<T>(bool searchAllScenes = false) where T : Component
        {
            var results = FindEvenInactiveComponentsInValidScenes<T>(searchAllScenes);
            return results.ToArray();
        }

        /// <summary>
        ///     The FindEvenInactiveComponentsInLoadedScenes method searches active and inactive game objects in all
        ///     loaded scenes for components matching the type supplied.
        /// </summary>
        /// <param name="searchAllScenes">If true, will search all loaded scenes, otherwise just the active scene.</param>
        /// <param name="stopOnMatch">If true, will stop searching objects as soon as a match is found.</param>
        /// <returns></returns>
        static IEnumerable<T> FindEvenInactiveComponentsInValidScenes<T>(bool searchAllScenes, bool stopOnMatch = false) where T : Component
        {
            IEnumerable<T> results;
            if (searchAllScenes)
            {
                var allSceneResults = new List<T>();
                for (var sceneIndex = 0; sceneIndex < SceneManager.sceneCount; sceneIndex++)
                {
                    allSceneResults.AddRange(FindEvenInactiveComponentsInScene<T>(SceneManager.GetSceneAt(sceneIndex), stopOnMatch));
                }

                results = allSceneResults;
            }
            else
            {
                results = FindEvenInactiveComponentsInScene<T>(SceneManager.GetActiveScene(), stopOnMatch);
            }

            return results;
        }

        /// <summary>
        ///     The FIndEvenInactiveComponentsInScene method searches the specified scene for components matching the type
        ///     supplied.
        /// </summary>
        /// <param name="scene">The scene to search. This scene must be valid, either loaded or loading.</param>
        /// <param name="stopOnMatch">If true, will stop searching objects as soon as a match is found.</param>
        /// <returns></returns>
        static IEnumerable<T> FindEvenInactiveComponentsInScene<T>(Scene scene, bool stopOnMatch = false)
        {
            var results = new List<T>();
            if (!scene.isLoaded)
            {
                return results;
            }

            foreach (var rootObject in scene.GetRootGameObjects())
            {
                if (stopOnMatch)
                {
                    var foundComponent = rootObject.GetComponentInChildren<T>(true);
                    if (foundComponent != null)
                    {
                        results.Add(foundComponent);
                        return results;
                    }
                }
                else
                {
                    results.AddRange(rootObject.GetComponentsInChildren<T>(true));
                }
            }

            return results;
        }


        public static string Path(this GameObject go)
        {
            var b = new StringBuilder();
            BuildPath(go, b);
            return b.ToString();
        }

        static void BuildPath(GameObject go, StringBuilder b)
        {
            if (go.transform.parent != null)
            {
                var pgo = go.transform.parent.gameObject;
                BuildPath(pgo, b);
                b.Append("/");
            }

            b.Append(go.name);
        }
    }
}