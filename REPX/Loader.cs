using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using REPX.Helpers;
using UnityEngine;

namespace REPX
{
	public static class Loader
	{
		public static void Load()
		{
			Log.Clear();
			AppDomain.CurrentDomain.AssemblyResolve += ResolveEmbeddedAssembly;
			SetupMenu();
			MonoHelper.Init();
			Log.LogInfo("Injection successful!", "Log");
		}

		private static void SetupMenu()
		{
			GameObject gameObject = new GameObject("REPXLoader");
			cheatGUI = gameObject.AddComponent<CheatGUI>();
			UnityEngine.Object.DontDestroyOnLoad(cheatGUI);
		}

		public static void Unload()
		{
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

		private static CheatGUI cheatGUI;

		private static readonly string[] EmbeddedDependencies = new string[] 
		{ 
			"0Harmony.dll", 
			"MonoMod.RuntimeDetour.dll", 
			"MonoMod.Utils.dll", 
			"Mono.Cecil.dll" 
		};
	}
}
