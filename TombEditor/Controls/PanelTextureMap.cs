﻿using DarkUI.Controls;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using TombEditor.Geometry;
using TombLib.Utils;
using RectangleF = System.Drawing.RectangleF;
using Color = System.Drawing.Color;

namespace TombEditor.Controls
{
    public class PanelTextureMap : Panel
    {
        private Editor _editor;

        private LevelTexture _visibleTexture;
        private TextureArea _selectedTexture;

        private Vector2 _viewPosition;
        private float _viewScale = 1.0f;

        private Vector2? _startPos;
        private int? _selectedTexCoordIndex;
        private Vector2? _viewMoveMouseTexCoord;
        private Point _lastMousePosition;

        private static readonly Pen textureSelectionPen = new Pen(Brushes.Yellow, 2.0f) { LineJoin = LineJoin.Round };
        private static readonly Pen textureSelectionPenTriangle = new Pen(Brushes.Red, 2.0f) { LineJoin = LineJoin.Round };
        private static readonly Brush textureSelectionBrush = new SolidBrush(Color.FromArgb(21, textureSelectionPen.Color.R, textureSelectionPen.Color.G, textureSelectionPen.Color.B));
        private static readonly Brush textureSelectionBrushTriangle = new SolidBrush(Color.FromArgb(33, textureSelectionPenTriangle.Color.R, textureSelectionPenTriangle.Color.G, textureSelectionPenTriangle.Color.B));
        private static readonly Brush textureSelectionBrushSelection = Brushes.DeepSkyBlue;
        private const float textureSelectionPointWidth = 6.0f;
        private const float textureSelectionPointSelectionRadius = 13.0f;
        private const float viewMargin = 10;

        private DarkScrollBarC _hScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Horizontal };
        private DarkScrollBarC _vScrollBar = new DarkScrollBarC { ScrollOrientation = DarkScrollOrientation.Vertical };

        private int _scrollSize => DarkUI.Config.Consts.ScrollBarSize;
        private int _scrollSizeTotal => _scrollSize + 2;

        public PanelTextureMap()
        {
            // Change default state
            SetStyle(ControlStyles.Selectable, true);
            BorderStyle = BorderStyle.FixedSingle;
            DoubleBuffered = true;

            // Scroll bars
            _hScrollBar.Size = new Size(Width - _scrollSize, _scrollSize);
            _hScrollBar.Location = new Point(0, Height - _scrollSize);
            _hScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Left;
            _hScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2((float)_hScrollBar.ValueCentered, ViewPosition.Y); };

            _vScrollBar.Size = new Size(_scrollSize, Height - _scrollSize);
            _vScrollBar.Location = new Point(Width - _scrollSize, 0);
            _vScrollBar.Anchor = AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            _vScrollBar.ValueChanged += (sender, e) => { ViewPosition = new Vector2(ViewPosition.X, (float)_vScrollBar.ValueCentered); };

            Controls.Add(_vScrollBar);
            Controls.Add(_hScrollBar);

            if (LicenseManager.UsageMode == LicenseUsageMode.Runtime)
            {
                _editor = Editor.Instance;
                _editor.EditorEventRaised += EditorEventRaised;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _editor.EditorEventRaised -= EditorEventRaised;
                _hScrollBar.Dispose();
                _vScrollBar.Dispose();
            }
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Reset texture map
            if ((obj is Editor.LevelChangedEvent) || (obj is Editor.LoadedTexturesChangedEvent))
                ResetVisibleTexture(_editor.Level.Settings.Textures.Count > 0 ? _editor.Level.Settings.Textures[0] : null);
        }

        public void ShowTexture(TextureArea area)
        {
            if (!(area.Texture is LevelTexture))
                return;

            VisibleTexture = (LevelTexture)(area.Texture);
            SelectedTexture = area;

            Vector2 min = Vector2.Min(Vector2.Min(area.TexCoord0, area.TexCoord1), Vector2.Min(area.TexCoord2, area.TexCoord3));
            Vector2 max = Vector2.Max(Vector2.Max(area.TexCoord0, area.TexCoord1), Vector2.Max(area.TexCoord2, area.TexCoord3));

            ViewPosition = (min + max) * 0.5f;
            float requiredScaleX = Width / (max.X - min.X);
            float requiredScaleY = Height / (max.Y - min.Y);
            ViewScale = Math.Min(requiredScaleX, requiredScaleY) * _editor.Configuration.TextureMap_TextureAreaToViewRelativeSize;

            LimitPosition();
        }

        private void UpdateScrollBars()
        {
            bool hasTexture = VisibleTexture?.IsAvailable ?? false;

            _vScrollBar.SetViewCentered(
                -viewMargin,
                (hasTexture ? VisibleTexture.Image.Height : 1) + viewMargin * 2,
                (Height - _scrollSizeTotal) / ViewScale,
                ViewPosition.Y, hasTexture);
            _hScrollBar.SetViewCentered(
                -viewMargin,
                (hasTexture ? VisibleTexture.Image.Width : 1) + viewMargin * 2,
                (Width - _scrollSizeTotal) / ViewScale,
                ViewPosition.X, hasTexture);
        }

        public Vector2 FromVisualCoord(PointF pos, bool limited = true)
        {
            Vector2 textureCoord = new Vector2(
               (pos.X - Width * 0.5f) / ViewScale + ViewPosition.X,
               (pos.Y - Height * 0.5f) / ViewScale + ViewPosition.Y);
            if (limited)
                textureCoord = Vector2.Min(VisibleTexture.Image.Size - new Vector2(0.5f), Vector2.Max(new Vector2(0.5f), textureCoord));
            return textureCoord;
        }

        public PointF ToVisualCoord(Vector2 texCoord)
        {
            return new PointF(
                (texCoord.X - ViewPosition.X) * ViewScale + Width * 0.5f,
                (texCoord.Y - ViewPosition.Y) * ViewScale + Height * 0.5f);
        }

        private void MoveToFixedPoint(PointF visualPoint, Vector2 worldPoint)
        {
            //Adjust ViewPosition in such a way, that the FixedPoint does not move visually
            ViewPosition = -worldPoint;
            ViewPosition = -FromVisualCoord(visualPoint, false);
        }

        private void LimitPosition()
        {
            bool hasTexture = VisibleTexture?.IsAvailable ?? false;
            Vector2 minimum = new Vector2(-viewMargin);
            Vector2 maximum = (hasTexture ? VisibleTexture.Image.Size : new Vector2(1)) + new Vector2(viewMargin);
            ViewPosition = Vector2.Min(maximum, Vector2.Max(minimum, ViewPosition));
        }

        protected struct SelectionPrecisionType
        {
            public float Precision { get; set; }
            public bool SelectFullTileAutomatically { get; set; }
            public SelectionPrecisionType(float precision, bool selectFullTileAutomatically)
            {
                Precision = precision;
                SelectFullTileAutomatically = selectFullTileAutomatically;
            }
        }

        protected virtual SelectionPrecisionType GetSelectionPrecision(bool rectangularSelection)
        {
            if (ModifierKeys.HasFlag(Keys.Alt))
                return new SelectionPrecisionType(0.0f, false);
            else if (ModifierKeys.HasFlag(Keys.Control))
                return new SelectionPrecisionType(1.0f, false);
            else if (ModifierKeys.HasFlag(Keys.Shift) == rectangularSelection)
                return new SelectionPrecisionType(16.0f, false);
            else
                return new SelectionPrecisionType(TileSelectionSize, true);
        }

        protected virtual float MaxTextureSize { get; } = 255;

        private Vector2 Quantize(Vector2 texCoord, bool endX, bool endY, bool rectangularSelection)
        {
            var selectionPrecision = GetSelectionPrecision(rectangularSelection);
            if (selectionPrecision.Precision == 0.0f)
                return texCoord;

            texCoord -= new Vector2(endX ? -0.5f : 0.5f, endY ? -0.5f : 0.5f);
            texCoord /= selectionPrecision.Precision;
            if ((selectionPrecision.Precision >= 64.0f) && rectangularSelection)
            {
                texCoord = new Vector2(
                    endX ? (float)Math.Ceiling(texCoord.X) : (float)Math.Floor(texCoord.X),
                    endY ? (float)Math.Ceiling(texCoord.Y) : (float)Math.Floor(texCoord.Y));
            }
            else
                texCoord = new Vector2((float)Math.Round(texCoord.X), (float)Math.Round(texCoord.Y));
            texCoord *= selectionPrecision.Precision;
            texCoord += new Vector2( endX ? -0.5f : 0.5f, endY ? -0.5f : 0.5f);

            return texCoord;
        }

        private void SetRectangularTextureWithMouse(Vector2 texCoordStart, Vector2 texCoordEnd)
        {
            Vector2 texCoordStartQuantized = Quantize(texCoordStart, texCoordStart.X > texCoordEnd.X, texCoordStart.Y > texCoordEnd.Y, true);
            Vector2 texCoordEndQuantized = Quantize(texCoordEnd, !(texCoordStart.X > texCoordEnd.X), !(texCoordStart.Y > texCoordEnd.Y), true);

            texCoordEndQuantized = Vector2.Min(texCoordStartQuantized + new Vector2(MaxTextureSize),
                Vector2.Max(texCoordStartQuantized - new Vector2(MaxTextureSize), texCoordEndQuantized));

            var selectedTexture = SelectedTexture;
            selectedTexture.TexCoord0 = new Vector2(texCoordStartQuantized.X, texCoordEndQuantized.Y);
            selectedTexture.TexCoord1 = texCoordStartQuantized;
            selectedTexture.TexCoord2 = new Vector2(texCoordEndQuantized.X, texCoordStartQuantized.Y);
            selectedTexture.TexCoord3 = texCoordEndQuantized;
            selectedTexture.Texture = VisibleTexture;
            SelectedTexture = selectedTexture;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);

            if (!Focused)
                Focus(); // Enable keyboard interaction
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!Focused)
                Focus(); // Enable keyboard interaction

            _lastMousePosition = e.Location;
            _startPos = null;

            if (!(VisibleTexture?.IsAvailable ?? false))
            {
                EditorActions.LoadTextures(Parent);
                return;
            }

            //https://stackoverflow.com/questions/14191219/receive-mouse-move-even-cursor-is-outside-control
            Capture = true; // Capture mouse for zoom and panning

            switch (e.Button)
            {
                case MouseButtons.Left:
                    var mousePos = FromVisualCoord(e.Location);

                    // Check if mouse was on existing texture
                    if (SelectedTexture.Texture == VisibleTexture)
                    {
                        var texCoords = SelectedTexture.TexCoords
                            .Where(texCoordPair => Vector2.Distance(texCoordPair.Value, mousePos) < textureSelectionPointSelectionRadius)
                            .OrderBy(texCoordPair => Vector2.Distance(texCoordPair.Value, mousePos))
                            .ToList();
                        if (texCoords.Count != 0)
                        {
                            // Select texture coords
                            _selectedTexCoordIndex = texCoords.First().Key;
                            Invalidate();
                            break;
                        }
                    }
                    if (_selectedTexCoordIndex != null)
                    {
                        _selectedTexCoordIndex = null;
                        Invalidate();
                    }

                    // Start selection ...
                    _startPos = mousePos;
                    break;

                case MouseButtons.Right:
                    // Move view with mouse curser
                    // Mouse curser is a fixed point
                    _viewMoveMouseTexCoord = FromVisualCoord(e.Location);
                    break;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_selectedTexCoordIndex.HasValue)
                    {
                        TextureArea currentTexture = SelectedTexture;

                        // Determine bounds
                        Vector2 texCoordMin = new Vector2(float.PositiveInfinity);
                        Vector2 texCoordMax = new Vector2(float.NegativeInfinity);
                        for (int i = 0; i < 4; ++i)
                        {
                            if (i == _selectedTexCoordIndex)
                                continue;
                            texCoordMin = Vector2.Min(texCoordMin, currentTexture.GetTexCoord(i));
                            texCoordMax = Vector2.Max(texCoordMax, currentTexture.GetTexCoord(i));
                        }
                        Vector2 texCoordMinBounds = texCoordMax - new Vector2(MaxTextureSize);
                        Vector2 texCoordMaxBounds = texCoordMin + new Vector2(MaxTextureSize);

                        //Move texture coord
                        Vector2 newTextureCoord = FromVisualCoord(e.Location);

                        float minArea = float.PositiveInfinity;
                        TextureArea minAreaTextureArea = currentTexture;
                        for (int i = 0; i < 4; ++i)
                        {
                            currentTexture.SetTexCoord(_selectedTexCoordIndex.Value,
                                Vector2.Min(texCoordMaxBounds, Vector2.Max(texCoordMinBounds,
                                    Quantize(newTextureCoord, (i & 1) != 0, (i & 2) != 0, false))));

                            float area = Math.Abs(currentTexture.QuadArea);
                            if (area < minArea)
                            {
                                minAreaTextureArea = currentTexture;
                                minArea = area;
                            }
                        }

                        // Use the configuration that covers the most area
                        SelectedTexture = minAreaTextureArea;
                    }

                    if (_startPos.HasValue)
                        SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;

                case MouseButtons.Right:
                    // Move view with mouse curser
                    // Mouse curser is a fixed point
                    if (_viewMoveMouseTexCoord.HasValue)
                        if (ModifierKeys.HasFlag(Keys.Control))
                        { // Zoom
                            float relativeDeltaY = (e.Location.Y - _lastMousePosition.Y) / (float)Height;
                            ViewScale *= (float)Math.Exp(_editor.Configuration.TextureMap_NavigationSpeedMouseZoom * relativeDeltaY);
                        }
                        else
                        { // Movement
                            MoveToFixedPoint(e.Location, _viewMoveMouseTexCoord.Value);
                            LimitPosition();
                        }
                    break;
            }
            _lastMousePosition = e.Location;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if (_startPos.HasValue)
                        if (GetSelectionPrecision(true).SelectFullTileAutomatically)
                            SetRectangularTextureWithMouse(_startPos.Value, FromVisualCoord(e.Location));
                    break;
            }
            _viewMoveMouseTexCoord = null;
            Capture = false;
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!(VisibleTexture?.IsAvailable ?? false))
                return;

            Vector2 FixedPointInWorld = FromVisualCoord(e.Location);
            ViewScale *= (float)Math.Exp(e.Delta * _editor.Configuration.TextureMap_NavigationSpeedMouseWheelZoom);
            MoveToFixedPoint(e.Location, FixedPointInWorld);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.IntersectClip(new RectangleF(0, 0, Width - _scrollSizeTotal, Height - _scrollSizeTotal));

            // Only proceed if texture is actually available
            if (VisibleTexture?.IsAvailable ?? false)
            {
                PointF drawStart = ToVisualCoord(new Vector2(0.0f, 0.0f));
                PointF drawEnd = ToVisualCoord(new Vector2(VisibleTexture.Image.Width, VisibleTexture.Image.Height));
                RectangleF drawArea = RectangleF.FromLTRB(drawStart.X, drawStart.Y, drawEnd.X, drawEnd.Y);

                // Draw background
                using (var textureBrush = new TextureBrush(Properties.Resources.TransparentBackground))
                    e.Graphics.FillRectangle(textureBrush, drawArea);

                // Draw image
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                VisibleTexture.Image.GetTempSystemDrawingBitmap((tempBitmap) =>
                    {
                        // System.Drawing being silly, it draws the first row of pixels only half, so everything would be shifted
                        // To work around it, we have to do some silly coodinate changes :/
                        e.Graphics.DrawImage(tempBitmap,
                            new RectangleF(drawArea.X, drawArea.Y, drawArea.Width + 0.5f * ViewScale, drawArea.Height + 0.5f * ViewScale),
                            new RectangleF(-0.5f, -0.5f, tempBitmap.Width + 0.5f, tempBitmap.Height + 0.5f),
                            GraphicsUnit.Pixel);
                    });

                OnPaintSelection(e);
            }
            else
            {
                e.Graphics.DrawString("Click here to load textures.",
                    Font, System.Drawing.Brushes.DarkGray,
                    ClientRectangle,
                    new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
            }

            // Draw border next to scroll bars
            using (Pen pen = new Pen(DarkUI.Config.Colors.LighterBackground, 1.0f))
                e.Graphics.DrawRectangle(pen, new RectangleF(-1, -1, Width - _scrollSizeTotal, Height - _scrollSizeTotal));
        }

        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            switch (e.KeyCode)
            {
                case Keys.Down:
                    ViewPosition += new Vector2(0.0f, _editor.Configuration.TextureMap_NavigationSpeedKeyMove / ViewScale);
                    Invalidate();
                    break;
                case Keys.Up:
                    ViewPosition += new Vector2(0.0f, -_editor.Configuration.TextureMap_NavigationSpeedKeyMove / ViewScale);
                    Invalidate();
                    break;
                case Keys.Left:
                    ViewPosition += new Vector2(-_editor.Configuration.TextureMap_NavigationSpeedKeyMove / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.Right:
                    ViewPosition += new Vector2(_editor.Configuration.TextureMap_NavigationSpeedKeyMove / ViewScale, 0.0f);
                    Invalidate();
                    break;
                case Keys.PageDown:
                    ViewScale *= (float)Math.Exp(-_editor.Configuration.TextureMap_NavigationSpeedKeyZoom);
                    Invalidate();
                    break;
                case Keys.PageUp:
                    ViewScale *= (float)Math.Exp(_editor.Configuration.TextureMap_NavigationSpeedKeyZoom);
                    Invalidate();
                    break;
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            UpdateScrollBars();
            Invalidate();
        }

        protected virtual bool DrawTriangle => true;

        protected virtual void OnPaintSelection(PaintEventArgs e)
        {
            // Draw selection
            var selectedTexture = SelectedTexture;
            if (selectedTexture.Texture == VisibleTexture)
            {
                // This texture is currently selected
                PointF[] points = new PointF[]
                    {
                            ToVisualCoord(selectedTexture.TexCoord0),
                            ToVisualCoord(selectedTexture.TexCoord1),
                            ToVisualCoord(selectedTexture.TexCoord2),
                            ToVisualCoord(selectedTexture.TexCoord3)
                    };

                // Draw fill color
                e.Graphics.FillPolygon(textureSelectionBrush, new PointF[] { points[0], points[2], points[3] });
                if (DrawTriangle)
                    e.Graphics.FillPolygon(textureSelectionBrushTriangle, new PointF[] { points[0], points[1], points[2] });

                // Draw outlines
                e.Graphics.DrawPolygon(textureSelectionPen, points);
                if (DrawTriangle)
                    e.Graphics.DrawPolygon(textureSelectionPenTriangle, new PointF[] { points[0], points[1], points[2] });

                for (int i = 0; i < 4; ++i)
                {
                    Brush brush = _selectedTexCoordIndex == i ? textureSelectionBrushSelection : textureSelectionPen.Brush;
                    e.Graphics.FillRectangle(brush,
                        points[i].X - textureSelectionPointWidth * 0.5f, points[i].Y - textureSelectionPointWidth * 0.5f,
                        textureSelectionPointWidth, textureSelectionPointWidth);
                }
            }
        }

        [DefaultValue(BorderStyle.FixedSingle)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public Vector2 ViewPosition
        {
            get { return _viewPosition; }
            set
            {
                if (_viewPosition == value)
                    return;
                _viewPosition = value;
                UpdateScrollBars();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public float ViewScale
        {
            get { return _viewScale; }
            set
            {
                value = Math.Min(value, _editor.Configuration.TextureMap_NavigationMaxZoom);
                value = Math.Max(value, _editor.Configuration.TextureMap_NavigationMinZoom);
                if (_viewScale == value)
                    return;
                _viewScale = value;
                UpdateScrollBars();
                Invalidate();
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public LevelTexture VisibleTexture
        {
            get { return _visibleTexture; }
            set
            {
                if (_visibleTexture == value)
                    return;
                ResetVisibleTexture(value);
            }
        }

        public void ResetVisibleTexture(LevelTexture texture)
        {
            _visibleTexture = texture;
            ViewPosition = new Vector2((VisibleTexture?.IsAvailable ?? false) ? VisibleTexture.Image.Width * 0.5f : 128, (Height - _scrollSizeTotal) * 0.5f);
            ViewScale = 1.0f;
            Invalidate();
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public TextureArea SelectedTexture
        {
            get { return _selectedTexture; }
            set
            {
                if (_selectedTexture == value)
                    return;
                _selectedTexture = value;
                SelectedTextureChanged?.Invoke(this, EventArgs.Empty);
                Invalidate();
            }
        }

        public event EventHandler<EventArgs> SelectedTextureChanged;

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [ReadOnly(true)]
        public float TileSelectionSize { get; set; } = 64.0f;
    }
}
