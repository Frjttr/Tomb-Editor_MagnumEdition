﻿using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;

namespace TombEditor.Forms
{
    public partial class FormCamera : DarkForm
    {
        private readonly CameraInstance _instance;

        public FormCamera(CameraInstance instance)
        {
            InitializeComponent();

            _instance = instance;
            comboCameraMode.SelectedIndex = (int)_instance.CameraMode;
            nudMoveTimer.Value = _instance.MoveTimer;
            ckGlideOut.Checked = _instance.GlideOut;

            if (_instance.Room.Level.Settings.GameVersion != TRVersion.Game.TR5)
                comboCameraMode.Items.RemoveAt((int)CameraInstanceMode.Sniper);
            if (_instance.Room.Level.Settings.GameVersion <= TRVersion.Game.TR3)
                comboCameraMode.Items.RemoveAt((int)CameraInstanceMode.Locked);
        }

        private void butOk_Click(object sender, EventArgs e)
        {
            _instance.CameraMode = (CameraInstanceMode)comboCameraMode.SelectedIndex;
            _instance.MoveTimer = (byte)nudMoveTimer.Value;
            _instance.GlideOut = ckGlideOut.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void comboCameraMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            nudMoveTimer.Enabled = (comboCameraMode.SelectedIndex != (int)CameraInstanceMode.Sniper);
            ckGlideOut.Enabled = nudMoveTimer.Enabled;
        }
    }
}
