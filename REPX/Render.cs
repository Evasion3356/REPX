using System;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX
{
	// Token: 0x0200000A RID: 10
	internal static class Render
	{
		// Cache commonly used values
		private static readonly float RadToDeg = 57.29578f;
		private static readonly float DegToRad = 0.017453292f;
		private static Texture2D _cachedWhiteTexture;
		private static Rect _tempRect = new Rect();

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

		private static Texture2D WhiteTexture
		{
			get
			{
				if (_cachedWhiteTexture == null)
				{
					_cachedWhiteTexture = Texture2D.whiteTexture;
				}
				return _cachedWhiteTexture;
			}
		}

		// Token: 0x06000047 RID: 71 RVA: 0x0000552D File Offset: 0x0000372D
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Line(Vector2 from, Vector2 to, float thickness, Color color)
		{
			Render.Color = color;
			Render.Line(from, to, thickness);
		}

		// Token: 0x06000048 RID: 72 RVA: 0x00005540 File Offset: 0x00003740
		internal static void Line(Vector2 from, Vector2 to, float thickness)
		{
			Vector2 delta = to - from;
			Vector2 normalized = delta.normalized;
			float angle = Mathf.Atan2(normalized.y, normalized.x) * RadToDeg;
			GUIUtility.RotateAroundPivot(angle, from);
			Render.Box(from, Vector2.right * delta.magnitude, thickness, false);
			GUIUtility.RotateAroundPivot(-angle, from);
		}

		// Token: 0x06000049 RID: 73 RVA: 0x000055A9 File Offset: 0x000037A9
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Box(Vector2 position, Vector2 size, float thickness, Color color, bool centered = true)
		{
			Render.Color = color;
			Render.Box(position, size, thickness, centered);
		}

		// Token: 0x0600004A RID: 74 RVA: 0x000055C0 File Offset: 0x000037C0
		internal static void Box(Vector2 position, Vector2 size, float thickness, bool centered = true)
		{
			// Use pre-allocated Rect to avoid GC allocations
			Texture2D tex = WhiteTexture;
			
			// Top line
			_tempRect.x = position.x;
			_tempRect.y = position.y;
			_tempRect.width = size.x;
			_tempRect.height = thickness;
			GUI.DrawTexture(_tempRect, tex);
			
			// Left line
			_tempRect.width = thickness;
			_tempRect.height = size.y;
			GUI.DrawTexture(_tempRect, tex);
			
			// Right line
			_tempRect.x = position.x + size.x;
			GUI.DrawTexture(_tempRect, tex);
			
			// Bottom line
			_tempRect.x = position.x;
			_tempRect.y = position.y + size.y;
			_tempRect.width = size.x + thickness;
			_tempRect.height = thickness;
			GUI.DrawTexture(_tempRect, tex);
		}

		// Token: 0x0600004B RID: 75 RVA: 0x00005682 File Offset: 0x00003882
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Cross(Vector2 position, Vector2 size, float thickness, Color color)
		{
			Render.Color = color;
			Render.Cross(position, size, thickness);
		}

		// Token: 0x0600004C RID: 76 RVA: 0x00005698 File Offset: 0x00003898
		internal static void Cross(Vector2 position, Vector2 size, float thickness)
		{
			Texture2D tex = WhiteTexture;
			float halfWidth = size.x * 0.5f;
			float halfHeight = size.y * 0.5f;
			
			// Horizontal line
			_tempRect.x = position.x - halfWidth;
			_tempRect.y = position.y;
			_tempRect.width = size.x;
			_tempRect.height = thickness;
			GUI.DrawTexture(_tempRect, tex);
			
			// Vertical line
			_tempRect.x = position.x;
			_tempRect.y = position.y - halfHeight;
			_tempRect.width = thickness;
			_tempRect.height = size.y;
			GUI.DrawTexture(_tempRect, tex);
		}

		// Token: 0x0600004D RID: 77 RVA: 0x00005706 File Offset: 0x00003906
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Dot(Vector2 position, Color color)
		{
			Render.Color = color;
			Render.Dot(position);
		}

		// Token: 0x0600004E RID: 78 RVA: 0x00005717 File Offset: 0x00003917
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		internal static void Dot(Vector2 position)
		{
			Render.Box(position - Vector2.one, Vector2.one * 2f, 1f, true);
		}

		// Token: 0x0600004F RID: 79 RVA: 0x00005740 File Offset: 0x00003940
		internal static void String(GUIStyle Style, float X, float Y, float W, float H, string str, Color col, bool centerx = false, bool centery = false)
		{
			GUIContent guicontent = new GUIContent(str);
			Vector2 vector = Style.CalcSize(guicontent);
			float num = centerx ? (X - vector.x * 0.5f) : X;
			float num2 = centery ? (Y - vector.y * 0.5f) : Y;
			
			// Draw shadow
			Style.normal.textColor = Color.black;
			_tempRect.x = num;
			_tempRect.y = num2;
			_tempRect.width = vector.x;
			_tempRect.height = H;
			GUI.Label(_tempRect, str, Style);
			
			// Draw main text
			Style.normal.textColor = col;
			_tempRect.x = num + 1f;
			_tempRect.y = num2 + 1f;
			GUI.Label(_tempRect, str, Style);
		}

		// Token: 0x06000050 RID: 80 RVA: 0x000057E8 File Offset: 0x000039E8
		internal static void Circle(Vector2 center, float radius, float thickness, Color color)
		{
			Render.Color = color;
			
			// Optimize segment count based on radius
			int segments = Mathf.Clamp((int)(radius * 2f), 12, 360);
			float angleStep = (360f / segments) * DegToRad;
			
			Vector2 previousPoint = center + new Vector2(radius, 0f);
			
			for (int i = 1; i <= segments; i++)
			{
				float angle = i * angleStep;
				float cosAngle = Mathf.Cos(angle);
				float sinAngle = Mathf.Sin(angle);
				Vector2 currentPoint = center + new Vector2(radius * cosAngle, radius * sinAngle);
				Render.Line(previousPoint, currentPoint, thickness);
				previousPoint = currentPoint;
			}
		}

		// Token: 0x06000051 RID: 81 RVA: 0x0000585C File Offset: 0x00003A5C
		internal static void FilledCircle(Vector2 center, float radius, Color color)
		{
			Render.Color = color;
			float radiusSquared = radius * radius;
			Texture2D tex = WhiteTexture;
			
			// Optimize by drawing horizontal lines instead of individual pixels
			for (float y = -radius; y <= radius; y += 1f)
			{
				float ySquared = y * y;
				float xMax = Mathf.Sqrt(radiusSquared - ySquared);
				
				if (xMax > 0f)
				{
					_tempRect.x = center.x - xMax;
					_tempRect.y = center.y + y;
					_tempRect.width = xMax * 2f;
					_tempRect.height = 1f;
					GUI.DrawTexture(_tempRect, tex);
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
				float angleStep = 360f / numSegments;
				
				for (int i = 0; i < numSegments; i++)
				{
					float angle = DegToRad * angleStep * i;
					this.Positions[i] = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle));
				}
			}
		}
	}
}
