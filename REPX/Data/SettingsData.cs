using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX.Data
{
	// Token: 0x0200001D RID: 29
	[Serializable]
	internal class SettingsData
	{
		// Token: 0x04000036 RID: 54
		public bool b_InfiniteStamina;

		// Token: 0x04000037 RID: 55
		public bool b_GodMode;

		// Token: 0x04000038 RID: 56
		public bool b_Invulnerable;

		// Token: 0x04000039 RID: 57
		public bool b_Invisibility;

		// Token: 0x0400003A RID: 58
		public bool b_AntiKnockBack;

		// Token: 0x0400003B RID: 59
		public bool b_NoTumble;

		// Token: 0x0400003C RID: 60
		public bool b_IndestructibleObjects;

		// Token: 0x0400003D RID: 61
		public bool b_HearEveryone;

		// Token: 0x0400003E RID: 62
		public bool b_Esp;

		// Token: 0x0400003F RID: 63
		public float f_EspRange = 500f;

		// Token: 0x04000040 RID: 64
		public bool b_Tracer;

		// Token: 0x04000041 RID: 65
		public bool b_ItemEsp;

		// Token: 0x04000042 RID: 66
		public bool b_ItemValueEsp;

		// Token: 0x04000043 RID: 67
		public bool b_PlayerEsp;

		// Token: 0x04000044 RID: 68
		public bool b_PlayerNameEsp;

		// Token: 0x04000045 RID: 69
		public bool b_EnemyEsp;

		// Token: 0x04000046 RID: 70
		public bool b_EnemyNameEsp;

		// Token: 0x04000047 RID: 71
		public bool b_Spoofing;

		// Token: 0x04000048 RID: 72
		public string s_SpoofedName = "Name";

		// Token: 0x04000049 RID: 73
		public string s_SpoofedSteamId = "";

		// Token: 0x0400004A RID: 74
		public bool b_FakePing;

		// Token: 0x0400004B RID: 75
		public int i_FakePingNum = 50;

		// Token: 0x0400004C RID: 76
		public int i_PlayerTriggerAction;

		// Token: 0x0400004D RID: 77
		public int i_ObjectTriggerAction;

		// Token: 0x0400004E RID: 78
		public bool b_IgnoreChat = true;

		// Token: 0x0400004F RID: 79
		public bool b_IgnoreMouseMovement = true;

		// Token: 0x04000050 RID: 80
		public bool b_IgnoreMovement;

		// Token: 0x04000051 RID: 81
		public bool b_AutoSave;

		// Token: 0x04000052 RID: 82
		public bool b_Tooltips = true;

		// Token: 0x04000053 RID: 83
		public Color c_Theme = new Color(1f, 1f, 1f, 1f);

		// Token: 0x04000054 RID: 84
		public Color c_PlayerEspColor = new Color(0.5f, 1f, 0.5f, 1f);

		// Token: 0x04000055 RID: 85
		public Color c_EnemyEspColor = Color.red;

		// Token: 0x04000056 RID: 86
		public Color c_ItemEspColorLow = Color.blue;
		public Color c_ItemEspColorMedium = new Color(0f, 0.5f, 1f, 1f);
		public Color c_ItemEspColorHigh = Color.cyan;
		public Color c_ItemEspColorDrone = Color.green;

		public Color c_CartEspColor = Color.yellow;

		public Color c_WeaponEspColor = Color.cyan;

		public bool b_LaserESP;

		public bool b_extractionESP;

		public bool b_truckESP;
	}
}
