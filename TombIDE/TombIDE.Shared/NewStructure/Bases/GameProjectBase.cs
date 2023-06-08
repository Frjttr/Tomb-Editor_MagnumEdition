﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TombIDE.Shared.NewStructure.Implementations;
using TombLib.LevelData;

namespace TombIDE.Shared.NewStructure
{
	/// <summary>
	/// Base class for all game project types.
	/// </summary>
	public abstract class GameProjectBase : IGameProject
	{
		#region Abstract region

		public abstract TRVersion.Game GameVersion { get; }

		public abstract string DataFileExtension { get; }
		public abstract string EngineExecutableFileName { get; }
		public abstract string MainScriptFilePath { get; }

		public abstract string GetDefaultGameLanguageFilePath();
		public abstract void SetScriptRootDirectory(string newDirectoryPath);

		#endregion Abstract region

		public Version TargetTrprojVersion { get; set; } = new(1, 0);

		public string Name { get; protected set; }
		public string DirectoryPath { get; protected set; }

		public string LevelsDirectoryPath { get; set; }
		public string ScriptDirectoryPath { get; protected set; }
		public string PluginsDirectoryPath { get; set; }

		public string DefaultGameLanguageName { get; set; } = "English";

		public List<string> ExternalLevelFilePaths { get; set; } = new();
		public List<string> GameLanguageNames { get; set; } = new() { "English" };

		public GameProjectBase(TrprojFile trproj, Version targetTrprojVersion)
		{
			TargetTrprojVersion = targetTrprojVersion;

			Name = trproj.ProjectName;
			DirectoryPath = Path.GetDirectoryName(trproj.FilePath);

			LevelsDirectoryPath = trproj.LevelsDirectoryPath;
			ScriptDirectoryPath = trproj.ScriptDirectoryPath;
			PluginsDirectoryPath = trproj.PluginsDirectoryPath;

			DefaultGameLanguageName = trproj.DefaultGameLanguageName;

			ExternalLevelFilePaths = trproj.ExternalLevelFilePaths;
			GameLanguageNames = trproj.GameLanguageNames;
		}

		public GameProjectBase(string name, string directoryPath, string levelsDirectoryPath, string scriptDirectoryPath, string pluginsDirectoryPath = null)
		{
			Name = name;
			DirectoryPath = directoryPath;
			LevelsDirectoryPath = levelsDirectoryPath;
			ScriptDirectoryPath = scriptDirectoryPath;
			PluginsDirectoryPath = pluginsDirectoryPath;
		}

		public virtual string GetTrprojFilePath()
			=> Path.Combine(DirectoryPath, Path.GetFileNameWithoutExtension(GetEngineExecutableFilePath()) + ".trproj");

		public virtual string GetLauncherFilePath()
		{
			string launcherFilePath = Directory.EnumerateFiles(DirectoryPath)
				.Where(filePath => FileVersionInfo.GetVersionInfo(filePath).OriginalFilename == "launch.exe")
				.FirstOrDefault();

			if (string.IsNullOrEmpty(launcherFilePath))
			{
				launcherFilePath = GetEngineExecutableFilePath(); // Potentially a legacy project

				if (string.IsNullOrEmpty(launcherFilePath))
					throw new FileNotFoundException("Couldn't find a valid game launching executable."); // Very unlikely to happen
			}

			return launcherFilePath;
		}

		public virtual string GetEngineRootDirectoryPath()
		{
			string engineDirectoryPath = Path.Combine(DirectoryPath, "Engine");

			return Directory.Exists(engineDirectoryPath)
				? engineDirectoryPath // Modern project
				: DirectoryPath; // Legacy project
		}

		public virtual string GetEngineExecutableFilePath()
		{
			string engineExecutableFilePath = Path.Combine(GetEngineRootDirectoryPath(), EngineExecutableFileName);

			return File.Exists(engineExecutableFilePath)
				? engineExecutableFilePath
				: throw new FileNotFoundException("The engine executable file could not be found.");
		}

		public virtual FileInfo[] GetAllValidTrlevFiles()
		{
			var result = new List<FileInfo>();

			result.AddRange(
				from filePath in ExternalLevelFilePaths
				where File.Exists(filePath)
				select new FileInfo(filePath)
			);

			var levelsDirectoryInfo = new DirectoryInfo(LevelsDirectoryPath);

			foreach (DirectoryInfo levelDirectoryInfo in levelsDirectoryInfo.GetDirectories("*", SearchOption.TopDirectoryOnly))
			{
				FileInfo[] trlevFiles = levelDirectoryInfo.GetFiles("*.trlev", SearchOption.TopDirectoryOnly);

				if (trlevFiles.Length is 0 or > 1)
					continue;
				else
					result.Add(trlevFiles[0]);
			}

			return result.ToArray();
		}

		public virtual LevelProject[] GetAllValidLevelProjects()
		{
			var result = new List<LevelProject>();

			foreach (FileInfo trlevFile in GetAllValidTrlevFiles())
			{
				try { result.Add(LevelProject.FromTrlev(trlevFile.FullName)); }
				catch { }
			}

			return result.ToArray();
		}

		public virtual void Rename(string newName, bool renameDirectory)
		{
			if (renameDirectory)
			{
				string newProjectPath = Path.Combine(Path.GetDirectoryName(DirectoryPath), newName);
				Directory.Move(DirectoryPath, newProjectPath);

				if (ScriptDirectoryPath.StartsWith(DirectoryPath))
					ScriptDirectoryPath = Path.Combine(newProjectPath, ScriptDirectoryPath.Remove(0, DirectoryPath.Length + 1));

				if (LevelsDirectoryPath.StartsWith(DirectoryPath))
					LevelsDirectoryPath = Path.Combine(newProjectPath, LevelsDirectoryPath.Remove(0, DirectoryPath.Length + 1));

				for (int i = 0; i < ExternalLevelFilePaths.Count; i++)
				{
					if (ExternalLevelFilePaths[i].StartsWith(DirectoryPath))
						ExternalLevelFilePaths[i] = Path.Combine(newProjectPath, ExternalLevelFilePaths[i].Remove(0, DirectoryPath.Length + 1));
				}

				DirectoryPath = newProjectPath;
			}

			Name = newName;
		}

		public virtual bool IsValid(out string errorMessage)
		{
			errorMessage = string.Empty;

			if (!Directory.Exists(DirectoryPath))
			{
				errorMessage = "Project directory doesn't exist.";
				return false;
			}

			if (Path.GetFileName(DirectoryPath).Equals("Engine", StringComparison.OrdinalIgnoreCase))
			{
				errorMessage = "Directory name cannot be \"Engine\"."; // LOL you ain't tricking me
				return false;
			}

			if (!Directory.Exists(ScriptDirectoryPath))
			{
				errorMessage = "The project's Script directory is missing.";
				return false;
			}

			if (!Directory.Exists(LevelsDirectoryPath))
			{
				errorMessage = "The project's Levels directory is missing.";
				return false;
			}

			try
			{
				GetEngineExecutableFilePath();
			}
			catch (Exception ex)
			{
				errorMessage = ex.Message;
				return false;
			}

			return true;
		}

		public virtual void Save()
		{
			if (TargetTrprojVersion == new Version(1, 0))
			{
				// We save the project as a LEGACY .trproj file, since we don't want to enforce new structure yet
				// We simply want to get ready for people to easily migrate in the future while keeping backwards compatibility

				var trproj = new LegacyTrprojFile
				{
					Name = Name,
					GameVersion = GameVersion,
					LevelsPath = LevelsDirectoryPath,
					ScriptPath = ScriptDirectoryPath,
					LaunchFilePath = GetLauncherFilePath()
				};

				foreach (LevelProject levelProject in GetAllValidLevelProjects())
				{
					levelProject.Save();

					trproj.Levels.Add(new LegacyProjectLevel
					{
						Name = levelProject.Name,
						FolderPath = levelProject.DirectoryPath,
						SpecificFile = levelProject.TargetPrj2FileName
					});
				}

				trproj.WriteToFile(GetTrprojFilePath());
			}
			else if (TargetTrprojVersion == new Version(2, 0))
			{
				var trproj = new TrprojFile
				{
					ProjectName = Name,
					TargetGameVersion = GameVersion,

					LevelsDirectoryPath = LevelsDirectoryPath,
					ScriptDirectoryPath = ScriptDirectoryPath,
					PluginsDirectoryPath = PluginsDirectoryPath,

					DefaultGameLanguageName = DefaultGameLanguageName,

					ExternalLevelFilePaths = ExternalLevelFilePaths,
					GameLanguageNames = GameLanguageNames
				};

				trproj.WriteToFile(GetTrprojFilePath());
			}
		}

		public static IGameProject FromTrproj(string trprojFilePath)
		{
			var trproj = TrprojFile.FromFile(trprojFilePath, out Version targetTrprojVersion);

			return trproj.TargetGameVersion switch
			{
				TRVersion.Game.TR1 => new Tomb1MainGameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR2 => new TR2GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR3 => new TR3GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TR4 => new TR4GameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TRNG => new TRNGGameProject(trproj, targetTrprojVersion),
				TRVersion.Game.TombEngine => new TENGameProject(trproj, targetTrprojVersion),
				_ => throw new NotSupportedException("The specified .trproj file is for an unsupported game version.")
			};
		}
	}
}
