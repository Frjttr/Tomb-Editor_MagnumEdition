﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using TombLib.Graphics;
using TombLib.Wad;
using TombLib.Wad.Catalog;

namespace WadTool
{
    public partial class FormStateChangesEditor : DarkUI.Forms.DarkForm
    {
        private readonly AnimationEditor _editor;
        private AnimationNode _animation;
        private bool _createdNew = false;

        private class WadStateChangeRow
        {
            public string StateName { get; set; }
            public ushort StateId { get; set; }
            public ushort LowFrame { get; set; }
            public ushort HighFrame { get; set; }
            public ushort NextAnimation { get; set; }
            public ushort NextFrame { get; set; }

            public WadStateChangeRow(string stateName, ushort stateId, ushort lowFrame, ushort highFrame, ushort nextAnimation, ushort nextFrame)
            {
                StateName = stateName;
                StateId = stateId;
                LowFrame = lowFrame;
                HighFrame = highFrame;
                NextAnimation = nextAnimation;
                NextFrame = nextFrame;
            }

            public WadStateChangeRow() { }
        }

        public List<WadStateChange> StateChanges { get; private set; }

        public FormStateChangesEditor(AnimationEditor editor, AnimationNode animation, WadStateChange newStateChange = null)
        {
            InitializeComponent();

            _editor = editor;

            dgvControls.CreateNewRow =()=> new WadStateChangeRow() { StateName = TrCatalog.GetStateName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, 0) };
            dgvControls.DataGridView = dgvStateChanges;
            dgvControls.Enabled = true;

            Initialize(animation, newStateChange);
            _editor.Tool.EditorEventRaised += Tool_EditorEventRaised;
        }

        private void Initialize(AnimationNode animation, WadStateChange newStateChange)
        {
            _animation = animation;

            lblStateChangeAnnouncement.Visible = false;
            dgvStateChanges.Rows.Clear();

            var rows = new List<WadStateChangeRow>();
            foreach (var sc in _animation.WadAnimation.StateChanges)
                foreach (var d in sc.Dispatches)
                    rows.Add(new WadStateChangeRow(TrCatalog.GetStateName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, sc.StateId), sc.StateId, d.InFrame, d.OutFrame, d.NextAnimation, d.NextFrame));

            if (newStateChange != null && newStateChange.Dispatches.Count == 1)
            {
                rows.Add(new WadStateChangeRow(TrCatalog.GetStateName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, newStateChange.StateId),
                                               newStateChange.StateId,
                                               newStateChange.Dispatches[0].InFrame,
                                               newStateChange.Dispatches[0].OutFrame,
                                               newStateChange.Dispatches[0].NextAnimation,
                                               newStateChange.Dispatches[0].NextFrame));
                _createdNew = true;
            }

            dgvStateChanges.DataSource = new BindingList<WadStateChangeRow>(new List<WadStateChangeRow>(rows));
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.Tool.EditorEventRaised -= Tool_EditorEventRaised;
            base.Dispose(disposing);
        }

        private void Tool_EditorEventRaised(IEditorEvent obj)
        {
            if (obj is WadToolClass.AnimationEditorCurrentAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorCurrentAnimationChangedEvent;
                if (e != null && e.Animation != _animation)
                    Initialize(e.Animation, null);
            }

            if (obj is WadToolClass.AnimationEditorAnimationChangedEvent)
            {
                var e = obj as WadToolClass.AnimationEditorAnimationChangedEvent;
                if (e != null && e.Animation == _animation)
                    Initialize(e.Animation, null);
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if(_createdNew)
            {
                dgvStateChanges.ClearSelection();
                dgvStateChanges.Rows[dgvStateChanges.Rows.Count - 1].Selected = true;
                dgvStateChanges.FirstDisplayedScrollingRowIndex = dgvStateChanges.SelectedRows[0].Index;
            }
        }

        private void ChangeState()
        {
            if (dgvStateChanges.SelectedRows.Count > 0)
            {
                var item = ((IEnumerable<WadStateChangeRow>)dgvStateChanges.DataSource).ElementAt(dgvStateChanges.SelectedRows[0].Index);
                _editor.Tool.ChangeState(item.NextAnimation, item.NextFrame, item.LowFrame, item.HighFrame);

                lblStateChangeAnnouncement.Visible = true;
                lblStateChangeAnnouncement.Text = "Pending state change to anim #" + item.NextAnimation;
            }
        }

        private void dgvStateChanges_CellFormattingSafe(object sender, DarkUI.Controls.DarkDataGridViewSafeCellFormattingEventArgs e)
        {
            if (!(e.Row.DataBoundItem is WadStateChangeRow))
                return;

            if (e.ColumnIndex == 0)
                e.CellStyle.ForeColor = Color.Gray;
            else if (e.ColumnIndex == 4)
            {
                var item = (WadStateChangeRow)e.Row.DataBoundItem;
                var cell = dgvStateChanges.Rows[e.RowIndex].Cells[e.ColumnIndex];
                cell.ToolTipText = TrCatalog.GetAnimationName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, item.NextAnimation);
            }
        }

        private void btOk_Click(object sender, EventArgs e)
        {
            // Update data
            StateChanges = new List<WadStateChange>();
            var tempDictionary = new Dictionary<int, WadStateChange>();
            foreach (var row in (IEnumerable<WadStateChangeRow>)dgvStateChanges.DataSource)
            {
                if (!tempDictionary.ContainsKey(row.StateId))
                    tempDictionary.Add(row.StateId, new WadStateChange());
                var sc = tempDictionary[row.StateId];
                sc.StateId = row.StateId;
                sc.Dispatches.Add(new WadAnimDispatch(row.LowFrame, row.HighFrame, row.NextAnimation, row.NextFrame));
                tempDictionary[row.StateId] = sc;
            }
            StateChanges.AddRange(tempDictionary.Values.ToList());

            // Undo
            _editor.Tool.UndoManager.PushAnimationChanged(_editor, _animation);

            // Add the new state changes
            _animation.WadAnimation.StateChanges.Clear();
            _animation.WadAnimation.StateChanges.AddRange(StateChanges);

            // Update state in parent window
            _editor.Tool.AnimationEditorAnimationChanged(_animation, false);

            // Close
            Close();
        }

        private void dgvStateChanges_CellMouseDoubleClick(object sender, System.Windows.Forms.DataGridViewCellMouseEventArgs e) => ChangeState();

        private void dgvStateChanges_CellValidating(object sender, System.Windows.Forms.DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0) return;

            try
            {
                var cell = dgvStateChanges.Rows[e.RowIndex].Cells[e.ColumnIndex];
                var name = dgvStateChanges.Columns[e.ColumnIndex].Name;

                Int16 parsedValue = 0;
                if (e.FormattedValue == null || !Int16.TryParse(e.FormattedValue.ToString(), out parsedValue))
                {
                    if (!Int16.TryParse(cell.Value.ToString(), out parsedValue))
                        parsedValue = 0;
                }

                var limit = Int16.MaxValue;

                if (name == columnNextAnimation.Name)
                {
                    limit = (Int16)(_editor.Animations.Count - 1);
                }
                else if (name == columnNextFrame.Name)
                {
                    Int16 limitNew = 0;
                    if (Int16.TryParse(dgvStateChanges.Rows[e.RowIndex].Cells[3].Value.ToString(), out limitNew))
                        limit = (Int16)(_editor.RealNumberOfFrames(limitNew));
                }
                else if (name == columnLowFrame.Name)
                {
                    Int16 limitNew = 0;
                    if (Int16.TryParse(dgvStateChanges.Rows[e.RowIndex].Cells[2].Value.ToString(), out limitNew))
                        limit = limitNew;
                }
                else if (name == columnHighFrame.Name)
                {
                    limit = (Int16)(_editor.RealNumberOfFrames());
                }

                if (parsedValue > limit)
                    cell.Value = limit;
                else if (parsedValue < 0)
                    cell.Value = (Int16)0;
                else
                    cell.Value = parsedValue;

                if (name == columnStateId.Name)
                    dgvStateChanges.Rows[e.RowIndex].Cells[0].Value = TrCatalog.GetStateName(_editor.Tool.DestinationWad.GameVersion, _editor.Moveable.Id.TypeId, (uint)parsedValue);
            }
            catch (Exception ex) { }
        }

        private void btCancel_Click(object sender, EventArgs e) => Close();
        private void butPlayStateChange_Click(object sender, EventArgs e) => ChangeState();
    }
}
