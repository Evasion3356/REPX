using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using REPX.Helpers;
using UnityEngine;

namespace REPX
{
	internal class ExternalWindow
	{
		// Win32 constants
		private const int WS_EX_LAYERED = 0x00080000;
		private const int WS_EX_TRANSPARENT = 0x00000020;
		private const int WS_EX_TOPMOST = 0x00000008;
		private const int WS_EX_TOOLWINDOW = 0x00000080; // Hides from ALT+TAB and taskbar
		private const int WS_EX_NOACTIVATE = 0x08000000; // Prevents window activation
		private const int WS_POPUP = unchecked((int)0x80000000);
		private const int WS_VISIBLE = 0x10000000;
		private const int WM_PAINT = 0x000F;
		private const int WM_DESTROY = 0x0002;
		private const int WM_ERASEBKGND = 0x0014;
		private const uint WM_USER = 0x0400;
		private const uint WM_USER_INVALIDATE = WM_USER + 1;

		// Win32 structs
		[StructLayout(LayoutKind.Sequential)]
		public struct WNDCLASS
		{
			public uint style;
			public IntPtr lpfnWndProc;
			public int cbClsExtra;
			public int cbWndExtra;
			public IntPtr hInstance;
			public IntPtr hIcon;
			public IntPtr hCursor;
			public IntPtr hbrBackground;
			public string lpszMenuName;
			public string lpszClassName;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct PAINTSTRUCT
		{
			public IntPtr hdc;
			public bool fErase;
			public RECT rcPaint;
			public bool fRestore;
			public bool fIncUpdate;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] rgbReserved;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left, top, right, bottom;
			
			// Helper method to check equality without allocations
			public bool Equals(ref RECT other)
			{
				return left == other.left && top == other.top && 
				       right == other.right && bottom == other.bottom;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct MSG
		{
			public IntPtr hWnd;
			public uint message;
			public IntPtr wParam;
			public IntPtr lParam;
			public uint time;
			public int pt_x;
			public int pt_y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct SIZE
		{
			public int cx;
			public int cy;
		}

		// Win32 imports
		[DllImport("user32.dll")]
		private static extern ushort RegisterClass(ref WNDCLASS lpWndClass);

		[DllImport("user32.dll")]
		private static extern IntPtr CreateWindowEx(
			int dwExStyle, string lpClassName, string lpWindowName,
			int dwStyle, int x, int y, int nWidth, int nHeight,
			IntPtr hWndParent, IntPtr hMenu, IntPtr hInstance, IntPtr lpParam);

		[DllImport("user32.dll")]
		private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

		[DllImport("user32.dll")]
		private static extern bool UpdateWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern IntPtr BeginPaint(IntPtr hWnd, out PAINTSTRUCT lpPaint);

		[DllImport("user32.dll")]
		private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

		[DllImport("user32.dll")]
		internal static extern IntPtr GetDC(IntPtr hWnd);

		[DllImport("user32.dll")]
		internal static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

		[DllImport("user32.dll")]
		private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool FillRect(IntPtr hDC, ref RECT lprc, IntPtr hbr);

		[DllImport("user32.dll")]
		private static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

		[DllImport("user32.dll")]
		private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

		[DllImport("user32.dll")]
		private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

		[DllImport("user32.dll")]
		private static extern bool SetWindowPos(
			IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateCompatibleDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

		[DllImport("gdi32.dll")]
		internal static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		internal static extern bool DeleteObject(IntPtr hObject);

		[DllImport("gdi32.dll")]
		internal static extern bool Rectangle(IntPtr hdc, int left, int top, int right, int bottom);

		[DllImport("gdi32.dll")]
		internal static extern bool TextOut(IntPtr hdc, int x, int y, string lpString, int c);

		[DllImport("gdi32.dll")]
		internal static extern uint SetTextColor(IntPtr hdc, uint color);

		[DllImport("gdi32.dll")]
		internal static extern uint SetBkColor(IntPtr hdc, uint color);

		[DllImport("gdi32.dll")]
		internal static extern int SetBkMode(IntPtr hdc, int mode);

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateSolidBrush(uint color);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreatePen(int fnPenStyle, int nWidth, uint crColor);

		[DllImport("user32.dll")]
		private static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

		[DllImport("user32.dll")]
		private static extern bool TranslateMessage(ref MSG lpMsg);

		[DllImport("user32.dll")]
		private static extern IntPtr DispatchMessage(ref MSG lpMsg);

		[DllImport("user32.dll")]
		private static extern bool DestroyWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		private static extern void PostQuitMessage(int nExitCode);

		[DllImport("user32.dll")]
		private static extern bool InvalidateRect(IntPtr hWnd, IntPtr lpRect, bool bErase);

		[DllImport("user32.dll")]
		private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		private static extern bool ValidateRect(IntPtr hWnd, IntPtr lpRect);

		[DllImport("user32.dll")]
		private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

		[DllImport("gdi32.dll")]
		internal static extern bool MoveToEx(IntPtr hdc, int x, int y, IntPtr lpPoint);

		[DllImport("gdi32.dll")]
		internal static extern bool LineTo(IntPtr hdc, int x, int y);

		[DllImport("gdi32.dll")]
		internal static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int width, int height);

		[DllImport("gdi32.dll")]
		internal static extern bool BitBlt(IntPtr hdcDest, int xDest, int yDest, int width, int height, IntPtr hdcSrc, int xSrc, int ySrc, uint rop);

		// Constants
		internal const int TRANSPARENT = 1;
		private const uint PM_REMOVE = 0x0001;
		internal const int OPAQUE = 2;
		private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
		private const uint SWP_NOMOVE = 0x0002;
		private const uint SWP_NOSIZE = 0x0001;
		private const uint SWP_NOACTIVATE = 0x0010;
		private const uint LWA_ALPHA = 0x00000002;
		private const uint LWA_COLORKEY = 0x00000001;
		private const uint RDW_INVALIDATE = 0x0001;
		private const uint RDW_INTERNALPAINT = 0x0002;
		private const uint RDW_ERASE = 0x0004;
		private const uint SRCCOPY = 0x00CC0020;
		
		// Color key for transparency (black RGB: 0, 0, 0 -> BGR: 0x000000)
		internal const uint TRANSPARENCY_KEY = 0x000000;

		// Window procedure delegate
		private delegate IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

		internal static IntPtr hWnd;
		private IntPtr gameWindow;
		private WndProc wndProcDelegate;
		private bool isRunning;
		private RECT lastGameRect;

		// Double buffering
		private static IntPtr _backBuffer = IntPtr.Zero;
		private static IntPtr _backBufferDC = IntPtr.Zero;
		private static int _bufferWidth = 0;
		private static int _bufferHeight = 0;

		// Flag to track if we should actually render
		private static bool _shouldRender = false;
		private static readonly object _renderLock = new object();

		// Rendering callback - called from external window thread
		internal static Action<IntPtr> OnRender;

		// Thread-safe ESP data cache - optimized structure
		internal sealed class ESPRenderData
		{
			public readonly List<ESPItem> Items;
			public readonly List<ESPLine> Tracers;
			public readonly List<ESPBox> Boxes;
			
			public ESPRenderData()
			{
				// Pre-allocate with reasonable capacity to reduce reallocations
				Items = new List<ESPItem>(128);
				Tracers = new List<ESPLine>(64);
				Boxes = new List<ESPBox>(128);
			}
			
			public void Clear()
			{
				Items.Clear();
				Tracers.Clear();
				Boxes.Clear();
			}
		}

		internal struct ESPItem
		{
			public Vector2 screenPos;
			public string label;
			public Color color;
		}

		internal struct ESPLine
		{
			public Vector2 start;
			public Vector2 end;
			public Color color;
		}

		internal struct ESPBox
		{
			public Vector2 topLeft;
			public Vector2 bottomRight;
			public Color color;
		}

		private static ESPRenderData _currentRenderData = new ESPRenderData();
		private static ESPRenderData _nextRenderData = new ESPRenderData();
		private static readonly object _renderDataLock = new object();
		private static bool _dataReady = false;

		// Frame counter for periodic window sync
		private static int _frameCounter = 0;
		private const int SYNC_INTERVAL_FRAMES = 144; // Sync window position every 144 frames (~1 second at 144fps)

		// Cached brush for clearing
		private static IntPtr _clearBrush = IntPtr.Zero;

		// Called from Unity's Update() thread - triggers repaint
		internal static void TriggerRepaint()
		{
			if (hWnd != IntPtr.Zero)
			{
				lock (_renderLock)
				{
					_shouldRender = true;
				}
				PostMessage(hWnd, WM_USER_INVALIDATE, IntPtr.Zero, IntPtr.Zero);
			}
		}

		// Called from Unity's Update() thread
		internal static void UpdateESPData(ESPRenderData newData)
		{
			lock (_renderDataLock)
			{
				_nextRenderData = newData;
				_dataReady = true;
			}
			
			// Trigger repaint immediately after updating data
			TriggerRepaint();
		}

		// Called from Win32 render thread
		internal static ESPRenderData GetRenderData()
		{
			lock (_renderDataLock)
			{
				if (_dataReady)
				{
					// Swap references instead of copying data
					ESPRenderData temp = _currentRenderData;
					_currentRenderData = _nextRenderData;
					_nextRenderData = temp;
					_nextRenderData.Clear(); // Clear for reuse
					_dataReady = false;
				}
				return _currentRenderData;
			}
		}

		public ExternalWindow()
		{
			isRunning = false;
		}

		private void InitializeBackBuffer(int width, int height)
		{
			// Clean up old buffer if exists
			if (_backBuffer != IntPtr.Zero)
			{
				DeleteObject(_backBuffer);
				_backBuffer = IntPtr.Zero;
			}
			if (_backBufferDC != IntPtr.Zero)
			{
				DeleteDC(_backBufferDC);
				_backBufferDC = IntPtr.Zero;
			}

			// Create new back buffer
			IntPtr screenDC = GetDC(hWnd);
			_backBufferDC = CreateCompatibleDC(screenDC);
			_backBuffer = CreateCompatibleBitmap(screenDC, width, height);
			SelectObject(_backBufferDC, _backBuffer);
			ReleaseDC(hWnd, screenDC);

			_bufferWidth = width;
			_bufferHeight = height;

			// Create cached clear brush
			if (_clearBrush != IntPtr.Zero)
			{
				DeleteObject(_clearBrush);
			}
			_clearBrush = CreateSolidBrush(TRANSPARENCY_KEY);

			// Clear buffer with transparency key
			RECT rect;
			rect.left = 0;
			rect.top = 0;
			rect.right = width;
			rect.bottom = height;
			FillRect(_backBufferDC, ref rect, _clearBrush);
		}

		private IntPtr MyWndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
		{
			switch (msg)
			{
				case WM_ERASEBKGND:
					// Don't erase - we're using double buffering
					return new IntPtr(1);
					
				case WM_USER_INVALIDATE:
					// Custom message from Unity to trigger repaint
					RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_INTERNALPAINT);
					
					// Periodically sync window position (not every frame to reduce overhead)
					_frameCounter++;
					if (_frameCounter >= SYNC_INTERVAL_FRAMES)
					{
						_frameCounter = 0;
						SyncWithGameWindow();
					}
					
					return IntPtr.Zero;
					
				case WM_PAINT:
					bool shouldRender = false;
					lock (_renderLock)
					{
						shouldRender = _shouldRender;
						_shouldRender = false;
					}

					if (shouldRender)
					{
						// Only render if Unity triggered it
						PAINTSTRUCT ps;
						IntPtr hdc = BeginPaint(hWnd, out ps);

						// Get window dimensions
						RECT clientRect;
						GetClientRect(hWnd, out clientRect);
						int width = clientRect.right - clientRect.left;
						int height = clientRect.bottom - clientRect.top;

						// Initialize or resize back buffer if needed
						if (_backBufferDC == IntPtr.Zero || _bufferWidth != width || _bufferHeight != height)
						{
							InitializeBackBuffer(width, height);
						}

						// Clear back buffer with transparency key (using cached brush)
						RECT bufferRect;
						bufferRect.left = 0;
						bufferRect.top = 0;
						bufferRect.right = width;
						bufferRect.bottom = height;
						FillRect(_backBufferDC, ref bufferRect, _clearBrush);

						// Render to back buffer
						if (OnRender != null)
						{
							try
							{
								OnRender(_backBufferDC);
							}
							catch (Exception ex)
							{
								Log.LogError("Render callback error: " + ex.ToString());
							}
						}

						// Copy back buffer to screen (this is atomic - no flashing!)
						BitBlt(hdc, 0, 0, width, height, _backBufferDC, 0, 0, SRCCOPY);

						EndPaint(hWnd, ref ps);
					}
					else
					{
						// Validate the rect to prevent Windows from repeatedly sending WM_PAINT
						ValidateRect(hWnd, IntPtr.Zero);
					}
					
					return IntPtr.Zero;
					
				case WM_DESTROY:
					// Clean up resources
					if (_clearBrush != IntPtr.Zero)
					{
						DeleteObject(_clearBrush);
						_clearBrush = IntPtr.Zero;
					}
					if (_backBuffer != IntPtr.Zero)
					{
						DeleteObject(_backBuffer);
						_backBuffer = IntPtr.Zero;
					}
					if (_backBufferDC != IntPtr.Zero)
					{
						DeleteDC(_backBufferDC);
						_backBufferDC = IntPtr.Zero;
					}

					isRunning = false;
					PostQuitMessage(0);
					return IntPtr.Zero;
					
				default:
					return DefWindowProc(hWnd, msg, wParam, lParam);
			}
		}

		private void SyncWithGameWindow()
		{
			if (gameWindow != IntPtr.Zero)
			{
				RECT gameRect;
				if (GetWindowRect(gameWindow, out gameRect))
				{
					// Only update position if it actually changed
					if (!lastGameRect.Equals(ref gameRect))
					{
						int width = gameRect.right - gameRect.left;
						int height = gameRect.bottom - gameRect.top;
						
						SetWindowPos(hWnd, HWND_TOPMOST, 
							gameRect.left, gameRect.top, width, height, 
							SWP_NOACTIVATE);
						
						lastGameRect = gameRect;
					}
				}
			}
		}

		public void Run()
		{
			try
			{
				gameWindow = FindWindow(null, "R.E.P.O.");
				if (gameWindow == IntPtr.Zero)
				{ 
					Log.LogError("Could not find game window 'R.E.P.O.'");
				}

				wndProcDelegate = new WndProc(MyWndProc);
				
				WNDCLASS wc = new WNDCLASS();
				wc.lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProcDelegate);
				wc.lpszClassName = "REPXOverlayClass";

				ushort classAtom = RegisterClass(ref wc);
				if (classAtom == 0)
				{
					Log.LogError("Failed to register window class");
					return;
				}

				int windowX = 0;
				int windowY = 0;
				int windowWidth = Screen.width;
				int windowHeight = Screen.height;

				if (gameWindow != IntPtr.Zero)
				{
					RECT gameRect;
					if (GetWindowRect(gameWindow, out gameRect))
					{
						windowX = gameRect.left;
						windowY = gameRect.top;
						windowWidth = gameRect.right - gameRect.left;
						windowHeight = gameRect.bottom - gameRect.top;
						lastGameRect = gameRect;
						Log.LogInfo(string.Format("Found game window at ({0}, {1}) with size {2}x{3}", windowX, windowY, windowWidth, windowHeight), "Log");
					}
				}

				// Create window with WS_EX_TOOLWINDOW and WS_EX_NOACTIVATE to hide from ALT+TAB and taskbar
				hWnd = CreateWindowEx(
					WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW | WS_EX_NOACTIVATE,
					wc.lpszClassName, "REPX Overlay",
					WS_POPUP | WS_VISIBLE,
					windowX, windowY, windowWidth, windowHeight,
					gameWindow, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero); // Set game window as parent

				if (hWnd == IntPtr.Zero)
				{
					Log.LogError("Failed to create overlay window");
					return;
				}

				SetLayeredWindowAttributes(hWnd, TRANSPARENCY_KEY, 0, LWA_COLORKEY);

				ShowWindow(hWnd, 1);
				UpdateWindow(hWnd);

				SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

				// Initialize back buffer
				InitializeBackBuffer(windowWidth, windowHeight);

				// DON'T overwrite OnRender here - it should be set by Loader.cs BEFORE calling Run()
				if (OnRender != null)
				{
					Log.LogInfo("OnRender callback is set and ready", "Log");
				}
				else
				{
					Log.LogWarning("OnRender callback is NOT set - nothing will be rendered!");
				}

				isRunning = true;
				Log.LogInfo(string.Format("External overlay window created at {0}x{1} (hidden from ALT+TAB/taskbar)", windowWidth, windowHeight), "Log");

				MSG msg;
				while (isRunning)
				{
					try
					{
						if (PeekMessage(out msg, IntPtr.Zero, 0, 0, PM_REMOVE))
						{
							if (msg.message == WM_DESTROY)
							{
								break;
							}
							TranslateMessage(ref msg);
							DispatchMessage(ref msg);
						}
						else
						{
							Thread.Sleep(1);
						}
					}
					catch (ThreadAbortException)
					{
						// Thread is being aborted - exit gracefully
						Log.LogInfo("External window message loop terminated", "Log");
						break;
					}
				}
			}
			catch (ThreadAbortException)
			{
				// Expected when unloading - this is a graceful shutdown
				Log.LogInfo("External window run loop terminated", "Log");
			}
			catch (Exception ex)
			{
				Log.LogError("ExternalWindow.Run error: " + ex.ToString());
			}
		}

		public void Close()
		{
			if (hWnd != IntPtr.Zero)
			{
				isRunning = false;
				DestroyWindow(hWnd);
				hWnd = IntPtr.Zero;
			}
		}

		internal static uint ColorToBGR(Color color)
		{
			byte r = (byte)(color.r * 255);
			byte g = (byte)(color.g * 255);
			byte b = (byte)(color.b * 255);
			return (uint)((b << 16) | (g << 8) | r);
		}
	}
}
