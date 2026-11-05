using REPX.Data;
using REPX.Helpers;
using REPX.Patches.Game.Player;
using System;
using System.IO;
using System.Linq;
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
			// Injected twice without unloading: tear the first instance down so we don't stack menus/windows/patches.
			if (cheatGUI != null || externalWindowThread != null)
			{
				Unload();
			}

			Log.Clear();
			LogBuildInfo();
			AppDomain.CurrentDomain.AssemblyResolve -= ResolveEmbeddedAssembly;
			AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
			SetupMenu();
			MonoHelper.Init();
			StartExternalWindow();
			Log.LogInfo("Injection successful! REPX Harmony patches active: " + CountActivePatches(), "Log");
		}

		// Identifies which build is actually running. The build time/version is stamped on every compile, so if it doesn't
		// change after you rebuild and reinject, Mono handed back the old loaded assembly. More than one REPX assembly in the
		// domain means earlier injections are still resident (expected: they can't be freed).
		private static void LogBuildInfo()
		{
			try
			{
				Assembly self = typeof(Loader).Assembly;
				int copies = 0;
				foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (a.GetName().Name == self.GetName().Name) copies++;
				}
				Log.LogInfo("Built " + FileVersionInfoOrStamp(self) + " | version " + self.GetName().Version + " | MVID " + self.ManifestModule.ModuleVersionId
					+ " | REPX assemblies in domain: " + copies
					+ " | REPX Harmony patches active before load: " + CountActivePatches(), "Build");
			}
			catch (Exception ex)
			{
				Log.LogError("Failed to log build info: " + ex.Message);
			}
		}

		// Harmony keeps a method in GetAllPatchedMethods() even after every patch on it was removed, so count only the
		// methods that still carry one of our patches.
		private static int CountActivePatches()
		{
			int count = 0;
			foreach (MethodBase method in HarmonyLib.Harmony.GetAllPatchedMethods().ToList())
			{
				HarmonyLib.Patches info = HarmonyLib.Harmony.GetPatchInfo(method);
				if (info != null && info.Owners.Contains(CheatGUI.GUID))
				{
					count++;
				}
			}
			return count;
		}

		private static string FileVersionInfoOrStamp(Assembly assembly)
		{
			object[] attrs = assembly.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute), false);
			return attrs.Length > 0 ? ((AssemblyInformationalVersionAttribute)attrs[0]).InformationalVersion : "unknown";
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
			// Managed assemblies injected into Unity's Mono root domain can't be freed (there is no FreeLibrary
			// equivalent), so "unloading" means undoing everything we did and leaving every static in a state a
			// later Load() can start from. Nothing here may null out static fields: Mono keeps the old assembly
			// loaded and doesn't re-run static initializers on a re-injection.
			try
			{
				ShutdownExternalWindow();
			}
			catch (Exception ex)
			{
				Log.LogError("Error shutting down external window: " + ex.ToString());
			}

			try
			{
				if (cheatGUI != null)
				{
					// OnDestroy removes the Harmony patches.
					UnityEngine.Object.DestroyImmediate(cheatGUI.gameObject);
					cheatGUI = null;
				}
			}
			catch (Exception ex)
			{
				Log.LogError("Error destroying menu: " + ex.ToString());
			}

			try
			{
				MonoHelper.Dispose();
				PlayerAvatarPatch.ResetState();
				Settings.Reset();
				ExternalWindow.ResetStatics();
			}
			catch (Exception ex)
			{
				Log.LogError("Error resetting state: " + ex.ToString());
			}

			AppDomain.CurrentDomain.AssemblyResolve -= ResolveEmbeddedAssembly;
			Log.LogInfo("Unloaded.", "Log");
		}

		private static void ShutdownExternalWindow()
		{
			if (externalWindow != null)
			{
				// Asks the window thread to destroy its own window and unregister its class.
				externalWindow.Close();
			}

			Thread thread = externalWindowThread;
			if (thread != null && thread.IsAlive)
			{
				if (!thread.Join(2000))
				{
					Log.LogWarning("External window thread did not exit in time, aborting it");
					thread.Abort();
					thread.Join(1000);
				}
			}

			externalWindow = null;
			externalWindowThread = null;
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
