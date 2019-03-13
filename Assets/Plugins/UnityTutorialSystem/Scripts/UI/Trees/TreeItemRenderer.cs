using UnityEngine;

namespace UnityTutorialSystem.UI.Trees
{
    /// <summary>
    ///     Abstract because its unity
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class TreeItemRenderer<T> : MonoBehaviour
    {
        TreePath<T> path;
        bool awake;

        public int IndentLevel
        {
            get
            {
                if (Path == null)
                {
                    return 0;
                }

                return Path.Count;
            }
        }

        public TreePath<T> Path
        {
            get { return path; }
            set
            {
                path = value;
                if (value != null)
                {
                    name = path.ToString();
                }
                else
                {
                    name = "[]";
                }

                if (awake)
                {
                    OnUpdateValue(path);
                }
            }
        }

        protected virtual void Awake()
        {
            awake = true;
            if (path != null)
            {
                OnUpdateValue(path);
            }
        }

        protected abstract void OnUpdateValue(TreePath<T> treePath);
    }
}