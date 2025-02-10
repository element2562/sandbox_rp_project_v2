using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Utilities
{
	public static class GameObjectHelper
	{
		public static GameObject FindObjectInChildrenByName( GameObject root, string name )
		{
			return FindObjectInChildrenByNameRecursive( root, name );
		}

		private static GameObject FindObjectInChildrenByNameRecursive( GameObject current, string name )
		{
			if ( current.Name == name )
				return current;

			foreach ( var child in current.Children )
			{
				var result = FindObjectInChildrenByNameRecursive( child, name );
				if ( result != null )
					return result;
			}

			return null;
		}

		public static void DisableAllChildren( GameObject root )
		{
			DisableAllChildrenRecursive( root );
		}

		private static void DisableAllChildrenRecursive( GameObject current )
		{
			foreach ( var child in current.Children )
			{
				child.Enabled = false;
				DisableAllChildrenRecursive( child );
			}
		}

		[Rpc.Broadcast]
		public static void SpawnPrefabInFrontOfPlayer( GameObject prefab, float distance = 50f )
		{
			var player = PlayerMovement.Local;
			if ( player == null || prefab == null )
				return;

			// Clone the prefab
			var clonedPrefab = prefab.Clone( new CloneConfig
			{
				StartEnabled = true,
				Parent = null, // No parent to ensure it's independent
			} );

			if ( clonedPrefab != null )
			{
				// Update prefab to ensure all components are initialized
				clonedPrefab.UpdateFromPrefab();

				// Calculate the spawn position in front of the player
				var spawnPosition = player.Head.WorldPosition + player.Head.WorldRotation.Forward * distance;

				// Apply the calculated position and rotation
				clonedPrefab.WorldPosition = spawnPosition;
				clonedPrefab.WorldRotation = player.WorldRotation;
				clonedPrefab.WorldScale = prefab.WorldScale;

				// Network spawn the prefab for all clients
				clonedPrefab.NetworkSpawn();
			}
		}


		[Rpc.Broadcast]
		public static void NetworkDestroy( GameObject gameObject )
		{
			gameObject.Destroy();
		}
	}
}
