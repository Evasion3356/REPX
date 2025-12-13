using System;
using System.IO;
using System.Runtime.CompilerServices;
using REPX.Helpers;
using UnityEngine;

namespace REPX.Data
{
	internal class Settings
	{
		// Token: 0x17000005 RID: 5
		// (get) Token: 0x060000A6 RID: 166 RVA: 0x0000705E File Offset: 0x0000525E
		// (set) Token: 0x060000A7 RID: 167 RVA: 0x00007066 File Offset: 0x00005266
		internal SettingsData SettingsData { get; private set; }

		// Token: 0x17000006 RID: 6
		// (get) Token: 0x060000A8 RID: 168 RVA: 0x00007070 File Offset: 0x00005270
		internal static Settings Instance
		{
			get
			{
				if (Settings._instance == null)
				{
					Settings._instance = new Settings();
				}
				return Settings._instance;
			}
		}

		// Token: 0x060000A9 RID: 169 RVA: 0x00007098 File Offset: 0x00005298
		internal Settings()
		{
			this._settingsFilePath = Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? Application.dataPath, "REPX-Settings.json");
			this.LoadSettings();
		}

		// Token: 0x060000AA RID: 170 RVA: 0x000070F8 File Offset: 0x000052F8
		private void LoadSettings()
		{
			try
			{
				bool flag = File.Exists(this._settingsFilePath);
				if (flag)
				{
					string text = File.ReadAllText(this._settingsFilePath);
					this.SettingsData = JsonUtility.FromJson<SettingsData>(text) ?? new SettingsData();
					Log.LogInfo("Successfully loaded settings!", "Log");
					this.SaveSettings();
				}
				else
				{
					this.SettingsData = new SettingsData();
					this.SaveSettings();
				}
			}
			catch (Exception ex)
			{
				Log.LogError(string.Format("[REPX] Failed to load settings: {0}", ex));
				this.SettingsData = new SettingsData();
			}
		}

		// Token: 0x060000AB RID: 171 RVA: 0x000071A0 File Offset: 0x000053A0
		internal void SaveSettings()
		{
			try
			{
				string text = JsonUtility.ToJson(this.SettingsData, true);
				File.WriteAllText(this._settingsFilePath, text);
			}
			catch (Exception ex)
			{
				Log.LogError(string.Format("[REPX] Failed to save settings: {0}", ex));
			}
		}

		// Token: 0x04000057 RID: 87
		private const string SettingsFileName = "REPX-Settings.json";

		// Token: 0x04000058 RID: 88
		private readonly string _settingsFilePath;

		// Token: 0x04000059 RID: 89
		internal static readonly float TextHeight = 30f;

		// Token: 0x0400005A RID: 90
		internal Rect WindowRect = new Rect(50f, 50f, 545f, 715f);

		// Token: 0x0400005B RID: 91
		internal bool b_IsMenuOpen;

		// Token: 0x0400005D RID: 93
		private static Settings _instance;
	}
}
