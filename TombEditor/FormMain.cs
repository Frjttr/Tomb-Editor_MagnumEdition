﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TombEditor.Geometry;
using SharpDX;
using TombEditor.Compilers;
using System.IO;
using System.Diagnostics;
using TombEngine;
using NLog;
using TombEditor.Geometry.IO;
using TombLib.Graphics;
using TombLib.Wad;

namespace TombEditor
{
    public partial class FormMain : DarkUI.Forms.DarkForm
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        
        private bool _pressedZorY = false;
        private Editor _editor;
        private DeviceManager _deviceManager = new DeviceManager();
        
        public FormMain()
        {
            InitializeComponent();
            lightPalette.SelectedColorChanged += delegate
            {
                    Light light = _editor.SelectedObject as Light;
                    if (light == null)
                        return;
                    light.Color = lightPalette.SelectedColor;
                    _editor.SelectedRoom.UpdateCompletely();
                    _editor.ObjectChange(light);
                };

            // Only how debug menu when a debugger is attached...
            debugToolStripMenuItem.Visible = System.Diagnostics.Debugger.IsAttached;

            // For each control bind its light parameter
            numLightIntensity.LightParameter = LightParameter.Intensity;
            numLightIn.LightParameter = LightParameter.In;
            numLightOut.LightParameter = LightParameter.Out;
            numLightLen.LightParameter = LightParameter.Len;
            numLightCutoff.LightParameter = LightParameter.CutOff;
            numLightDirectionX.LightParameter = LightParameter.DirectionX;
            numLightDirectionY.LightParameter = LightParameter.DirectionY;

            // Initialize controls
            _editor = Editor.Instance;
            _editor.EditorEventRaised += EditorEventRaised;
            _editor.Level = Level.CreateSimpleLevel();

            // Initialize panels
            panel3D.InitializePanel(_deviceManager);
            panelItem.InitializePanel(_deviceManager);

            // Initialize the geometry importer class
            GeometryImporterExporter.Initialize(_deviceManager);

            // Update 3D view
            but3D_Click(null, null);

            this.Text = "Tomb Editor " + Application.ProductVersion + " - Untitled";

            logger.Info("Tomb Editor is ready :)");
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _editor.EditorEventRaised -= EditorEventRaised;
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void EditorEventRaised(IEditorEvent obj)
        {
            // Update editor mode
            if (obj is Editor.ModeChangedEvent)
            {
                EditorMode mode = ((Editor.ModeChangedEvent)obj).Current;
                but2D.Checked = mode == EditorMode.Map2D;
                but3D.Checked = mode == EditorMode.Geometry;
                butLightingMode.Checked = mode == EditorMode.Lighting;
                butFaceEdit.Checked = mode == EditorMode.FaceEdit;
                multiPage1.SelectedIndex = mode == EditorMode.Map2D ? 1 : 0;
            }

            // Update the room list
            if (obj is Editor.RoomListChangedEvent)
            {
                // Adjust the amount of entries in the combo list
                while (comboRoom.Items.Count > _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.RemoveAt(comboRoom.Items.Count - 1);
                while (comboRoom.Items.Count < _editor.Level.Rooms.GetLength(0))
                    comboRoom.Items.Add("");

                // Update the room list
                for (int i = 0; i < _editor.Level.Rooms.GetLength(0); i++)
                    if (_editor.Level.Rooms[i] != null)
                        comboRoom.Items[i] = i + ": " + _editor.Level.Rooms[i].Name;
                    else
                        comboRoom.Items[i] = i + ": --- Empty room ---";
            }

            // Update the room property controls
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomPropertiesChangedEvent))
            {
                Room room = ((IEditorRoomChangedEvent)obj).Room;
                if (obj is Editor.SelectedRoomChangedEvent)
                    comboRoom.SelectedIndex = _editor.Level.Rooms.ReferenceIndexOf(room);

                // Update the state of other controls
                if (room.FlagQuickSand)
                    comboRoomType.SelectedIndex = 7;
                else if (room.FlagSnow)
                    comboRoomType.SelectedIndex = 6;
                else if (room.FlagRain)
                    comboRoomType.SelectedIndex = 5;
                else if (room.FlagWater)
                    comboRoomType.SelectedIndex = room.WaterLevel;
                else
                    comboRoomType.SelectedIndex = 0;

                panelRoomAmbientLight.BackColor = room.AmbientLight;

                comboMist.SelectedIndex = room.MistLevel;
                comboReflection.SelectedIndex = room.ReflectionLevel;
                comboReverberation.SelectedIndex = (int)room.Reverberation;

                cbFlagCold.Checked = room.FlagCold;
                cbFlagDamage.Checked = room.FlagDamage;
                cbFlagOutside.Checked = room.FlagOutside;
                cbHorizon.Checked = room.FlagHorizon;
                cbNoPathfinding.Checked = room.ExcludeFromPathFinding;

                butFlipMap.Checked = room.Flipped && (room.AlternateRoom == null);
                comboFlipMap.Enabled = !(room.Flipped && (room.AlternateRoom == null));
                comboFlipMap.SelectedIndex = room.Flipped ? (room.AlternateGroup + 1) : 0;
            }

            // Update the trigger control
            if ((obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomSectorPropertiesChangedEvent))
            {
                lstTriggers.Items.Clear();

                if ((_editor.Level != null) && _editor.SelectedSectors.Valid)
                {
                    // Search for unique triggers inside the selected area
                    var triggers = new List<TriggerInstance>();
                    var area = _editor.SelectedSectors.Area;
                    for (int x = area.X; x <= area.Right; x++)
                        for (int z = area.Y; z <= area.Bottom; z++)
                            foreach (var trigger in _editor.SelectedRoom.Blocks[x, z].Triggers)
                                if (!triggers.Contains(trigger))
                                    triggers.Add(trigger);

                    // Add triggers to listbox
                    foreach (TriggerInstance trigger in triggers)
                        lstTriggers.Items.Add(trigger);
                }
            } 

            // Update the trigger control selection
            if ((obj is Editor.SelectedSectorsChangedEvent) ||
                (obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.SelectedObjectChangedEvent))
            {
                var trigger = _editor.SelectedObject as TriggerInstance;
                lstTriggers.SelectedItem = (trigger != null) && lstTriggers.Items.Contains(trigger) ? trigger : null;
            }

            // Update texture properties
            if (obj is Editor.SelectedTexturesChangedEvent)
            {
                var e = (Editor.SelectedTexturesChangedEvent)obj;
                butTransparent.Checked = e.Current.Transparent;
                butDoubleSided.Checked = e.Current.DoubleSided;
                butInvisible.Checked = e.Current.Invisible;
            }

            // Update room information on the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.RoomGeometryChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if (room == null)
                    statusStripSelectedRoom.Text = "Selected room: None";
                else
                    statusStripSelectedRoom.Text = "Selected room: " +
                        "Name = " + room + " | " +
                        "X = " + room.Position.X + " | " +
                        "Y = " + room.Position.Y + " | " +
                        "Z = " + room.Position.Z + " | " +
                        "Floor = " + (room.Position.Y + room.GetLowestCorner()) + " | " +
                         "Ceiling = " + (room.Position.Y + room.GetHighestCorner());
            }

            // Update selection information of the status strip
            if ((obj is Editor.SelectedRoomChangedEvent) ||
                (obj is Editor.SelectedSectorsChangedEvent))
            {
                var room = _editor.SelectedRoom;
                if ((room == null) || !_editor.SelectedSectors.Valid)
                    statusStripSelectionArea.Text = "Selected area: None";
                else
                    statusStripSelectionArea.Text = "Selected area: " +
                        "X₀ = " + (room.Position.X + _editor.SelectedSectors.Area.X) + " | " +
                        "Z₀ = " + (room.Position.Z + _editor.SelectedSectors.Area.Y) + " | " +
                        "X₁ = " + (room.Position.X + _editor.SelectedSectors.Area.Right) + " | " +
                        "Z₁ = " + (room.Position.Z + _editor.SelectedSectors.Area.Bottom);
            }

            // Update available items combo box
            if ((obj is Editor.LoadedWadsChangedEvent))
            {
                comboItems.Items.Clear();

                if (_editor.Level?.Wad != null)
                {
                    foreach (var movable in _editor.Level.Wad.WadMoveables.Values)
                        comboItems.Items.Add(movable);
                    foreach (var staticMesh in _editor.Level.Wad.WadStatics.Values)
                        comboItems.Items.Add(staticMesh);
                    comboItems.SelectedIndex = 0;
                }
            }

            // Update selection of items combo box
            if (obj is Editor.ChosenItemChangedEvent)
            {
                var e = (Editor.ChosenItemChangedEvent)obj;
                if (!e.Current.HasValue)
                    comboItems.SelectedIndex = -1;
                else if (e.Current.Value.IsStatic)
                    comboItems.Items.Add(_editor.Level.Wad.WadStatics[e.Current.Value.Id]);
                else
                    comboItems.Items.Add(_editor.Level.Wad.WadMoveables[e.Current.Value.Id]);
            }

            // Update item color control
            if (obj is Editor.SelectedObjectChangedEvent)
            {
                ItemInstance itemInstance = ((Editor.SelectedObjectChangedEvent)obj).Current as ItemInstance;
                panelStaticMeshColor.BackColor = itemInstance?.Color ?? System.Drawing.Color.Black;
            }

            // Update application title
            if (obj is Editor.LevelFileNameChanged)
            {
                string LevelName = string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath) ? "Untitled" :
                    Path.GetFileNameWithoutExtension(_editor.Level.Settings.LevelFilePath);
                Text = "Tomb Editor " + Application.ProductVersion.ToString() + " - " + LevelName;
            }

            // Update light UI
            if ((obj is Editor.ObjectChangedEvent) ||
               (obj is Editor.SelectedObjectChangedEvent))
            {
                var light = _editor.SelectedObject as Light;
                
                bool IsLight = false;
                bool HasInOutRange = false;
                bool HasLenCutoffRange = false;
                bool HasDirection = false;
                bool CanCastShadows = false;
                bool CanIlluminateStaticAndDynamicGeometry = false;
                if (light != null)
                {
                    IsLight = true;
                    switch (light.Type)
                    {
                        case LightType.Light:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Shadow:
                            HasInOutRange = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Effect:
                        case LightType.FogBulb:
                            HasInOutRange = true;
                            break;

                        case LightType.Spot:
                            HasInOutRange = true;
                            HasLenCutoffRange = true;
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;

                        case LightType.Sun:
                            HasDirection = true;
                            CanCastShadows = true;
                            CanIlluminateStaticAndDynamicGeometry = true;
                            break;
                    }
                    
                    panelLightColor.BackColor = light.Color;
                    numLightIntensity.Value = light.Intensity;
                    cbLightEnabled.Checked = light.Enabled;
                    cbLightCastsShadows.Checked = light.CastsShadows;
                    cbLightIsDynamicallyUsed.Checked = light.IsDynamicallyUsed;
                    cbLightIsStaticallyUsed.Checked = light.IsStaticallyUsed;
                    numLightIn.Value = light.In;
                    numLightOut.Value = light.Out;
                    numLightLen.Value = light.Len;
                    numLightCutoff.Value = light.Cutoff;
                    numLightDirectionX.Value = light.RotationX;
                    numLightDirectionY.Value = light.RotationY;
                }
                else
                    panelLightColor.BackColor = System.Drawing.Color.FromArgb(60, 63, 65);

                // Set enabled state
                panelLightColor.Enabled = IsLight;
                numLightIntensity.Enabled = IsLight;
                cbLightEnabled.Enabled = IsLight;
                cbLightCastsShadows.Enabled = CanCastShadows;
                cbLightIsDynamicallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                cbLightIsStaticallyUsed.Enabled = CanIlluminateStaticAndDynamicGeometry;
                numLightIn.Enabled = HasInOutRange;
                numLightOut.Enabled = HasInOutRange;
                numLightLen.Enabled = HasLenCutoffRange;
                numLightCutoff.Enabled = HasLenCutoffRange;
                numLightDirectionX.Enabled = HasDirection;
                numLightDirectionY.Enabled = HasDirection;
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _editor.Configuration.SaveTry();
        }
        
        private void butWall_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetWall(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.Box);
        }

        private void butDeath_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.DeathFire);
        }

        private void butMonkey_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area,  BlockFlags.Monkey);
        }

        private void butPortal_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;

            try
            {
                EditorActions.AddPortal(_editor.SelectedRoom, _editor.SelectedSectors.Area);
            }
            catch (Exception exc)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Unable to create portal: " + exc.Message, "Error", DarkUI.Forms.DarkDialogButton.Ok);
                logger.Warn(exc, "Portal creation failed.");
            }
        }

        private void butClimbNorth_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbPositiveX);
        }

        private void butClimbEast_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbPositiveZ);
        }

        private void butClimbSouth_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbNegativeX);
        }

        private void butClimbWest_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.ClimbNegativeZ);
        }

        private void butNotWalkableBox_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.NotWalkableFloor);
        }

        private void butFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalFloorSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalCeilingSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butDiagonalWall_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SetDiagonalWallSplit(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butFlagBeetle_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.Beetle);
        }

        private void butFlagTriggerTriggerer_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.ToggleBlockFlag(_editor.SelectedRoom, _editor.SelectedSectors.Area, BlockFlags.TriggerTriggerer);
        }

        private void butAddPointLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Light };
        }

        private void butAddShadow_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Shadow };
        }

        private void butAddSun_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Sun };
        }

        private void butAddSpotLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Spot };
        }

        private void butAddEffectLight_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.Effect };
        }

        private void butAddFogBulb_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceLight, LightType = LightType.FogBulb };
        }
        
        private void comboRoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            Room selectedRoom = _editor.Level.Rooms[comboRoom.SelectedIndex];
            if (selectedRoom == null)
            {
                selectedRoom = new Room(_editor.Level, 20, 20, "Room " + comboRoom.SelectedIndex);
                _editor.Level.Rooms[comboRoom.SelectedIndex] = selectedRoom;
                _editor.RoomListChange();
            }
            _editor.SelectRoomAndCenterCamera(selectedRoom);
        }

        private void panelRoomAmbientLight_Click(object sender, EventArgs e)
        {
            Room room = _editor.SelectedRoom;

            colorDialog.Color = room.AmbientLight;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelRoomAmbientLight.BackColor = colorDialog.Color;

            _editor.SelectedRoom.AmbientLight = colorDialog.Color;
            _editor.SelectedRoom.UpdateCompletely();
            _editor.RoomPropertiesChange(room);
        }

        private void but3D_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.Geometry;
            _editor.Action = EditorAction.None;
        }

        private void but2D_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.Map2D;
            _editor.Action = EditorAction.None;
        }

        private void butFaceEdit_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.FaceEdit;
            _editor.Action = EditorAction.None;
        }

        private void butLightingMode_Click(object sender, EventArgs e)
        {
            _editor.Mode = EditorMode.Lighting;
            _editor.Action = EditorAction.None;
        }

        private void butCenterCamera_Click(object sender, EventArgs e)
        {
            _editor.CenterCamera();
        }

        private void butDrawPortals_Click(object sender, EventArgs e)
        {
            panel3D.DrawPortals = !panel3D.DrawPortals;
            butDrawPortals.Checked = panel3D.DrawPortals;
            panel3D.Invalidate();
        }
        
        private void butNoOpacity_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.None);
        }

        private void butOpacity1_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.Opacity1);
        }

        private void butOpacity2_Click(object sender, EventArgs e)
        {
            SetPortalOpacity(PortalOpacity.Opacity2);
        }

        private void butTextureFloor_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;
            EditorActions.TexturizeAllFloor(_editor.SelectedRoom);
        }

        private void butTextureCeiling_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;
            EditorActions.TexturizeAllCeiling(_editor.SelectedRoom);
        }

        private void butTextureWalls_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;
            EditorActions.TexturizeAllWalls(_editor.SelectedRoom);
        }

        private void butTransparent_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.Transparent = butTransparent.Checked;
            _editor.SelectedTexture = selectedTexture;
        }

        private void butDoubleSided_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.DoubleSided = butDoubleSided.Checked;
            _editor.SelectedTexture = selectedTexture;
        }

        private void butInvisible_Click(object sender, EventArgs e)
        {
            var selectedTexture = _editor.SelectedTexture;
            selectedTexture.Invisible = butInvisible.Checked;
            _editor.SelectedTexture = selectedTexture;
        }

        private void SetPortalOpacity(PortalOpacity opacity)
        {
            var portal = _editor.SelectedObject as Portal;
            if ((_editor.SelectedRoom == null) || (portal == null))
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You have to select a portal first",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return;
            }
            EditorActions.SetPortalOpacity(_editor.SelectedRoom, portal, opacity);
        }

        private void butAddCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceCamera };
        }

        private void butAddFlybyCamera_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceFlyByCamera };
        }

        private void butAddSoundSource_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSoundSource };
        }

        private void butAddSink_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.PlaceSink };
        }

        private void UpdateLight<T>(Func<Light, T> getLightValue, Action<Light, T> setLightValue, Func<T, T?> getGuiValue) where T : struct
        {
            var light = _editor.SelectedObject as Light;
            if (light == null)
                return;
            
            T? newValue = getGuiValue(getLightValue(light));
            if ((!newValue.HasValue) || newValue.Value.Equals(getLightValue(light)))
                return;

            setLightValue(light, newValue.Value);
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();
            _editor.ObjectChange(light);
        }

        private void panelLightColor_Click(object sender, EventArgs e)
        {
            UpdateLight((light) => light.Color, (light, value) => light.Color = value,
                (value) =>
                {
                    colorDialog.Color = value;
                    if (colorDialog.ShowDialog(this) != DialogResult.OK)
                        return null;
                    return colorDialog.Color;
                });
        }

        private void cbLightEnabled_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.Enabled, (light, value) => light.Enabled = value,
                (value) => cbLightEnabled.Checked);
        }
        
        private void cbLightCastsShadows_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.CastsShadows, (light, value) => light.CastsShadows = value,
                (value) => cbLightCastsShadows.Checked);
        }

        private void cbLightIsStaticallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.IsStaticallyUsed, (light, value) => light.IsStaticallyUsed = value,
                (value) => cbLightIsStaticallyUsed.Checked);
        }

        private void cbLightIsDynamicallyUsed_CheckedChanged(object sender, EventArgs e)
        {
            UpdateLight((light) => light.IsDynamicallyUsed, (light, value) => light.IsDynamicallyUsed = value,
                (value) => cbLightIsDynamicallyUsed.Checked);
        }

        private void newProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to create a new project?",
                    "New project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;
            _editor.Level = Level.CreateSimpleLevel();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            // Stamp

            switch (e.KeyCode)
            {
                case Keys.Z: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    break;

                case Keys.Escape: // End any action
                    _editor.Action = EditorAction.None;
                    _editor.SelectedSectors = SectorSelection.None;
                    _editor.SelectedObject = null;
                    break;

                case Keys.C: // Copy
                    if (e.Control)
                        butCopy_Click(null, null);
                    break;

                case Keys.V: // Paste
                    if (e.Control)
                        butPaste_Click(null, null);
                    break;

                case Keys.B: // Stamp
                    if (e.Control)
                        butClone_Click(null, null);
                    break;

                case Keys.Delete: // Delete object
                    if (_editor.SelectedRoom == null)
                        return;
                    if (_editor.SelectedObject != null)
                        EditorActions.DeleteObjectWithWarning(_editor.SelectedRoom, _editor.SelectedObject, this);
                    break;

                case Keys.T: // Add trigger
                    if ((_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
                    return;

                case Keys.O: // Show options dialog
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.EditObject(_editor.SelectedRoom, _editor.SelectedObject, this);
                    break;

                case Keys.Left:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, -1);
                    break;

                case Keys.Right: 
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, 1);
                    break;

                case Keys.Up:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.X, 1);
                    break;

                case Keys.Down:
                    if (e.Control) // Rotate objects with cones
                        if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                            EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.X, -1);
                    break;

                case Keys.Q:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.A:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.W:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.S:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.E:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.D:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 2, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.R: // Rotate object
                    if ((_editor.SelectedRoom != null) && (_editor.SelectedObject != null))
                        EditorActions.RotateObject(_editor.SelectedRoom, _editor.SelectedObject, EditorActions.RotationAxis.Y, e.Shift ? 5.0f : 45.0f);
                    else if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.F:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, _editor.SelectedSectors.Arrow, 3, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.Y: // Set camera relocation mode (Z on american keyboards, Y on german keyboards)
                    _pressedZorY = true;
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.H:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalFloorCorner, 0, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.U:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)(e.Shift ? 4 : 1), e.Control);
                    break;

                case Keys.J:
                    if (_editor.Mode == EditorMode.Geometry && (_editor.SelectedRoom != null) && _editor.SelectedSectors.Valid)
                        EditorActions.EditSectorGeometry(_editor.SelectedRoom, _editor.SelectedSectors.Area, EditorArrowType.DiagonalCeilingCorner, 1, (short)-(e.Shift ? 4 : 1), e.Control);
                    break;
            }

            // Set camera relocation mode based on previous inputs
            if (e.Alt && _pressedZorY)
            {
                EditorAction action = _editor.Action;
                action.RelocateCameraActive = true;
                _editor.Action = action;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);
            if ((e.KeyCode == Keys.Menu) || (e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
            {
                EditorAction action = _editor.Action;
                action.RelocateCameraActive = false;
                _editor.Action = action;
            }
            if ((e.KeyCode == Keys.Y) || (e.KeyCode == Keys.Z))
                _pressedZorY = false;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            
            EditorAction action = _editor.Action;
            action.RelocateCameraActive = false;
            _editor.Action = action;
            _pressedZorY = false;
        }
        
        private void loadTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ResourceLoader.BrowseTextureFile(_editor.Level.Settings, this))
                return;
            _editor.Level.ReloadTexture();
            _editor.LoadedTexturesChange();
        }
        
        private void unloadTextureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.Settings.TextureFilePath = null;
            _editor.Level.ReloadTexture();
            _editor.LoadedTexturesChange();
        }

        private void reloadTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.ReloadTexture();
            _editor.LoadedTexturesChange();
        }

        private void textureFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureFloor_Click(null, null);
        }

        private void textureCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureCeiling_Click(null, null);
        }

        private void textureWallsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureWalls_Click(null, null);
        }

        private void importConvertTextureToPng_Click(object sender, EventArgs e)
        {
            if (_editor.Level == null)
                return;
            if (_editor.Level.Settings.LevelFilePath == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Currently there is no texture loaded to convert it.",
                    "No texture loaded");
                return;
            }

            string pngFilePath = Path.Combine(
                Path.GetDirectoryName(_editor.Level.Settings.LevelFilePath),
                Path.GetFileNameWithoutExtension(_editor.Level.Settings.LevelFilePath) + ".png");

            if (File.Exists(pngFilePath))
            {
                if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                        "There is already a file at \"" + pngFilePath + "\". Continue and overwrite the file?",
                        "File exist already", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                    return;
            }

            logger.Debug("Converting texture map to PNG format");

            {
                Stopwatch watch = new Stopwatch();
                watch.Start();

                try
                {
                    //Convert...
                    Bitmap bitmap = TombLib.Graphics.TextureLoad.LoadToBitmap(_editor.Level.Settings.LevelFilePath);
                    try
                    {
                        Utils.ConvertTextureTo256Width(ref bitmap);
                        bitmap.Save(pngFilePath, System.Drawing.Imaging.ImageFormat.Png);
                    }
                    finally
                    {
                        bitmap.Dispose();
                    }
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "There was an error while converting TGA in PNG format.");
                    DarkUI.Forms.DarkMessageBox.ShowError("There was an error while converting TGA in PNG format. " + exc.Message, "Error");
                    return;
                }

                watch.Stop();

                logger.Info("Texture map converted");
                logger.Info("    Elapsed time: " + watch.ElapsedMilliseconds + " ms");
            }

            DarkUI.Forms.DarkMessageBox.ShowInformation(
                "TGA texture map was converted to PNG without errors and saved at \"" + pngFilePath + "\".", "Success");
            _editor.Level.Settings.TextureFilePath = pngFilePath;
            _editor.LoadedTexturesChange();
        }

        private void loadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ResourceLoader.BrowseObjectFile(_editor.Level.Settings, this))
                return;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(_editor.Level.Wad);
        }

        private void unloadWADToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.Settings.WadFilePath = null;
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }
        
        private void reloadWadToolStripMenuItem_Click(object sender, EventArgs e)
        {
            _editor.Level.ReloadWad();
            _editor.LoadedWadsChange(null);
        }

        private void comboItems_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((comboItems.SelectedItem == null) || (_editor?.Level?.Wad == null))
                _editor.ChosenItem = null;
            if (comboItems.SelectedItem is WadMoveable)
                _editor.ChosenItem = new ItemType(false, ((WadMoveable)(comboItems.SelectedItem)).ObjectID);
            else if (comboItems.SelectedItem is WadStatic)
                _editor.ChosenItem = new ItemType(true, ((WadStatic)(comboItems.SelectedItem)).ObjectID);
        }
        
        private ItemType? GetCurrentItemWithMessage()
        {
            ItemType? result = _editor.ChosenItem;
            if (result == null)
                DarkUI.Forms.DarkMessageBox.ShowError("Select an item first", "Error");
            return result;
        }

        private void butAddItem_Click(object sender, EventArgs e)
        {
            var currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            if ((!currentItem.Value.IsStatic) && _editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateRoom == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("You can't add moveables to a flipped room", "Error");
                return;
            }

            _editor.Action = new EditorAction { Action = EditorActionType.PlaceItem, ItemType = currentItem.Value };
        }
        
        private void butDeleteRoom_Click(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;
            EditorActions.DeleteRoom(_editor.SelectedRoom);
        }

        private void butCropRoom_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.CropRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void addCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddCamera_Click(null, null);
        }

        private void addFlybyCameraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddFlybyCamera_Click(null, null);
        }

        private void addSinkToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddSink_Click(null, null);
        }

        private void addSoundSourceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butAddSoundSource_Click(null, null);
        }

        private void openProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            Level level = Prj2Loader.LoadFromPrj2(openFileDialogPRJ2.FileName, new ProgressReporterSimple(this));
            if (level == null)
            {
                DarkUI.Forms.DarkMessageBox.ShowError(
                    "There was an error while opening project file. File may be in use or may be corrupted", "Error");
                return;
            }
            _editor.Level = level;
        }
        
        private void importTRLEPRJToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Choose actions
            if (DarkUI.Forms.DarkMessageBox.ShowWarning(
                    "Your project will be lost. Do you really want to open an existing project?",
                    "Open project", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            if (openFileDialogPRJ.ShowDialog(this) != DialogResult.OK)
                return;
            string fileName = openFileDialogPRJ.FileName;

            // Start import process
            Level newLevel = null;
            try
            {
                using (var form = new FormOperationDialog("Import PRJ", false, (progressReporter) =>
                    newLevel = PrjLoader.LoadFromPrj(fileName, progressReporter)))
                {
                    if (form.ShowDialog(this) != DialogResult.OK || newLevel == null)
                        return;
                    _editor.Level = newLevel;
                    newLevel = null;
                }
            }
            finally
            {
                newLevel?.Dispose();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(_editor.Level.Settings.LevelFilePath))
            {
                saveFileDialogPRJ2.InitialDirectory = Path.GetDirectoryName(_editor.Level.Settings.LevelFilePath);
                saveFileDialogPRJ2.FileName = Path.GetFileName(_editor.Level.Settings.LevelFilePath);
            }
            if (saveFileDialogPRJ2.ShowDialog(this) != DialogResult.OK)
                return;

            if (Prj2Writer.SaveToPrj2(saveFileDialogPRJ2.FileName, _editor.Level))
            {
                _editor.Level.Settings.LevelFilePath = saveFileDialogPRJ2.FileName;
                _editor.LevelFileNameChange();
            }
            else
            {
                DarkUI.Forms.DarkMessageBox.ShowError("There was an error while saving project file", "Error");
            }
        }
        private void butRoomUp_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, 1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.SelectedRoom.Portals)
            {
                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }
        }

        private void butRoomDown_Click(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Position += new Vector3(0.0f, -1.0f, 0.0f);

            _editor.SelectedRoom.BuildGeometry();
            _editor.SelectedRoom.CalculateLightingForThisRoom();
            _editor.SelectedRoom.UpdateBuffers();

            foreach (var portal in _editor.SelectedRoom.Portals)
            {
                portal.AdjoiningRoom.BuildGeometry();
                portal.AdjoiningRoom.CalculateLightingForThisRoom();
                portal.AdjoiningRoom.UpdateBuffers();
            }
        }

        private bool BuildLevel(bool autoCloseWhenDone)
        {
            Level level = _editor.Level;
            string fileName = level.Settings.MakeAbsolute(level.Settings.GameLevelFilePath);
            
            using (var form = new FormOperationDialog("Build *.tr4 level", autoCloseWhenDone, (progressReporter) =>
                new LevelCompilerTr4(level, fileName, progressReporter).CompileLevel()))
            {
                form.ShowDialog(this);
                return form.DialogResult != DialogResult.Cancel;
            }
        }

        private void butCompileLevel_Click(object sender, EventArgs e)
        {
            BuildLevel(false);
        }

        private void butCompileLevelAndPlay_Click(object sender, EventArgs e)
        {
            if (!BuildLevel(true))
                return;

            string executablePath = _editor.Level.Settings.MakeAbsolute(_editor.Level.Settings.GameExecutableFilePath);
            var info = new ProcessStartInfo
            {
                WorkingDirectory = Path.GetDirectoryName(executablePath),
                FileName = executablePath
            };

            Process.Start(info);
        }

        private void buildLevelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCompileLevel_Click(null, null);
        }

        private void buildLevelPlayToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCompileLevelAndPlay_Click(null, null);
        }

        private void darkButton15_Click(object sender, EventArgs e)
        {
            using (FormAnimatedTextures form = new FormAnimatedTextures())
                form.ShowDialog(this);
        }

        private void animationRangesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            darkButton15_Click(null, null);
        }

        private void butTextureSounds_Click(object sender, EventArgs e)
        {
            using (var form = new FormTextureSounds())
                form.ShowDialog(this);
        }

        private void textureSoundsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butTextureSounds_Click(null, null);
        }

        private void butItemsBack_Click(object sender, EventArgs e)
        {
            if ((comboItems.SelectedIndex - 1) < 0)
                return;
            comboItems.SelectedIndex = comboItems.SelectedIndex - 1;
        }

        private void butItemsNext_Click(object sender, EventArgs e)
        {
            if ((comboItems.SelectedIndex + 1) >= comboItems.Items.Count)
                return;
            comboItems.SelectedIndex = comboItems.SelectedIndex + 1;
        }

        private void butCopyRoom_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.CopyRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butSplitRoom_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SplitRoom(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butEditRoomName_Click(object sender, EventArgs e)
        {
            using (var form = new FormInputBox())
            {
                form.Title = "Edit room's name";
                form.Message = "Insert the name of this room:";
                form.Value = _editor.SelectedRoom.Name;

                if (form.ShowDialog(this) == DialogResult.Cancel)
                    return;

                _editor.SelectedRoom.Name = form.Value;
                _editor.RoomListChange();
            }
        }

        private bool CheckForRoomAndBlockSelection()
        {
            if ((_editor.SelectedRoom == null) || !_editor.SelectedSectors.Valid)
            {
                DarkUI.Forms.DarkMessageBox.ShowError("Please select a valid group of sectors",
                    "Error", DarkUI.Forms.DarkDialogButton.Ok);
                return false;
            }
            return true;
        }

        private void smoothRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void smoothRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void smoothRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SmoothRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }


        private void sharpRandomFloorUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomFloorDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void sharpRandomCeilingUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, 1);
        }

        private void sharpRandomCeilingDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.SharpRandomCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area, -1);
        }

        private void butFlattenFloor_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void butFlattenCeiling_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenFloorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenFloor(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void flattenCeilingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.FlattenCeiling(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CheckForRoomAndBlockSelection())
                EditorActions.GridWalls3(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void gridWallsIn5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.GridWalls5(_editor.SelectedRoom, _editor.SelectedSectors.Area);
        }

        private void panelStaticMeshColor_Click(object sender, EventArgs e)
        {
            var instance = _editor.SelectedObject as ItemInstance;
            if (instance == null)
                return;

            colorDialog.Color = instance.Color;
            if (colorDialog.ShowDialog(this) != DialogResult.OK)
                return;

            panelStaticMeshColor.BackColor = colorDialog.Color;
            instance.Color = colorDialog.Color;
            _editor.ObjectChange(instance);
        }
        
        private void butFindItem_Click(object sender, EventArgs e)
        {
            ItemType? currentItem = GetCurrentItemWithMessage();
            if (currentItem == null)
                return;

            // Search for matching objects after the previous one
            ObjectInstance previousFind = _editor.SelectedObject;
            ObjectInstance instance = _editor.Level.Rooms
                .Where(room => room != null)
                .SelectMany(room => room.Objects)
                .FindFirstAfterWithWrapAround(
                (obj) => previousFind == obj,
                (obj) => (obj is ItemInstance) && ((ItemInstance)obj).ItemType == currentItem.Value);
            
            // Show result
            if (instance == null)
                DarkUI.Forms.DarkMessageBox.ShowInformation("No object of the selected item type found.", "No object found");
            else
                _editor.ShowObject(instance);
        }

        private void butResetSearch_Click(object sender, EventArgs e)
        {
            _editor.SelectedObject = null;
        }

        private void butDeleteTrigger_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.DeleteObject(_editor.SelectedRoom, _editor.SelectedObject);
        }

        private void butEditTrigger_Click(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || !(_editor.SelectedObject is TriggerInstance))
                return;
            EditorActions.EditObject(_editor.SelectedRoom, _editor.SelectedObject, this);
        }

        private void findObjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butFindItem_Click(null, null);
        }

        private void resetFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butResetSearch_Click(null, null);
        }
        
        private void moveLaraToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;

            // Search for first Lara and remove her
            MoveableInstance lara;
            foreach (Room room in _editor.Level.Rooms.Where(room => room != null))
                foreach (var instance in room.Objects)
                {
                    lara = instance as MoveableInstance;
                    if ((lara != null) && (lara.WadObjectId == 0))
                    {
                        room.RemoveObject(_editor.Level, instance);
                        goto FoundLara;
                    }
                }
            lara = new MoveableInstance { WadObjectId = 0 }; // Lara
            FoundLara:

            // Add lara to current sector
            {
                var room = _editor.SelectedRoom;
                int y = room.GetHighestFloorCorner(_editor.SelectedSectors.Start.X, _editor.SelectedSectors.Start.Y);
                lara.Position = new Vector3(_editor.SelectedSectors.Start.X * 1024 + 512, y * 256, _editor.SelectedSectors.Start.Y * 1024 + 512);
                room.AddObject(_editor.Level, lara);
                _editor.ObjectChange(lara);
            }
        }

        private void comboFlipMap_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_editor.SelectedRoom == null)
                return;

            var room = _editor.SelectedRoom;

            // Delete flipped room
            if (comboFlipMap.SelectedIndex == 0 && room.Flipped)
            {
                EditorActions.AlternateRoomDisable(room);
                return;
            }

            // Change flipped map number, not much to do here
            if (comboFlipMap.SelectedIndex != 0 && room.Flipped)
            {
                if (room.AlternateGroup == (comboFlipMap.SelectedIndex - 1))
                    return;

                room.AlternateGroup = (short)(comboFlipMap.SelectedIndex - 1);
                _editor.RoomPropertiesChange(room);
                return;
            }

            // Create a new flipped room
            if (comboFlipMap.SelectedIndex != 0 && !room.Flipped)
            {
                EditorActions.AlternateRoomEnable(room, (short)(comboFlipMap.SelectedIndex - 1));
                return;
            }
        }
        
        private void butFlipMap_Click(object sender, EventArgs e)
        {
            butFlipMap.Checked = !butFlipMap.Checked;

            if (butFlipMap.Checked)
            {
                if (_editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateRoom != null)
                    _editor.SelectedRoom = _editor.SelectedRoom.AlternateRoom;
            }
            else
            {
                if (_editor.SelectedRoom.Flipped && _editor.SelectedRoom.AlternateBaseRoom != null)
                    _editor.SelectedRoom = _editor.SelectedRoom.AlternateBaseRoom;
            }
        }

        private void saveProjectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveAsToolStripMenuItem_Click(sender, e);
        }

        private void cbFlagDamage_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagDamage = cbFlagDamage.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagCold_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagCold = cbFlagCold.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbFlagOutside_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagOutside = cbFlagOutside.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbHorizon_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.FlagHorizon = cbHorizon.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void cbNoPathfinding_CheckedChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.ExcludeFromPathFinding = cbNoPathfinding.Checked;
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }
        
        private void comboReverberation_SelectedIndexChanged(object sender, EventArgs e)
        {
            _editor.SelectedRoom.Reverberation = (Reverberation)(comboReverberation.SelectedIndex);
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboRoomType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (comboRoomType.SelectedIndex)
            {
                case 0:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 1:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 1;
                    break;

                case 2:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 2;
                    break;

                case 3:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 3;
                    break;

                case 4:
                    _editor.SelectedRoom.FlagWater = true;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 4;
                    break;

                case 5:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = true;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 6:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = true;
                    _editor.SelectedRoom.FlagQuickSand = false;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;

                case 7:
                    _editor.SelectedRoom.FlagWater = false;
                    _editor.SelectedRoom.FlagRain = false;
                    _editor.SelectedRoom.FlagSnow = false;
                    _editor.SelectedRoom.FlagQuickSand = true;
                    _editor.SelectedRoom.WaterLevel = 0;
                    break;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboReflection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagReflection = false;
                _editor.SelectedRoom.ReflectionLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagReflection = true;
                _editor.SelectedRoom.ReflectionLevel = (short)comboReflection.SelectedIndex;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void comboMist_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboReflection.SelectedIndex == 0)
            {
                _editor.SelectedRoom.FlagMist = false;
                _editor.SelectedRoom.MistLevel = 0;
            }
            else
            {
                _editor.SelectedRoom.FlagMist = true;
                _editor.SelectedRoom.MistLevel = (short)comboMist.SelectedIndex;
            }
            _editor.RoomPropertiesChange(_editor.SelectedRoom);
        }

        private void butDrawRoomNames_Click(object sender, EventArgs e)
        {
            panel3D.DrawRoomNames = !panel3D.DrawRoomNames;
            butDrawRoomNames.Checked = panel3D.DrawRoomNames;
            panel3D.Invalidate();
        }

        private void cropRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCropRoom_Click(null, null);
        }

        private void splitRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butSplitRoom_Click(null, null);
        }

        private void copyRoomToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCopyRoom_Click(null, null);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butCopy_Click(null, null);
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butPaste_Click(null, null);
        }

        private void stampToolStripMenuItem_Click(object sender, EventArgs e)
        {
            butClone_Click(null, null);
        }
        
        private void butCopy_Click(object sender, EventArgs e)
        {
            var instance = _editor.SelectedObject as PositionBasedObjectInstance;
            if (instance == null)
            {
                MessageBox.Show(this, "You have to select an object, before you can copy it.", "No object selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            Clipboard.Copy(instance);
        }

        private void butPaste_Click(object sender, EventArgs e)
        {
            _editor.Action = new EditorAction { Action = EditorActionType.Paste };
        }

        private void butClone_Click(object sender, EventArgs e)
        {
            butCopy_Click(null, null);
            _editor.Action = new EditorAction { Action = EditorActionType.Stamp };
        }

        private void newRoomUpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, (room) => room.GetHighestCorner(), 12);
        }

        private void newRoomDownToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EditorActions.CreateRoomAboveOrBelow(_editor.SelectedRoom, (room) => room.GetLowestCorner() - 12, 12);
        }
        
        private void butNoCollision_Click(object sender, EventArgs e)
        {
            if (butNoCollision.Checked)
                _editor.Action = EditorAction.None;
            else
                _editor.Action = new EditorAction { Action = EditorActionType.PlaceNoCollision };
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DarkUI.Forms.DarkMessageBox.ShowWarning("Your project will be lost. Do you really want to exit?",
                    "Exit", DarkUI.Forms.DarkDialogButton.YesNo) != DialogResult.Yes)
                return;

            Close();
        }
        
        private void lstTriggers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((_editor.SelectedRoom == null) || (lstTriggers.SelectedItem == null))
                return;
            _editor.SelectedObject = (ObjectInstance)(lstTriggers.SelectedItem);
        }

        private void butAddTrigger_Click(object sender, EventArgs e)
        {
            if (!CheckForRoomAndBlockSelection())
                return;
            EditorActions.AddTrigger(_editor.SelectedRoom, _editor.SelectedSectors.Area, this);
        }

        private void butDrawHorizon_Click(object sender, EventArgs e)
        {
            panel3D.DrawHorizon = !panel3D.DrawHorizon;
            butDrawHorizon.Checked = panel3D.DrawHorizon;
            panel3D.Invalidate();
        }
    
        private void levelSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FormLevelSettings form = new FormLevelSettings(_editor))
                form.ShowDialog(this);
        }

        // Only for debugging purposes...

        private void debugAction0ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load(""); 
            var level = new TombRaider4Level("e:\\trle\\data\\settomb.tr4");
            level.Load("originale");

            level = new TombRaider4Level("E:\\Vecchi\\Tomb-Editor\\Build\\Game\\Data\\settomb.tr4");
            level.Load("editor");

            //level = new TombEngine.TombRaider4Level("e:\\trle\\data\\tut1.tr4");
            //level.Load("originale");
        }

        private void debugAction1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //level.Load("");

            var level = new TombRaider3Level("e:\\tomb3\\data\\crash.tr2");
            level.Load("crash");

            level = new TombRaider3Level("e:\\tomb3\\data\\jungle.tr2");
            level.Load("jungle");
        }

        private void debugAction2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempColors = new List<int>();

            var bmp = (Bitmap)Image.FromFile("Editor\\Palette.png");
            for (int y = 2; y < bmp.Height; y += 14)
            {
                for (int x = 2; x < bmp.Width; x += 14)
                {
                    var col = bmp.GetPixel(x, y);
                    if (col.A == 0)
                        continue;
                    /* if (!tempColors.Contains(col.ToArgb()))*/
                    tempColors.Add(col.ToArgb());
                }
            }
            File.Delete("Editor\\Palette.bin");
            using (var writer = new BinaryWriter(new FileStream("Editor\\Palette.bin", FileMode.Create, FileAccess.Write, FileShare.None)))
            {
                foreach (int c in tempColors)
                {
                    var col2 = System.Drawing.Color.FromArgb(c);
                    writer.Write(col2.R);
                    writer.Write(col2.G);
                    writer.Write(col2.B);
                }
            }
        }

        private void debugAction3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GeometryImporterExporter.ExportRoomToObj(_editor.SelectedRoom, "room.obj");
        }

        private void debugAction4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            /*  GeometryImporter.LoadModel("low-poly-wooden-door.obj");
              RoomGeometryInstance instance = new RoomGeometryInstance(0, _editor.SelectedRoom);
              instance.Model = GeometryImporter.Models["low-poly-wooden-door.obj"];
              instance.Position = new Vector3(4096, 512, 4096);
              _editor.SelectedRoom.RoomGeometryObjects.Add(instance);*/

            GeometryImporterExporter.LoadModel("room.obj", 1.0f);
            RoomGeometryInstance instance = new RoomGeometryInstance();
            instance.Model = GeometryImporterExporter.Models["room.obj"];
            instance.Position = new Vector3(4096, 512, 4096);
            _editor.SelectedRoom.AddObject(_editor.Level, instance);
        }

        private void debugAction5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string result = Utils.GetRelativePath("E:\\Vecchi\\Tomb-Editor\\Build\\coastal.prj",
                            "E:\\Vecchi\\Tomb-Editor\\Build\\Graphics\\Wads\\coastal.wad");
        }
    }
}
