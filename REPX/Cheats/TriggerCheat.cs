using System;
using System.Linq;
using System.Runtime.CompilerServices;
using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using UnityEngine;

namespace REPX.Cheats
{
	internal static class TriggerCheat
	{
		private static MonoBehaviour[] Monos
		{
			get
			{
				return EnemyDirector.instance.enemiesSpawned.Select(ep => (MonoBehaviour)ep)
					.Concat(GameDirector.instance.PlayerList.Cast<MonoBehaviour>())
					.Concat(MonoHelper.CatchedPhysGrabObjects.Cast<MonoBehaviour>())
					.ToArray();
			}
		}

		internal static void GetMonoTrigger()
		{
			triggerObject = null;
			if (SemiFunc.IsMainMenu() || SemiFunc.RunIsLobbyMenu())
				return;

			Camera main = Camera.main;
			if (main == null)
				return;

			float defaultMaxAngle = 1.2f;
			float closestAngle = float.MaxValue;
			MonoBehaviour closestObject = null;

			foreach (MonoBehaviour mono in Monos)
			{
				if (mono == null)
					continue;

				Vector3 targetPosition = Vector3.zero;
				float currentMaxAngle = defaultMaxAngle;

				PlayerAvatar playerAvatar = mono as PlayerAvatar;
				if (playerAvatar != null)
				{
					if (playerAvatar.IsLocalPlayer())
						continue;

					float visionTargetHeight = playerAvatar.GetComponent<PlayerVisionTarget>().GetField<float>("TargetPosition");
					Vector3 headPosition = playerAvatar.transform.position + new Vector3(0f, visionTargetHeight, 0f);
					targetPosition = Vector3.Lerp(playerAvatar.transform.position, headPosition, 0.65f);
					SetAngleFromRigidBody(playerAvatar.GetField<Rigidbody>("rb"), defaultMaxAngle, ref currentMaxAngle);
				}
				else
				{
					EnemyParent enemyParent = mono as EnemyParent;
					if (enemyParent != null)
					{
						Enemy enemy = enemyParent.GetEnemy();
						targetPosition = enemy.CenterTransform.position;
						var enemyRb = enemy.GetField<Rigidbody>("Rigidbody");
						if (enemyRb != null)
						{
							SetAngleFromRigidBody(enemyRb.GetField<Rigidbody>("rb"), defaultMaxAngle, ref currentMaxAngle);
						}
					}
					else
					{
						PhysGrabObject physGrabObject = mono as PhysGrabObject;
						if (physGrabObject != null)
						{
							targetPosition = physGrabObject.centerPoint;
							SetAngleFromRigidBody(physGrabObject.rb, defaultMaxAngle, ref currentMaxAngle);
						}
						else
						{
							targetPosition = mono.transform.position;
						}
					}
				}

				if (currentMaxAngle < 0.5f)
				{
					currentMaxAngle = 0.5f;
				}

				Vector3 viewportPoint = main.WorldToViewportPoint(targetPosition);
				
				// Check if object is visible in viewport
				if (viewportPoint.z > 0f && viewportPoint.x > 0f && viewportPoint.x < 1f && 
				    viewportPoint.y > 0f && viewportPoint.y < 1f)
				{
					Vector2 screenCenter = new Vector2(0.5f, 0.5f);
					Vector2 objectScreenPos = new Vector2(viewportPoint.x, viewportPoint.y);
					float angleToCenter = Vector2.Angle(screenCenter, objectScreenPos);
					
					if (angleToCenter < currentMaxAngle && angleToCenter < closestAngle)
					{
						closestAngle = angleToCenter;
						closestObject = mono;
					}
				}
			}

			triggerObject = closestObject;
			Trigger();
		}

		private static void Trigger()
		{
			if (!Input.GetKeyDown(KeyCode.Mouse2)) // Middle mouse button
				return;

			EnemyParent enemyParent = triggerObject as EnemyParent;
			if (enemyParent != null)
			{
				// Enemy trigger actions (currently empty in decompiled code)
			}

			PlayerAvatar playerAvatar = triggerObject as PlayerAvatar;
			if (playerAvatar != null)
			{
				int playerAction = Settings.Instance.SettingsData.i_PlayerTriggerAction;
				switch (playerAction)
				{
					case 1: // Kill
						if (!playerAvatar.IsDead())
						{
							playerAvatar.PlayerDeath(-1);
						}
						break;
					case 2: // Ragdoll
						playerAvatar.tumble.TumbleSet(true, false);
						break;
				}
			}

			PhysGrabObject physGrabObject = triggerObject as PhysGrabObject;
			if (physGrabObject != null)
			{
				int objectAction = Settings.Instance.SettingsData.i_ObjectTriggerAction;
				if (objectAction == 1) // Despawn
				{
					physGrabObject.Despawn();
				}
			}
		}

		private static void SetAngleFromRigidBody(Rigidbody rb, float defaultMaxAngle, ref float currentMaxAngle)
		{
			if (rb != null)
			{
				float massMultiplier = Mathf.Clamp(rb.mass, 0.1f, 10f) / 10f;
				float velocityMultiplier = Mathf.Clamp(rb.velocity.magnitude, 0f, 10f) / 10f;
				currentMaxAngle = defaultMaxAngle * (1f + massMultiplier + velocityMultiplier);
			}
		}

		private static MonoBehaviour triggerObject;
	}
}
