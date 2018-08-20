using System;
using System.Collections.Generic;
using Assets.Phunk.Core;
using UnityEngine;

public class C_PlayerEquipmentController : MonoBehaviour
{
	public event EventHandler EquippedItemChanged;

	public static C_PlayerEquipmentController m_Instance { get; private set; }

	private void Awake()
	{
		C_PlayerEquipmentController.m_Instance = this;
		if (this.PlayerFacingTransform == null)
		{
			Log.ErrorFormat("PlayerFacingTransform is null in PlayerEquipmentController on {0}.", new object[]
			{
				base.gameObject.name
			});
		}
	}

	private void Start()
	{
		this.m_ActiveEquipSlot = -1;
		this.m_LastSoundTime = 0f;
		this.m_ReloadStateSet = false;
		this.m_pC_TrappedMonitor = base.gameObject.GetComponent<C_TrappedMonitor>();
		this.GuaranteeDefaultEquipment();
		this.m_IsActive = true;
	}

	public void AssignNewSlotItem(string itemName, int whichSlot)
	{
		if (this.m_ActiveEquipSlot == whichSlot)
		{
			this.TryEquipNoWeapon();
		}
		for (int i = 0; i < this.m_SlotItems.Length; i++)
		{
			if (this.m_SlotItems[i] == itemName)
			{
				this.m_SlotItems[i] = "NoWeapon";
			}
		}
		this.m_SlotItems[whichSlot] = itemName;
		if (this.m_ActiveEquipSlot == -1)
		{
			this.TryEquipNoWeapon();
		}
		else if (this.m_SlotItems[this.m_ActiveEquipSlot] == "NoWeapon")
		{
			this.TryEquipNoWeapon();
		}
	}

	public void GuaranteeDefaultEquipment()
	{
		if (this.PlayerInventory.Items.Count == 0)
		{
			int whichClass = C_SaveManager.m_Instance.m_PlayerData.m_WhichClass;
			foreach (C_PickupItem c_PickupItem in C_PlayerClassLoadOuts.m_Instance.m_ClassLoadOuts[whichClass].m_Items)
			{
				string itemName = c_PickupItem.ItemName;
				bool flag = false;
				foreach (C_PickupItem c_PickupItem2 in this.PlayerInventory.Items)
				{
					if (c_PickupItem2.ItemName == c_PickupItem.ItemName)
					{
						flag = true;
					}
				}
				if (!flag)
				{
					C_PickupItem item = UnityEngine.Object.Instantiate(c_PickupItem) as C_PickupItem;
					this.PlayerInventory.AddItem(item);
				}
			}
		}
	}

	private void CheckEquipmentStillValid()
	{
		if (this._equippedItem == null)
		{
			Log.Message("NO WEAPON ANYMORE ITs DESTROYED!");
			this.TryEquipNoWeapon();
			return;
		}
		bool flag = false;
		string itemName = this._equippedItem.GetComponent<C_PickupItem>().ItemName;
		foreach (C_PickupItem c_PickupItem in this.PlayerInventory.Items)
		{
			if (c_PickupItem.ItemName == itemName)
			{
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			Log.Message("NO WEAPON ANYMORE ITS NOT IN INVENTORY!");
			this.TryEquipNoWeapon();
			return;
		}
	}

	private void EquipNextAvailableSlot()
	{
		int num = this.m_ActiveEquipSlot;
		bool flag = false;
		int num2 = 0;
		while (!flag)
		{
			num++;
			if (num >= this.m_SlotItems.Length)
			{
				num = 0;
			}
			flag = (num == this.m_ActiveEquipSlot || this.TryEquipFromPlayerSlot(num));
			num2++;
			if (this.m_SlotItems[num] == "NoWeapon")
			{
				flag = false;
			}
			if (num2 > this.m_SlotItems.Length)
			{
				flag = true;
			}
		}
	}

	private void EquipPreviousAvailableSlot()
	{
		int num = this.m_ActiveEquipSlot;
		bool flag = false;
		int num2 = 0;
		while (!flag)
		{
			num--;
			if (num < 0)
			{
				num = this.m_SlotItems.Length - 1;
			}
			flag = (num == this.m_ActiveEquipSlot || this.TryEquipFromPlayerSlot(num));
			num2++;
			if (this.m_SlotItems[num] == "NoWeapon")
			{
				flag = false;
			}
			if (num2 > this.m_SlotItems.Length)
			{
				flag = true;
			}
		}
	}

	public bool TryEquipNoWeapon()
	{
		return this.TryEquipItem("NoWeapon", -1);
	}

	private bool TryEquipFromPlayerSlot(int whichslot)
	{
		return this.TryEquipItem(this.m_SlotItems[whichslot], whichslot);
	}

	private bool TryEquipFromHardCodedSlot(int whichslot)
	{
		if (whichslot == 1)
		{
			return this.TryEquipItem("Rifle", 1);
		}
		if (whichslot == 2)
		{
			return this.TryEquipItem("Shotgun", 2);
		}
		if (whichslot == 3)
		{
			if (this.TryEquipItem("ThrowDynamite", 3))
			{
				this.TryReloadEquippedItem();
				return true;
			}
		}
		else if (whichslot == 4)
		{
			if (this.TryEquipItem("ThrowStones", 4))
			{
				this.TryReloadEquippedItem();
				return true;
			}
		}
		else if (whichslot == 5)
		{
			if (this.TryEquipItem("ThrowBottles", 5))
			{
				this.TryReloadEquippedItem();
				return true;
			}
		}
		else
		{
			if (whichslot == 6)
			{
				return this.TryEquipItem("Binoculars", 6);
			}
			if (whichslot == 7)
			{
				return this.TryEquipItem("Axe", 7);
			}
			if (whichslot == 8)
			{
				return this.TryEquipItem("Revolver", 8);
			}
			if (whichslot == 9 && this.TryEquipItem("ThrowTraps", 9))
			{
				this.TryReloadEquippedItem();
				return true;
			}
		}
		return false;
	}

	private bool TryEquipItem(string itemName, int NewSlotID)
	{
		foreach (C_PickupItem c_PickupItem in this.PlayerInventory.Items)
		{
			if (c_PickupItem.ItemName == itemName)
			{
				C_EquippableItem component = c_PickupItem.GetComponent<C_EquippableItem>();
				if (component != null && component.m_CurrentlyEquippable)
				{
					if (this._equippedItem != null)
					{
						this._equippedItem.m_IsEquipped = false;
					}
					this.CancelCurrentWeaponStates();
					this._equippedItem = component;
					this._equippedItem.Equip(this.PlayerViewCamera);
					this.m_ActiveEquipSlot = NewSlotID;
					this.OnEquippedItemChanged(EventArgs.Empty);
					this.ReApplyCurrentWeaponStates();
					return true;
				}
			}
		}
		return false;
	}

	private void Update()
	{
		if (C_UnderWaterMonitor.m_Instance.m_IsInWater)
		{
			if (this.m_ActiveEquipSlot >= 0)
			{
				this.TryEquipNoWeapon();
			}
			return;
		}
		this.CheckEquipmentStillValid();
		if (this.m_IsActive)
		{
			if (cInput.GetKeyDown("Equip Slot 1"))
			{
				this.TryEquipFromPlayerSlot(0);
			}
			if (cInput.GetKeyDown("Equip Slot 2"))
			{
				this.TryEquipFromPlayerSlot(1);
			}
			if (cInput.GetKeyDown("Equip Slot 3"))
			{
				this.TryEquipFromPlayerSlot(2);
			}
			if (cInput.GetKeyDown("Equip Slot 4"))
			{
				this.TryEquipFromPlayerSlot(3);
			}
			if (cInput.GetKeyDown("Equip Slot 5"))
			{
				this.TryEquipFromPlayerSlot(4);
			}
			if (cInput.GetKeyDown("Equip Slot 6"))
			{
				this.TryEquipFromPlayerSlot(5);
			}
			if (cInput.GetKeyDown("Equip Slot 7"))
			{
				this.TryEquipFromPlayerSlot(6);
			}
			if (Input.GetKeyDown(KeyCode.Minus))
			{
				this.TryEquipNoWeapon();
			}
			if (!cInput.GetButtonDown("Zoom Out/Equip Prev") && this.m_EquipScrollState == -1)
			{
				this.m_EquipScrollState = 0;
				Log.Message("stopped with teh Prev equip stuff");
			}
			if (!cInput.GetButtonDown("Zoom In/Equip Next") && this.m_EquipScrollState == 1)
			{
				this.m_EquipScrollState = 0;
				Log.Message("stopped with teh Next equip stuff");
			}
			if (this.m_CanScroll && cInput.GetButtonDown("Zoom Out/Equip Prev") && this.m_EquipScrollState == 0)
			{
				this.EquipPreviousAvailableSlot();
				this.m_EquipScrollState = -1;
			}
			if (this.m_CanScroll && cInput.GetButtonDown("Zoom In/Equip Next") && this.m_EquipScrollState == 0)
			{
				this.EquipNextAvailableSlot();
				this.m_EquipScrollState = 1;
			}
			if (this.m_pC_TrappedMonitor.m_IsTrapped && !this.m_pC_TrappedMonitor.m_CanShoot)
			{
				this.TryEquipNoWeapon();
				return;
			}
			if (cInput.GetKey("Fire"))
			{
				this.TryUseEquippedItem();
			}
			else if (cInput.GetKeyDown("Reload"))
			{
				this.TryReloadEquippedItem();
			}
		}
		this.CheckReloadStatus();
		if (this.EquipThis != null)
		{
			this._equippedItem = (UnityEngine.Object.Instantiate(this.EquipThis) as C_EquippableItem);
			this._equippedItem.Equip(this.PlayerViewCamera);
			this.EquipThis = null;
		}
	}

	private void CancelCurrentWeaponStates()
	{
		if (this._equippedItem != null)
		{
			C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
			if (component != null)
			{
				component.StopReload();
			}
		}
		this.m_pC_FPSPlayer.StopWeaponRunState();
		this.m_pC_FPSPlayer.StopWeaponZoomState();
		this.m_pC_FPSPlayer.StopWeaponReloadState();
		this.m_pC_FPSPlayer.StopWeaponCrouchState();
		this.m_pC_FPSPlayer.SetState("Zoom", false);
		base.audio.Stop();
	}

	private void ReApplyCurrentWeaponStates()
	{
		if (this._equippedItem != null)
		{
			C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
			if (component != null)
			{
				component.StopReload();
			}
		}
		if (this.m_pC_FPSPlayer.m_IsRunning)
		{
			this.m_pC_FPSPlayer.StartWeaponRunState();
		}
		if (this.m_pC_FPSPlayer.m_IsCrouching)
		{
			this.m_pC_FPSPlayer.StartWeaponCrouchState();
		}
	}

	private void TryUseEquippedItem()
	{
		if (this._equippedItem != null)
		{
			C_UsableItem component = this._equippedItem.GetComponent<C_UsableItem>();
			C_PickupItem c_PickupItem = null;
			if (component != null)
			{
				c_PickupItem = component.GetComponent<C_PickupItem>();
			}
			if (c_PickupItem == null)
			{
				return;
			}
			// Commented lines to make two handed weapons being able to shoot while running
			// if (this.m_pC_FPSPlayer.m_IsRunning && c_PickupItem.ItemName == "Rifle")
			// {
			// 	  return;
			// }
			// if (this.m_pC_FPSPlayer.m_IsRunning && c_PickupItem.ItemName == "Shotgun")
			// {
			// 	  return;
			// }
			// if (this.m_pC_FPSPlayer.m_IsRunning && c_PickupItem.ItemName == "Blunderbuss")
			// {
			// 	  return;
			// }
			if (component != null)
			{
				Ray usePosition = new Ray(this.PlayerFacingTransform.position, this.PlayerFacingTransform.forward);
				if (!component.TryToUse(usePosition, base.gameObject) && this.PlayerInventory != null)
				{
					this.PlayerInventory.RemoveItem(c_PickupItem, true);
				}
			}
		}
	}

	public void DoneReloadEquippedItem()
	{
		if (this._equippedItem != null)
		{
			C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
			if (component == null)
			{
				return;
			}
			if (component._currentAmmo == component.MaximumClipSize)
			{
				return;
			}
			if (component != null)
			{
				// SUPERGUN Check
				if (God.Settings.InfiniteAmmo || God.Settings.Supergun)
				{
					component.Reload(component.MaximumClipSize, true);
				}
				else
				{
					int num = this.UpdateAmmoInInventory();
					if (num > 0)
					{
						component._currentAmmo += num;
						if (component._currentAmmo > component.MaximumClipSize)
						{
							component._currentAmmo = component.MaximumClipSize;
						}
						return;
					}
					this.PlaySound(component.NoAmmoSound, base.transform.position);
				}
			}
		}
	}

	public void TryReloadEquippedItem()
	{
		if (this._equippedItem != null)
		{
            C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
			// SUPERGUN check
			if (God.Settings.Supergun) {
				component.Reload(component.MaximumClipSize, true);
			}
			if (component == null)
			{
				return;
			}
			if (component._currentAmmo == component.MaximumClipSize)
			{
				return;
			}
			if (component._isReloading)
			{
				return;
			}
			if (component != null)
			{
				if (God.Settings.InfiniteAmmo)
				{
					component.Reload(component.MaximumClipSize, true);
				}
				else
				{
					int num = this.CountAmmoInInventory();
					if (num > 0)
					{
						component.Reload(num, true);
						if (component.ReloadSound != null)
						{
							this.PlaySound(component.ReloadSound, base.transform.position);
						}
						this.m_pC_FPSPlayer.Reload();
						this.m_ReloadStateSet = true;
						return;
					}
					if (component.NoAmmoSound != null)
					{
						this.PlaySound(component.NoAmmoSound, base.transform.position);
					}
				}
			}
		}
	}

	public void CheckReloadStatus()
	{
		if (!this.m_ReloadStateSet)
		{
			return;
		}
		if (this._equippedItem != null)
		{
			C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
			if (component == null)
			{
				return;
			}
			if (!component._isReloading)
			{
				this.m_pC_FPSPlayer.SetState("Reload", false);
				this.m_ReloadStateSet = false;
			}
		}
	}

	public void PlaySound(AudioClip pClip, Vector3 pos)
	{
		if ((double)Time.time > (double)this.m_LastSoundTime + 0.2)
		{
			C_AudioLayerManager.PlayClipAtPoint("Player Equipment", pClip, pos);
			this.m_LastSoundTime = Time.time;
		}
	}

	private int CountAmmoInInventory()
	{
		C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
		int num = component.MaximumClipSize;
		num -= component._currentAmmo;
		int num2 = 0;
		List<C_PickupItem> list = new List<C_PickupItem>();
		foreach (C_PickupItem c_PickupItem in this.PlayerInventory.Items)
		{
			if (c_PickupItem.ItemName == component.AmmoItemName)
			{
				int num3 = c_PickupItem.GetComponent<C_AmmoInClip>().m_CurrentAmmo;
				if (num3 <= num)
				{
					num2 += num3;
					num -= num3;
				}
				else
				{
					num2 += num3;
					num3 -= num;
				}
				if (num2 <= 0)
				{
					break;
				}
			}
		}
		return num2;
	}

	private int UpdateAmmoInInventory()
	{
		C_AmmoOnGun component = this._equippedItem.GetComponent<C_AmmoOnGun>();
		int num = component.MaximumClipSize;
		num -= component._currentAmmo;
		int num2 = 0;
		Log.Message(string.Concat(new object[]
		{
			"looking to fill with ",
			num,
			" rounds of ",
			component.AmmoItemName
		}));
		List<C_PickupItem> list = new List<C_PickupItem>();
		foreach (C_PickupItem c_PickupItem in this.PlayerInventory.Items)
		{
			if (c_PickupItem.ItemName == component.AmmoItemName)
			{
				int currentAmmo = c_PickupItem.GetComponent<C_AmmoInClip>().m_CurrentAmmo;
				if (currentAmmo <= num)
				{
					num2 += currentAmmo;
					list.Add(c_PickupItem);
					num -= currentAmmo;
				}
				else
				{
					num2 += currentAmmo;
					c_PickupItem.GetComponent<C_AmmoInClip>().m_CurrentAmmo = currentAmmo - num;
					num -= currentAmmo;
					Log.Message("THis clip removing only " + currentAmmo + " rounds");
				}
				if (num <= 0)
				{
					break;
				}
			}
		}
		foreach (C_PickupItem item in list)
		{
			this.PlayerInventory.RemoveItem(item, true);
		}
		return num2;
	}

	public int CheckReplaceSingleAmmoItem(string whichItem, bool UseIt)
	{
		int result = 0;
		List<C_PickupItem> list = new List<C_PickupItem>();
		foreach (C_PickupItem c_PickupItem in this.PlayerInventory.Items)
		{
			if (c_PickupItem.ItemName == whichItem)
			{
				result = 1;
				if (UseIt)
				{
					this.PlayerInventory.RemoveItem(c_PickupItem, true);
				}
				return result;
			}
		}
		return result;
	}

	public virtual void OnEquippedItemChanged(EventArgs e)
	{
		if (this.EquippedItemChanged != null)
		{
			this.EquippedItemChanged(this, e);
		}
	}

	public bool m_IsActive = true;

	public Transform PlayerFacingTransform;

	public vp_FPSCamera PlayerViewCamera;

	public C_Inventory PlayerInventory;

	public C_EquippableItem EquipThis;

	public C_FPSPlayer m_pC_FPSPlayer;

	public string[] m_SlotItems;

	public int m_ActiveEquipSlot;

	public float m_LastSoundTime;

	public GameObject StonePrefab;

	public C_PickupItem[] InitialItems;

	public bool m_CanScroll = true;

	public int m_EquipScrollState;

	private static readonly Vector3 _screenCentre = new Vector3(0.5f, 0.5f, 0f);

	private bool m_ReloadStateSet;

	public C_TrappedMonitor m_pC_TrappedMonitor;

	public C_EquippableItem _equippedItem;

	private float m_nextEquipTime;
}
