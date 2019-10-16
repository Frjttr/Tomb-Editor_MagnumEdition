﻿namespace TombEditor.ToolWindows
{
    partial class Lighting
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
            this.cbLightIsDynamicallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightIsStaticallyUsed = new DarkUI.Controls.DarkCheckBox();
            this.cbLightIsObstructedByRoomGeometry = new DarkUI.Controls.DarkCheckBox();
            this.cbLightEnabled = new DarkUI.Controls.DarkCheckBox();
            this.panelLightColor = new DarkUI.Controls.DarkPanel();
            this.darkLabel12 = new DarkUI.Controls.DarkLabel();
            this.darkLabel13 = new DarkUI.Controls.DarkLabel();
            this.darkLabel11 = new DarkUI.Controls.DarkLabel();
            this.darkLabel9 = new DarkUI.Controls.DarkLabel();
            this.darkLabel10 = new DarkUI.Controls.DarkLabel();
            this.darkLabel8 = new DarkUI.Controls.DarkLabel();
            this.darkLabel7 = new DarkUI.Controls.DarkLabel();
            this.darkLabel6 = new DarkUI.Controls.DarkLabel();
            this.butAddFogBulb = new DarkUI.Controls.DarkButton();
            this.butAddEffectLight = new DarkUI.Controls.DarkButton();
            this.butAddSpotLight = new DarkUI.Controls.DarkButton();
            this.butAddSun = new DarkUI.Controls.DarkButton();
            this.butAddShadow = new DarkUI.Controls.DarkButton();
            this.butAddPointLight = new DarkUI.Controls.DarkButton();
            this.darkLabel5 = new DarkUI.Controls.DarkLabel();
            this.numIntensity = new DarkUI.Controls.DarkNumericUpDown();
            this.numInnerRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numOuterRange = new DarkUI.Controls.DarkNumericUpDown();
            this.numInnerAngle = new DarkUI.Controls.DarkNumericUpDown();
            this.numOuterAngle = new DarkUI.Controls.DarkNumericUpDown();
            this.numDirectionX = new DarkUI.Controls.DarkNumericUpDown();
            this.numDirectionY = new DarkUI.Controls.DarkNumericUpDown();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.cbLightIsUsedForImportedGeometry = new DarkUI.Controls.DarkCheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterRange)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterAngle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionY)).BeginInit();
            this.SuspendLayout();
            // 
            // cbLightIsDynamicallyUsed
            // 
            this.cbLightIsDynamicallyUsed.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLightIsDynamicallyUsed.Enabled = false;
            this.cbLightIsDynamicallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsDynamicallyUsed.Location = new System.Drawing.Point(360, 85);
            this.cbLightIsDynamicallyUsed.Name = "cbLightIsDynamicallyUsed";
            this.cbLightIsDynamicallyUsed.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsDynamicallyUsed.TabIndex = 17;
            this.cbLightIsDynamicallyUsed.Text = "Dynamic";
            this.toolTip.SetToolTip(this.cbLightIsDynamicallyUsed, "Use light for moveables ingame");
            this.cbLightIsDynamicallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsDynamicallyUsed_CheckedChanged);
            // 
            // cbLightIsStaticallyUsed
            // 
            this.cbLightIsStaticallyUsed.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLightIsStaticallyUsed.Enabled = false;
            this.cbLightIsStaticallyUsed.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsStaticallyUsed.Location = new System.Drawing.Point(360, 66);
            this.cbLightIsStaticallyUsed.Name = "cbLightIsStaticallyUsed";
            this.cbLightIsStaticallyUsed.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsStaticallyUsed.TabIndex = 16;
            this.cbLightIsStaticallyUsed.Text = "Static";
            this.toolTip.SetToolTip(this.cbLightIsStaticallyUsed, "Use light for room geometry lighting");
            this.cbLightIsStaticallyUsed.CheckedChanged += new System.EventHandler(this.cbLightIsStaticallyUsed_CheckedChanged);
            // 
            // cbLightIsObstructedByRoomGeometry
            // 
            this.cbLightIsObstructedByRoomGeometry.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLightIsObstructedByRoomGeometry.Enabled = false;
            this.cbLightIsObstructedByRoomGeometry.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsObstructedByRoomGeometry.Location = new System.Drawing.Point(360, 47);
            this.cbLightIsObstructedByRoomGeometry.Name = "cbLightIsObstructedByRoomGeometry";
            this.cbLightIsObstructedByRoomGeometry.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsObstructedByRoomGeometry.TabIndex = 15;
            this.cbLightIsObstructedByRoomGeometry.Text = "Obstruct";
            this.toolTip.SetToolTip(this.cbLightIsObstructedByRoomGeometry, "Determines whether the effect of this light is obstructed by room geometry.");
            this.cbLightIsObstructedByRoomGeometry.CheckedChanged += new System.EventHandler(this.cbLightIsObstructedByRoomGeometry_CheckedChanged);
            // 
            // cbLightEnabled
            // 
            this.cbLightEnabled.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLightEnabled.Enabled = false;
            this.cbLightEnabled.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightEnabled.Location = new System.Drawing.Point(360, 28);
            this.cbLightEnabled.Name = "cbLightEnabled";
            this.cbLightEnabled.Size = new System.Drawing.Size(70, 22);
            this.cbLightEnabled.TabIndex = 14;
            this.cbLightEnabled.Text = "Enabled";
            this.toolTip.SetToolTip(this.cbLightEnabled, "Light is enabled");
            this.cbLightEnabled.CheckedChanged += new System.EventHandler(this.cbLightEnabled_CheckedChanged);
            // 
            // panelLightColor
            // 
            this.panelLightColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelLightColor.Enabled = false;
            this.panelLightColor.Location = new System.Drawing.Point(198, 28);
            this.panelLightColor.Name = "panelLightColor";
            this.panelLightColor.Size = new System.Drawing.Size(60, 22);
            this.panelLightColor.TabIndex = 6;
            this.toolTip.SetToolTip(this.panelLightColor, "Light color");
            this.panelLightColor.Click += new System.EventHandler(this.panelLightColor_Click);
            // 
            // darkLabel12
            // 
            this.darkLabel12.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel12.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel12.Location = new System.Drawing.Point(260, 77);
            this.darkLabel12.Name = "darkLabel12";
            this.darkLabel12.Size = new System.Drawing.Size(38, 22);
            this.darkLabel12.TabIndex = 80;
            this.darkLabel12.Text = "Dir Y";
            this.darkLabel12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel13
            // 
            this.darkLabel13.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel13.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel13.Location = new System.Drawing.Point(260, 102);
            this.darkLabel13.Name = "darkLabel13";
            this.darkLabel13.Size = new System.Drawing.Size(38, 22);
            this.darkLabel13.TabIndex = 79;
            this.darkLabel13.Text = "Dir X";
            this.darkLabel13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel11
            // 
            this.darkLabel11.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel11.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel11.Location = new System.Drawing.Point(147, 52);
            this.darkLabel11.Name = "darkLabel11";
            this.darkLabel11.Size = new System.Drawing.Size(51, 22);
            this.darkLabel11.TabIndex = 78;
            this.darkLabel11.Text = "Intensity";
            this.darkLabel11.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel9
            // 
            this.darkLabel9.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel9.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel9.Location = new System.Drawing.Point(260, 52);
            this.darkLabel9.Name = "darkLabel9";
            this.darkLabel9.Size = new System.Drawing.Size(38, 22);
            this.darkLabel9.TabIndex = 77;
            this.darkLabel9.Text = "Out α";
            this.darkLabel9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel10
            // 
            this.darkLabel10.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel10.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel10.Location = new System.Drawing.Point(260, 27);
            this.darkLabel10.Name = "darkLabel10";
            this.darkLabel10.Size = new System.Drawing.Size(38, 22);
            this.darkLabel10.TabIndex = 73;
            this.darkLabel10.Text = "In α";
            this.darkLabel10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel8
            // 
            this.darkLabel8.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel8.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel8.Location = new System.Drawing.Point(147, 102);
            this.darkLabel8.Name = "darkLabel8";
            this.darkLabel8.Size = new System.Drawing.Size(51, 22);
            this.darkLabel8.TabIndex = 71;
            this.darkLabel8.Text = "Out d";
            this.darkLabel8.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel7
            // 
            this.darkLabel7.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel7.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel7.Location = new System.Drawing.Point(147, 77);
            this.darkLabel7.Name = "darkLabel7";
            this.darkLabel7.Size = new System.Drawing.Size(51, 22);
            this.darkLabel7.TabIndex = 70;
            this.darkLabel7.Text = "In d";
            this.darkLabel7.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // darkLabel6
            // 
            this.darkLabel6.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel6.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel6.Location = new System.Drawing.Point(147, 27);
            this.darkLabel6.Name = "darkLabel6";
            this.darkLabel6.Size = new System.Drawing.Size(51, 22);
            this.darkLabel6.TabIndex = 68;
            this.darkLabel6.Text = "Color";
            this.darkLabel6.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // butAddFogBulb
            // 
            this.butAddFogBulb.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddFogBulb.Image = global::TombEditor.Properties.Resources.objects_Fog_16;
            this.butAddFogBulb.Location = new System.Drawing.Point(77, 102);
            this.butAddFogBulb.Name = "butAddFogBulb";
            this.butAddFogBulb.Size = new System.Drawing.Size(68, 23);
            this.butAddFogBulb.TabIndex = 5;
            this.butAddFogBulb.Tag = "AddFogBulb";
            this.butAddFogBulb.Text = "Fog";
            this.butAddFogBulb.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butAddEffectLight
            // 
            this.butAddEffectLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddEffectLight.Image = global::TombEditor.Properties.Resources.objects_Effect_16;
            this.butAddEffectLight.Location = new System.Drawing.Point(77, 73);
            this.butAddEffectLight.Name = "butAddEffectLight";
            this.butAddEffectLight.Size = new System.Drawing.Size(68, 23);
            this.butAddEffectLight.TabIndex = 4;
            this.butAddEffectLight.Tag = "AddEffectLight";
            this.butAddEffectLight.Text = "Effect";
            this.butAddEffectLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butAddSpotLight
            // 
            this.butAddSpotLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddSpotLight.Image = global::TombEditor.Properties.Resources.objects_Spotlight_16;
            this.butAddSpotLight.Location = new System.Drawing.Point(77, 44);
            this.butAddSpotLight.Name = "butAddSpotLight";
            this.butAddSpotLight.Size = new System.Drawing.Size(68, 23);
            this.butAddSpotLight.TabIndex = 3;
            this.butAddSpotLight.Tag = "AddSpotLight";
            this.butAddSpotLight.Text = "Spot";
            this.butAddSpotLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butAddSun
            // 
            this.butAddSun.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddSun.Image = global::TombEditor.Properties.Resources.objects_sun_16;
            this.butAddSun.Location = new System.Drawing.Point(3, 102);
            this.butAddSun.Name = "butAddSun";
            this.butAddSun.Size = new System.Drawing.Size(68, 23);
            this.butAddSun.TabIndex = 2;
            this.butAddSun.Tag = "AddSunLight";
            this.butAddSun.Text = "Sun";
            this.butAddSun.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butAddShadow
            // 
            this.butAddShadow.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddShadow.Image = global::TombEditor.Properties.Resources.objects_Shadow_16;
            this.butAddShadow.Location = new System.Drawing.Point(3, 73);
            this.butAddShadow.Name = "butAddShadow";
            this.butAddShadow.Size = new System.Drawing.Size(68, 23);
            this.butAddShadow.TabIndex = 1;
            this.butAddShadow.Tag = "AddShadow";
            this.butAddShadow.Text = "Shadow";
            this.butAddShadow.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // butAddPointLight
            // 
            this.butAddPointLight.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.butAddPointLight.Image = global::TombEditor.Properties.Resources.objects_LightPoint_16;
            this.butAddPointLight.Location = new System.Drawing.Point(3, 44);
            this.butAddPointLight.Name = "butAddPointLight";
            this.butAddPointLight.Size = new System.Drawing.Size(68, 23);
            this.butAddPointLight.TabIndex = 0;
            this.butAddPointLight.Tag = "AddPointLight";
            this.butAddPointLight.Text = "Point";
            this.butAddPointLight.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            // 
            // darkLabel5
            // 
            this.darkLabel5.AutoSize = true;
            this.darkLabel5.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.darkLabel5.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel5.Location = new System.Drawing.Point(1, 27);
            this.darkLabel5.Name = "darkLabel5";
            this.darkLabel5.Size = new System.Drawing.Size(55, 13);
            this.darkLabel5.TabIndex = 61;
            this.darkLabel5.Text = "Add light";
            // 
            // numIntensity
            // 
            this.numIntensity.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numIntensity.DecimalPlaces = 2;
            this.numIntensity.Enabled = false;
            this.numIntensity.ForeColor = System.Drawing.Color.Gainsboro;
            this.numIntensity.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numIntensity.IncrementAlternate = new decimal(new int[] {
            12,
            0,
            0,
            131072});
            this.numIntensity.Location = new System.Drawing.Point(198, 53);
            this.numIntensity.Maximum = new decimal(new int[] {
            128,
            0,
            0,
            0});
            this.numIntensity.Minimum = new decimal(new int[] {
            128,
            0,
            0,
            -2147483648});
            this.numIntensity.LoopValues = false;
            this.numIntensity.Name = "numIntensity";
            this.numIntensity.Size = new System.Drawing.Size(60, 22);
            this.numIntensity.TabIndex = 7;
            this.numIntensity.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numIntensity, "Light intensity");
            this.numIntensity.ValueChanged += new System.EventHandler(this.numIntensity_ValueChanged);
            // 
            // numInnerRange
            // 
            this.numInnerRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numInnerRange.DecimalPlaces = 2;
            this.numInnerRange.Enabled = false;
            this.numInnerRange.ForeColor = System.Drawing.Color.Gainsboro;
            this.numInnerRange.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numInnerRange.IncrementAlternate = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numInnerRange.Location = new System.Drawing.Point(198, 78);
            this.numInnerRange.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numInnerRange.LoopValues = false;
            this.numInnerRange.Name = "numInnerRange";
            this.numInnerRange.Size = new System.Drawing.Size(60, 22);
            this.numInnerRange.TabIndex = 8;
            this.numInnerRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numInnerRange, "Inner radius or distance");
            this.numInnerRange.ValueChanged += new System.EventHandler(this.numInnerRange_ValueChanged);
            // 
            // numOuterRange
            // 
            this.numOuterRange.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numOuterRange.DecimalPlaces = 2;
            this.numOuterRange.Enabled = false;
            this.numOuterRange.ForeColor = System.Drawing.Color.Gainsboro;
            this.numOuterRange.Increment = new decimal(new int[] {
            3,
            0,
            0,
            131072});
            this.numOuterRange.IncrementAlternate = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numOuterRange.Location = new System.Drawing.Point(198, 103);
            this.numOuterRange.Maximum = new decimal(new int[] {
            256,
            0,
            0,
            0});
            this.numOuterRange.LoopValues = false;
            this.numOuterRange.Name = "numOuterRange";
            this.numOuterRange.Size = new System.Drawing.Size(60, 22);
            this.numOuterRange.TabIndex = 9;
            this.numOuterRange.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numOuterRange, "Outer radius or distance");
            this.numOuterRange.ValueChanged += new System.EventHandler(this.numOuterRange_ValueChanged);
            // 
            // numInnerAngle
            // 
            this.numInnerAngle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numInnerAngle.DecimalPlaces = 2;
            this.numInnerAngle.Enabled = false;
            this.numInnerAngle.ForeColor = System.Drawing.Color.Gainsboro;
            this.numInnerAngle.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numInnerAngle.Location = new System.Drawing.Point(298, 28);
            this.numInnerAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numInnerAngle.LoopValues = false;
            this.numInnerAngle.Name = "numInnerAngle";
            this.numInnerAngle.Size = new System.Drawing.Size(60, 22);
            this.numInnerAngle.TabIndex = 10;
            this.numInnerAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numInnerAngle, "Inner cone angle");
            this.numInnerAngle.ValueChanged += new System.EventHandler(this.numInnerAngle_ValueChanged);
            // 
            // numOuterAngle
            // 
            this.numOuterAngle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numOuterAngle.DecimalPlaces = 2;
            this.numOuterAngle.Enabled = false;
            this.numOuterAngle.ForeColor = System.Drawing.Color.Gainsboro;
            this.numOuterAngle.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numOuterAngle.Location = new System.Drawing.Point(298, 53);
            this.numOuterAngle.Maximum = new decimal(new int[] {
            180,
            0,
            0,
            0});
            this.numOuterAngle.LoopValues = false;
            this.numOuterAngle.Name = "numOuterAngle";
            this.numOuterAngle.Size = new System.Drawing.Size(60, 22);
            this.numOuterAngle.TabIndex = 11;
            this.numOuterAngle.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numOuterAngle, "Outer cone angle");
            this.numOuterAngle.ValueChanged += new System.EventHandler(this.numOuterAngle_ValueChanged);
            // 
            // numDirectionX
            // 
            this.numDirectionX.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numDirectionX.DecimalPlaces = 2;
            this.numDirectionX.Enabled = false;
            this.numDirectionX.ForeColor = System.Drawing.Color.Gainsboro;
            this.numDirectionX.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDirectionX.Location = new System.Drawing.Point(298, 103);
            this.numDirectionX.Maximum = new decimal(new int[] {
            90,
            0,
            0,
            0});
            this.numDirectionX.Minimum = new decimal(new int[] {
            90,
            0,
            0,
            -2147483648});
            this.numDirectionX.LoopValues = false;
            this.numDirectionX.Name = "numDirectionX";
            this.numDirectionX.Size = new System.Drawing.Size(60, 22);
            this.numDirectionX.TabIndex = 13;
            this.numDirectionX.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numDirectionX, "Angle around the X axis (vertical rotation)");
            this.numDirectionX.ValueChanged += new System.EventHandler(this.numDirectionX_ValueChanged);
            // 
            // numDirectionY
            // 
            this.numDirectionY.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.numDirectionY.DecimalPlaces = 2;
            this.numDirectionY.Enabled = false;
            this.numDirectionY.ForeColor = System.Drawing.Color.Gainsboro;
            this.numDirectionY.IncrementAlternate = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numDirectionY.Location = new System.Drawing.Point(298, 78);
            this.numDirectionY.Maximum = new decimal(new int[] {
            720,
            0,
            0,
            0});
            this.numDirectionY.Minimum = new decimal(new int[] {
            360,
            0,
            0,
            -2147483648});
            this.numDirectionY.LoopValues = false;
            this.numDirectionY.Name = "numDirectionY";
            this.numDirectionY.Size = new System.Drawing.Size(60, 22);
            this.numDirectionY.TabIndex = 12;
            this.numDirectionY.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.toolTip.SetToolTip(this.numDirectionY, "Angle around the Y axis (horizontal rotation)");
            this.numDirectionY.ValueChanged += new System.EventHandler(this.numDirectionY_ValueChanged);
            // 
            // toolTip
            // 
            this.toolTip.AutoPopDelay = 5000;
            this.toolTip.InitialDelay = 500;
            this.toolTip.ReshowDelay = 100;
            // 
            // cbLightIsUsedForImportedGeometry
            // 
            this.cbLightIsUsedForImportedGeometry.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.cbLightIsUsedForImportedGeometry.Enabled = false;
            this.cbLightIsUsedForImportedGeometry.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cbLightIsUsedForImportedGeometry.Location = new System.Drawing.Point(360, 104);
            this.cbLightIsUsedForImportedGeometry.Name = "cbLightIsUsedForImportedGeometry";
            this.cbLightIsUsedForImportedGeometry.Size = new System.Drawing.Size(70, 22);
            this.cbLightIsUsedForImportedGeometry.TabIndex = 18;
            this.cbLightIsUsedForImportedGeometry.Text = "Imported";
            this.toolTip.SetToolTip(this.cbLightIsUsedForImportedGeometry, "Use light for imported geometry");
            this.cbLightIsUsedForImportedGeometry.CheckedChanged += new System.EventHandler(this.cbLightIsUsedForImportedGeometry_CheckedChanged);
            // 
            // Lighting
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.cbLightIsUsedForImportedGeometry);
            this.Controls.Add(this.numDirectionY);
            this.Controls.Add(this.numDirectionX);
            this.Controls.Add(this.numOuterAngle);
            this.Controls.Add(this.numInnerAngle);
            this.Controls.Add(this.numOuterRange);
            this.Controls.Add(this.numInnerRange);
            this.Controls.Add(this.numIntensity);
            this.Controls.Add(this.cbLightIsDynamicallyUsed);
            this.Controls.Add(this.cbLightIsStaticallyUsed);
            this.Controls.Add(this.cbLightIsObstructedByRoomGeometry);
            this.Controls.Add(this.cbLightEnabled);
            this.Controls.Add(this.panelLightColor);
            this.Controls.Add(this.darkLabel12);
            this.Controls.Add(this.darkLabel13);
            this.Controls.Add(this.darkLabel11);
            this.Controls.Add(this.darkLabel9);
            this.Controls.Add(this.darkLabel10);
            this.Controls.Add(this.darkLabel8);
            this.Controls.Add(this.darkLabel7);
            this.Controls.Add(this.darkLabel6);
            this.Controls.Add(this.butAddFogBulb);
            this.Controls.Add(this.butAddEffectLight);
            this.Controls.Add(this.butAddSpotLight);
            this.Controls.Add(this.butAddSun);
            this.Controls.Add(this.butAddShadow);
            this.Controls.Add(this.butAddPointLight);
            this.Controls.Add(this.darkLabel5);
            this.DefaultDockArea = DarkUI.Docking.DarkDockArea.Bottom;
            this.DockText = "Lighting";
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.MinimumSize = new System.Drawing.Size(432, 128);
            this.Name = "Lighting";
            this.SerializationKey = "Lighting";
            this.Size = new System.Drawing.Size(432, 128);
            ((System.ComponentModel.ISupportInitialize)(this.numIntensity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterRange)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numInnerAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numOuterAngle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDirectionY)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private DarkUI.Controls.DarkCheckBox cbLightIsDynamicallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightIsStaticallyUsed;
        private DarkUI.Controls.DarkCheckBox cbLightIsObstructedByRoomGeometry;
        private DarkUI.Controls.DarkCheckBox cbLightEnabled;
        private DarkUI.Controls.DarkPanel panelLightColor;
        private DarkUI.Controls.DarkLabel darkLabel12;
        private DarkUI.Controls.DarkLabel darkLabel13;
        private DarkUI.Controls.DarkLabel darkLabel11;
        private DarkUI.Controls.DarkLabel darkLabel9;
        private DarkUI.Controls.DarkLabel darkLabel10;
        private DarkUI.Controls.DarkLabel darkLabel8;
        private DarkUI.Controls.DarkLabel darkLabel7;
        private DarkUI.Controls.DarkLabel darkLabel6;
        private DarkUI.Controls.DarkButton butAddFogBulb;
        private DarkUI.Controls.DarkButton butAddEffectLight;
        private DarkUI.Controls.DarkButton butAddSpotLight;
        private DarkUI.Controls.DarkButton butAddSun;
        private DarkUI.Controls.DarkButton butAddShadow;
        private DarkUI.Controls.DarkButton butAddPointLight;
        private DarkUI.Controls.DarkLabel darkLabel5;
        private DarkUI.Controls.DarkNumericUpDown numIntensity;
        private DarkUI.Controls.DarkNumericUpDown numInnerRange;
        private DarkUI.Controls.DarkNumericUpDown numOuterRange;
        private DarkUI.Controls.DarkNumericUpDown numInnerAngle;
        private DarkUI.Controls.DarkNumericUpDown numOuterAngle;
        private DarkUI.Controls.DarkNumericUpDown numDirectionX;
        private DarkUI.Controls.DarkNumericUpDown numDirectionY;
        private System.Windows.Forms.ToolTip toolTip;
        private DarkUI.Controls.DarkCheckBox cbLightIsUsedForImportedGeometry;
    }
}
