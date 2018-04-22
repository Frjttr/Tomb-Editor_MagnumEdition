﻿using DarkUI.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TombLib.Graphics;
using TombLib.Wad;

namespace WadTool
{
    public partial class FormAnimationEditor : DarkUI.Forms.DarkForm
    {
        private WadMoveableId _moveableId;
        private Wad2 _wad;
        private WadMoveable _moveable;
        private WadToolClass _tool;
        private List<AnimationNode> _workingAnimations;
        private DeviceManager _deviceManager;
        private List<WadBone> _bones;
        private AnimationNode _selectedNode;
        private WadRenderer _renderer;
        private AnimatedModel _model;

        public FormAnimationEditor(WadToolClass tool, DeviceManager deviceManager, Wad2 wad, WadMoveableId id)
        {
            InitializeComponent();

            _renderer = new WadRenderer(deviceManager.Device);
            _tool = tool;
            _moveableId = id;
            _wad = wad;
            _moveable = _wad.Moveables[_moveableId];
            _model = _renderer.GetMoveable(_moveable);
            _deviceManager = deviceManager;

            // Initialize the panel
            var skin = _moveableId;
            if (_moveableId.TypeId == 0)
            {
                if (_wad.SuggestedGameVersion == WadGameVersion.TR4_TRNG && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
                if (_wad.SuggestedGameVersion == WadGameVersion.TR5 && _wad.Moveables.ContainsKey(WadMoveableId.LaraSkin))
                    skin = WadMoveableId.LaraSkin;
            }
            panelRendering.InitializePanel(_tool, _wad, _deviceManager, _moveableId, skin);

            // Get a copy of the skeleton in linearized form
            _bones = _moveable.Skeleton.LinearizedBones.ToList<WadBone>();

            // Load skeleton in combobox
            foreach (var bone in panelRendering.Model.Bones)
                comboSkeleton.Items.Add(bone.Name);
            comboSkeleton.SelectedIndex = 0;

            // NOTE: we work with a pair WadAnimation - Animation. All changes to animation data like name, 
            // framerate, next animation, state changes... will be saved directly to WadAnimation.
            // All changes to keyframes will be instead stored directly in the renderer's Animation class.
            // While saving, WadAnimation and Animation will be combined and original animations will be overwritten.
            _workingAnimations = new List<AnimationNode>();
            foreach (var animation in _moveable.Animations)
                _workingAnimations.Add(new AnimationNode(animation.Clone(), Animation.FromWad2(_bones, animation)));
            ReloadAnimations();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _renderer.Dispose();
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void ReloadAnimations()
        {
            treeAnimations.Nodes.Clear();

            var list = new List<DarkUI.Controls.DarkTreeNode>();
            int index = 0;
            foreach (var animation in _workingAnimations)
            {
                var node = new DarkUI.Controls.DarkTreeNode(index++ + ": " + animation.WadAnimation.Name);
                node.Tag = animation;
                list.Add(node);
            }

            treeAnimations.Nodes.AddRange(list);
        }

        private void addNewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeAnimations_Click(object sender, EventArgs e)
        {
            if (treeAnimations.SelectedNodes.Count == 0)
                return;
            var node = (AnimationNode)treeAnimations.SelectedNodes[0].Tag;
            SelectAnimation(node);
        }

        private void SelectAnimation(AnimationNode node)
        {
            _selectedNode = node;

            tbName.Text = node.WadAnimation.Name;
            tbFramerate.Text = node.WadAnimation.FrameRate.ToString();
            tbNextAnimation.Text = node.WadAnimation.NextAnimation.ToString();
            tbNextFrame.Text = node.WadAnimation.NextFrame.ToString();
            tbStateId.Text = node.WadAnimation.StateId.ToString();

            panelRendering.Animation = node;

            if (node.WadAnimation.KeyFrames.Count != 0)
            {
                trackFrames.Visible = true;

                trackFrames.Minimum = 0;
                trackFrames.Maximum = node.DirectXAnimation.KeyFrames.Count - 1;
                SelectFrame(0);
            }
            else
            {
                trackFrames.Visible = false;
            }

            panelRendering.Invalidate();

        }

        private void trackFrames_ValueChanged(object sender, EventArgs e)
        {
            SelectFrame(trackFrames.Value);
        }

        private void SelectFrame(int frameIndex)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[frameIndex];
                panelRendering.Model.BuildAnimationPose(keyFrame);
                panelRendering.CurrentKeyFrame = frameIndex;
                panelRendering.Invalidate();

                // Update GUI
                statusFrame.Text = "Frame: " + (trackFrames.Value + 1) + "/" + _selectedNode.WadAnimation.KeyFrames.Count;

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
            }
        }

        private void saveChangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _moveable.Animations.Clear();
            foreach (var animation in _workingAnimations)
            {

            }
        }

        private void butCalculateCollisionBox_Click(object sender, EventArgs e)
        {
            if (_selectedNode != null)
            {
                var keyFrame = _selectedNode.DirectXAnimation.KeyFrames[trackFrames.Value];
                keyFrame.CalculateBoundingBox(panelRendering.Model);

                panelRendering.Invalidate();

                tbCollisionBoxMinX.Text = keyFrame.BoundingBox.Minimum.X.ToString();
                tbCollisionBoxMinY.Text = keyFrame.BoundingBox.Minimum.Y.ToString();
                tbCollisionBoxMinZ.Text = keyFrame.BoundingBox.Minimum.Z.ToString();
                tbCollisionBoxMaxX.Text = keyFrame.BoundingBox.Maximum.X.ToString();
                tbCollisionBoxMaxY.Text = keyFrame.BoundingBox.Maximum.Y.ToString();
                tbCollisionBoxMaxZ.Text = keyFrame.BoundingBox.Maximum.Z.ToString();
            }
        }

        private void addNewToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            AddNewAnimation();
        }

        private void butDeleteAnimation_Click(object sender, EventArgs e)
        {
            DeleteAnimation();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteAnimation();
        }

        private void AddNewAnimation()
        {
            var wadAnimation = new WadAnimation();
            wadAnimation.FrameRate = 1;
            wadAnimation.Name = "New Animation " + _workingAnimations.Count;

            var keyFrame = new WadKeyFrame();
            foreach (var bone in _moveable.Skeleton.LinearizedBones)
                keyFrame.Angles.Add(new WadKeyFrameRotation());
            wadAnimation.KeyFrames.Add(keyFrame);

            var dxAnimation = Animation.FromWad2(_moveable.Skeleton.LinearizedBones.ToList(), wadAnimation);
            var node = new AnimationNode(wadAnimation, dxAnimation);
            var treeNode = new DarkUI.Controls.DarkTreeNode(wadAnimation.Name);
            treeNode.Tag = node;

            _workingAnimations.Add(node);
            treeAnimations.Nodes.Add(treeNode);
        }

        private void DeleteAnimation()
        {
            if (_selectedNode != null)
            { 
                if (DarkMessageBox.Show(this, "Do you really want to delete '" + _selectedNode.WadAnimation.Name + "'?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int currentIndex = _workingAnimations.IndexOf(_selectedNode);

                    // Update all references
                    for (int i = 0; i < _workingAnimations.Count; i++)
                    {
                        // Ignore the animation I'm deleting
                        if (i == currentIndex)
                            continue;

                        var animation = _workingAnimations[i];

                        // Update NextAnimation
                        if (animation.WadAnimation.NextAnimation > currentIndex)
                            animation.WadAnimation.NextAnimation--;

                        // Update state changes
                        foreach (var stateChange in animation.WadAnimation.StateChanges)
                            foreach (var dispatch in stateChange.Dispatches)
                                if (dispatch.NextAnimation > currentIndex)
                                    dispatch.NextAnimation--;
                    }

                    // Remove the animation
                    _workingAnimations.Remove(_selectedNode);

                    // Update GUI
                    treeAnimations.Nodes.Remove(treeAnimations.SelectedNodes[0]);
                    if (treeAnimations.Nodes.Count != 0)
                        SelectAnimation(treeAnimations.Nodes[0].Tag as AnimationNode);
                    else
                        _selectedNode = null;
                    panelRendering.Invalidate();
                }
            }
        }

        private void DeleteFrame()
        {
            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                if (DarkMessageBox.Show(this, "Do you really want to delete frame " + panelRendering.CurrentKeyFrame + "?",
                                        "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    int currentIndex = _workingAnimations.IndexOf(_selectedNode);

                    // Update all references
                    for (int i = 0; i < _workingAnimations.Count; i++)
                    {
                        // Ignore the animation I'm deleting
                        if (i == currentIndex)
                            continue;

                        var animation = _workingAnimations[i];

                        // Update NextAnimation
                      /*  if (animation.WadAnimation.NextFrame > panelRendering.CurrentKeyFrame)
                            animation.WadAnimation.NextFrame--;

                        // Update state changes
                        foreach (var stateChange in animation.WadAnimation.StateChanges)
                            foreach (var dispatch in stateChange.Dispatches)
                                if (dispatch.NextAnimation > currentIndex)
                                    dispatch.NextAnimation--;*/
                    }

                    // Remove the frame
                    _selectedNode.DirectXAnimation.KeyFrames.RemoveAt(panelRendering.CurrentKeyFrame);

                    // Update GUI
                    if (_selectedNode.DirectXAnimation.KeyFrames.Count != 0)
                        SelectFrame(panelRendering.CurrentKeyFrame);
                    trackFrames.Maximum--;
                    panelRendering.Invalidate();
                }
            }
        }

        private void drawGridToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGrid = !panelRendering.DrawGrid;
            panelRendering.Invalidate();
        }

        private void drawGizmoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawGizmo= !panelRendering.DrawGizmo;
            panelRendering.Invalidate();
        }

        private void drawCollisionBoxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panelRendering.DrawCollisionBox = !panelRendering.DrawCollisionBox;
            panelRendering.Invalidate();
        }

        private void insertFrameAfterCurrentOneToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AddNewKeyFrame(panelRendering.CurrentKeyFrame + 1);
        }

        private void AddNewKeyFrame(int index)
        {
            if (_selectedNode != null)
            {
                var keyFrame = new KeyFrame();
                foreach (var bone in _moveable.Skeleton.LinearizedBones)
                {
                    keyFrame.Rotations.Add(Vector3.Zero);
                    keyFrame.RotationsMatrices.Add(Matrix4x4.CreateFromYawPitchRoll(0, 0, 0));
                    keyFrame.Translations.Add(bone.Translation);
                    keyFrame.TranslationsMatrices.Add(Matrix4x4.CreateTranslation(bone.Translation));
                }
             
                _selectedNode.DirectXAnimation.KeyFrames.Insert(index, keyFrame);
            }
        }

        private void tbName_Validated(object sender, EventArgs e)
        {
            if (_selectedNode != null && tbName.Text.Trim() != "")
            {
                _selectedNode.WadAnimation.Name = tbName.Text.Trim();
                treeAnimations.SelectedNodes[0].Text = treeAnimations.SelectedNodes[0].VisibleIndex + ": " + _selectedNode.WadAnimation.Name;
            }
        }

        private void tbFramerate_Validated(object sender, EventArgs e)
        {
            byte result = 0;
            if (!byte.TryParse(tbFramerate.Text, out result))
                return;

            if (_selectedNode != null)
                _selectedNode.WadAnimation.FrameRate = result;
        }

        private void tbNextAnimation_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbNextAnimation.Text, out result))
                return;

            if (_selectedNode != null)
                _selectedNode.WadAnimation.NextAnimation = result;
        }

        private void tbNextFrame_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbNextFrame.Text, out result))
                return;

            if (_selectedNode != null)
                _selectedNode.WadAnimation.NextFrame = result;
        }

        private void tbStateId_Validated(object sender, EventArgs e)
        {
            ushort result = 0;
            if (!ushort.TryParse(tbStateId.Text, out result))
                return;

            if (_selectedNode != null)
                _selectedNode.WadAnimation.StateId = result;
        }

        private void butDeleteFrame_Click(object sender, EventArgs e)
        {
            DeleteFrame();
        }

        private void tbCollisionBoxMinX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(result, bb.Minimum.Y, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void tbCollisionBoxMinY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, result, bb.Minimum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void tbCollisionBoxMinZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMinZ.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Minimum = new Vector3(bb.Minimum.X, bb.Minimum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void tbCollisionBoxMaxX_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxX.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(result, bb.Maximum.Y, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void tbCollisionBoxMaxY_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxY.Text, out result))
                return;

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, result, bb.Maximum.Z);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void tbCollisionBoxMaxZ_Validated(object sender, EventArgs e)
        {
            short result = 0;
            if (!short.TryParse(tbCollisionBoxMaxZ.Text, out result))
                return; 

            if (_selectedNode != null && _selectedNode.DirectXAnimation.KeyFrames.Count != 0)
            {
                var bb = _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox;
                bb.Maximum = new Vector3(bb.Maximum.X, bb.Maximum.Y, result);
                _selectedNode.DirectXAnimation.KeyFrames[panelRendering.CurrentKeyFrame].BoundingBox = bb;
                panelRendering.Invalidate();
            }
        }

        private void deleteFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteFrame();
        }

        private void comboSkeleton_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboSkeleton.SelectedIndex < 1)
                return;
            panelRendering.SelectedMesh = panelRendering.Model.Meshes[comboSkeleton.SelectedIndex - 1];
            panelRendering.Invalidate();
        }
    }
}
