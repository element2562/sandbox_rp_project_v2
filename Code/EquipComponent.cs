using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Interfaces;
using Sandbox.Utilities;

namespace Sandbox
{
	public sealed class EquipComponent : Component, IInteractable
	{
		[Property] public InventoryItem InventoryItem { get; set; }
		public string InteractionMessage => $"Press [E] to equip {InventoryItem.Name}";

		public void OnCarry( PlayerMovement player )
		{
			throw new NotImplementedException();
		}

		public void OnInteract( PlayerMovement player )
		{
			var inventory = player.Components.Get<InventoryManager>();
			if ( inventory != null )
			{
				inventory.AddItem( InventoryItem ); // Add item to inventory

				ServerObjectRemover.DeleteObject( InventoryItem.GameObject );
			}
		}
	}

}
