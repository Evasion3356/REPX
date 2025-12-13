using System;
using HarmonyLib;
using REPX.Data;

namespace REPX.Patches.UI
{
	// Token: 0x0200000B RID: 11
	[HarmonyPatch(typeof(InputManager))]
	internal class InputManagerPatch
	{
		// Token: 0x06000052 RID: 82 RVA: 0x000058E8 File Offset: 0x00003AE8
		[HarmonyPatch("KeyHold")]
		[HarmonyPrefix]
		private static bool KeyHold_Prefix(InputKey key)
		{
			return InputManagerPatch.CheckKey(key);
		}

		// Token: 0x06000053 RID: 83 RVA: 0x00005900 File Offset: 0x00003B00
		[HarmonyPatch("KeyDown")]
		[HarmonyPrefix]
		private static bool KeyDown_Prefix(InputKey key)
		{
			return InputManagerPatch.CheckKey(key);
		}

		// Token: 0x06000054 RID: 84 RVA: 0x00005918 File Offset: 0x00003B18
		[HarmonyPatch("KeyUp")]
		[HarmonyPrefix]
		private static bool KeyUp_Prefix(InputKey key)
		{
			return InputManagerPatch.CheckKey(key);
		}

		// Token: 0x06000055 RID: 85 RVA: 0x00005930 File Offset: 0x00003B30
		[HarmonyPatch("GetMovement")]
		[HarmonyPrefix]
		private static bool GetMovement_Prefix()
		{
			return InputManagerPatch.CheckMovement();
		}

		// Token: 0x06000056 RID: 86 RVA: 0x00005948 File Offset: 0x00003B48
		[HarmonyPatch("GetMovementY")]
		[HarmonyPrefix]
		private static bool GetMovementY_Prefix()
		{
			return InputManagerPatch.CheckMovement();
		}

		// Token: 0x06000057 RID: 87 RVA: 0x00005960 File Offset: 0x00003B60
		[HarmonyPatch("GetMovementX")]
		[HarmonyPrefix]
		private static bool GetMovementX_Prefix()
		{
			return InputManagerPatch.CheckMovement();
		}

		// Token: 0x06000058 RID: 88 RVA: 0x00005978 File Offset: 0x00003B78
		[HarmonyPatch("GetMouseX")]
		[HarmonyPrefix]
		private static bool GetMouseX_Prefix()
		{
			return InputManagerPatch.CheckMouseMovement();
		}

		// Token: 0x06000059 RID: 89 RVA: 0x00005990 File Offset: 0x00003B90
		[HarmonyPatch("GetMouseY")]
		[HarmonyPrefix]
		private static bool GetMouseY_Prefix()
		{
			return InputManagerPatch.CheckMouseMovement();
		}

		// Token: 0x0600005A RID: 90 RVA: 0x000059A8 File Offset: 0x00003BA8
		private static bool CheckKey(InputKey key)
		{
			bool b_IsMenuOpen = Settings.Instance.b_IsMenuOpen;
			if (b_IsMenuOpen)
			{
				bool b_IgnoreChat = Settings.Instance.SettingsData.b_IgnoreChat;
				bool flag = b_IgnoreChat;
				bool flag2;
				if (flag)
				{
					flag2 = key == InputKey.Chat || key == InputKey.Confirm || key == InputKey.ChatDelete;
					flag = flag2;
				}
				bool flag3 = flag;
				if (flag3)
				{
					return false;
				}
				bool b_IgnoreMovement = Settings.Instance.SettingsData.b_IgnoreMovement;
				bool flag4 = b_IgnoreMovement;
				if (flag4)
				{
					flag2 = key <= InputKey.Jump || key == InputKey.Crouch || key == InputKey.Tumble;
					flag4 = flag2;
				}
				bool flag5 = flag4;
				if (flag5)
				{
					return false;
				}
				flag2 = key - InputKey.SpectateNext <= 1;
				bool flag6 = flag2;
				if (flag6)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600005B RID: 91 RVA: 0x00005A78 File Offset: 0x00003C78
		private static bool CheckMovement()
		{
			bool b_IsMenuOpen = Settings.Instance.b_IsMenuOpen;
			if (b_IsMenuOpen)
			{
				bool b_IgnoreMovement = Settings.Instance.SettingsData.b_IgnoreMovement;
				if (b_IgnoreMovement)
				{
					return false;
				}
			}
			return true;
		}

		// Token: 0x0600005C RID: 92 RVA: 0x00005AB4 File Offset: 0x00003CB4
		private static bool CheckMouseMovement()
		{
			bool b_IsMenuOpen = Settings.Instance.b_IsMenuOpen;
			if (b_IsMenuOpen)
			{
				bool b_IgnoreMouseMovement = Settings.Instance.SettingsData.b_IgnoreMouseMovement;
				if (b_IgnoreMouseMovement)
				{
					return false;
				}
			}
			return true;
		}
	}
}
