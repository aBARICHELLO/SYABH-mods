using System;
using UnityEngine;

public class C_AmmoInClip : MonoBehaviour
{
	private void Start()
	{
		// SUPERGUN check
		if (God.Settings.Supergun) {
			this.m_CurrentAmmo = m_MaxAmmo;
			return;
		}
		if (this.m_MaxAmmo == 0)
		{
			this.m_MaxAmmo = 1;
		}
		if (this.m_CurrentAmmo == 0)
		{
			this.m_CurrentAmmo = this.m_MaxAmmo;
		}
	}

	public int m_CurrentAmmo;

	public int m_MaxAmmo;
}
