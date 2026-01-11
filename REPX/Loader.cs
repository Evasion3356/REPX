using REPX.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using UnityEngine;

namespace REPX
{
	public static class Loader
	{
		private static CheatGUI cheatGUI;
		private static Thread externalWindowThread;
		private static ExternalWindow externalWindow;

		private static readonly string[] EmbeddedDependencies = new string[] 
		{ 
			"0Harmony.dll", 
			"MonoMod.RuntimeDetour.dll", 
			"MonoMod.Utils.dll", 
			"Mono.Cecil.dll" 
		};

		public static void Load()
		{
			Log.Clear();
			AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
			SetupMenu();
			MonoHelper.Init();
			StartExternalWindow();
			Log.LogInfo("Injection successful!", "Log");
		}

		private static void SetupMenu()
		{
			GameObject gameObject = new GameObject("REPXLoader");
			cheatGUI = gameObject.AddComponent<CheatGUI>();
			UnityEngine.Object.DontDestroyOnLoad(cheatGUI);
		}

		private static void StartExternalWindow()
		{
			try
			{
				externalWindowThread = new Thread(() =>
				{
					try
					{
						externalWindow = new ExternalWindow();

						// Set up the render callback
						ExternalWindow.OnRender = (hdc) =>
						{
							try
							{
								ExternalRender.BeginFrame(hdc);
								var espData = ExternalWindow.GetRenderData();

								// Render all labels (items, enemies, players)
								foreach (var item in espData.Items)
								{
									ExternalRender.String(
										item.screenPos.x,
										item.screenPos.y,
										200f,
										20f,
										item.label,
										item.color,
										true,
										false
									);
								}

								// Render boxes
								foreach (var box in espData.Boxes)
								{
									Vector2 size = new Vector2(
										box.bottomRight.x - box.topLeft.x,
										box.bottomRight.y - box.topLeft.y
									);
									ExternalRender.Box(
										box.topLeft,
										size,
										2f,
										box.color,
										false  // not centered, already calculated topLeft
									);
								}

								// Render tracers
								foreach (var tracer in espData.Tracers)
								{
									ExternalRender.Line(
										tracer.start,
										tracer.end,
										1f,
										tracer.color
									);
								}
							}
							catch (Exception ex)
							{
								Log.LogError("Render callback error: " + ex.ToString());
							}
						};

						externalWindow.Run();
					}
					catch (ThreadAbortException)
					{
						// Expected when unloading - don't log as error
						Log.LogInfo("External window thread terminated gracefully", "Log");
					}
					catch (Exception ex)
					{
						Log.LogError("External window thread error: " + ex.ToString());
					}
				});
				externalWindowThread.SetApartmentState(ApartmentState.STA);
				externalWindowThread.IsBackground = true;
				externalWindowThread.Start();
				Log.LogInfo("External window thread started", "Log");
			}
			catch (Exception ex)
			{
				Log.LogError("Failed to start external window: " + ex.ToString());
			}
		}

		public static void Unload()
		{
			if (externalWindow != null)
			{
				externalWindow.Close();
				externalWindow = null;
			}

			if (externalWindowThread != null && externalWindowThread.IsAlive)
			{
				try
				{
					externalWindowThread.Abort();
					// Give the thread a moment to abort gracefully
					if (!externalWindowThread.Join(1000))
					{
						Log.LogWarning("External window thread did not terminate within timeout");
					}
				}
				catch (Exception ex)
				{
					Log.LogError("Error aborting external window thread: " + ex.ToString());
				}
				externalWindowThread = null;
			}

			if (cheatGUI != null)
			{
				UnityEngine.Object.DestroyImmediate(cheatGUI.gameObject);
			}

			MonoHelper.Dispose();
			PurgeAssembly();
			AppDomain.CurrentDomain.AssemblyResolve -= ResolveEmbeddedAssembly;
		}

		public static void PurgeAssembly()
		{
			Assembly assembly = typeof(Loader).Assembly;
			foreach (Type type in assembly.GetTypes())
			{
				foreach (FieldInfo fieldInfo in type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
				{
					if (!fieldInfo.IsInitOnly && fieldInfo.FieldType.IsClass)
					{
						try
						{
							fieldInfo.SetValue(null, null);
						}
						catch
						{
							// Ignore errors during cleanup
						}
					}
				}
			}
		}

		private static Assembly ResolveEmbeddedAssembly(object sender, ResolveEventArgs args)
		{
			AssemblyName assemblyName = new AssemblyName(args.Name);
			foreach (string embeddedDep in EmbeddedDependencies)
			{
				if (assemblyName.Name == Path.GetFileNameWithoutExtension(embeddedDep))
				{
					string resourceName = "REPX.Resources." + embeddedDep;
					using (Stream manifestResourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
					{
						if (manifestResourceStream == null)
						{
							Log.LogError("Failed to find embedded resource: " + resourceName);
							return null;
						}
						try
						{
							byte[] assemblyData = new byte[manifestResourceStream.Length];
							manifestResourceStream.Read(assemblyData, 0, assemblyData.Length);
							return Assembly.Load(assemblyData);
						}
						catch (Exception ex)
						{
							Log.LogError(string.Format("Failed to load assembly {0}: {1}", resourceName, ex));
							return null;
						}
					}
				}
			}
			return null;
		}
	}
}
