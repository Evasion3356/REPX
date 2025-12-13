using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX
{
	// Token: 0x0200000A RID: 10
	internal static class Render
	{
		// Token: 0x17000002 RID: 2
		// (get) Token: 0x06000045 RID: 69 RVA: 0x0000550C File Offset: 0x0000370C
		// (set) Token: 0x06000046 RID: 70 RVA: 0x00005523 File Offset: 0x00003723
		internal static Color Color
		{
			get
			{
				return GUI.color;
			}
			set
			{
				GUI.color = value;
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000552D File Offset: 0x0000372D
		internal static void Line(Vector2 from, Vector2 to, float thickness, Color color)
		{
			Render.Color = color;
			Render.Line(from, to, thickness);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00005540 File Offset: 0x00003740
		internal static void Line(Vector2 from, Vector2 to, float thickness)
		{
			Vector2 normalized = (to - from).normalized;
			float num = Mathf.Atan2(normalized.y, normalized.x) * 57.29578f;
			GUIUtility.RotateAroundPivot(num, from);
			Render.Box(from, Vector2.right * (from - to).magnitude, thickness, false);
			GUIUtility.RotateAroundPivot(-num, from);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000055A9 File Offset: 0x000037A9
		internal static void Box(Vector2 position, Vector2 size, float thickness, Color color, bool centered = true)
		{
			Render.Color = color;
			Render.Box(position, size, thickness, centered);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000055C0 File Offset: 0x000037C0
		internal static void Box(Vector2 position, Vector2 size, float thickness, bool centered = true)
		{
			Vector2 vector = (centered ? (position - size / 2f) : position);
			GUI.DrawTexture(new Rect(position.x, position.y, size.x, thickness), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(position.x, position.y, thickness, size.y), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(position.x + size.x, position.y, thickness, size.y), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(position.x, position.y + size.y, size.x + thickness, thickness), Texture2D.whiteTexture);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00005682 File Offset: 0x00003882
		internal static void Cross(Vector2 position, Vector2 size, float thickness, Color color)
		{
			Render.Color = color;
			Render.Cross(position, size, thickness);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00005698 File Offset: 0x00003898
		internal static void Cross(Vector2 position, Vector2 size, float thickness)
		{
			GUI.DrawTexture(new Rect(position.x - size.x / 2f, position.y, size.x, thickness), Texture2D.whiteTexture);
			GUI.DrawTexture(new Rect(position.x, position.y - size.y / 2f, thickness, size.y), Texture2D.whiteTexture);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00005706 File Offset: 0x00003906
		internal static void Dot(Vector2 position, Color color)
		{
			Render.Color = color;
			Render.Dot(position);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00005717 File Offset: 0x00003917
		internal static void Dot(Vector2 position)
		{
			Render.Box(position - Vector2.one, Vector2.one * 2f, 1f, true);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00005740 File Offset: 0x00003940
		internal static void String(GUIStyle Style, float X, float Y, float W, float H, string str, Color col, bool centerx = false, bool centery = false)
		{
			GUIContent guicontent = new GUIContent(str);
			Vector2 vector = Style.CalcSize(guicontent);
			float num = (centerx ? (X - vector.x / 2f) : X);
			float num2 = (centery ? (Y - vector.y / 2f) : Y);
			Style.normal.textColor = Color.black;
			GUI.Label(new Rect(num, num2, vector.x, H), str, Style);
			Style.normal.textColor = col;
			GUI.Label(new Rect(num + 1f, num2 + 1f, vector.x, H), str, Style);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000057E8 File Offset: 0x000039E8
		internal static void Circle(Vector2 center, float radius, float thickness, Color color)
		{
			Render.Color = color;
			Vector2 vector = center + new Vector2(radius, 0f);
			for (int i = 1; i <= 360; i++)
			{
				float num = (float)i * 0.017453292f;
				Vector2 vector2 = center + new Vector2(radius * Mathf.Cos(num), radius * Mathf.Sin(num));
				Render.Line(vector, vector2, thickness);
				vector = vector2;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x0000585C File Offset: 0x00003A5C
		internal static void FilledCircle(Vector2 center, float radius, Color color)
		{
			Render.Color = color;
			float num = radius * radius;
			for (float num2 = -radius; num2 <= radius; num2 += 1f)
			{
				for (float num3 = -radius; num3 <= radius; num3 += 1f)
				{
					bool flag = num3 * num3 + num2 * num2 <= num;
					if (flag)
					{
						Render.Line(center + new Vector2(num3, num2), center + new Vector2(num3 + 1f, num2), 1f);
					}
				}
			}
		}

		// Token: 0x0200002B RID: 43
		private class RingArray
		{
			// Token: 0x17000008 RID: 8
			// (get) Token: 0x060000E2 RID: 226 RVA: 0x00008C90 File Offset: 0x00006E90
			// (set) Token: 0x060000E3 RID: 227 RVA: 0x00008C98 File Offset: 0x00006E98
			internal Vector2[] Positions { get; private set; }

			// Token: 0x060000E4 RID: 228 RVA: 0x00008CA4 File Offset: 0x00006EA4
			internal RingArray(int numSegments)
			{
				this.Positions = new Vector2[numSegments];
				float num = 360f / (float)numSegments;
				for (int i = 0; i < numSegments; i++)
				{
					float num2 = 0.017453292f * num * (float)i;
					this.Positions[i] = new Vector2(Mathf.Sin(num2), Mathf.Cos(num2));
				}
			}
		}
	}
}
