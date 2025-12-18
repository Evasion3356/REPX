using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Photon.Pun;
using Photon.Realtime;
using REPX.Cheats;
using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using UnityEngine;

namespace REPX
{
	internal class CheatGUI : MonoBehaviour
	{
		internal static CheatGUI Instance { get; private set; }

		private void Awake()
		{
			bool flag = CheatGUI.harmony == null;
			if (flag)
			{
				CheatGUI.harmony = new Harmony(CheatGUI.GUID);
				Harmony harmony = CheatGUI.harmony;
				if (harmony != null)
				{
					harmony.PatchAll();
				}
			}
			bool flag2 = CheatGUI.Instance != null && CheatGUI.Instance != this;
			if (flag2)
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				CheatGUI.Instance = this;
				this.Initialize();
			}
		}

		private void OnDestroy()
		{
			this._initialized = false;
			Harmony harmony = CheatGUI.harmony;
			if (harmony != null)
			{
				harmony.UnpatchAll(CheatGUI.GUID);
			}
			this._settingsData = null;
		}

		private void Initialize()
		{
			this._settingsData = Settings.Instance.SettingsData;
			this._initialized = true;
		}

		private void OnGUI()
		{
			// Collect and render ESP data for external window
			RenderExternalESP();

			bool flag = !this._initialized;
			if (!flag)
			{
				if (this._style == null)
				{
					this._style = new GUIStyle(GUI.skin.label)
					{
						normal =
						{
							textColor = Color.white
						},
						fontStyle = FontStyle.Bold
					};
				}
				bool flag2 = Settings.Instance != null && Settings.Instance.b_IsMenuOpen;
				if (flag2)
				{
					Settings.Instance.WindowRect = GUILayout.Window(0, Settings.Instance.WindowRect, new GUI.WindowFunction(this.MenuContent), "REPX", Array.Empty<GUILayoutOption>());
				}
			}
		}

		private void Update()
		{
			this.MenuUpdate();
			this.SelectedPlayerUpdate();
		}

		private void RenderExternalESP()
		{

			try
			{
				var espData = new ExternalWindow.ESPRenderData();

				if (!Settings.Instance.SettingsData.b_Esp ||
					(/*Input.GetKey(KeyCode.F12) ||*/ Input.GetKey(KeyCode.RightShift)) ||
					(Camera.main == null || SemiFunc.IsMainMenu() || SemiFunc.RunIsLobbyMenu() || LoadingUI.instance.gameObject.activeSelf))
				{
					ExternalWindow.UpdateESPData(espData);
					return; 
				}
				var cam = Camera.main;
				var settings = Settings.Instance.SettingsData;

				// Render Players
				if (settings.b_PlayerEsp && GameDirector.instance != null)
				{
					foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
					{
						try
						{
							if (playerAvatar.IsLocalPlayer()) continue;

							Vector3 targetPosition = playerAvatar.playerAvatarVisuals.bodyTopUpTransform.position;
							float sizeX = 1f;
							float sizeY = 2.5f;
							Color color = settings.c_PlayerEspColor;
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
								if (settings.b_PlayerNameEsp)
								{
									string playerName = playerAvatar.GetPlayerName();
									name = playerName;
								}
							}
							else
							{
								if (settings.b_PlayerNameEsp)
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

							AddEspElement(espData, cam, targetPosition, (sizeX, sizeY), name, color, float.MaxValue, settings.b_Tracer);
						}
						catch (Exception ex)
						{
							Log.LogError("RenderExternalESP - Player Failed: " + ex.ToString());
						}
					}
				}

				// Render Enemies
				if (settings.b_EnemyEsp && EnemyDirector.instance != null)
				{
					foreach (EnemyParent enemyParent in EnemyDirector.instance.enemiesSpawned)
					{
						try
						{
							if (enemyParent == null) continue;

							Enemy enemy = enemyParent.GetField<Enemy>("Enemy");
							if (enemy == null) continue;

							EnemyHealth enemyHealth = enemy.GetField<EnemyHealth>("Health");
							if (enemyHealth == null) continue;

							bool isDead = enemyHealth.GetField<bool>("dead");
							bool isSpawned = enemyParent.GetField<bool>("Spawned");

							if (!isDead && isSpawned)
							{
								string name = string.Empty;
								if (settings.b_EnemyNameEsp)
								{
									int enemyCurrentHealth = enemyHealth.GetField<int>("healthCurrent");
									name = string.Format("{0} {1}HP", enemyParent.enemyName, enemyCurrentHealth);
								}

								AddEspElement(espData, cam, enemy.CenterTransform.position, (1f, 1f), name, settings.c_EnemyEspColor, settings.f_EspRange, settings.b_Tracer);
							}
						}
						catch (Exception ex)
						{
							Log.LogError("RenderExternalESP - Enemy Failed: " + ex.ToString());
						}
					}
				}

				// Render Items
				if (settings.b_ItemEsp && !SemiFunc.RunIsLobby())
				{
					foreach (PhysGrabObject physGrabObject in MonoHelper.CatchedPhysGrabObjects)
					{
						try
						{
							if (physGrabObject == null || !physGrabObject.GetField<bool>("isActive")) continue;

							ItemAttributes itemAttributes = physGrabObject.GetComponent<ItemAttributes>();
							float distance = Vector3.Distance(physGrabObject.centerPoint, cam.transform.parent.position);
							var bounds = CalculateObjectBounds(physGrabObject, distance);

							// Valuables
							if (physGrabObject.GetField<bool>("isValuable"))
							{
								int value = (int)physGrabObject.GetComponent<ValuableObject>().GetField<float>("dollarValueCurrent");
								Color color;
								if (value < 5000)
									color = settings.c_ItemEspColorLow;
								else if (value >= 5000 && value <= 10000)
									color = settings.c_ItemEspColorMedium;
								else
									color = settings.c_ItemEspColorHigh;

								string drawName = settings.b_ItemValueEsp ? string.Format("${0:N0}", value) : string.Empty;
								AddEspElement(espData, cam, physGrabObject.centerPoint, bounds, drawName, color, settings.f_EspRange, settings.b_Tracer);
							}

							// Carts
							if (physGrabObject.GetField<bool>("isCart"))
							{
								PhysGrabCart cart = physGrabObject.GetComponent<PhysGrabCart>();
								int cartValue = cart.GetField<int>("haulCurrent");
								string itemName = itemAttributes.GetField<string>("itemName");
								string drawName = settings.b_ItemValueEsp ? string.Format("{0} ${1:N0}", itemName, cartValue) : string.Empty;
								AddEspElement(espData, cam, physGrabObject.centerPoint, bounds, drawName, settings.c_CartEspColor, settings.f_EspRange, settings.b_Tracer);
							}

							// Weapons (Guns & Melee)
							bool isGun = physGrabObject.GetField<bool>("isGun");
							bool isMelee = physGrabObject.GetField<bool>("isMelee");
							if (isGun || isMelee)
							{
								string itemName = itemAttributes.GetField<string>("itemName");
								AddEspElement(espData, cam, physGrabObject.centerPoint, bounds, itemName, settings.c_WeaponEspColor, settings.f_EspRange, settings.b_Tracer);
							}

							// Drones
							if (physGrabObject.GetComponent<ItemDrone>())
							{
								string itemName = itemAttributes.GetField<string>("itemName");
								AddEspElement(espData, cam, physGrabObject.centerPoint, bounds, itemName, settings.c_ItemEspColorDrone, settings.f_EspRange, settings.b_Tracer);
							}
						}
						catch (Exception ex)
						{
							Log.LogError("RenderExternalESP - Item Failed: " + ex.ToString());
						}
					}
				}

				// Render Objects (Extraction & Truck)
				if (settings.b_extractionESP && RoundDirector.instance != null)
				{
					int num = 0;
					foreach (GameObject gameObject in RoundDirector.instance.GetField<List<GameObject>>("extractionPointList"))
					{
						num++;
						try
						{
							if (gameObject == null) continue;

							ExtractionPoint ep = gameObject.GetComponent<ExtractionPoint>();
							if (ep != null)
							{
								if (ep.GetField<ExtractionPoint.State>("currentState") == ExtractionPoint.State.Complete) continue;

								Color color = Color.white;
								if (ep == RoundDirector.instance.GetField<ExtractionPoint>("extractionPointCurrent"))
								{
									color = Color.green;
								}

								AddEspElement(espData, cam, gameObject.transform.position, (1f, 1f), string.Format("Extraction Point ({0})", num), color, settings.f_EspRange, false);
							}
						}
						catch (Exception ex)
						{
							Log.LogError("RenderExternalESP - Extraction Failed: " + ex.ToString());
						}
					}
				}

				if (settings.b_truckESP && LevelGenerator.Instance != null)
				{
					try
					{
						var levelPathTruck = LevelGenerator.Instance.LevelPathTruck;
						if (levelPathTruck)
						{
							Color orange = new Color(1f, 0.5f, 0f, 1f);
							AddEspElement(espData, cam, levelPathTruck.transform.position, (2f, 2f), "Truck", orange, settings.f_EspRange, false);
						}
					}
					catch (Exception ex)
					{
						Log.LogError("RenderExternalESP - Truck Failed: " + ex.ToString());
					}
				}

				// Render Laser (gun laser sight)
				if (settings.b_LaserESP && !SemiFunc.RunIsLobby())
				{
					foreach (PhysGrabObject physGrabObject in MonoHelper.CatchedPhysGrabObjects)
					{
						try
						{
							if (physGrabObject == null || !physGrabObject.grabbedLocal || !physGrabObject.GetField<bool>("isActive")) continue;

							bool isGun = physGrabObject.GetField<bool>("isGun");
							if (isGun)
							{
								ItemGun itemGun = physGrabObject.GetComponent<ItemGun>();
								if (itemGun != null && itemGun.gunMuzzle != null)
								{
									Vector3 barrelPosition = itemGun.gunMuzzle.position;
									Vector3 barrelDirection = itemGun.gunMuzzle.forward;

									float laserDistance = 100f;
									Vector3 laserEndPoint = barrelPosition + (barrelDirection * laserDistance);

									Vector3 viewportStart = cam.WorldToViewportPoint(barrelPosition);
									Vector3 viewportEnd = cam.WorldToViewportPoint(laserEndPoint);

									if (viewportStart.z > 0 && viewportEnd.z > 0)
									{
										Vector2 screenStart = new Vector2(viewportStart.x * Screen.width, (1 - viewportStart.y) * Screen.height);
										Vector2 screenEnd = new Vector2(viewportEnd.x * Screen.width, (1 - viewportEnd.y) * Screen.height);

										espData.Tracers.Add(new ExternalWindow.ESPLine
										{
											start = screenStart,
											end = screenEnd,
											color = Color.red
										});
									}
								}
							}
						}
						catch (Exception ex)
						{
							Log.LogError("RenderExternalESP - Laser Failed: " + ex.ToString());
						}
					}
				}

				// Update the external window with collected data
				ExternalWindow.UpdateESPData(espData);
			}
			catch (Exception ex)
			{
				Log.LogError("RenderExternalESP Failed: " + ex.ToString());
			}
		}

		private (float width, float height) CalculateObjectBounds(PhysGrabObject physGrabObject, float distance)
		{
			try
			{
				Camera cam = Camera.main;
				if (cam == null) return (1f, 1f);

				Vector3 boundingBoxSize = physGrabObject.boundingBox;
				Vector3 cameraToObject = (physGrabObject.centerPoint - cam.transform.position).normalized;
				Vector3 objectRight = physGrabObject.transform.right;
				Vector3 objectForward = physGrabObject.transform.forward;

				float rightDot = Mathf.Abs(Vector3.Dot(cameraToObject, objectRight));
				float forwardDot = Mathf.Abs(Vector3.Dot(cameraToObject, objectForward));

				float width = (rightDot < forwardDot) ? boundingBoxSize.x : boundingBoxSize.z;
				float height = boundingBoxSize.y;

				float distanceScale = Mathf.Clamp(10f / distance, 1f, 10f);

				return (width * distanceScale, height * distanceScale);
			}
			catch
			{
				return (1f, 1f);
			}
		}

		private void AddEspElement(ExternalWindow.ESPRenderData espData, Camera cam, Vector3 worldPos, (float x, float y) size, string name, Color color, float range, bool tracer)
		{
			Vector3 viewportPos = cam.WorldToViewportPoint(worldPos);
			float distance = Vector3.Distance(worldPos, cam.transform.parent.position);

			if (viewportPos.z < 0 || distance > range) return;

			Vector2 screenPos = new Vector2(
				viewportPos.x * Screen.width,
				(1 - viewportPos.y) * Screen.height
			);

			float baseBoxSize = Mathf.Clamp(1000f / distance, 10f, 50f);
			float boxWidth = baseBoxSize * size.x;
			float boxHeight = baseBoxSize * size.y;

			bool isInView = viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1;

			if (isInView)
			{
				Vector2 boxTopLeft = new Vector2(
					screenPos.x - (boxWidth * 0.5f),
					screenPos.y - (boxHeight * 0.5f)
				);
				Vector2 boxBottomRight = new Vector2(
					screenPos.x + (boxWidth * 0.5f),
					screenPos.y + (boxHeight * 0.5f)
				);

				espData.Boxes.Add(new ExternalWindow.ESPBox
				{
					topLeft = boxTopLeft,
					bottomRight = boxBottomRight,
					color = color
				});

				if (!string.IsNullOrEmpty(name))
				{
					Vector2 labelPos = new Vector2(
						screenPos.x,
						screenPos.y - (boxHeight * 0.5f) - 20f
					);

					espData.Items.Add(new ExternalWindow.ESPItem
					{
						screenPos = labelPos,
						label = name,
						color = color
					});
				}

				if (tracer)
				{
					Vector2 screenCenter = new Vector2(Screen.width / 2f, Screen.height / 2f);
					espData.Tracers.Add(new ExternalWindow.ESPLine
					{
						start = screenCenter,
						end = screenPos,
						color = color
					});
				}
			}
		}

		private void MenuUpdate()
		{
			bool b_IsMenuOpen = Settings.Instance.b_IsMenuOpen;
			bool flag = b_IsMenuOpen;
			if (flag)
			{
				bool flag2 = !Cursor.visible;
				if (flag2)
				{
					Cursor.visible = true;
				}
				bool flag3 = Cursor.lockState == CursorLockMode.Locked;
				if (flag3)
				{
					Cursor.lockState = CursorLockMode.None;
				}
			}
			bool keyDown = Input.GetKeyDown(KeyCode.Insert);
			if (keyDown)
			{
				Settings.Instance.b_IsMenuOpen = !b_IsMenuOpen;
				this.UpdateCursorState();
			}
		}

		private void SelectedPlayerUpdate()
		{
			float deltaTime = Time.deltaTime;
			bool flag = this._playerSel == 0;
			List<PlayerAvatar> playerList = GameDirector.instance.PlayerList;
			bool flag2 = playerList.Count > 0;
			if (flag2)
			{
				List<PlayerAvatar> list;
				if (!flag)
				{
					(list = new List<PlayerAvatar>(1)).Add(playerList.ElementAt(this._playerSel - 1));
				}
				else
				{
					list = GameDirector.instance.PlayerList;
				}
				List<PlayerAvatar> list2 = list;
				bool flag10 = this._spamChatMessage && this._msgTime > 0.25f;
				if (flag10)
				{
					foreach (PlayerAvatar playerAvatar4 in list2)
					{
						bool flag11 = playerAvatar4.GetField<bool>("isCrouching");
						bool field = playerAvatar4.GetField<bool>("isDisabled");
						if (field)
						{
							flag11 = true;
						}
						playerAvatar4.photonView.RPC("ChatMessageSendRPC", RpcTarget.All, new object[] { this._chatMessage, flag11 });
						this._msgTime = 0f;
					}
				}
				this._msgTime += deltaTime;
			}
		}

		private void UpdateCursorState()
		{
			bool flag = Settings.Instance == null;
			if (!flag)
			{
				bool b_IsMenuOpen = Settings.Instance.b_IsMenuOpen;
				CursorLockMode cursorLockMode = (b_IsMenuOpen ? CursorLockMode.None : CursorLockMode.Locked);
				Cursor.visible = b_IsMenuOpen;
				bool flag2 = Cursor.lockState != cursorLockMode;
				if (flag2)
				{
					Cursor.lockState = cursorLockMode;
				}
			}
		}

		private void DrawWatermark()
		{
			bool flag = this._style == null;
			if (!flag)
			{
				GUI.backgroundColor = new Color(0.09019608f, 0.09019608f, 0.09019608f, 1f);
				GUI.contentColor = Color.white;
				SettingsData settingsData = this._settingsData;
				GUI.color = ((settingsData != null) ? settingsData.c_Theme : Color.white);
				string text = "REPX";
				bool flag2 = this._settingsData != null;
				if (flag2)
				{
					text = text + " | v" + CheatGUI.version;
					bool flag3 = Settings.Instance != null && !Settings.Instance.b_IsMenuOpen;
					if (flag3)
					{
						text += " | Press INSERT";
					}
				}
				GUI.Label(new Rect(10f, 5f, 500f, 25f), text, this._style);
			}
		}

		private void MenuContent(int windowID)
		{
			bool flag = this._settingsData == null || Settings.Instance == null;
			if (!flag)
			{
				GUI.DragWindow(new Rect(0f, 0f, Settings.Instance.WindowRect.width, 20f));
				GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
				UI.Tab<UI.Tabs>("About", ref UI.nTab, UI.Tabs.About, false);
				UI.Tab<UI.Tabs>("ESP", ref UI.nTab, UI.Tabs.ESP, false);
				bool flag2 = !SemiFunc.IsMainMenu();
				if (flag2)
				{
					bool flag3 = !SemiFunc.RunIsLobbyMenu();
					if (flag3)
					{
						UI.Tab<UI.Tabs>("Self", ref UI.nTab, UI.Tabs.Self, false);
					}
					UI.Tab<UI.Tabs>("Players", ref UI.nTab, UI.Tabs.Players, false);
					UI.Tab<UI.Tabs>("Level", ref UI.nTab, UI.Tabs.Level, false);
				}
				UI.Tab<UI.Tabs>("Misc", ref UI.nTab, UI.Tabs.Misc, false);
				UI.Tab<UI.Tabs>("Settings", ref UI.nTab, UI.Tabs.Settings, false);
				GUILayout.EndHorizontal();
				this._menuScrollPos = GUILayout.BeginScrollView(this._menuScrollPos, Array.Empty<GUILayoutOption>());
				UI.Reset();
				switch (UI.nTab)
				{
					case UI.Tabs.About:
						this.DrawAboutTab();
						break;
					case UI.Tabs.ESP:
						this.DrawESPTab();
						break;
					case UI.Tabs.Self:
						{
							bool flag5 = SemiFunc.IsMainMenu() || SemiFunc.RunIsLobbyMenu();
							if (flag5)
							{
								UI.nTab = UI.Tabs.About;
							}
							else
							{
								this.DrawSelfTab();
							}
							break;
						}
					case UI.Tabs.Players:
						{
							bool flag6 = SemiFunc.IsMainMenu();
							if (flag6)
							{
								UI.nTab = UI.Tabs.About;
							}
							else
							{
								this.DrawPlayersTab();
							}
							break;
						}
					case UI.Tabs.Level:
						this.DrawLevelTab();
						break;
					case UI.Tabs.Misc:
						this.DrawMiscTab();
						break;
					case UI.Tabs.Settings:
						this.DrawSettingsTab();
						break;
				}
				GUILayout.EndScrollView();
				bool b_Tooltips = this._settingsData.b_Tooltips;
				if (b_Tooltips)
				{
					UI.RenderTooltip();
				}
				GUI.DragWindow(new Rect(0f, 0f, 0f, 20f));
			}
		}

		private void DrawAboutTab()
		{
			string text = "Welcome to REPX v" + CheatGUI.version + " by DiegoTheWise & gir489\n\nFeatures:\nInformation Spoofing\nESP\nMisc Cheats\nSavable Config\nAnd more!";
			string text2 = "\n\nChange Log:\nAdded: Level Tab.\nAdded: All Players option in Players Tab.\nAdded: Force Name in Players Tab.\nAdded: Force Message in Players Tab.\nAdded: Shake Screen in Players Tab.\nAdded: Fake Lag in Players Tab.\nAdded: Disable Player in Players Tab.\nAdded: Kick Player in Players Tab.\nAdded: Softlock Game in Trolling Tab.\nAnd much more!\n\nv1.1.1:\nUpdated for v0.3.2 of RE.P.O by gir489.\n\nv1.1.2:\nAdded stream-proof ESP.";
			GUILayout.Label(text + text2, GUI.skin.textArea, new GUILayoutOption[] { GUILayout.ExpandHeight(true) });
		}

		private void DrawESPTab()
		{
			bool flag = this._settingsData != null;
			if (flag)
			{
				UI.Checkbox(ref this._settingsData.b_Esp, "Esp", "Toggle all ESP features on/off");
				UI.Slider(ref this._settingsData.f_EspRange, 5f, 1000f, "Esp Range", "Set the maximum distance for ESP rendering.", false);
				UI.Checkbox(ref this._settingsData.b_Tracer, "Esp Tracer", "Draw lines from player to ESP targets.");
				UI.Checkbox(ref this._settingsData.b_ItemEsp, "Item Esp", "Highlight items in the world.");
				bool b_ItemEsp = this._settingsData.b_ItemEsp;
				if (b_ItemEsp)
				{
					UI.Checkbox(ref this._settingsData.b_ItemValueEsp, "Item Value Esp", "Show how much the item is worth above the item.");
				}
				UI.Checkbox(ref this._settingsData.b_PlayerEsp, "Player Esp", "Highlight other players.");
				bool b_PlayerEsp = this._settingsData.b_PlayerEsp;
				if (b_PlayerEsp)
				{
					UI.Checkbox(ref this._settingsData.b_PlayerNameEsp, "Player Name Esp", "Show names above highlighted players.");
				}
				UI.Checkbox(ref this._settingsData.b_EnemyEsp, "Enemy Esp", "Highlight enemies.");
				bool b_EnemyEsp = this._settingsData.b_EnemyEsp;
				if (b_EnemyEsp)
				{
					UI.Checkbox(ref this._settingsData.b_EnemyNameEsp, "Enemy Name Esp", "Show names above highlighted enemies.");
				}
				UI.Checkbox(ref this._settingsData.b_LaserESP, "Weapon Laser ESP", "Draws a laser from your gun to where it's aiming while holding it.");
				UI.Checkbox(ref this._settingsData.b_extractionESP, "Extraction ESP", "Draws a box on the extraction point.");
				UI.Checkbox(ref this._settingsData.b_truckESP, "Truck ESP", "Draws a box on truck.");
			}
		}

		private void DrawSelfTab()
		{
			bool flag = this._settingsData != null;
			if (flag)
			{
				UI.Checkbox(ref this._settingsData.b_InfiniteStamina, "Infinite Stamina", "Gives the local player Infinite Stamina.");
				UI.Checkbox(ref this._settingsData.b_GodMode, "God Mode", "Puts the local player in a invulnerable state.");
				UI.Checkbox(ref this._settingsData.b_Invulnerable, "Invulnerable", "Makes the local player be ignored by Enemy AI and other interactions.");
				UI.Checkbox(ref this._settingsData.b_Invisibility, "Invisibility", "Makes the player invisible to other players, only works in multiplayer!");
				UI.Checkbox(ref this._settingsData.b_AntiKnockBack, "Anti-Knock Back", "Prevents the local player from being able to be knocked back by force.");
				UI.Checkbox(ref this._settingsData.b_NoTumble, "No Tumble", "Prevents the local player from tumbling by unvoluntary action.");
				UI.Checkbox(ref this._settingsData.b_IndestructibleObjects, "Indestructible Objects", "Makes objects not take damage by local actions.");
				UI.Checkbox(ref this._settingsData.b_HearEveryone, "Hear Everyone", "Makes it where you can hear everyone no matter the range.");
			}
		}

		private void DrawPlayersTab()
		{
			if (this._settingsData == null || GameDirector.instance == null)
				return;

			var players = GameDirector.instance.PlayerList;
			if (players.Count <= 0)
			{
				GUILayout.Label("No players found.", new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
				return;
			}

			List<string> playerNames = new List<string> { "All Players" };
			playerNames.AddRange(players.Select(pa => pa.GetPlayerName()));

			UI.Dropdown(ref this._playerSel, "Player", playerNames.ToArray(), "");

			bool isAllPlayers = this._playerSel == 0;
			List<PlayerAvatar> selectedPlayers;
			if (!isAllPlayers)
			{
				selectedPlayers = new List<PlayerAvatar>(1) { players.ElementAt(this._playerSel - 1) };
			}
			else
			{
				selectedPlayers = GameDirector.instance.PlayerList;
			}

			string playerLabel = isAllPlayers ? "All" : selectedPlayers[0].GetPlayerName();
			GUILayout.Label("Player: " + playerLabel, new GUILayoutOption[] { GUILayout.ExpandWidth(false) });

			if (!isAllPlayers)
			{
				string steamId = selectedPlayers[0].GetSteamId();
				UI.TextBox(ref steamId, "Steam Id:", "", 200, 0);
			}

			if (!SemiFunc.RunIsLobbyMenu())
			{
				UI.Slider(ref this._dhAmount, 0f, 200f, "Heal Amount", "the Heal amount.", true);
				UI.Button("Heal", "Heal the player.", () =>
				{
					foreach (PlayerAvatar playerAvatar in selectedPlayers)
					{
						if (!playerAvatar.IsDead())
						{
							playerAvatar.playerHealth.HealOther((int)this._dhAmount, false);
						}
					}
				});
			}

			if (SemiFunc.IsMultiplayer())
			{
				UI.Header("Multiplayer");
				if (!isAllPlayers)
				{
					UI.Button("Teleport " + playerLabel + " To Self", "Teleports the selected player to the local player.", () =>
					{
						PlayerAvatar playerAvatar = selectedPlayers[0];
						if (!playerAvatar.IsLocalPlayer())
						{
							playerAvatar.TeleportExploit(PlayerAvatar.instance.transform.position, null, 0);
						}
					});

					UI.Button("Teleport All Players To " + playerLabel, "Teleports all players to the selected player.", () =>
					{
						PlayerAvatar targetPlayer = selectedPlayers[0];
						foreach (PlayerAvatar playerAvatar in players)
						{
							if (playerAvatar != targetPlayer)
							{
								playerAvatar.TeleportExploit(targetPlayer.transform.position, null, 0);
							}
						}
					});
				}

				UI.Divider();
				UI.TextBox(ref this._forcePlayerName, "New Player Name", "The new player named to set.", 200, 0);
				UI.Button("Force Player Name", "Forcibly changes the selected player's name.", () =>
				{
					foreach (PlayerAvatar playerAvatar in selectedPlayers)
					{
						playerAvatar.SetNameExploit(this._forcePlayerName, null);
					}
				});

				UI.Divider();
				UI.Checkbox(ref this._hideMessage, "Hide message from selected player", "Prevents the selected player from seeing the forced message.");
				UI.TextBox(ref this._chatMessage, "Chat Message", "Contents of Force Chat Message.", 300, 100);
				UI.Button("Force Chat Message", "Forces the selected player to send a chat message.", () =>
				{
					foreach (PlayerAvatar playerAvatar in selectedPlayers)
					{
						bool isCrouching = playerAvatar.GetField<bool>("isCrouching");
						bool isDisabled = playerAvatar.GetField<bool>("isDisabled");
						if (isDisabled)
						{
							isCrouching = true;
						}

						if (!this._hideMessage)
						{
							playerAvatar.photonView.RPC("ChatMessageSendRPC", RpcTarget.All, new object[]
							{
								this._chatMessage,
								isCrouching
							});
						}
						else
						{
							foreach (Player player in PhotonNetwork.PlayerList)
							{
								if (player != playerAvatar.photonView.Owner)
								{
									playerAvatar.photonView.RPC("ChatMessageSendRPC", player, new object[]
									{
										this._chatMessage,
										isCrouching
									});
								}
							}
						}
					}
				});

				UI.Checkbox(ref this._spamChatMessage, "Spam Chat Message", "Makes the selected player spam a chat message.");
			}

			if (!isAllPlayers)
			{
				UI.Header("Upgrades");
				StatsManager stats = PunManager.instance.GetField<StatsManager>("statsManager");

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeHealth, "Health Upgrade", "Upgrade Health", (str, num) =>
				{
					stats.playerUpgradeHealth[str] += num;
					PunManager.instance.InvokeMethod("UpdateHealthRightAway", new object[] { str, num });
				});

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeStrength, "Grab Strength Upgrade", "Upgrade Grab Strength", (str, num) =>
				{
					stats.playerUpgradeStrength[str] += num;
					PunManager.instance.InvokeMethod("UpdateGrabStrengthRightAway", new object[] { str, num });
				});

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeStamina, "Sprint Energy", "Upgrade Energy", (str, num) =>
				{
					stats.playerUpgradeStamina[str] += num;
					PunManager.instance.InvokeMethod("UpdateEnergyRightAway", new object[] { str, num });
				});

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeSpeed, "Sprint Speed Upgrade", "Upgrade Sprint Speed", (str, num) =>
				{
					stats.playerUpgradeSpeed[str] += num;
					PunManager.instance.InvokeMethod("UpdateSprintSpeedRightAway", new object[] { str, num });
				});

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeExtraJump, "Extra Jump Upgrade", "Upgrade Extra Jump", (str, num) =>
				{
					stats.playerUpgradeExtraJump[str] += num;
					PunManager.instance.InvokeMethod("UpdateExtraJumpRightAway", new object[] { str, num });
				});

				this.DrawUpgradeButton(selectedPlayers, stats.playerUpgradeRange, "Grab Range Upgrade", "Upgrade Grab Range", (str, num) =>
				{
					stats.playerUpgradeRange[str] += num;
					PunManager.instance.InvokeMethod("UpdateGrabRangeRightAway", new object[] { str, num });
				});
			}
		}

		private void DrawKillButton(List<PlayerAvatar> selectedPlayers)
		{
			UI.Button("Kill", "Kills the selected player.", () =>
			{
				foreach (PlayerAvatar playerAvatar in selectedPlayers)
				{
					if (!playerAvatar.IsDead())
					{
						playerAvatar.PlayerDeath(-1);
					}
				}
			});
		}

		private void DrawSoftlockButton(List<PlayerAvatar> selectedPlayers)
		{
			UI.Button("Softlock Player", "Soft locks the selected player by forcing outro state.", () =>
			{
				foreach (PlayerAvatar playerAvatar in selectedPlayers)
				{
					playerAvatar.OutroExploit();
				}
			});
		}

		private void DrawUpgradeButton(List<PlayerAvatar> selectedPlayers, Dictionary<string, int> upgradeDict, string label, string tooltip, Action<string, int> upgradeAction)
		{
			PlayerAvatar player = selectedPlayers[0];
			string steamId = player.GetSteamId();
			int currentValue = 0;
			if (upgradeDict.ContainsKey(steamId))
			{
				currentValue = upgradeDict[steamId];
			}

			GUILayout.BeginHorizontal(Array.Empty<GUILayoutOption>());
			GUILayout.Label(string.Format("{0}: {1}", label, currentValue), new GUILayoutOption[] { GUILayout.Width(200f) });

			if (GUILayout.Button("+1", new GUILayoutOption[] { GUILayout.Width(50f) }))
			{
				upgradeAction(steamId, 1);
			}
			if (GUILayout.Button("+5", new GUILayoutOption[] { GUILayout.Width(50f) }))
			{
				upgradeAction(steamId, 5);
			}
			if (GUILayout.Button("+10", new GUILayoutOption[] { GUILayout.Width(50f) }))
			{
				upgradeAction(steamId, 10);
			}
			if (GUILayout.Button("Reset", new GUILayoutOption[] { GUILayout.Width(60f) }))
			{
				upgradeAction(steamId, -currentValue);
			}

			GUILayout.EndHorizontal();
		}

		private void DrawLevelTab()
		{
			bool flag = this._settingsData != null;
			if (flag)
			{
				UI.Button("Force Arena Game", "Forces to load the Arena Game.", () =>
				{
					foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
					{
						if (!playerAvatar.IsDead())
						{
							playerAvatar.PlayerDeath(-1);
						}
					}
				});

				bool flag2 = SemiFunc.RunIsLevel();
				if (flag2)
				{
					List<GameObject> extractionPoints = RoundDirector.instance.GetField<List<GameObject>>("extractionPointList");
					bool flag3 = extractionPoints.Count > 0;
					if (flag3)
					{
						string[] pointNames = extractionPoints.Select((point, index) => string.Format("Extraction Point {0}", index + 1)).ToArray();
						UI.Header("Extraction");
						UI.Dropdown(ref this._extractionIndex, "Extraction Points", pointNames, "");

						GameObject selectedObj = extractionPoints.ElementAt(this._extractionIndex);
						ExtractionPoint selectedExtraction = (selectedObj != null) ? selectedObj.GetComponent<ExtractionPoint>() : null;

						if (selectedExtraction != null)
						{
							ExtractionPoint.State currentState = selectedExtraction.GetField<ExtractionPoint.State>("currentState");

							if (currentState != ExtractionPoint.State.Complete)
							{
								if (currentState != ExtractionPoint.State.Idle)
								{
									GUILayout.Label(string.Format("Current Goal: ({0}/{1})", selectedExtraction.GetField<int>("haulCurrent"), selectedExtraction.haulGoal), new GUILayoutOption[] { GUILayout.ExpandWidth(false) });
									UI.Slider(ref this._newExtractionGoal, 0, 100000, "New Goal", "Amount of cash needed for new extraction goal.");
									UI.Button("Set Extraction Goal", "Sets a new cash goal for the current selected extraction.", () =>
									{
										if (!SemiFunc.IsMultiplayer())
										{
											selectedExtraction.HaulGoalSetRPC(this._newExtractionGoal);
										}
									});

									UI.Slider(ref this._surplusExtractionGoal, 0, 100000, "Surplus Amount", "Amount of cash set when activating extraction..");
									UI.Button("Surplus Extraction", "Activates extraction to calculate input amount of cash.", () =>
									{
										if (!SemiFunc.IsMultiplayer())
										{
											selectedExtraction.ExtractionPointSurplusRPC(this._surplusExtractionGoal);
										}
									});
								}
							}
						}
					}
				}
			}
		}

		private void DrawMiscTab()
		{
			bool flag = this._settingsData != null;
			if (flag)
			{
				bool flag2 = !SemiFunc.IsMainMenu();
				if (flag2)
				{
					UI.Button("Force Exit Game", "Forces the local player to exit the current game.", () =>
					{
						NetworkManager.instance.LeavePhotonRoom();
						PhotonNetwork.NetworkingClient.State = ClientState.Leaving;
					});
				}
			}
		}

		private void DrawSettingsTab()
		{
			bool flag = this._settingsData != null;
			if (flag)
			{
				if (_cachedUnloadAction == null)
				{
					_cachedUnloadAction = new Action(Loader.Unload);
				}
				UI.Button("Unload Menu", "Unloads the menu from the game.", _cachedUnloadAction);
				UI.Button("Save Settings", "Saves all settings", new Action(Settings.Instance.SaveSettings));
				UI.Checkbox(ref this._settingsData.b_AutoSave, "Auto Save", "Automatically saves config when a value changes.");
				UI.Checkbox(ref this._settingsData.b_Tooltips, "Tooltips", "");
				UI.Checkbox(ref this._settingsData.b_IgnoreChat, "Ignore Chat", "Ignore chat inputs while menu is open.");
				UI.Checkbox(ref this._settingsData.b_IgnoreMouseMovement, "Ignore Mosue Movement", "Ignore mouse movement inputs while menu is open.");
				UI.Checkbox(ref this._settingsData.b_IgnoreMovement, "Ignore Movement", "Ignore movement inputs while menu is open.");
				UI.ColorPicker(ref this._settingsData.c_Theme, "Menu Theme");
				UI.ColorPicker(ref this._settingsData.c_PlayerEspColor, "Player Esp Color");
				UI.ColorPicker(ref this._settingsData.c_EnemyEspColor, "Enemy Esp Color");
				UI.ColorPicker(ref this._settingsData.c_ItemEspColorLow, "Item Low Value Esp Color");
				UI.ColorPicker(ref this._settingsData.c_ItemEspColorMedium, "Item Medium Value Esp Color");
				UI.ColorPicker(ref this._settingsData.c_ItemEspColorHigh, "Item High Value Esp Color");
				UI.ColorPicker(ref this._settingsData.c_ItemEspColorDrone, "Drone Esp Color");
				UI.ColorPicker(ref this._settingsData.c_CartEspColor, "Cart Esp Color");
				UI.ColorPicker(ref this._settingsData.c_WeaponEspColor, "Weapon Esp Color");
			}
		}

		internal static float Round(float value, int digits)
		{
			float num = Mathf.Pow(10f, (float)digits);
			return Mathf.Round(value * num) / num;
		}

		internal static readonly string GUID = "com.repx.loader";
		internal static readonly string version = "1.1.2";
		internal static Harmony harmony;
		private GUIStyle _style;
		private SettingsData _settingsData;
		private bool _initialized;
		private float _msgTime = 0f;
		private Dictionary<PlayerAvatar, Vector3> _lagLastPos = new Dictionary<PlayerAvatar, Vector3>();
		private Vector2 _menuScrollPos;
		private int _playerSel = 0;
		private float _dhAmount = 25f;
		private string _forcePlayerName = string.Empty;
		private bool _hideMessage;
		private string _chatMessage = string.Empty;
		private bool _spamChatMessage;
		private int _newExtractionGoal;
		private int _surplusExtractionGoal;
		private int _extractionIndex;
		private readonly string[] _playerTriggerActions = new string[] { "None", "Kill", "Ragdoll" };
		private readonly string[] _objectTriggerActions = new string[] { "None", "Despawn" };
		private static Action _cachedUnloadAction;
	}
}