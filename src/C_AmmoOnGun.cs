using System;
using System.Collections;
using Assets.Phunk.Core;
using UnityEngine;

public class C_AmmoOnGun : MonoBehaviour
{
	public event EventHandler AmmoChanged;

	private void Start()
	{
		if (this._currentAmmo == -1)
		{
			this._currentAmmo = this.StartingAmmo;
		}
		this.OnAmmoChanged(EventArgs.Empty);
		if (this.ReloadSound != null)
		{
			this.ReloadTimeInSeconds = this.ReloadSound.length;
		}
	}

	public bool CanConsume(int amountToConsume)
	{
		if (God.Settings.Supergun) {
			return true;
		}
		return amountToConsume <= this._currentAmmo && !this._isReloading;
	}

	public void Consume(int amountToConsume)
	{
		// SUPERGUN check
		if (God.Settings.Supergun) {
			this._currentAmmo = 99;
			this.ReloadTimeInSeconds = 0f;
			return;
		}

		if (!this.CanConsume(amountToConsume))
		{
			Log.Error("Trying to consume when you can't.");
		}
		this._currentAmmo -= amountToConsume;
		this.OnAmmoChanged(EventArgs.Empty);
	}

	public void Reload(int amountToReload, bool isPlayer)
	{
		this.m_IsOnPlayer = isPlayer;
		using (new ProfilerSample("AmmoClip.Reload()", base.gameObject))
		{
			if (!this._isReloading)
			{
				this._isReloading = true;
				this.OnAmmoChanged(EventArgs.Empty);
				base.StartCoroutine("FinishReloading", amountToReload);
			}
		}
	}

	public void StopReload()
	{
		base.StopCoroutine("FinishReloading");
		this._isReloading = false;
	}

	private IEnumerator FinishReloading(int amountToReload)
	{
		yield return new WaitForSeconds(this.ReloadTimeInSeconds);
		this._isReloading = false;
		if (!this.m_IsOnPlayer)
		{
			this._currentAmmo += amountToReload;
		}
		else
		{
			C_PlayerEquipmentController.m_Instance.DoneReloadEquippedItem();
		}
		if (this._currentAmmo > this.MaximumClipSize)
		{
			this._currentAmmo = this.MaximumClipSize;
		}
		yield break;
	}

	public virtual void OnAmmoChanged(EventArgs e)
	{
		if (this.AmmoChanged != null)
		{
			this.AmmoChanged(this, e);
		}
	}

	public string AmmoItemName;

	public int MaximumClipSize = 10;

	public int StartingAmmo = 5;

	public float ReloadTimeInSeconds = 1f;

	public AudioClip ReloadSound;

	public AudioClip NoAmmoSound;

	public int _currentAmmo = -1;

	public bool _isReloading;

	private bool m_IsOnPlayer;
}
