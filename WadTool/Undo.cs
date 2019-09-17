﻿using TombLib.Graphics;
using TombLib.Utils;

namespace WadTool
{
    public abstract class AnimationEditorUndoRedoInstance : UndoRedoInstance
    {
        public AnimationEditor Parent { get; internal set; }
        protected int AnimCount;

        protected AnimationEditorUndoRedoInstance(AnimationEditor parent) { Parent = parent; AnimCount = Parent.Animations.Count; }
    }

    public class AnimationUndoInstance : AnimationEditorUndoRedoInstance
    {
        private AnimationNode Animation;

        public AnimationUndoInstance(AnimationEditor parent, AnimationNode anim) : base(parent)
        {
            Animation = anim.Clone();

            Valid = () => Parent.Animations.Count == AnimCount &&
                          Animation.DirectXAnimation != null &&
                          Animation.WadAnimation != null && 
                          Animation.Index >= 0;

            UndoAction = () =>
            {
                Parent.Animations[Animation.Index] = Animation;
                Parent.Tool.AnimationEditorAnimationChanged(Animation);
            };

            RedoInstance = () => new AnimationUndoInstance(Parent, Parent.Animations[Animation.Index]);
        }
    }

    public class WadToolUndoManager : UndoManager
    {
        public WadToolClass Tool;

        public WadToolUndoManager(WadToolClass tool, int undoDepth) : base(undoDepth)
        {
            Tool = tool;

            UndoStackChanged += (s, e) => Tool.UndoStackChanged();
            MessageSent += (s, e) => Tool.SendMessage(e.Message, TombLib.Forms.PopupType.Warning);
        }

        public void PushAnimationChanged(AnimationEditor editor, AnimationNode anim) => Push(new AnimationUndoInstance(editor, anim));
    }
}
