﻿using NLog;
using SharpDX;
using DarkUI.Docking;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using System.Xml.Serialization;

namespace TombEditor
{
    // Just add properties to this class to add now configuration options.
    // They will be loaded and saved automatically.
    public class Configuration
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public float RenderingItem_NavigationSpeedMouseWheelZoom { get; set; } = 6.0f;
        public float RenderingItem_NavigationSpeedMouseZoom { get; set; } = 300.0f;
        public float RenderingItem_NavigationSpeedMouseTranslate { get; set; } = 200.0f;
        public float RenderingItem_NavigationSpeedMouseRotate { get; set; } = 4.0f;
        public float RenderingItem_FieldOfView { get; set; } = 50.0f;

        public int Rendering3D_DrawRoomsMaxDepth { get; set; } = 6;
        public float Rendering3D_NavigationSpeedKeyRotate { get; set; } = 0.17f;
        public float Rendering3D_NavigationSpeedKeyZoom { get; set; } = 3000.0f;
        public float Rendering3D_NavigationSpeedMouseWheelZoom { get; set; } = 25.0f;
        public float Rendering3D_NavigationSpeedMouseZoom { get; set; } = 72000.0f;
        public float Rendering3D_NavigationSpeedMouseTranslate { get; set; } = 22000.0f;
        public float Rendering3D_NavigationSpeedMouseRotate { get; set; } = 2.2f;
        public float Rendering3D_LineWidth { get; set; } = 10.0f;
        public float Rendering3D_FieldOfView { get; set; } = 50.0f;
        public Vector4 Rendering3D_BackgroundColor { get; set; } = new Vector4(0.65f, 0.65f, 0.65f, 1.0f);
        public Vector4 Rendering3D_BackgroundColorFlipRoom { get; set; } = new Vector4(0.13f, 0.13f, 0.13f, 1.0f);
        public Vector4 Rendering3D_TextColor { get; set; } = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);

        public float Map2D_NavigationSpeedMouseWheelZoom { get; set; } = 0.001f;
        public float Map2D_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float Map2D_NavigationSpeedKeyZoom { get; set; } = 0.17f;
        public float Map2D_NavigationSpeedKeyMove { get; set; } = 107.0f;

        public float TextureMap_NavigationSpeedMouseWheelZoom { get; set; } = 0.0015f;
        public float TextureMap_NavigationSpeedMouseZoom { get; set; } = 7.5f;
        public float TextureMap_TextureAreaToViewRelativeSize { get; set; } = 0.32f;

        public float Gizmo_Size { get; set; } = 1024.0f;
        public float Gizmo_TranslationSphereSize { get; set; } = 220.0f;
        public float Gizmo_CenterCubeSize { get; set; } = 128.0f;

        public Point Window_Position { get; set; } = new Point(32, 32);
        public Size Window_Size { get; set; } = Window_SizeDefault;
        public bool Window_Maximized { get; set; } = true;
        public DockPanelState Window_Layout { get; set; } = Window_LayoutDefault;

        public static readonly Size Window_SizeDefault = new Size(1212, 763);
        public static readonly DockPanelState Window_LayoutDefault = new DockPanelState
        {
            Regions = new List<DockRegionState>
            {
                new DockRegionState
                {
                    Area = DarkDockArea.Document,
                    Size = new Size(0, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "MainView" },
                            VisibleContent = "MainView",
                            Order = 0,
                            Size = new Size(0 ,0)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Bottom,
                    Size = new Size(1007, 134),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "Lighting" },
                            VisibleContent = "Lighting",
                            Order = 0,
                            Size = new Size(442,134)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "Palette" },
                            VisibleContent = "Palette",
                            Order = 1,
                            Size = new Size(645,134)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Left,
                    Size = new Size(286, 893),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "SectorOptions" },
                            VisibleContent = "SectorOptions",
                            Order = 0,
                            Size = new Size(285,280)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "RoomOptions" },
                            VisibleContent = "RoomOptions",
                            Order = 1,
                            Size = new Size(285,211)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "ObjectBrowser" },
                            VisibleContent = "ObjectBrowser",
                            Order = 2,
                            Size = new Size(285,259)
                        },
                        new DockGroupState
                        {
                            Contents = new List<string> { "TriggerList" },
                            VisibleContent = "TriggerList",
                            Order = 3,
                            Size = new Size(285,174)
                        }
                    }
                },
                new DockRegionState
                {
                    Area = DarkDockArea.Right,
                    Size = new Size(286, 0),
                    Groups = new List<DockGroupState>
                    {
                        new DockGroupState
                        {
                            Contents = new List<string> { "TexturePanel" },
                            VisibleContent = "TexturePanel",
                            Order = 0,
                            Size = new Size(285,700)
                        }
                    }
                }
            }
        };

        public static string GetDefaultPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + "/TombEditorConfiguration.xml";
        }

        public void Save(Stream stream)
        {
            new XmlSerializer(typeof(Configuration)).Serialize(stream, this);
        }

        public void Save(string path)
        {
            using (var stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None))
                Save(stream);
        }

        public void Save()
        {
            Save(GetDefaultPath());
        }

        public void SaveTry()
        {
            try
            {
                Save();
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to save configuration to \"" + GetDefaultPath() + "\"");
            }
        }

        public static Configuration Load(Stream stream)
        {
            return (Configuration)(new XmlSerializer(typeof(Configuration)).Deserialize(stream));
        }

        public static Configuration Load(string path)
        {
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return Load(stream);
        }

        public static Configuration Load()
        {
            return Load(GetDefaultPath());
        }

        public static Configuration LoadOrUseDefault()
        {
            try
            {
                return Load();
            }
            catch (Exception exc)
            {
                logger.Info(exc, "Unable to load configuration from \"" + GetDefaultPath() + "\"");
                return new Configuration();
            }
        }
    }
}
