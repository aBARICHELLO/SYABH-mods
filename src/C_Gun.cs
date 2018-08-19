using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(C_EquippableItem))]
public class C_Gun : C_UsableItem
{
	private void Awake()
	{
		if (C_Gun._projectileParent == null)
		{
			object syncRoot = C_Gun._syncRoot;
			lock (syncRoot)
			{
				if (C_Gun._projectileParent == null)
				{
					C_Gun._projectileParent = new GameObject("__Projectiles").transform;
				}
			}
		}
	}

	private void Start()
	{
		this._ammoClip = base.GetComponent<C_AmmoOnGun>();
		this._equippableItem = base.GetComponent<C_EquippableItem>();
		this.m_LastSoundTime = 0f;
	}

	public void PlaySound(AudioClip pClip, Vector3 pos)
	{
		if ((double)Time.time > (double)this.m_LastSoundTime + 0.2)
		{
			C_AudioLayerManager.PlayClipAtPoint("Robot Guns", pClip, pos);
			this.m_LastSoundTime = Time.time;
		}
	}

	public override bool CanUse(Ray usePosition)
	{
		// SUPERGUN check
		if (!this.m_ConsumesAmmo || God.Settings.Supergun)
		{
			return true;
		}
		if (this._isBetweenShots)
		{
			return false;
		}
		if (this._ammoClip.enabled && this._ammoClip._currentAmmo == 0)
		{
			if (this._shooter == null)
			{
				GameObject equippableGameObject = this._equippableItem.m_EquippableGameObject;
				this._shooter = ((!(equippableGameObject == null)) ? equippableGameObject.GetComponent<vp_FPSShooter>() : null);
			}
			if (this.MagEmptySound != null)
			{
				this.PlaySound(this.MagEmptySound, this._shooter.gameObject.transform.position);
			}
		}
		return this._ammoClip == null || !this._ammoClip.enabled || this._ammoClip.CanConsume(this.AmmoConsumedPerShot);
	}

	public override bool Use(Ray usePosition, GameObject user)
	{
		bool result;
		using (new ProfilerSample("Gun.Use()", user))
		{
			this._isBetweenShots = true;
			bool flag = true;
			if (this.m_pC_AxeSwing != null)
			{
				this.m_pC_AxeSwing.StartSwing(usePosition, user, this.ShotDelayTime);
			}
			else
			{
				using (new ProfilerSample("Firing shells", user))
				{
					for (int i = 0; i < this.ShellsFiredPerShot; i++)
					{
						base.StartCoroutine(this.FireShell(usePosition, user, this.ShotDelayTime));
					}
				}
			}
			using (new ProfilerSample("Effects", user))
			{
				if (this.m_HasMuzzleFlash)
				{
					this.MakeMuzzleFlash();
				}
				this.MakeGunshotSound(usePosition.origin, user);
			}
			using (new ProfilerSample("VP Shooter check", user))
			{
				if (this._shooter == null)
				{
					GameObject equippableGameObject = this._equippableItem.m_EquippableGameObject;
					this._shooter = ((!(equippableGameObject == null)) ? equippableGameObject.GetComponent<vp_FPSShooter>() : null);
				}
				if (this._shooter != null)
				{
					this._shooter.Fire();
				}
			}
			if (!this.m_ConsumesAmmo)
			{
				int num = UnityEngine.Random.Range(0, 3);
				if (num == 0)
				{
					user.GetComponent<C_FPSPlayer>().CurrentWeapon.animation.Play("Axe4");
				}
				else if (num == 1)
				{
					user.GetComponent<C_FPSPlayer>().CurrentWeapon.animation.Play("Axe4");
				}
				else if (num == 2)
				{
					user.GetComponent<C_FPSPlayer>().CurrentWeapon.animation.Play("Axe4");
				}
			}
			if (this._ammoClip != null && this._ammoClip.enabled && flag)
			{
				this._ammoClip.Consume(this.AmmoConsumedPerShot);
			}
			base.StartCoroutine(this.FinishShooting());
			result = true;
		}
		return result;
	}

	public vp_MuzzleFlash MuzzleFlash { get; set; }

	private void MakeGunshotSound(Vector3 position, GameObject user)
	{
		if (this.ShotSound != null)
		{
			if (base.transform.root.GetComponent<C_NPCManager>() == null)
			{
				if (user.transform.Find("FPSCamera") == null)
				{
					return;
				}
				AudioSource component = user.transform.Find("FPSCamera").GetComponent<AudioSource>();
				if (component == null)
				{
					return;
				}
				component.clip = this.ShotSound;
				component.Play();
				if (this.m_pC_AxeSwing == null)
				{
					C_GameStatsManager.m_Instance.m_GameStatsData.m_ShotsFired++;
				}
				if (this.m_ConsumesAmmo)
				{
					C_PlayerStealthMonitor.m_Instance.AddSpike();
				}
			}
			else
			{
				base.audio.clip = this.ShotSound;
				base.audio.Play();
			}
			int type = 0;
			if (user != null)
			{
				C_Character component2 = user.GetComponent<C_Character>();
				if (component2 != null)
				{
					type = component2.m_Faction;
				}
			}
			C_AudioBroadcaster.MakeSound(user, position, this.GunshotVolume, type);
		}
	}

	private void MakeMuzzleFlash()
	{
		if (this.MuzzleFlash != null)
		{
			this.MuzzleFlash.FadeSpeed = this.MuzzleFlashFadeSpeed;
			this.MuzzleFlash.Shoot();
		}
	}

	private IEnumerator FireShell(Ray shotDirection, GameObject user, float delay)
	{
		yield return new WaitForSeconds(delay);
		Vector3 shotOrigin = shotDirection.origin + shotDirection.direction.normalized * 0.1f;
		GameObject bullet = UnityEngine.Object.Instantiate(this.ProjectilePrefab, shotOrigin, Quaternion.LookRotation(shotDirection.direction)) as GameObject;
		bullet.transform.parent = C_Gun._projectileParent;
		bullet.GetComponent<C_HitscanBullet>().FiringUser = user;
		bullet.transform.Rotate(0f, 0f, (float)UnityEngine.Random.Range(0, 360));
		bullet.transform.Rotate(0f, UnityEngine.Random.Range(-this.ProjectileSpread, this.ProjectileSpread), 0f);
		yield break;
	}

	private IEnumerator FinishShooting()
	{
		yield return new WaitForSeconds(this.SecondsBetweenShots);
		this._isBetweenShots = false;
		yield break;
	}

	public bool m_ConsumesAmmo = true;

	public bool m_HasMuzzleFlash = true;

	public float MaximumRangeForAi = 40f;

	public float OptimumRangeForAi = 20f;

	public int ShellsFiredPerShot = 1;

	public int AmmoConsumedPerShot = 1;

	public float SecondsBetweenShots = 1f;

	public float ShotDelayTime;

	public float ProjectileSpread;

	public float GunshotVolume = 10f;

	public AudioClip ShotSound;

	public AudioClip MagEmptySound;

	public float MuzzleFlashFadeSpeed = 0.075f;

	public GameObject ProjectilePrefab;

	public float m_LastSoundTime;

	public C_AxeSwing m_pC_AxeSwing;

	private C_AmmoOnGun _ammoClip;

	private vp_FPSShooter _shooter;

	private C_EquippableItem _equippableItem;

	private bool _isBetweenShots;

	internal static Transform _projectileParent;

	private static object _syncRoot = new object();
}
