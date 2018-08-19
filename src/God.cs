using System;
using Assets.Phunk.Core;
using UnityEngine;

public class God : MonoBehaviour
{
	public God()
	{
	}

	static God()
	{
	}

	public bool m_CheatsOn
	{
		get
		{
			return true;
		}
	}

	public static God m_Instance { get; private set; }

	private void Awake()
	{
		God.m_Instance = this;
	}

	private void Start()
	{
	}

	private void FindComponents()
	{
		if (this.cheats.PlayerGameObject == null)
		{
			this.cheats.PlayerGameObject = GameObject.Find("P_PlayerSystem");
		}
		if (this.cheats.PlayerGameObject == null)
		{
			Log.WarningFormat("{0}: God script has no player object; player-related cheats won't work.", new object[]
			{
				base.gameObject.name
			});
		}
		if (this.cheats.SunScript == null)
		{
			this.cheats.SunScript = GameObject.Find("P_SkyLighting").GetComponent<C_SunManager>();
		}
		if (this.cheats.Notification == null)
		{
			GameObject gameObject = GameObject.Find("_Notifications");
			if (gameObject != null)
			{
				this.cheats.Notification = gameObject.GetComponent<C_Notification>();
			}
		}
		if (this.cheats.Notification == null)
		{
			Log.WarningFormat("{0}: God script has no player hud assigned", new object[]
			{
				base.gameObject.name
			});
		}
		if (this.cheats.SqaudToSpawn == null)
		{
			Log.WarningFormat("{0}: God script has no squad object to spawn", new object[]
			{
				base.gameObject.name
			});
		}
		if (this.cheats.UiCamera == null)
		{
			this.cheats.UiCamera = GameObject.Find("UiCamera").GetComponent<Camera>();
		}
		if (this.Crosshair == null)
		{
			this.Crosshair = GameObject.Find("z_Crosshair");
		}
		if (this.m_pC_HUDFPS == null)
		{
			this.m_pC_HUDFPS = base.GetComponent<C_HUDFPS>();
		}
	}

	private void ToggleUI()
	{
		if (C_ProgressManager.m_Instance.m_IsGoingToEndGame)
		{
			return;
		}
		this._uiVisible = !this._uiVisible;
		this.cheats.UiCamera.nearClipPlane = ((!this._uiVisible) ? 10f : 0.3f);
		if (this.Crosshair != null)
		{
			this.Crosshair.SetActive(this._uiVisible);
		}
	}

	public void HideUI()
	{
		this._uiVisible = false;
		this.cheats.UiCamera.nearClipPlane = ((!this._uiVisible) ? 10f : 0.3f);
		if (this.Crosshair != null)
		{
			this.Crosshair.SetActive(this._uiVisible);
		}
	}

	private void Update()
	{
		this.FindComponents();
		if (cInput.GetKeyDown("ToggleUI"))
		{
			this.ToggleUI();
		}
		if (cInput.GetKeyDown("ToggleFPS"))
		{
			if (this.m_pC_HUDFPS.m_Active)
			{
				this.m_pC_HUDFPS.m_Active = false;
			}
			else if (!this.m_pC_HUDFPS.m_Active)
			{
				this.m_pC_HUDFPS.m_Active = true;
			}
		}
		if (!this.m_CheatsOn)
		{
			return;
		}
        if (Input.GetKeyDown(KeyCode.Keypad9))
		{
			God.Settings.Supergun = !God.Settings.Supergun;
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "Supergun" + God.Settings.Supergun.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad7))
		{
			God.Settings.Invisible = !God.Settings.Invisible;
			Log.Message("Invisible" + God.Settings.Invisible.ToString());
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "Invis" + God.Settings.Invisible.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad8))
		{
			God.Settings.NoPlayerDamage = !God.Settings.NoPlayerDamage;
			Log.Message("No Player Damage" + God.Settings.NoPlayerDamage.ToString());
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "NoPlyrDam" + God.Settings.NoPlayerDamage.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad4))
		{
			God.Settings.InfiniteAmmo = !God.Settings.InfiniteAmmo;
			Log.Message("InfiniteAmmo" + God.Settings.InfiniteAmmo.ToString());
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "InfAmmo" + God.Settings.InfiniteAmmo.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad5))
		{
			God.Settings.NoDamage = !God.Settings.NoDamage;
			Log.Message("No Damage" + God.Settings.NoDamage.ToString());
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "NoDam" + God.Settings.NoDamage.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad6))
		{
			God.Settings.IgnoreAudio = !God.Settings.IgnoreAudio;
			Log.Message("Ignore Audio" + God.Settings.IgnoreAudio.ToString());
			if (this.cheats.Notification)
			{
				this.cheats.Notification.m_pLabel.Text = "IgnrAud" + God.Settings.IgnoreAudio.ToString();
			}
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		checked
		{
			if (Input.GetKeyDown(KeyCode.Keypad0) && this.cheats.SqaudToSpawn)
			{
				UnityEngine.Object @object = UnityEngine.Object.Instantiate(this.cheats.SqaudToSpawn, Vector3.zero, Quaternion.identity) as GameObject;
				this.CloneSquadNumber++;
				@object.name = "SquadClone" + this.CloneSquadNumber;
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad3) && this.cheats.PheasToSpawn)
		{
			Vector3 position = this.cheats.PlayerGameObject.transform.position;
			RaycastHit raycastHit;
			if (Physics.Raycast(new Vector3(position.x + (float)UnityEngine.Random.Range(-15, 15), position.y + 20f, position.z + (float)UnityEngine.Random.Range(-15, 15)), -Vector3.up, out raycastHit))
			{
				if (!(raycastHit.collider == C_WorldTerrain.m_Instance.transform.collider))
				{
					return;
				}
				(UnityEngine.Object.Instantiate(this.cheats.PheasToSpawn, raycastHit.point, Quaternion.identity) as GameObject).name = "CheatPheasClone";
			}
		}
		if (Input.GetKeyDown(KeyCode.Keypad1))
		{
			this.cheats.SunScript.m_CycleSpeed -= 0.5f;
			this.cheats.Notification.m_pLabel.Text = "Sun Speed = " + this.cheats.SunScript.m_CycleSpeed.ToString();
			this.cheats.Notification.m_FadeTimer = 1f;
		}
		if (Input.GetKeyDown(KeyCode.Keypad2))
		{
			this.cheats.SunScript.m_CycleSpeed += 0.5f;
			this.cheats.Notification.m_pLabel.Text = "Sun Speed = " + this.cheats.SunScript.m_CycleSpeed.ToString();
			this.cheats.Notification.m_FadeTimer = 1f;
		}
	}

	public God.Cheats cheats = God.Settings;

	public C_HUDFPS m_pC_HUDFPS;

	public GameObject Crosshair;

	private int CloneSquadNumber;

	private int Chance;

	private bool _uiVisible = true;

	public static God.Cheats Settings = new God.Cheats();

	[Serializable]
	public class Cheats
	{
		public Cheats()
		{
		}

		public bool DrawGunshots;

		public bool NoDamage;

		public bool NoPlayerDamage;

		public bool Invisible;

		public bool InfiniteAmmo;

		public bool IgnoreAudio;

		public GameObject PlayerGameObject;

		public bool DisableBotSteering;

		public C_Notification Notification;

		public GameObject SqaudToSpawn;

		public bool invertMouseY;

		public bool drawHouseZones;

		public C_SunManager SunScript;

		public Camera UiCamera;

		public GameObject PheasToSpawn;

		public bool Supergun;
	}
}
