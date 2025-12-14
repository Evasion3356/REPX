using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace REPX.Modules
{
	// Token: 0x02000015 RID: 21
	[RequireComponent(typeof(PlayerAvatar))]
	internal class ExtendedPlayerData : MonoBehaviour
	{
		// Token: 0x17000003 RID: 3
		// (get) Token: 0x0600007B RID: 123 RVA: 0x00006473 File Offset: 0x00004673
		// (set) Token: 0x0600007C RID: 124 RVA: 0x0000647B File Offset: 0x0000467B
		internal PlayerAvatar PlayerAvatar { get; private set; }

		// Token: 0x0600007D RID: 125 RVA: 0x00006484 File Offset: 0x00004684
		private void Awake()
		{
			this.PlayerAvatar = base.GetComponent<PlayerAvatar>();
			bool flag = this.PlayerAvatar == null || ExtendedPlayerData._extendedPlayerData.ContainsKey(this.PlayerAvatar);
			if (flag)
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				ExtendedPlayerData._extendedPlayerData[this.PlayerAvatar] = this;
			}
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000064E0 File Offset: 0x000046E0
		private void OnDestroy()
		{
			bool flag = this.PlayerAvatar != null;
			if (flag)
			{
				ExtendedPlayerData._extendedPlayerData.Remove(this.PlayerAvatar);
			}
			else
			{
				foreach (PlayerAvatar playerAvatar in ExtendedPlayerData._extendedPlayerData.Keys.ToList<PlayerAvatar>())
				{
					bool flag2 = playerAvatar == this;
					if (flag2)
					{
						ExtendedPlayerData._extendedPlayerData.Remove(playerAvatar);
						break;
					}
				}
			}
		}

		// Token: 0x0400002B RID: 43
		internal static Dictionary<PlayerAvatar, ExtendedPlayerData> _extendedPlayerData = new Dictionary<PlayerAvatar, ExtendedPlayerData>();
	}
}
