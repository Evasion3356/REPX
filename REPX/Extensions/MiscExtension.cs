using System;
using System.Runtime.CompilerServices;
using Photon.Pun;
using UnityEngine;

namespace REPX.Extensions
{
	internal static class MiscExtension
	{
		internal static void Despawn(this PhysGrabObject grabObject)
		{
			grabObject.Teleport(new Vector3(0f, -10000f, 0f), default(Quaternion));
		}

		internal static Enemy GetEnemy(this EnemyParent enemyParent)
		{
			return enemyParent.GetField<Enemy>("Enemy");
		}

		internal static PhotonView GetPhotonView(this MonoBehaviour mono, string name = "photonView")
		{
			return mono.GetField<PhotonView>(name);
		}
	}
}
