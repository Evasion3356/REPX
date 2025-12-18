using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace REPX
{
	internal static class ExternalRender
	{
		private static IntPtr currentHdc;
		private static uint currentColor = 0xFFFFFF;

		// Called at the start of each frame
		internal static void BeginFrame(IntPtr hdc)
		{
			currentHdc = hdc;
			ExternalWindow.SetBkMode(hdc, ExternalWindow.TRANSPARENT);
		}

		internal static Color Color
		{
			get
			{
				// Convert BGR back to Color (not really needed but for compatibility)
				byte r = (byte)(currentColor & 0xFF);
				byte g = (byte)((currentColor >> 8) & 0xFF);
				byte b = (byte)((currentColor >> 16) & 0xFF);
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			}
			set
			{
				currentColor = ExternalWindow.ColorToBGR(value);
				if (currentHdc != IntPtr.Zero)
				{
					ExternalWindow.SetTextColor(currentHdc, currentColor);
				}
			}
		}

		internal static void Line(Vector2 from, Vector2 to, float thickness, Color color)
		{
			Color = color;
			Line(from, to, thickness);
		}

		internal static void Line(Vector2 from, Vector2 to, float thickness)
		{
			if (currentHdc == IntPtr.Zero) return;

			IntPtr pen = ExternalWindow.CreatePen(0, (int)thickness, currentColor);
			IntPtr oldPen = ExternalWindow.SelectObject(currentHdc, pen);

			ExternalWindow.MoveToEx(currentHdc, (int)from.x, (int)from.y, IntPtr.Zero);
			ExternalWindow.LineTo(currentHdc, (int)to.x, (int)to.y);

			ExternalWindow.SelectObject(currentHdc, oldPen);
			ExternalWindow.DeleteObject(pen);
		}

		internal static void Box(Vector2 position, Vector2 size, float thickness, Color color, bool centered = true)
		{
			Color = color;
			Box(position, size, thickness, centered);
		}

		internal static void Box(Vector2 position, Vector2 size, float thickness, bool centered = true)
		{
			if (currentHdc == IntPtr.Zero) return;

			Vector2 topLeft = centered ? (position - size / 2f) : position;

			// Draw four lines for the box
			Line(new Vector2(topLeft.x, topLeft.y), new Vector2(topLeft.x + size.x, topLeft.y), thickness);
			Line(new Vector2(topLeft.x, topLeft.y), new Vector2(topLeft.x, topLeft.y + size.y), thickness);
			Line(new Vector2(topLeft.x + size.x, topLeft.y), new Vector2(topLeft.x + size.x, topLeft.y + size.y), thickness);
			Line(new Vector2(topLeft.x, topLeft.y + size.y), new Vector2(topLeft.x + size.x, topLeft.y + size.y), thickness);
		}

		internal static void Cross(Vector2 position, Vector2 size, float thickness, Color color)
		{
			Color = color;
			Cross(position, size, thickness);
		}

		internal static void Cross(Vector2 position, Vector2 size, float thickness)
		{
			if (currentHdc == IntPtr.Zero) return;

			Line(new Vector2(position.x - size.x / 2f, position.y), new Vector2(position.x + size.x / 2f, position.y), thickness);
			Line(new Vector2(position.x, position.y - size.y / 2f), new Vector2(position.x, position.y + size.y / 2f), thickness);
		}

		internal static void Dot(Vector2 position, Color color)
		{
			Color = color;
			Dot(position);
		}

		internal static void Dot(Vector2 position)
		{
			Box(position - Vector2.one, Vector2.one * 2f, 1f, true);
		}

		internal static void String(float X, float Y, float W, float H, string str, Color col, bool centerx = false, bool centery = false)
		{
			if (currentHdc == IntPtr.Zero || string.IsNullOrEmpty(str)) return;

			Color = col;
			
			// Simple centering approximation (GDI doesn't have CalcSize equivalent)
			float estimatedWidth = str.Length * 8; // Rough estimate
			float x = centerx ? (X - estimatedWidth / 2f) : X;
			float y = centery ? (Y - 8) : Y; // Rough height estimate

			ExternalWindow.TextOut(currentHdc, (int)x, (int)y, str, str.Length);
		}

		internal static void Circle(Vector2 center, float radius, float thickness, Color color)
		{
			Color = color;
			Vector2 prev = center + new Vector2(radius, 0f);
			
			for (int i = 1; i <= 360; i++)
			{
				float angle = (float)i * 0.017453292f;
				Vector2 current = center + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
				Line(prev, current, thickness);
				prev = current;
			}
		}

		internal static void FilledCircle(Vector2 center, float radius, Color color)
		{
			Color = color;
			float radiusSquared = radius * radius;
			
			for (float y = -radius; y <= radius; y += 1f)
			{
				for (float x = -radius; x <= radius; x += 1f)
				{
					if (x * x + y * y <= radiusSquared)
					{
						Line(center + new Vector2(x, y), center + new Vector2(x + 1f, y), 1f);
					}
				}
			}
		}
	}
}
