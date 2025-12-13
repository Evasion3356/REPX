using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using REPX.Data;
using REPX.Extensions;
using REPX.Helpers;
using UnityEngine;

namespace REPX.Cheats
{
	internal static class EspCheat
	{
		internal static void RenderEsp()
		{
			bool flag = !Settings.Instance.SettingsData.b_Esp;
			if (!flag)
			{
				bool flag2 = Camera.main == null;
				if (!flag2)
				{
					Camera main = Camera.main;
					try
					{
						EspCheat.RenderPlayers(main);
						EspCheat.RenderEnemies(main);
						EspCheat.RenderItems(main);
						EspCheat.RenderObjects(main);
					}
					catch (Exception ex)
					{
						Log.LogError(ex);
					}
				}
			}
		}

		private static void RenderPlayers(Camera cam)
		{
			bool flag = !Settings.Instance.SettingsData.b_PlayerEsp;
			if (!flag)
			{
				bool flag2 = GameDirector.instance == null;
				if (!flag2)
				{
					foreach (PlayerAvatar playerAvatar in GameDirector.instance.PlayerList)
					{
						bool flag3 = playerAvatar == null || playerAvatar.IsLocalPlayer() || playerAvatar.IsDead();
						if (!flag3)
						{
							float field = playerAvatar.GetComponent<PlayerVisionTarget>().GetField<float>("TargetPosition");
							Vector3 vector = playerAvatar.transform.position + new Vector3(0f, field, 0f);
							EspCheat.RenderEspElement(Vector3.Lerp(playerAvatar.transform.position, vector, 0.65f), Settings.Instance.SettingsData.b_PlayerNameEsp ? playerAvatar.GetPlayerName() : string.Empty, Settings.Instance.SettingsData.c_PlayerEspColor, cam, float.MaxValue, Settings.Instance.SettingsData.b_Tracer);
						}
					}
				}
			}
		}

		private static void RenderEnemies(Camera cam)
		{
			bool flag = !Settings.Instance.SettingsData.b_EnemyEsp;
			if (!flag)
			{
				bool flag2 = EnemyDirector.instance == null;
				if (!flag2)
				{
					foreach (EnemyParent enemyParent in EnemyDirector.instance.enemiesSpawned)
					{
						bool flag3 = enemyParent == null;
						if (!flag3)
						{
							Enemy field = enemyParent.GetField<Enemy>("Enemy");
							bool field2 = field.GetField<object>("Health").GetField<bool>("dead");
							if (!field2)
							{
								EspCheat.RenderEspElement(field.CenterTransform.position, Settings.Instance.SettingsData.b_EnemyNameEsp ? enemyParent.enemyName : string.Empty, Settings.Instance.SettingsData.c_EnemyEspColor, cam, Settings.Instance.SettingsData.f_EspRange, Settings.Instance.SettingsData.b_Tracer);
							}
						}
					}
				}
			}
		}

		private static void RenderItems(Camera cam)
		{
			bool flag = !Settings.Instance.SettingsData.b_ItemEsp;
			if (!flag)
			{
				bool flag2 = SemiFunc.RunIsLobby();
				if (!flag2)
				{
					foreach (PhysGrabObject physGrabObject in MonoHelper.CatchedPhysGrabObjects)
					{
						bool flag3 = physGrabObject == null;
						if (!flag3)
						{
							bool flag4 = !physGrabObject.GetField<bool>("isValuable") && !physGrabObject.GetField<bool>("isCart");
							if (!flag4)
							{
								EspCheat.RenderEspElement(physGrabObject.centerPoint, Settings.Instance.SettingsData.b_ItemNameEsp ? physGrabObject.name.Replace("(Clone)", "") : string.Empty, Settings.Instance.SettingsData.c_ItemEspColor, cam, Settings.Instance.SettingsData.f_EspRange, Settings.Instance.SettingsData.b_Tracer);
							}
						}
					}
				}
			}
		}

		private static void RenderObjects(Camera cam)
		{
			int num = 0;
			foreach (GameObject gameObject in RoundDirector.instance.GetField<List<GameObject>>("extractionPointList"))
			{
				num++;
				bool flag = gameObject == null;
				if (!flag)
				{
					EspCheat.RenderEspElement(gameObject.transform.position, string.Format("Extraction Point ({0})", num), Color.white, cam, Settings.Instance.SettingsData.f_EspRange, false);
				}
			}
		}

		private static void RenderEspElement(Vector3 worldPos, string name, Color color, Camera cam, float Range = 3.4028235E+38f, bool tracer = false)
		{
			Vector3 vector = cam.WorldToViewportPoint(worldPos);
			float num = Mathf.Round(Vector3.Distance(worldPos, cam.transform.position));
			bool flag = vector.z < 0f || num > Range;
			if (!flag)
			{
				Vector2 vector2 = new Vector2(vector.x * (float)Screen.width, (1f - vector.y) * (float)Screen.height);
				float num2 = Mathf.Clamp(1000f / num, 10f, 50f);
				bool flag2 = vector.x >= 0f && vector.x <= 1f && vector.y >= 0f && vector.y <= 1f;
				bool flag3 = flag2;
				if (flag3)
				{
					Vector2 vector3 = new Vector2(vector2.x - num2 * 0.5f, vector2.y - num2 * 0.5f);
					Render.Box(vector3, new Vector2(num2, num2), 2f, color, true);
					bool flag4 = name != string.Empty;
					if (flag4)
					{
						Render.String(GUI.skin.label, vector2.x, vector2.y - num2 - 5f, 200f, 20f, name, Color.white, true, false);
					}
				}
				if (tracer)
				{
					Vector2 vector4 = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
					Vector2 vector5 = (flag2 ? vector2 : new Vector2(Mathf.Clamp(vector.x * (float)Screen.width, 0f, (float)Screen.width), Mathf.Clamp((1f - vector.y) * (float)Screen.height, 0f, (float)Screen.height)));
					Render.Line(vector4, vector5, 1f, color);
				}
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
