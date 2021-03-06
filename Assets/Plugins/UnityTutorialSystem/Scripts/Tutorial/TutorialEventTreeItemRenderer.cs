﻿using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityTutorialSystem.Events;
using UnityTutorialSystem.UI;
using UnityTutorialSystem.UI.Trees;

namespace UnityTutorialSystem.Tutorial
{
    /// <summary>
    ///  <para>
    ///   A TreeItemRenderer for TutorialEventMessage instances. This behaviour must
    ///   be added on the root object of the tree-item prefab or template object.
    ///   This class receives a TutorialEventMessage from the TreeView and configures
    ///   the assigned TextMeshProUGUI instance based on the formatting parameters
    ///   set in the inspector.
    ///  </para>
    ///  <para>
    ///   This class is lenient in which BasicEventStream messages it consumes
    ///   (basically to work around Unity's built-in limitations centred around generics
    ///   and instantiating generic serialized objects in a scene). If the
    ///   message given is a TutorialEventMessage, this renderer will use the extended
    ///   texts provided by the message, otherwise it will fall back to use the message
    ///   object's name as text.
    ///  </para>
    /// </summary>
    public class TutorialEventTreeItemRenderer : TreeItemRenderer<EventStreamTreeModelData>
    {
        [SerializeField] Color whenNextEventColor;
        [SerializeField] Color whenCompletedColor;
        [SerializeField] Color defaultColor;
        [SerializeField] FontStyles whenNextEventFontStyle;
        [SerializeField] FontStyles whenCompletedFontStyle;
        [SerializeField] FontStyles defaultFontStyle;

        [SerializeField] Toggle toggle;
        [SerializeField] TextMeshProUGUI label;
        [SerializeField] float indentPerLevel;
        [SerializeField] int ignoreLeadingIndent;

        LayoutGroup layout;
        bool anchorStored;
        int indentCorrection;
        Vector2 anchor;

        void Reset()
        {
            whenNextEventColor = Color.yellow;
            whenCompletedColor = Color.green;
            defaultColor = Color.white;
            toggle = GetComponentInChildren<Toggle>();
            label = GetComponentInChildren<TextMeshProUGUI>();
        }

        bool IsNextEvent(EventStreamTreeModelData data)
        {
            if (data.ExpectedNext == false)
            {
                return false;
            }

            var message = data.SourceMessage;
            if ((message != null) && (message.Stream != null) && message.Stream.IgnoreForNextEventIndicatorHint)
            {
                return false;
            }

            return true;
        }

        string GetNextMessage(BasicEventStreamMessage msg)
        {
            var tutorialEventMessage = msg as TutorialEventMessage;
            if (tutorialEventMessage != null)
            {
                var txt = tutorialEventMessage.TaskOpenMessage;
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    return txt;
                }
            }

            return msg.name;
        }

        string GetFailureMessage(BasicEventStreamMessage msg)
        {
            var tutorialEventMessage = msg as TutorialEventMessage;
            if (tutorialEventMessage != null)
            {
                var txt = tutorialEventMessage.TaskFailureMessage;
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    return txt;
                }
            }

            return msg.name;
        }

        protected override void Awake()
        {
            ignoreLeadingIndent = Math.Max(ignoreLeadingIndent, 0);
            layout = GetComponent<LayoutGroup>();
            base.Awake();
        }

        string GetSuccessMessage(BasicEventStreamMessage msg)
        {
            var tutorialEventMessage = msg as TutorialEventMessage;
            if (tutorialEventMessage != null)
            {
                var txt = tutorialEventMessage.TaskSuccessMessage;
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    return txt;
                }
            }

            return msg.name;
        }

        void MarkLayoutDirty()
        {
            var rectTransform = GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                LayoutRebuilder.MarkLayoutForRebuild(rectTransform);
            }
        }

        protected override void OnUpdateValue(TreePath<EventStreamTreeModelData> treePath)
        {
            if (treePath == null)
            {
                ResetContents();
                MarkLayoutDirty();
                return;
            }

            EventStreamTreeModelData data;
            if (treePath.TryGetLastComponent(out data) && (data.SourceMessage != null))
            {
                if (label != null)
                {
                    if (IsNextEvent(data))
                    {
                        label.text = GetNextMessage(data.SourceMessage);
                        label.fontStyle = whenNextEventFontStyle;
                        label.color = whenNextEventColor;
                    }
                    else if (data.Completed)
                    {
                        label.text = GetSuccessMessage(data.SourceMessage);
                        label.fontStyle = whenCompletedFontStyle;
                        label.color = whenCompletedColor;
                    }
                    else
                    {
                        label.text = GetFailureMessage(data.SourceMessage);
                        label.fontStyle = defaultFontStyle;
                        label.color = defaultColor;
                    }
                }

                if (toggle != null)
                {
                    toggle.isOn = data.Completed;
                    var rt = toggle.transform as RectTransform;
                    ApplyIndent(rt);
                }

                MarkLayoutDirty();
            }
            else
            {
                ResetContents();
                MarkLayoutDirty();
            }
        }

        void ApplyIndent(RectTransform rt)
        {
            if (rt != null)
            {
                if (!anchorStored)
                {
                    var tree = gameObject.GetComponentInParent<TreeView<EventStreamTreeModelData>>();
                    if (tree != null)
                    {
                        indentCorrection = tree.ShowRootNode ? 0 : -1;
                    }

                    anchorStored = true;
                    anchor = rt.anchoredPosition;
                }

                var effectiveIndent = Math.Max(0, indentCorrection + IndentLevel - 1 - ignoreLeadingIndent);
                var indentSpacing = indentPerLevel * effectiveIndent;
                if (layout != null)
                {
                    var padding = layout.padding;
                    layout.padding = new RectOffset((int)indentSpacing, padding.top, padding.right, padding.bottom);
                }
                rt.anchoredPosition = new Vector2(anchor.x + indentSpacing, anchor.y);
            }
        }

        void ResetContents()
        {
            if (label != null)
            {
                label.text = "";
                label.fontStyle = FontStyles.Normal;
            }

            if (toggle != null)
            {
                toggle.isOn = false;
            }
        }
    }
}