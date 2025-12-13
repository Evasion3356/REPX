using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

namespace REPX
{
	// Token: 0x02000008 RID: 8
	internal static class Utils
	{
		// Token: 0x06000024 RID: 36
		[DllImport("User32.dll")]
		internal static extern short GetAsyncKeyState(int key);

		// Token: 0x06000025 RID: 37
		[DllImport("User32.dll")]
		internal static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

		// Token: 0x06000026 RID: 38 RVA: 0x00003D51 File Offset: 0x00001F51
		internal static void ShowMessageBox(string message)
		{
			Utils.MessageBox(IntPtr.Zero, message, "REPX", 0U);
		}

		// Token: 0x06000027 RID: 39 RVA: 0x00003D68 File Offset: 0x00001F68
		internal static void SetValue(object instance, string variableName, object value, BindingFlags bindingFlags)
		{
			Type type = instance.GetType();
			FieldInfo field = type.GetField(variableName, bindingFlags);
			if (field != null)
			{
				field.SetValue(instance, value);
			}
		}

		// Token: 0x06000028 RID: 40 RVA: 0x00003D94 File Offset: 0x00001F94
		internal static object GetValue(object instance, string variableName, BindingFlags bindingFlags)
		{
			Type type = instance.GetType();
			FieldInfo field = type.GetField(variableName, bindingFlags);
			bool flag = field != null;
			object obj;
			if (flag)
			{
				obj = field.GetValue(instance);
			}
			else
			{
				obj = null;
			}
			return obj;
		}

		// Token: 0x06000029 RID: 41 RVA: 0x00003DCC File Offset: 0x00001FCC
		internal static object CallMethod(object instance, string methodName, BindingFlags bindingFlags, params object[] parameters)
		{
			Type type = instance.GetType();
			MethodInfo method = type.GetMethod(methodName, bindingFlags);
			bool flag = method != null;
			object obj2;
			if (flag)
			{
				object obj = method.Invoke(instance, parameters);
				obj2 = obj;
			}
			else
			{
				obj2 = null;
			}
			return obj2;
		}

		// Token: 0x0600002A RID: 42 RVA: 0x00003E0C File Offset: 0x0000200C
		internal static string ConvertFirstLetterToUpperCase(string input)
		{
			bool flag = string.IsNullOrEmpty(input);
			string text;
			if (flag)
			{
				text = input;
			}
			else
			{
				TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
				text = textInfo.ToTitleCase(input);
			}
			return text;
		}

		// Token: 0x0600002B RID: 43 RVA: 0x00003E40 File Offset: 0x00002040
		internal static string TruncateString(string inputStr, int charLimit)
		{
			bool flag = inputStr.Length <= charLimit;
			string text;
			if (flag)
			{
				text = inputStr;
			}
			else
			{
				text = inputStr.Substring(0, charLimit - 3) + "...";
			}
			return text;
		}

		// Token: 0x0600002C RID: 44 RVA: 0x00003E7C File Offset: 0x0000207C
		internal static bool WorldToScreen(Camera camera, Vector3 world, out Vector3 screen)
		{
			screen = camera.WorldToViewportPoint(world);
			screen.x *= (float)Screen.width;
			screen.y *= (float)Screen.height;
			screen.y = (float)Screen.height - screen.y;
			return screen.z > 0f;
		}

		// Token: 0x0600002D RID: 45 RVA: 0x00003EDC File Offset: 0x000020DC
		internal static float GetDistance(Vector3 pos1, Vector3 pos2)
		{
			return (float)Math.Round((double)Vector3.Distance(pos1, pos2));
		}

		// Token: 0x04000024 RID: 36
		internal static BindingFlags protectedFlags = BindingFlags.Instance | BindingFlags.NonPublic;
	}
}
