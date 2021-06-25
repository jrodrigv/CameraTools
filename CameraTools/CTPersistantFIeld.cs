using System;
using System.IO;
using UnityEngine;

namespace CameraTools
{
	[AttributeUsage(AttributeTargets.Field)]
	public class CTPersistantField : Attribute
	{
		static string oldSettingsURL = "GameData/CameraTools/settings.cfg"; // Migrate from the old settings file to the new one in PluginData so that we don't invalidate the ModuleManager cache.
		public static string settingsURL = "GameData/CameraTools/PluginData/settings.cfg";

		public CTPersistantField() { }

		public static void Save()
		{
			ConfigNode fileNode = ConfigNode.Load(settingsURL);
			if (fileNode == null)
				fileNode = new ConfigNode();

			if (!fileNode.HasNode("CToolsSettings"))
				fileNode.AddNode("CToolsSettings");

			ConfigNode settings = fileNode.GetNode("CToolsSettings");

			foreach (var field in typeof(CamTools).GetFields())
			{
				if (field == null) continue;
				if (!field.IsDefined(typeof(CTPersistantField), false)) continue;

				settings.SetValue(field.Name, field.GetValue(CamTools.fetch).ToString(), true);
			}

			if (!Directory.GetParent(settingsURL).Exists)
			{ Directory.GetParent(settingsURL).Create(); }
			var success = fileNode.Save(settingsURL);
			if (success && File.Exists(oldSettingsURL)) // Remove the old settings if it exists and the new settings were saved.
			{ File.Delete(oldSettingsURL); }
		}

		public static void Load()
		{
			ConfigNode fileNode = ConfigNode.Load(settingsURL);
			if (fileNode == null)
			{
				fileNode = ConfigNode.Load(oldSettingsURL); // Try the old location.
				if (fileNode == null)
					return; // No config file.
				Debug.LogWarning("[CameraTools]: Loading settings from old config file. New config file is now in GameData/CameraTools/PluginData to improve compatibility with ModuleManager.");
			}

			if (fileNode.HasNode("CToolsSettings"))
			{
				ConfigNode settings = fileNode.GetNode("CToolsSettings");

				foreach (var field in typeof(CamTools).GetFields())
				{
					if (field == null) continue;
					if (!field.IsDefined(typeof(CTPersistantField), false)) continue;

					if (settings.HasValue(field.Name))
					{
						object parsedValue = ParseValue(field.FieldType, settings.GetValue(field.Name));
						if (parsedValue != null)
						{
							field.SetValue(CamTools.fetch, parsedValue);
						}
					}
				}
			}
		}

		public static object ParseValue(Type type, string value)
		{
			if (type == typeof(string))
			{
				return value;
			}

			if (type == typeof(bool))
			{
				return bool.Parse(value);
			}
			else if (type.IsEnum)
			{
				return Enum.Parse(type, value);
			}
			else if (type == typeof(float))
			{
				return float.Parse(value);
			}
			else if (type == typeof(Single))
			{
				return Single.Parse(value);
			}


			UnityEngine.Debug.LogError("[CameraTools]: Failed to parse settings field of type " + type.ToString() + " and value " + value);

			return null;
		}
	}
}

