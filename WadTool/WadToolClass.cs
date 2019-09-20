﻿using System;
using System.Linq;
using System.Threading;
using TombLib.Forms;
using TombLib.Graphics;
using TombLib.LevelData;
using TombLib.Wad;

namespace WadTool
{
    public interface IEditorEvent { }
    public interface IWadChangedEvent : IEditorEvent { }

    public enum WadArea
    {
        Source,
        Destination
    }

    public struct MainSelection
    {
        public WadArea WadArea;
        public IWadObjectId Id;
    }

    public class WadToolClass : IDisposable
    {
        // The editor event
        public event Action<IEditorEvent> EditorEventRaised;

        public void RaiseEvent(IEditorEvent eventObj)
        {
            SynchronizationContext.Current.Send(eventObj_ => EditorEventRaised?.Invoke((IEditorEvent)eventObj_), eventObj);
        }

        // The configuration
        public Configuration Configuration { get; }

        // Open wads
        private Wad2 _destinationWad;
        public Wad2 DestinationWad
        {
            get { return _destinationWad; }
            set
            {
                if (_destinationWad == value)
                    return;
                _destinationWad = value;
                DestinationWadChanged();

                // Update selection
                if (_mainSelection.HasValue)
                    if (_mainSelection.Value.WadArea == WadArea.Destination)
                        if (value.Contains(_mainSelection.Value.Id))
                            RaiseEvent(new MainSelectionChangedEvent());
                        else
                            MainSelection = null;
            }
        }

        private Wad2 _sourceWad;
        public Wad2 SourceWad
        {
            get { return _sourceWad; }
            set
            {
                if (_sourceWad == value)
                    return;
                _sourceWad = value;

                SourceWadChanged();

                // Update selection
                if (_mainSelection.HasValue)
                    if (_mainSelection.Value.WadArea == WadArea.Source)
                        if (value.Contains(_mainSelection.Value.Id))
                            RaiseEvent(new MainSelectionChangedEvent());
                        else
                            MainSelection = null;
            }
        }

        private Level _referenceLevel;
        public Level ReferenceLevel
        {
            get { return _referenceLevel; }
            set
            {
                if (value == _referenceLevel)
                    return;

                _referenceLevel = value;
                RaiseEvent(new ReferenceLevelChangedEvent());
            }
        }

        public Wad2 GetWad(WadArea? wadArea)
        {
            if (!wadArea.HasValue)
                return null;
            switch (wadArea.Value)
            {
                case WadArea.Source: return SourceWad;
                case WadArea.Destination: return DestinationWad;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        public void WadChanged(WadArea wadArea)
        {
            switch (wadArea)
            {
                case WadArea.Source:
                    SourceWadChanged();
                    break;
                case WadArea.Destination:
                    DestinationWadChanged();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public class DestinationWadChangedEvent : IWadChangedEvent
        { }
        public void DestinationWadChanged()
        {
            RaiseEvent(new DestinationWadChangedEvent());
        }

        public class SourceWadChangedEvent : IWadChangedEvent
        { }
        public void SourceWadChanged()
        {
            RaiseEvent(new SourceWadChangedEvent());
        }

        // Selection
        public class MainSelectionChangedEvent : IWadChangedEvent
        { }
        private MainSelection? _mainSelection;
        public MainSelection? MainSelection
        {
            get { return _mainSelection; }
            set
            {
                if (_mainSelection == null && value == null)
                    return;
                if (_mainSelection != null && value != null && _mainSelection.Equals(value))
                    return;
                _mainSelection = value;
                RaiseEvent(new MainSelectionChangedEvent());
            }
        }

        public class ReferenceLevelChangedEvent : IEditorEvent
        { }
        public void ReferenceLevelChanged()
        {
            RaiseEvent(new ReferenceLevelChangedEvent());
        }

        public class StaticSelectedLightChangedEvent : IEditorEvent
        { }
        public void StaticSelectedLightChanged()
        {
            RaiseEvent(new StaticSelectedLightChangedEvent());
        }

        public class StaticLightsChangedEvent : IEditorEvent
        { }
        public void StaticLightsChanged()
        {
            RaiseEvent(new StaticLightsChangedEvent());
        }

        public class BoneOffsetMovedEvent : IEditorEvent
        { }
        public void BoneOffsetMoved()
        {
            RaiseEvent(new BoneOffsetMovedEvent());
        }

        public class BonePickedEvent : IEditorEvent
        { }
        public void BonePicked()
        {
            RaiseEvent(new BonePickedEvent());
        }

        public class SelectedObjectEditedEvent : IEditorEvent
        { }
        public void SelectedObjectEdited()
        {
            RaiseEvent(new SelectedObjectEditedEvent());
        }

        public class AnimationEditorMeshSelectedEvent : IEditorEvent
        {
            public ObjectMesh Mesh { get; set; }
            public AnimatedModel Model { get; set; }
            public AnimationEditorMeshSelectedEvent(AnimatedModel model, ObjectMesh mesh)
            {
                Model = model;
                Mesh = mesh;
            }
        }
        public void AnimationEditorMeshSelected(AnimatedModel model, ObjectMesh mesh)
        {
            RaiseEvent(new AnimationEditorMeshSelectedEvent(model, mesh));
        }

        public class AnimationEditorGizmoPickedEvent : IEditorEvent
        {
            public AnimationEditorGizmoPickedEvent() { }
        }
        public void AnimationEditorGizmoPicked()
        {
            RaiseEvent(new AnimationEditorGizmoPickedEvent());
        }

        public class AnimationEditorAnimationChangedEvent : IEditorEvent
        {
            public AnimationNode Animation { get; set; }
            public bool Focus { get; set; }

            public AnimationEditorAnimationChangedEvent(AnimationNode anim, bool focus)
            {
                Animation = anim;
                Focus = focus;
            }
        }
        public void AnimationEditorAnimationChanged(AnimationNode anim, bool focus)
        {
            RaiseEvent(new AnimationEditorAnimationChangedEvent(anim, focus));
        }

        public class AnimationEditorCurrentAnimationChangedEvent : IEditorEvent
        {
            public AnimationNode Animation { get; set; }

            public AnimationEditorCurrentAnimationChangedEvent(AnimationNode anim)
            {
                Animation = anim;
            }
        }
        public void AnimationEditorCurrentAnimationChanged(AnimationNode anim)
        {
            RaiseEvent(new AnimationEditorCurrentAnimationChangedEvent(anim));
        }

        // Send message
        public class MessageEvent : IEditorEvent
        {
            public string Message { get; internal set; }
            public PopupType Type { get; internal set; }
        }
        public void SendMessage(string message = "", PopupType type = PopupType.None)
        {
            RaiseEvent(new MessageEvent { Message = message, Type = type });
        }

        // Undo-redo manager
        public WadToolUndoManager UndoManager { get; private set; }

        public class UndoStackChangedEvent : IEditorEvent
        {
            public bool UndoPossible { get; set; }
            public bool RedoPossible { get; set; }
            public bool UndoReversible { get; set; }
            public bool RedoReversible { get; set; }
        }
        public void UndoStackChanged()
        {
            RaiseEvent(new UndoStackChangedEvent()
            {
                UndoPossible = UndoManager.UndoPossible,
                RedoPossible = UndoManager.RedoPossible,
                UndoReversible = UndoManager.UndoReversible,
                RedoReversible = UndoManager.UndoReversible
            });
        }

        // Construction and destruction
        public WadToolClass(Configuration configuration)
        {
            Configuration = configuration;
            UndoManager = new WadToolUndoManager(this, configuration.AnimationEditor_UndoDepth);
        }

        public void Dispose()
        {

        }
    }
}
