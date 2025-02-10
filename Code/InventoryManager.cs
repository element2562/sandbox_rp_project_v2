using Sandbox;
using Sandbox.Utilities;
using System.Collections.Generic;

public sealed class InventoryManager : Component
{
	private List<InventoryItem> _items = new();
	private InventoryItem _equippedItem; // Track the currently equipped item
	public IReadOnlyList<InventoryItem> Items => _items;
	public InventoryItem EquippedItem => _equippedItem;

	public void AddItem( InventoryItem item )
	{
		_items.Add( item );
	}

	public InventoryItem GetItem( InventoryItem item )
	{
		return _items.Find( i => i == item );
	}

	public void RemoveItem( InventoryItem item )
	{
		if ( _items.Contains( item ) )
		{
			_items.Remove( item );
			if ( _equippedItem == item )
			{
				UnequipItemImmediately();
				_equippedItem = null; // Unequip if the removed item was equipped
			}
		}
	}

	public void EquipItem( InventoryItem item )
	{
		if ( item == null || !_items.Contains( item ) || Network.IsProxy ) return;

		// Unequip the current item if necessary
		if ( _equippedItem != null )
		{
			UnequipItemImmediately();
		}

		_equippedItem = item;

		// If the item is a weapon, spawn its models
		if ( item is WeaponItem weapon )
		{
			weapon.SpawnViewModel();
			weapon.SpawnWorldModel( PlayerMovement.Local.Body );
		}
	}

	public async void UnequipItem()
	{
		if ( _equippedItem != null )
		{
			// If the equipped item is a weapon, destroy its models
			if ( _equippedItem is WeaponItem weapon )
			{
				weapon.ViewModelRenderer.Set( "b_holster", true );

				await GameTask.Delay( 1500 );

				weapon.DestroyViewModel();
				weapon.DestroyWorldModel();
			}

			Log.Info( $"Unequipped {_equippedItem.Name}" );
			_equippedItem = null;
		}
	}

	public void UnequipItemImmediately()
	{
		if ( _equippedItem != null )
		{
			// If the equipped item is a weapon, destroy its models
			if ( _equippedItem is WeaponItem weapon )
			{
				weapon.DestroyViewModel();
				weapon.DestroyWorldModel();
			}
			_equippedItem = null;
		}
	}

	public void DropItem(InventoryItem item)
	{
		var prefab = item.WorldModel;
		GameObjectHelper.SpawnPrefabInFrontOfPlayer( prefab );
		RemoveItem( item );
	}

	public bool HasItem( string itemName )
	{
		return _items.Exists( item => item.Name == itemName );
	}
}
