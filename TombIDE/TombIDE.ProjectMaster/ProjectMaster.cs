﻿using System.IO;
using System.Reflection;
using System.Windows.Forms;
using TombIDE.Shared;
using TombLib.LevelData;

namespace TombIDE.ProjectMaster
{
	public partial class ProjectMaster : UserControl
	{
		private IDE _ide;

		public ProjectMaster()
		{
			InitializeComponent();
		}

		public void Initialize(IDE ide)
		{
			_ide = ide;

			string programPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			string pluginsPath = Path.Combine(programPath, "Plugins");

			if (!Directory.Exists(pluginsPath))
				Directory.CreateDirectory(pluginsPath);

			// Initialize the watchers
			prj2FileWatcher.Path = _ide.Project.LevelsPath;
			levelFolderWatcher.Path = _ide.Project.LevelsPath;
			dllFileWatcher.Path = pluginsPath;

			// Initialize the sections
			section_LevelList.Initialize(_ide);
			section_LevelProperties.Initialize(_ide);
			section_ProjectInfo.Initialize(_ide);
			section_PluginList.Initialize(_ide);

			// Collapse the "Plugins" section if the engine doesn't support plugins
			if (_ide.Project.GameVersion == GameVersion.TR4 || _ide.Project.GameVersion == GameVersion.TR5)
				splitContainer_Info.Panel2Collapsed = true;
		}

		// Deleting .prj2 files is critical, so watch out
		private void prj2FileWatcher_Deleted(object sender, FileSystemEventArgs e) =>
			_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		private void levelFolderWatcher_Deleted(object sender, FileSystemEventArgs e) =>
			_ide.RaiseEvent(new IDE.PRJ2FileDeletedEvent());

		private void dllFileWatcher_Deleted(object sender, FileSystemEventArgs e)
		{
			if (e.Name.ToLower().StartsWith("plugin_"))
				_ide.RaiseEvent(new IDE.PluginDeletedEvent());
		}
	}
}
