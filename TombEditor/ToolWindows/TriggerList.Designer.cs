﻿namespace TombEditor.ToolWindows
{
    partial class TriggerList
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.panelTriggerTools = new System.Windows.Forms.Panel();
            this.butAddTrigger = new DarkUI.Controls.DarkButton();
            this.butEditTrigger = new DarkUI.Controls.DarkButton();
            this.butDeleteTrigger = new DarkUI.Controls.DarkButton();
            this.panelTriggerList = new System.Windows.Forms.Panel();
            this.lstTriggers = new DarkUI.Controls.DarkListBox(this.components);
            this.panelTriggerTools.SuspendLayout();
            this.panelTriggerList.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelTriggerTools
            // 
            this.panelTriggerTools.Controls.Add(this.butAddTrigger);
            this.panelTriggerTools.Controls.Add(this.butEditTrigger);
            this.panelTriggerTools.Controls.Add(this.butDeleteTrigger);
            this.panelTriggerTools.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panelTriggerTools.Location = new System.Drawing.Point(0, 305);
            this.panelTriggerTools.Name = "panelTriggerTools";
            this.panelTriggerTools.Size = new System.Drawing.Size(279, 31);
            this.panelTriggerTools.TabIndex = 57;
            // 
            // butAddTrigger
            // 
            this.butAddTrigger.Image = global::TombEditor.Properties.Resources.plus_math_16;
            this.butAddTrigger.Location = new System.Drawing.Point(3, 3);
            this.butAddTrigger.Name = "butAddTrigger";
            this.butAddTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butAddTrigger.Size = new System.Drawing.Size(24, 24);
            this.butAddTrigger.TabIndex = 56;
            this.butAddTrigger.Click += new System.EventHandler(this.butAddTrigger_Click);
            // 
            // butEditTrigger
            // 
            this.butEditTrigger.Image = global::TombEditor.Properties.Resources.edit_16;
            this.butEditTrigger.Location = new System.Drawing.Point(33, 3);
            this.butEditTrigger.Name = "butEditTrigger";
            this.butEditTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butEditTrigger.Size = new System.Drawing.Size(24, 24);
            this.butEditTrigger.TabIndex = 53;
            this.butEditTrigger.Click += new System.EventHandler(this.butEditTrigger_Click);
            // 
            // butDeleteTrigger
            // 
            this.butDeleteTrigger.Image = global::TombEditor.Properties.Resources.trash_16;
            this.butDeleteTrigger.Location = new System.Drawing.Point(63, 4);
            this.butDeleteTrigger.Name = "butDeleteTrigger";
            this.butDeleteTrigger.Padding = new System.Windows.Forms.Padding(5);
            this.butDeleteTrigger.Size = new System.Drawing.Size(24, 24);
            this.butDeleteTrigger.TabIndex = 52;
            this.butDeleteTrigger.Click += new System.EventHandler(this.butDeleteTrigger_Click);
            // 
            // panelTriggerList
            // 
            this.panelTriggerList.Controls.Add(this.lstTriggers);
            this.panelTriggerList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelTriggerList.Location = new System.Drawing.Point(0, 25);
            this.panelTriggerList.Name = "panelTriggerList";
            this.panelTriggerList.Padding = new System.Windows.Forms.Padding(2);
            this.panelTriggerList.Size = new System.Drawing.Size(279, 280);
            this.panelTriggerList.TabIndex = 58;
            // 
            // lstTriggers
            // 
            this.lstTriggers.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.lstTriggers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTriggers.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.lstTriggers.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstTriggers.ForeColor = System.Drawing.Color.White;
            this.lstTriggers.FormattingEnabled = true;
            this.lstTriggers.IntegralHeight = false;
            this.lstTriggers.ItemHeight = 18;
            this.lstTriggers.Location = new System.Drawing.Point(2, 2);
            this.lstTriggers.Name = "lstTriggers";
            this.lstTriggers.Size = new System.Drawing.Size(275, 276);
            this.lstTriggers.TabIndex = 56;
            // 
            // TriggerList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.Controls.Add(this.panelTriggerList);
            this.Controls.Add(this.panelTriggerTools);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Left;
            this.DockText = "Triggers";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.Name = "TriggerList";
            this.SerializationKey = "TriggerList";
            this.Size = new System.Drawing.Size(279, 336);
            this.panelTriggerTools.ResumeLayout(false);
            this.panelTriggerList.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Panel panelTriggerTools;
        private DarkUI.Controls.DarkButton butAddTrigger;
        private DarkUI.Controls.DarkButton butEditTrigger;
        private DarkUI.Controls.DarkButton butDeleteTrigger;
        private System.Windows.Forms.Panel panelTriggerList;
        private DarkUI.Controls.DarkListBox lstTriggers;
    }
}
