using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EnemySpinny;

namespace REPX.Cheats
{
	internal static class EspCheat
	{
		static readonly Color orange = new Color(1f, 0.5f, 0f, 1f);
		internal static void RenderEsp()
		{
			if (!Settings.Instance.SettingsData.b_Esp) return;
			if (Camera.main == null) return;

			try
			{
				EspCheat.RenderPlayers();
				EspCheat.RenderEnemies();
				EspCheat.RenderItems();
				EspCheat.RenderObjects();
				EspCheat.RenderLaser();
			}
			catch (Exception ex)
			{
				Log.LogError(ex);
			}
		}

		private static void RenderPlayers()
		{
			if (!Settings.Instance.SettingsData.b_PlayerEsp) return;
			if (GameDirector.instance == null) return;

			foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
			{
				if (playerAvatar.IsLocalPlayer()) continue;

				Vector3 targetPosition = playerAvatar.playerAvatarVisuals.bodyTopUpTransform.position;

				float sizeX = 1f;
				float sizeY = 2.5f;
				var color = Settings.Instance.SettingsData.c_PlayerEspColor;
				string name = string.Empty;

				if (playerAvatar.IsDead())
				{ 	
					sizeY = 1f;
					PlayerDeathHead playerDeathHead = playerAvatar.playerDeathHead;
					if (playerDeathHead != null)
					{
						targetPosition = playerDeathHead.GetField<PhysGrabObject>("physGrabObject").centerPoint;
					}
					color = Color.magenta;
					if (Settings.Instance.SettingsData.b_PlayerNameEsp)
					{
						string playerName = playerAvatar.GetPlayerName();
						int health = playerAvatar.playerHealth.GetField<int>("health");
						name = string.Format("{0}", playerName);
					}
				}
				else
				{
					if (Settings.Instance.SettingsData.b_PlayerNameEsp)
					{
						string playerName = playerAvatar.GetPlayerName();
						int health = playerAvatar.playerHealth.GetField<int>("health");
						name = string.Format("{0} {1}HP", playerName, health);
					}
					if (playerAvatar.GetField<bool>("isCrouching"))
					{
						sizeY = 1.5f;
					}
					else if (playerAvatar.GetField<bool>("isCrawling") || playerAvatar.GetField<bool>("isTumbling") || playerAvatar.GetField<bool>("isSliding"))
					{
						sizeY = 1f;
					}
				}

				RenderEspElement(
					targetPosition,
					(sizeX, sizeY),
					name,
					color,
					float.MaxValue,
					Settings.Instance.SettingsData.b_Tracer
				);
			}
		}

		private static void RenderEnemies()
		{
			bool flag = !Settings.Instance.SettingsData.b_EnemyEsp;
			if (!flag)
			{
				bool flag2 = EnemyDirector.instance == null;
				if (!flag2)
				{
					foreach (EnemyParent enemyParent in EnemyDirector.instance.enemiesSpawned)
					{
						try
						{
							if (enemyParent == null) continue;
							Enemy enemy = enemyParent.GetField<Enemy>("Enemy");
							EnemyHealth enemyHealth = enemy.GetField<EnemyHealth>("Health");
							bool isDead = enemyHealth.GetField<bool>("dead");
							bool isSpawned = enemyParent.GetField<bool>("Spawned");
							string name = string.Empty;
							if (Settings.Instance.SettingsData.b_EnemyNameEsp)
							{
								int enemyCurrentHealth = enemyHealth.GetField<int>("healthCurrent");
								name = string.Format("{0} {1}HP", enemyParent.enemyName, enemyCurrentHealth);
							}
							if (!isDead && isSpawned)
							{
								
								RenderEspElement(
									enemy.CenterTransform.position,
									(1f, 1f),
									name,
									Settings.Instance.SettingsData.c_EnemyEspColor,
									Settings.Instance.SettingsData.f_EspRange,
									Settings.Instance.SettingsData.b_Tracer
								);
							}
						}
						catch (Exception ex)
						{
							Log.LogInfo("RenderEnemies Failed");
							Log.LogError(ex);
						}
					}
				}
			}
		}

		private static (float width, float height) CalculateObjectBounds(PhysGrabObject physGrabObject, float distance)
		{
			try
			{
				Camera cam = Camera.main;
				if (cam == null) return (1f, 1f);

				// Get the bounding box size (Vector3)
				Vector3 boundingBoxSize = physGrabObject.boundingBox;

				// Get the direction from camera to object
				Vector3 cameraToObject = (physGrabObject.centerPoint - cam.transform.position).normalized;

				// Get object's right and forward vectors (world space)
				Vector3 objectRight = physGrabObject.transform.right;
				Vector3 objectForward = physGrabObject.transform.forward;

				// Calculate how much each axis is facing the camera using dot product
				float rightDot = Mathf.Abs(Vector3.Dot(cameraToObject, objectRight));
				float forwardDot = Mathf.Abs(Vector3.Dot(cameraToObject, objectForward));

				// Use the axis that's most perpendicular to the camera view (smaller dot = more visible)
				float width = (rightDot < forwardDot) ? boundingBoxSize.x : boundingBoxSize.z;
				float height = boundingBoxSize.y;

				// Scale factor inversely with distance to maintain consistent appearance
				float distanceScale = Mathf.Clamp(10f / distance, 1f, 10f);

				return (width * distanceScale, height * distanceScale);
			}
			catch (Exception ex)
			{
				Log.LogError(ex);
			}

			// Fallback to default size
			return (1f, 1f);
		}

		private static void RenderItems()
		{
			bool flag = !Settings.Instance.SettingsData.b_ItemEsp;
			if (!flag)
			{
				bool flag2 = SemiFunc.RunIsLobby();
				if (!flag2)
				{
					foreach (PhysGrabObject physGrabObject in MonoHelper.CatchedPhysGrabObjects)
					{
						try
						{
							bool flag3 = physGrabObject == null;
							if (!flag3 && physGrabObject.GetField<bool>("isActive"))
							{
								ItemAttributes itemAttributes = physGrabObject.GetComponent<ItemAttributes>();
								// Calculate distance once for reuse
								float distance = Vector3.Distance(physGrabObject.centerPoint, Camera.main.transform.parent.position);
								
								if (physGrabObject.GetField<bool>("isValuable"))
								{
									int value = (int)physGrabObject.GetComponent<ValuableObject>().GetField<float>("dollarValueCurrent");
									// Determine color based on value ranges
									Color color;
									if (value < 5000)
									{
										color = Settings.Instance.SettingsData.c_ItemEspColorLow;
									}
									else if (value >= 5000 && value <= 10000)
									{
										color = Settings.Instance.SettingsData.c_ItemEspColorMedium;
									}
									else // value > 10000
									{
										color = Settings.Instance.SettingsData.c_ItemEspColorHigh;
									}
									var draw_name = Settings.Instance.SettingsData.b_ItemValueEsp ? string.Format("${0:N0}", value) : string.Empty;
									RenderEspElement(
										physGrabObject.centerPoint,
										CalculateObjectBounds(physGrabObject, distance), 
										draw_name, 
										color, 
										Settings.Instance.SettingsData.f_EspRange, 
										Settings.Instance.SettingsData.b_Tracer
									);
								}
								if (physGrabObject.GetField<bool>("isCart"))
								{
									PhysGrabCart cart = physGrabObject.GetComponent<PhysGrabCart>();
									int cartValue = cart.GetField<int>("haulCurrent");
									var name = itemAttributes.GetField<string>("itemName");
									var color = Settings.Instance.SettingsData.c_CartEspColor;
									var draw_name = Settings.Instance.SettingsData.b_ItemValueEsp ? string.Format("{0} ${1:N0}", name, cartValue) : string.Empty;
									RenderEspElement(
										physGrabObject.centerPoint,
										CalculateObjectBounds(physGrabObject, distance),
										draw_name,
										color,
										Settings.Instance.SettingsData.f_EspRange,
										Settings.Instance.SettingsData.b_Tracer
									);
								}
								bool isGun = physGrabObject.GetField<bool>("isGun");
								bool isMelee = physGrabObject.GetField<bool>("isMelee");
								if (isGun || isMelee)
								{
									var name = itemAttributes.GetField<string>("itemName");
									/*if (isGun)
									{
										name = physGrabObject.name.Replace("Item Gun ", "");
									}
									else if (isMelee)
									{
										name = physGrabObject.name.Replace("Item Melee ", "");
									}
									name.Replace("(Clone)", "").Trim();*/
									var color = Settings.Instance.SettingsData.c_WeaponEspColor;
									RenderEspElement(
										physGrabObject.centerPoint,
										CalculateObjectBounds(physGrabObject, distance),
										name,
										color,
										Settings.Instance.SettingsData.f_EspRange,
										Settings.Instance.SettingsData.b_Tracer
									);
								}
								if (physGrabObject.GetComponent<ItemDrone>())
								{
									var name = itemAttributes.GetField<string>("itemName");
									//name.Replace("Item Drone", "");
									//name.Replace("(Clone))", "").Trim();
									var color = Settings.Instance.SettingsData.c_ItemEspColorDrone;
									RenderEspElement(
										physGrabObject.centerPoint,
										CalculateObjectBounds(physGrabObject, distance),
										name,
										color,
										Settings.Instance.SettingsData.f_EspRange,
										Settings.Instance.SettingsData.b_Tracer
									);
								}
							}
						}
						catch (Exception ex)
						{
							Log.LogInfo("RenderItems Failed");
							Log.LogError(ex);
						}
					}
				}
			}
		}

		private static void RenderObjects()
		{
			if (Settings.Instance.SettingsData.b_extractionESP)
			{
				int num = 0;
				foreach (GameObject gameObject in RoundDirector.instance.GetField<List<GameObject>>("extractionPointList"))
				{
					num++;
					bool flag = gameObject == null;
					if (!flag)
					{
						ExtractionPoint ep = gameObject.GetComponent<ExtractionPoint>();
						if (ep != null)
						{
							if (ep.GetField<ExtractionPoint.State>("currentState") == ExtractionPoint.State.Complete) continue;

							Color color = Color.white;
							if (ep == RoundDirector.instance.GetField<ExtractionPoint>("extractionPointCurrent"))
							{
								color = Color.green;
							}
							EspCheat.RenderEspElement(
								gameObject.transform.position,
								(1f, 1f),
								string.Format("Extraction Point ({0})", num),
								color,
								Settings.Instance.SettingsData.f_EspRange,
								false
							);
						}
					}
				}
			}
			if (Settings.Instance.SettingsData.b_truckESP)
			{
				var levelPathTruck = LevelGenerator.Instance.LevelPathTruck;
				if (levelPathTruck)
				{
					EspCheat.RenderEspElement(
						levelPathTruck.transform.position,
						(2f, 2f),
						"Truck",
						orange,
						Settings.Instance.SettingsData.f_EspRange,
						false
					);
				}
			}
		}

		private static void RenderLaser()
		{
			bool flag = !Settings.Instance.SettingsData.b_LaserESP;
			if (!flag)
			{
				bool flag2 = SemiFunc.RunIsLobby();
				if (!flag2)
				{
					foreach (PhysGrabObject physGrabObject in MonoHelper.CatchedPhysGrabObjects)
					{
						try
						{
							bool flag3 = physGrabObject == null;
							if (!flag3 && physGrabObject.grabbedLocal && physGrabObject.GetField<bool>("isActive"))
							{
								bool isGun = physGrabObject.GetField<bool>("isGun");
								if (isGun)
								{
									ItemGun itemGun = physGrabObject.GetComponent<ItemGun>();
									if (itemGun != null && itemGun.gunMuzzle != null)
									{
										Vector3 barrelPosition = itemGun.gunMuzzle.position;
										Vector3 barrelDirection = itemGun.gunMuzzle.forward;

										// Calculate the end point of the laser (e.g., 100 units forward)
										float laserDistance = 100f;
										Vector3 laserEndPoint = barrelPosition + (barrelDirection * laserDistance);

										// Convert world positions to screen positions
										Vector3 screenStart;
										Vector3 screenEnd;
										if (Utils.WorldToScreen(Camera.main, barrelPosition, out screenStart) &&
											Utils.WorldToScreen(Camera.main, laserEndPoint, out screenEnd))
										{
											// Draw the laser line
											Render.Line(
												new Vector2(screenStart.x, screenStart.y),
												new Vector2(screenEnd.x, screenEnd.y),
												2f,
												Color.red
											);
										}
									}
								}
							}
						}
						catch (Exception ex)
						{
							Log.LogInfo("RenderLaser Failed");
							Log.LogError(ex);
						}
					}
				}
			}
		}

		private static void RenderEspElement(Vector3 worldPos, (float x, float y) size, string name, Color color, float Range = float.MaxValue, bool tracer = false)
		{
			var cam = Camera.main;
			Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

			float distance = Mathf.Round(Vector3.Distance(worldPos, cam.transform.parent.position));
			if (viewportPos.z < 0 || distance > Range) return;

			Vector2 screenPos = new Vector2(
				viewportPos.x * Screen.width,
				(1 - viewportPos.y) * Screen.height
			);

			// Base box size (scales with distance)
			float baseBoxSize = Mathf.Clamp(1000f / distance, 10f, 50f);

			// Apply size multiplier (e.g., for width/height adjustments)
			float boxWidth = baseBoxSize * size.x;
			float boxHeight = baseBoxSize * size.y;

			bool isInView = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

			if (isInView)
			{
				// Calculate the top-left corner to keep the box centered on screenPos
				Vector2 boxTopLeft = new Vector2(
					screenPos.x - (boxWidth * 0.5f),  // Center horizontally
					screenPos.y - (boxHeight * 0.5f)  // Center vertically
				);

				// Draw the box
				Render.Box(
					boxTopLeft,
					new Vector2(boxWidth, boxHeight),
					2f,
					color
				);

				// Draw the name label (above the box)
				if (!string.IsNullOrEmpty(name))
				{
					Render.String(
						GUI.skin.label,
						screenPos.x,  // Centered horizontally
						screenPos.y - (boxHeight * 0.5f) - 20f,  // Place above the box
						200f, 20f,
						name,
						Color.white,
						true
					);
				}
			}

			if (tracer)
			{
				Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
				Vector2 tracerEndPos = isInView ? screenPos : new Vector2(
					Mathf.Clamp(viewportPos.x * Screen.width, 0, Screen.width),
					Mathf.Clamp((1 - viewportPos.y) * Screen.height, 0, Screen.height)
				);
				Render.Line(screenCenter, tracerEndPos, 1f, color);
			}
		}

		public static void RenderEspElementExternal(Vector3 worldPos, (float x, float y) size, string name, Color color, float Range = float.MaxValue, bool tracer = false)
		{
			var cam = Camera.main;
			Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);

			float distance = Mathf.Round(Vector3.Distance(worldPos, cam.transform.parent.position));
			if (viewportPos.z < 0 || distance > Range) return;

			Vector2 screenPos = new Vector2(
				viewportPos.x * Screen.width,
				(1 - viewportPos.y) * Screen.height
			);

			// Base box size (scales with distance)
			float baseBoxSize = Mathf.Clamp(1000f / distance, 10f, 50f);

			// Apply size multiplier (e.g., for width/height adjustments)
			float boxWidth = baseBoxSize * size.x;
			float boxHeight = baseBoxSize * size.y;

			bool isInView = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

			if (isInView)
			{
				// Calculate the top-left corner to keep the box centered on screenPos
				Vector2 boxTopLeft = new Vector2(
					screenPos.x - (boxWidth * 0.5f),  // Center horizontally
					screenPos.y - (boxHeight * 0.5f)  // Center vertically
				);

				// Draw the box
				ExternalRender.Box(
					boxTopLeft,
					new Vector2(boxWidth, boxHeight),
					2f,
					color
				);

				// Draw the name label (above the box)
				if (!string.IsNullOrEmpty(name))
				{
					ExternalRender.String(
						screenPos.x,  // Centered horizontally
						screenPos.y - (boxHeight * 0.5f) - 20f,  // Place above the box
						200f, 20f,
						name,
						Color.white,
						true
					);
				}
			}

			if (tracer)
			{
				Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
				Vector2 tracerEndPos = isInView ? screenPos : new Vector2(
					Mathf.Clamp(viewportPos.x * Screen.width, 0, Screen.width),
					Mathf.Clamp((1 - viewportPos.y) * Screen.height, 0, Screen.height)
				);
				Render.Line(screenCenter, tracerEndPos, 1f, color);
			}
		}

		private static Vector2 WorldToScreenPoint(Vector3 worldPos, Matrix4x4 matrix)
		{
			Vector4 vector = matrix * new Vector4(worldPos.x, worldPos.y, worldPos.z, 1f);
			bool flag = vector.w <= 0.1f;
			Vector2 vector2;
			if (flag)
			{
				vector2 = new Vector2(float.NaN, float.NaN);
			}
			else
			{
				Vector3 vector3 = new Vector3(vector.x / vector.w, vector.y / vector.w, vector.z / vector.w);
				vector2 = new Vector2((vector3.x + 1f) * 0.5f * (float)Screen.width, (1f - (vector3.y + 1f) * 0.5f) * (float)Screen.height);
			}
			return vector2;
		}
	}
}
