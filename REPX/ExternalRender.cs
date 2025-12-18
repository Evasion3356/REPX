using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace REPX
{
	internal static class ExternalRender
	{
		private static IntPtr currentHdc;
		private static uint currentColor = 0xFFFFFF;
		private static IntPtr currentPen = IntPtr.Zero;
		private static float currentThickness = 1f;

		[StructLayout(LayoutKind.Sequential)]
		private struct SIZE
		{
			public int cx;
			public int cy;
		}

		[DllImport("gdi32.dll")]
		private static extern bool GetTextExtentPoint32(IntPtr hdc, string lpString, int c, out SIZE psizl);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateSolidBrush(uint color);

		[DllImport("gdi32.dll")]
		private static extern bool Ellipse(IntPtr hdc, int left, int top, int right, int bottom);

		// Called at the start of each frame
		internal static void BeginFrame(IntPtr hdc)
		{
			currentHdc = hdc;
			ExternalWindow.SetBkMode(hdc, ExternalWindow.TRANSPARENT);
			
			// Reset cached pen
			if (currentPen != IntPtr.Zero)
			{
				ExternalWindow.DeleteObject(currentPen);
				currentPen = IntPtr.Zero;
			}
		}

		internal static void EndFrame()
		{
			// Clean up cached pen at end of frame
			if (currentPen != IntPtr.Zero)
			{
				ExternalWindow.DeleteObject(currentPen);
				currentPen = IntPtr.Zero;
			}
		}

		internal static Color Color
		{
			get
			{
				byte r = (byte)(currentColor & 0xFF);
				byte g = (byte)((currentColor >> 8) & 0xFF);
				byte b = (byte)((currentColor >> 16) & 0xFF);
				return new Color(r / 255f, g / 255f, b / 255f, 1f);
			}
			set
			{
				uint newColor = ExternalWindow.ColorToBGR(value);
				if (newColor != currentColor)
				{
					currentColor = newColor;
					if (currentHdc != IntPtr.Zero)
					{
						ExternalWindow.SetTextColor(currentHdc, currentColor);
					}
					// Invalidate cached pen
					if (currentPen != IntPtr.Zero)
					{
						ExternalWindow.DeleteObject(currentPen);
						currentPen = IntPtr.Zero;
					}
				}
			}
		}

		private static IntPtr GetOrCreatePen(float thickness)
		{
			if (currentPen == IntPtr.Zero || Math.Abs(currentThickness - thickness) > 0.01f)
			{
				if (currentPen != IntPtr.Zero)
				{
					ExternalWindow.DeleteObject(currentPen);
				}
				currentPen = ExternalWindow.CreatePen(0, (int)thickness, currentColor);
				currentThickness = thickness;
			}
			return currentPen;
		}

		internal static void Line(Vector2 from, Vector2 to, float thickness, Color color)
		{
			Color = color;
			Line(from, to, thickness);
		}

		internal static void Line(Vector2 from, Vector2 to, float thickness)
		{
			if (currentHdc == IntPtr.Zero) return;

			IntPtr pen = GetOrCreatePen(thickness);
			IntPtr oldPen = ExternalWindow.SelectObject(currentHdc, pen);

			ExternalWindow.MoveToEx(currentHdc, (int)from.x, (int)from.y, IntPtr.Zero);
			ExternalWindow.LineTo(currentHdc, (int)to.x, (int)to.y);

			ExternalWindow.SelectObject(currentHdc, oldPen);
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

			// Batch pen selection for all four lines
			IntPtr pen = GetOrCreatePen(thickness);
			IntPtr oldPen = ExternalWindow.SelectObject(currentHdc, pen);

			// Top
			ExternalWindow.MoveToEx(currentHdc, (int)topLeft.x, (int)topLeft.y, IntPtr.Zero);
			ExternalWindow.LineTo(currentHdc, (int)(topLeft.x + size.x), (int)topLeft.y);
			
			// Right
			ExternalWindow.LineTo(currentHdc, (int)(topLeft.x + size.x), (int)(topLeft.y + size.y));
			
			// Bottom
			ExternalWindow.LineTo(currentHdc, (int)topLeft.x, (int)(topLeft.y + size.y));
			
			// Left
			ExternalWindow.LineTo(currentHdc, (int)topLeft.x, (int)topLeft.y);

			ExternalWindow.SelectObject(currentHdc, oldPen);
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
			
			SIZE textSize;
			GetTextExtentPoint32(currentHdc, str, str.Length, out textSize);

			float x = centerx ? (X - textSize.cx / 2f) : X;
			float y = centery ? (Y - textSize.cy / 2f) : Y;

			ExternalWindow.TextOut(currentHdc, (int)x, (int)y, str, str.Length);
		}

		internal static void Circle(Vector2 center, float radius, float thickness, Color color)
		{
			Color = color;
			Circle(center, radius, thickness);
		}

		internal static void Circle(Vector2 center, float radius, float thickness)
		{
			if (currentHdc == IntPtr.Zero) return;

			IntPtr pen = GetOrCreatePen(thickness);
			IntPtr oldPen = ExternalWindow.SelectObject(currentHdc, pen);

			Vector2 prev = center + new Vector2(radius, 0f);
			ExternalWindow.MoveToEx(currentHdc, (int)prev.x, (int)prev.y, IntPtr.Zero);
			
			// Reduce segments for better performance (36 segments instead of 360)
			for (int i = 1; i <= 36; i++)
			{
				float angle = (float)i * 0.17453292f; // 10 degrees per segment
				Vector2 current = center + new Vector2(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
				ExternalWindow.LineTo(currentHdc, (int)current.x, (int)current.y);
			}

			ExternalWindow.SelectObject(currentHdc, oldPen);
		}

		internal static void FilledCircle(Vector2 center, float radius, Color color)
		{
			if (currentHdc == IntPtr.Zero) return;

			Color = color;
			
			IntPtr brush = CreateSolidBrush(currentColor);
			IntPtr oldBrush = ExternalWindow.SelectObject(currentHdc, brush);
			
			// Use GDI Ellipse for much better performance
			int left = (int)(center.x - radius);
			int top = (int)(center.y - radius);
			int right = (int)(center.x + radius);
			int bottom = (int)(center.y + radius);
			
			Ellipse(currentHdc, left, top, right, bottom);
			
			ExternalWindow.SelectObject(currentHdc, oldBrush);
			ExternalWindow.DeleteObject(brush);
		}
	}
}
