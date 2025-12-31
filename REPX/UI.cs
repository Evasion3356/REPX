using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using REPX.Data;
using UnityEngine;

namespace REPX
{
	internal static class UI
	{
		internal static void Reset()
		{
			UI.strTooltip = null;
		}

		internal static void TabContents(string strTabName, UI.Tabs tabToDisplay, Action tabContent)
		{
			bool flag = UI.nTab == tabToDisplay;
			if (flag)
			{
				bool flag2 = strTabName != null;
				if (flag2)
				{
					UI.Header(strTabName);
				}
				tabContent();
			}
		}

		internal static bool CenteredButton(string strName)
		{
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.FlexibleSpace();
			bool flag = GUILayout.Button(strName, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
			GUILayout.FlexibleSpace();
			GUILayout.EndHorizontal();
			return flag;
		}

		internal static void Tab<T>(string strTabName, ref T iTab, T iTabEle, bool bCenter = false)
		{
			bool flag = (bCenter ? UI.CenteredButton(strTabName) : GUILayout.Button(strTabName, Array.Empty<GUILayoutOption>()));
			if (flag)
			{
				iTab = iTabEle;
			}
		}

		internal static void Header(string str)
		{
			GUIStyle guistyle = new GUIStyle(GUI.skin.label)
			{
				alignment = TextAnchor.MiddleCenter,
				fontStyle = FontStyle.Bold,
				fontSize = 14
			};
			GUILayout.Space(25f);
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.Label(str, guistyle, Array.Empty<GUILayoutOption>());
			GUILayout.EndVertical();
		}

		internal static void Divider()
		{
			GUILayout.Space(25f);
			Texture2D texture2D = new Texture2D(1, 1);
			texture2D.SetPixel(0, 0, Color.gray);
			texture2D.Apply();
			GUILayout.Label("", new GUILayoutOption[]
			{
				GUILayout.Height(1f),
				GUILayout.ExpandWidth(true)
			});
			GUI.DrawTexture(GUILayoutUtility.GetLastRect(), texture2D);
			GUILayout.Space(25f);
		}

		internal static void ColorPicker(ref Color col, string label)
		{
			Color color = col;
			GUILayout.BeginVertical(GUI.skin.box, Array.Empty<GUILayoutOption>());
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label(label, Array.Empty<GUILayoutOption>());
			Rect rect = GUILayoutUtility.GetRect(50f, 20f);
			GUI.Box(rect, "", UI.GetStyleWithBackground(col));
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			col.r = GUILayout.HorizontalSlider(col.r, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(100f) });
			col.r = UI.ParseColorInput(GUILayout.TextField(Mathf.RoundToInt(col.r * 255f).ToString(), new GUILayoutOption[] { GUILayout.Width(40f) }), col.r);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			col.g = GUILayout.HorizontalSlider(col.g, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(100f) });
			col.g = UI.ParseColorInput(GUILayout.TextField(Mathf.RoundToInt(col.g * 255f).ToString(), new GUILayoutOption[] { GUILayout.Width(40f) }), col.g);
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			col.b = GUILayout.HorizontalSlider(col.b, 0f, 1f, new GUILayoutOption[] { GUILayout.Width(100f) });
			col.b = UI.ParseColorInput(GUILayout.TextField(Mathf.RoundToInt(col.b * 255f).ToString(), new GUILayoutOption[] { GUILayout.Width(40f) }), col.b);
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
			bool flag = color != col;
			if (flag)
			{
				bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
				if (b_AutoSave)
				{
					Settings.Instance.SaveSettings();
				}
			}
		}

		private static float ParseColorInput(string text, float currentValue)
		{
			int num;
			bool flag = int.TryParse(text, out num);
			float num2;
			if (flag)
			{
				num2 = Mathf.Clamp01((float)num / 255f);
			}
			else
			{
				num2 = currentValue;
			}
			return num2;
		}

		private static GUIStyle GetStyleWithBackground(Color col)
		{
			return new GUIStyle(GUI.skin.box)
			{
				normal = 
				{
					background = UI.MakeTex(1, 1, col)
				}
			};
		}

		internal static bool Checkbox(ref bool @bool, string label, string tooltip = "")
		{
			bool flag = @bool;
			@bool = GUILayout.Toggle(@bool, label, Array.Empty<GUILayoutOption>());
			bool flag2 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			if (flag2)
			{
				UI.strTooltip = tooltip;
			}
			bool flag3 = flag != @bool;
			if (flag3)
			{
				bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
				if (b_AutoSave)
				{
					Settings.Instance.SaveSettings();
				}
			}
			return flag != @bool;
		}

		internal static bool Togggle(ref bool @bool, Action callback, string label, string tooltip = "")
		{
			bool flag = @bool;
			@bool = GUILayout.Toggle(@bool, label, Array.Empty<GUILayoutOption>());
			bool flag2 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			if (flag2)
			{
				UI.strTooltip = tooltip;
			}
			bool flag3 = flag != @bool;
			if (flag3)
			{
				bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
				if (b_AutoSave)
				{
					Settings.Instance.SaveSettings();
				}
			}
			if (callback != null)
			{
				callback();
			}
			return flag != @bool;
		}

		internal static void Button(string label, string tooltip, Action action)
		{
			bool flag = GUILayout.Button(label, Array.Empty<GUILayoutOption>());
			if (flag)
			{
				action();
			}
			bool flag2 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			if (flag2)
			{
				UI.strTooltip = tooltip;
			}
		}

		internal static float Slider(ref int value, int min, int max, string label, string tooltip = "")
		{
			float num = (float)value;
			UI.Slider(ref num, (float)min, (float)max, label, tooltip, true);
			value = (int)num;
			return (float)value;
		}

		internal static float Slider(ref float value, float min, float max, string label, string tooltip = "", bool isInt = false)
		{
			float num = value;
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			string text = (isInt ? string.Format("{0}", (int)value) : string.Format("{0:F2}", value));
			GUILayout.Label(label + ": ", new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
			string text2 = GUILayout.TextField(text, new GUILayoutOption[] { GUILayout.Width(60f) });
			float num2;
			bool flag = float.TryParse(text2, out num2);
			if (flag)
			{
				value = Mathf.Clamp(num2, min, max);
				if (isInt)
				{
					value = Mathf.Round(value);
				}
			}
			float num3 = GUILayout.HorizontalSlider(value, min, max, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
			if (isInt)
			{
				num3 = Mathf.Round(num3);
			}
			value = num3;
			GUILayout.EndHorizontal();
			bool flag2 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			if (flag2)
			{
				UI.strTooltip = tooltip;
			}
			bool flag3 = num != value;
			if (flag3)
			{
				bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
				if (b_AutoSave)
				{
					Settings.Instance.SaveSettings();
				}
			}
			return value;
		}

		internal static string TextBox(ref string text, string label = "", string tooltip = "", int width = 200, int height = 0)
		{
			string text2 = text;
			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			bool flag = !string.IsNullOrEmpty(label);
			if (flag)
			{
				GUILayout.Label(label, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
			}
			bool flag2 = height > 0;
			string text3;
			if (flag2)
			{
				text3 = GUILayout.TextArea(text, new GUILayoutOption[]
				{
					GUILayout.Width((float)width),
					GUILayout.Height((float)height)
				});
			}
			else
			{
				text3 = GUILayout.TextField(text, new GUILayoutOption[] { GUILayout.Width((float)width) });
			}
			bool flag3 = text3 != text;
			if (flag3)
			{
				text = text3;
			}
			GUILayout.EndHorizontal();
			bool flag4 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
			if (flag4)
			{
				UI.strTooltip = tooltip;
			}
			bool flag5 = text2 != text;
			if (flag5)
			{
				bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
				if (b_AutoSave)
				{
					Settings.Instance.SaveSettings();
				}
			}
			return text;
		}

		internal static int Dropdown(ref int selectedIndex, string label, string[] options, string tooltip = "")
		{
			int num = selectedIndex;
			bool flag = options == null || options.Length == 0;
			int num2;
			if (flag)
			{
				selectedIndex = -1;
				num2 = selectedIndex;
			}
			else
			{
				selectedIndex = Mathf.Clamp(selectedIndex, 0, options.Length - 1);
				string text = label + "-" + string.Join(",", options);
				bool flag2 = !UI.dropdownStates.ContainsKey(text);
				if (flag2)
				{
					UI.dropdownStates[text] = new UI.DropdownState();
				}
				UI.DropdownState dropdownState = UI.dropdownStates[text];
				GUIStyle guistyle = new GUIStyle(GUI.skin.button)
				{
					fixedHeight = 30f,
					padding = new RectOffset(10, 10, 5, 5),
					margin = new RectOffset(2, 2, 2, 2),
					fontSize = 12
				};
				GUIStyle guistyle2 = new GUIStyle(GUI.skin.box)
				{
					normal = 
					{
						background = UI.MakeTex(2, 2, new Color(0.2f, 0.2f, 0.2f))
					},
					padding = new RectOffset(5, 5, 5, 5),
					margin = new RectOffset(0, 0, 0, 0)
				};
				GUILayout.BeginVertical(Array.Empty<GUILayoutOption>());
				GUILayout.Label(label, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				bool flag3 = GUILayout.Button(options[selectedIndex], guistyle, new GUILayoutOption[] { GUILayout.ExpandWidth(true) });
				if (flag3)
				{
					dropdownState.isExpanded = !dropdownState.isExpanded;
					Event.current.Use();
				}
				GUILayout.Label(dropdownState.isExpanded ? "▲" : "▼", new GUILayoutOption[]
				{
					GUILayout.Width(25f),
					GUILayout.Height(30f)
				});
				GUILayout.EndHorizontal();
				bool flag4 = GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition) && !string.IsNullOrEmpty(tooltip);
				if (flag4)
				{
					GUI.tooltip = tooltip;
				}
				bool isExpanded = dropdownState.isExpanded;
				if (isExpanded)
				{
					float num3 = 30f;
					float num4 = 2f;
					float num5 = Mathf.Min((float)options.Length * (num3 + num4), 400f);
					GUILayout.BeginVertical(guistyle2, Array.Empty<GUILayoutOption>());
					dropdownState.scrollPos = GUILayout.BeginScrollView(dropdownState.scrollPos, new GUILayoutOption[]
					{
						GUILayout.Height(num5),
						GUILayout.ExpandWidth(true)
					});
					for (int i = 0; i < options.Length; i++)
					{
						bool flag5 = i > 0;
						if (flag5)
						{
							GUILayout.Space(num4);
						}
						bool flag6 = GUILayout.Button(options[i], guistyle, new GUILayoutOption[]
						{
							GUILayout.Height(num3),
							GUILayout.ExpandWidth(true)
						});
						if (flag6)
						{
							selectedIndex = i;
							dropdownState.isExpanded = false;
							Event.current.Use();
						}
					}
					GUILayout.EndScrollView();
					GUILayout.EndVertical();
					bool flag7 = Event.current.type == EventType.MouseDown && !GUILayoutUtility.GetLastRect().Contains(Event.current.mousePosition);
					if (flag7)
					{
						dropdownState.isExpanded = false;
						Event.current.Use();
					}
				}
				GUILayout.EndVertical();
				bool flag8 = num != selectedIndex;
				if (flag8)
				{
					bool b_AutoSave = Settings.Instance.SettingsData.b_AutoSave;
					if (b_AutoSave)
					{
						Settings.Instance.SaveSettings();
					}
				}
				num2 = selectedIndex;
			}
			return num2;
		}

		private static Texture2D MakeTex(int width, int height, Color col)
		{
			Color[] array = new Color[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = col;
			}
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		internal static void RenderTooltip()
		{
			bool flag = !Settings.Instance.SettingsData.b_Tooltips || string.IsNullOrEmpty(UI.strTooltip);
			if (!flag)
			{
				GUIStyle label = GUI.skin.label;
				GUIContent guicontent = new GUIContent(UI.strTooltip);
				float num = label.CalcSize(guicontent).x + 10f;
				float num2 = label.CalcHeight(guicontent, num - 10f) + 10f;
				Vector2 mousePosition = Event.current.mousePosition;
				Color c_Theme = Settings.Instance.SettingsData.c_Theme;
				GUI.color = new Color(c_Theme.r, c_Theme.g, c_Theme.b, 0.8f);
				Rect rect = new Rect(mousePosition.x + 20f, mousePosition.y + 20f, num, num2);
				GUI.Box(rect, GUIContent.none);
				GUI.color = Color.white;
				GUI.Label(new Rect(rect.x + 5f, rect.y + 5f, num - 10f, num2 - 10f), UI.strTooltip);
			}
		}

		internal static void Keybind(ref int Key)
		{
			string text = "Unbound";
			bool flag = Key > 0;
			if (flag)
			{
				text = (UI.keyNames.ContainsKey(Key) ? UI.keyNames[Key] : "Unknown");
			}
			GUILayout.Button(text, Array.Empty<GUILayoutOption>());
			Rect lastRect = GUILayoutUtility.GetLastRect();
			Event current = Event.current;
			bool flag2 = lastRect.Contains(current.mousePosition);
			if (flag2)
			{
				for (int i = 0; i < 256; i++)
				{
					bool flag3 = i == 1 || i == 45;
					if (!flag3)
					{
						bool flag4 = i > 6 && Event.current.type != EventType.KeyDown;
						if (!flag4)
						{
							bool flag5 = (Utils.GetAsyncKeyState(i) & 1) != 0;
							if (flag5)
							{
								Key = ((i == 27) ? 0 : i);
								break;
							}
						}
					}
				}
			}
		}

		internal static Texture2D MakeTexture(int width, int height, Color color)
		{
			Color[] array = new Color[width * height];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = color;
			}
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		internal static Texture2D MakeGradientTexture(int width, int height, Color startColor, Color endColor, bool isHorizontal = true)
		{
			Color[] array = new Color[width * height];
			for (int i = 0; i < height; i++)
			{
				for (int j = 0; j < width; j++)
				{
					float num = (isHorizontal ? ((float)j / (float)(width - 1)) : ((float)i / (float)(height - 1)));
					array[i * width + j] = Color.Lerp(startColor, endColor, num);
				}
			}
			Texture2D texture2D = new Texture2D(width, height);
			texture2D.SetPixels(array);
			texture2D.Apply();
			return texture2D;
		}

		internal static UI.Tabs nTab = UI.Tabs.ESP;
		internal static string strTooltip = null;
		private static Dictionary<string, UI.DropdownState> dropdownStates = new Dictionary<string, UI.DropdownState>();
		private static readonly Dictionary<int, string> keyNames = new Dictionary<int, string>
		{
			{ 1, "Mouse1" }, { 2, "Mouse2" }, { 3, "Control-break processing" }, { 4, "Mouse3" }, { 5, "Mouse4" }, { 6, "Mouse5" },
			{ 8, "Backspace" }, { 9, "Tab" }, { 12, "Clear" }, { 13, "Enter" }, { 16, "Shift" }, { 17, "Ctrl" },
			{ 18, "Alt" }, { 19, "Pause" }, { 20, "Caps Lock" }, { 27, "Esc" }, { 32, "Spacebar" }, { 33, "Page Up" },
			{ 34, "Page Down" }, { 35, "End" }, { 36, "Home" }, { 37, "Left arrow" }, { 38, "Up arrow" }, { 39, "Right arrow" },
			{ 40, "Down arrow" }, { 45, "Insert" }, { 46, "Delete" }, { 48, "0" }, { 49, "1" }, { 50, "2" },
			{ 51, "3" }, { 52, "4" }, { 53, "5" }, { 54, "6" }, { 55, "7" }, { 56, "8" },
			{ 57, "9" }, { 65, "A" }, { 66, "B" }, { 67, "C" }, { 68, "D" }, { 69, "E" },
			{ 70, "F" }, { 71, "G" }, { 72, "H" }, { 73, "I" }, { 74, "J" }, { 75, "K" },
			{ 76, "L" }, { 77, "M" }, { 78, "N" }, { 79, "O" }, { 80, "P" }, { 81, "Q" },
			{ 82, "R" }, { 83, "S" }, { 84, "T" }, { 85, "U" }, { 86, "V" }, { 87, "W" },
			{ 88, "X" }, { 89, "Y" }, { 90, "Z" }, { 91, "Left Windows" }, { 92, "Right Windows" }, { 93, "Applications" },
			{ 95, "Sleep" }, { 96, "Numeric keypad 0" }, { 97, "Numeric keypad 1" }, { 98, "Numeric keypad 2" }, { 99, "Numeric keypad 3" },
			{ 100, "Numeric keypad 4" }, { 101, "Numeric keypad 5" }, { 102, "Numeric keypad 6" }, { 103, "Numeric keypad 7" }, { 104, "Numeric keypad 8" },
			{ 105, "Numeric keypad 9" }, { 106, "Multiply" }, { 107, "Add" }, { 108, "Separator" }, { 109, "Subtract" },
			{ 110, "Decimal" }, { 111, "Divide" }, { 112, "F1" }, { 113, "F2" }, { 114, "F3" },
			{ 115, "F4" }, { 116, "F5" }, { 117, "F6" }, { 118, "F7" }, { 119, "F8" },
			{ 120, "F9" }, { 121, "F10" }, { 122, "F11" }, { 123, "F12" }, { 124, "F13" },
			{ 125, "F14" }, { 126, "F15" }, { 127, "F16" }, { 128, "F17" }, { 129, "F18" },
			{ 130, "F19" }, { 131, "F20" }, { 132, "F21" }, { 133, "F22" }, { 134, "F23" },
			{ 135, "F24" }, { 144, "Num Lock" }, { 145, "Scroll Lock" }, { 160, "Left Shift" }, { 161, "Right Shift" },
			{ 162, "Left Ctrl" }, { 163, "Right Ctrl" }, { 164, "Left Alt" }, { 165, "Right Alt" }, { 186, "Semicolon" },
			{ 187, "Plus" }, { 188, "Comma" }, { 189, "Minus" }, { 190, "Period" }, { 191, "Forward slash" },
			{ 192, "Tilde" }, { 219, "Left bracket" }, { 220, "Backslash" }, { 221, "Right bracket" }, { 222, "Apostrophe" }
		};

		internal enum Tabs
		{
			ESP, Self, Players, Level, Misc, Settings
		}

		private class DropdownState
		{
			public bool isExpanded = false;
			public Vector2 scrollPos = Vector2.zero;
		}
	}
}
