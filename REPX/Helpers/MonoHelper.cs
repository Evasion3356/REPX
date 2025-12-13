using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using REPX.Modules;
using Unity.VisualScripting;
using UnityEngine;

namespace REPX.Helpers
{
	// Token: 0x02000018 RID: 24
	internal class MonoHelper
	{
		// Token: 0x0600008A RID: 138 RVA: 0x0000672C File Offset: 0x0000492C
		internal static void Init()
		{
			PhysGrabObject[] array = UnityEngine.Object.FindObjectsOfType<PhysGrabObject>(true);
			List<PhysGrabObject> list = new List<PhysGrabObject>(array.Length);
			foreach (PhysGrabObject physGrabObject in array)
			{
				list.Add(physGrabObject);
			}
			MonoHelper.CatchedPhysGrabObjects = list;
			foreach (PlayerAvatar playerAvatar in UnityEngine.Object.FindObjectsOfType<PlayerAvatar>(true))
			{
				ComponentHolderProtocol.AddComponent<ExtendedPlayerData>(playerAvatar);
			}
		}

		// Token: 0x0600008B RID: 139 RVA: 0x0000679C File Offset: 0x0000499C
		internal static void Dispose()
		{
			MonoHelper.CatchedPhysGrabObjects.Clear();
			foreach (ExtendedPlayerData extendedPlayerData in ExtendedPlayerData._extendedPlayerData.Values)
			{
				UnityEngine.Object.DestroyImmediate(extendedPlayerData);
			}
			ExtendedPlayerData._extendedPlayerData.Clear();
		}

		// Token: 0x04000030 RID: 48
		internal static List<PhysGrabObject> CatchedPhysGrabObjects = new List<PhysGrabObject>();
	}
}
