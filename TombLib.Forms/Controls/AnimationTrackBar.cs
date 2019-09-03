﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using TombLib.Wad;
using TombLib.Graphics;
using System.Drawing.Drawing2D;

namespace TombLib.Controls
{
    public partial class AnimationTrackBar : UserControl
    {
        private static readonly Pen _frameBorderPen = new Pen(Color.FromArgb(140, 140, 140), 1);
        private static readonly Pen _keyFrameBorderPen = new Pen(Color.FromArgb(180, 160, 160), 2);
        private static readonly Brush _cursorBrush = new SolidBrush(Color.FromArgb(180, 240, 140, 50));
        private static readonly Brush _stateChangeBrush = new SolidBrush(Color.FromArgb(30, 220, 100, 200));
        private static readonly Brush _animCommandSoundBrush = new SolidBrush(Color.FromArgb(220, 30, 50, 250));
        private static readonly Brush _animCommandFlipeffectBrush = new SolidBrush(Color.FromArgb(220, 230, 40, 20));

        private static readonly int _cursorWidth = 6;
        private static readonly int _animCommandMarkerRadius = 13;
        private static readonly int _stateChangeMarkerThicknessDivider = 2;

        private int _minimum;
        public int Minimum
        {
            get { return _minimum; }
            set
            {
                if (_minimum == value) return;
                if (value >= _maximum) return;

                _minimum = value;
                picSlider.Invalidate();
                Refresh();
            }
        }

        private int _maximum;
        public int Maximum
        {
            get { return _maximum; }
            set
            {
                if (_maximum == value) return;
                if (value < _minimum) return;

                _maximum = value;
                picSlider.Invalidate();
                Refresh();
            }
        }

        private int _value;
        public int Value
        {
            get { return _value; }
            set
            {
                if (value == _value) return;
                if (value < _minimum) value = _minimum;
                if (value > _maximum) value = _maximum;

                _value = value;

                ValueChanged?.Invoke(this, new EventArgs());
                picSlider.Invalidate();
                Refresh();
            }
        }

        public override Color BackColor
        {
            get { return picSlider.BackColor; }
            set { picSlider.BackColor = value; picSlider.Invalidate(); }
        }

        public event EventHandler ValueChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public AnimationNode Animation { get; set; }

        private bool mouseDown = false;

        public AnimationTrackBar()
        {
            InitializeComponent();
            picSlider.MouseWheel += picSlider_MouseWheel;
            picSlider.MouseEnter += picSlider_MouseEnter;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        private void picSlider_MouseEnter(object sender, EventArgs e)
        {
            picSlider.Focus();
        }

        private void picSlider_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta < 0)
                if (Value < Maximum) Value++; else Value = 0;
            else if (e.Delta > 0)
                if (Value > Minimum) Value--; else Value = Maximum;
        }

        private void picSlider_MouseDown(object sender, MouseEventArgs e)
        {
            mouseDown = true;
            Value = XtoValue(e.X);
        }

        private void picSlider_MouseMove(object sender, MouseEventArgs e)
        {
            if (!mouseDown) return;
            Value = XtoValue(e.X);
        }

        private void picSlider_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private int XtoValue(int x, bool interpolate = true)
        {
            return (int)Math.Round((double)(Minimum + (Maximum - Minimum) * x) / (double)(picSlider.ClientSize.Width - 1), MidpointRounding.ToEven);
        }

        private int ValueToX(int value)
        {
            if (Maximum - Minimum == 0)
                return 0; // Prevent division by zero

            return (picSlider.ClientSize.Width - picSlider.Padding.Left - picSlider.Padding.Right) * (value - Minimum) / (int)(Maximum - Minimum);
        }

        private void picSlider_Paint(object sender, PaintEventArgs e)
        {
            if (Animation == null)
                return;

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Calculate the needle's X coordinate.
            var x = ValueToX(Value);

            int addShift = -_cursorWidth / 2;
            if (Value == 0)
                addShift += _cursorWidth / 2;
            else if (Value == Maximum)
                addShift += -_cursorWidth / 2;

            int realFrameCount = Animation.WadAnimation.FrameRate * (Animation.WadAnimation.KeyFrames.Count - 1) + 1;
            int marginWidth = picSlider.ClientSize.Width - picSlider.Padding.Left - picSlider.Padding.Right - 1;
            float frameStep = realFrameCount <= 1 ? marginWidth : (float)marginWidth / (float)(realFrameCount - 1);

            // Draw state change ranges
            foreach (var sch in Animation.WadAnimation.StateChanges)
                foreach (var disp in sch.Dispatches)
                {
                    int realOutFrame = disp.OutFrame >= realFrameCount ? realFrameCount - 1 : disp.OutFrame;
                    e.Graphics.FillRectangle(_stateChangeBrush, new RectangleF(picSlider.Padding.Left + (disp.InFrame * frameStep), 
                        picSlider.Padding.Top, 
                        (realOutFrame - disp.InFrame) * frameStep,
                        picSlider.ClientSize.Height / _stateChangeMarkerThicknessDivider - picSlider.Padding.Bottom - 2));
                }

            // Draw needle.
            e.Graphics.FillRectangle(_cursorBrush, new RectangleF(x + addShift + picSlider.Padding.Left, picSlider.Padding.Top, _cursorWidth, picSlider.ClientSize.Height - picSlider.Padding.Bottom - 2));

            // Draw frame-specific animcommands, numericals and dividers.
            for (int passes = 0; passes < 2; passes++)
                for (int i = 0; i < realFrameCount; ++i)
                {
                    int currX = (int)(frameStep * i) + picSlider.Padding.Left;
                    bool isKeyFrame = (i % Animation.WadAnimation.FrameRate == 0);

                    if (passes == 0)
                    {
                        // Draw animcommands
                        foreach (var ac in Animation.WadAnimation.AnimCommands)
                        {
                            Rectangle currRect = new Rectangle(currX - 6, picSlider.Padding.Top - _animCommandMarkerRadius / 2, _animCommandMarkerRadius, _animCommandMarkerRadius);

                            switch (ac.Type)
                            {
                                case WadAnimCommandType.PlaySound:
                                    if (ac.Parameter1 == i)
                                        e.Graphics.FillPie(_animCommandSoundBrush, currRect, 0, 180);
                                    break;
                                case WadAnimCommandType.FlipEffect:
                                    if (ac.Parameter1 == i)
                                        e.Graphics.FillPie(_animCommandFlipeffectBrush, currRect, 0, 180);
                                    break;
                            }
                        }

                        // Draw dividers
                        if (isKeyFrame)
                            e.Graphics.DrawLine(_keyFrameBorderPen, currX, picSlider.Padding.Top, currX, picSlider.Height / 2);  // Draw keyframe
                        else
                            e.Graphics.DrawLine(_frameBorderPen, currX, picSlider.Padding.Top, currX, picSlider.Height / 3);  // Draw ordinary frame
                    }

                    // Align first and last numerical entries so they are not concealed by control border
                    StringAlignment align = StringAlignment.Center;
                    int shift = 0;
                    if (i == 0)
                    {
                        shift -= picSlider.Padding.Left;
                        align = StringAlignment.Near;
                    }
                    else if (i >= realFrameCount - 1)
                    {
                        shift += picSlider.Padding.Left;
                        align = StringAlignment.Far;
                    }

                    // Draw frame number
                    if ((passes == 0 && !isKeyFrame) || (passes != 0 && isKeyFrame))
                        e.Graphics.DrawString(i.ToString(), Font, (isKeyFrame ? Brushes.White : Brushes.DimGray), currX + shift, picSlider.Height,
                        new StringFormat { Alignment = align, LineAlignment = StringAlignment.Far });
            }

            // Draw horizontal guide
            e.Graphics.DrawLine(_keyFrameBorderPen, picSlider.Padding.Left, picSlider.Padding.Top, picSlider.ClientSize.Width - picSlider.Padding.Left, picSlider.Padding.Top);
        }

        private void picSlider_SizeChanged(object sender, EventArgs e)
        {
            picSlider.Invalidate();
        }
    }
}
