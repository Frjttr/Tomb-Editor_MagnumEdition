﻿using System;
using System.Windows.Forms;
using DarkUI.Forms;
using TombLib.LevelData;
using TombLib;

namespace TombEditor.Forms
{
    public partial class FormSink : DarkForm
    {
        public bool IsNew { get; set; }
        private readonly SinkInstance _sink;

        public FormSink(SinkInstance sink)
        {
            _sink = sink;
            InitializeComponent();
        }

        private void butCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void FormSink_Load(object sender, EventArgs e)
        {
            nudStrength.Value = MathC.Clamp(_sink.Strength + 1, 1, 32);
        }

        private void butOK_Click(object sender, EventArgs e)
        {
            _sink.Strength = (short)(nudStrength.Value - 1);

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
