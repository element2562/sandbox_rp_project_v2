using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class PlayerHealthController : Component
	{
		[Property] public int MaxHealth { get; set; } = 100;
		[Property] public PlayerMovement PlayerMovement { get; set; }
		[Property] private PlayerStats _playerStats { get; set; }

		[Sync] public int Health { get; set; }
		[Sync] public bool IsDead { get; set; } = false;
		[Sync] public string KilledByName { get; set; }

		// Respawn logic
		public int SecondsUntilRespawn = 0;
		private readonly float _respawnCooldown = 5f;
		private float _timeOfDeath = 0f;
		private bool _canRespawn = false;

		private ModelPhysics _ragdoll;

		protected override void OnAwake()
		{
			Health = MaxHealth;
		}

		protected override void OnUpdate()
		{
			if(Network.IsProxy ) return;
			if (Health <= 0 && !IsDead)
			{
				Die();
			}

			if ( IsDead )
			{
				var timeSinceDeath = RealTime.Now - _timeOfDeath;
				_canRespawn = timeSinceDeath >= _respawnCooldown;
				SecondsUntilRespawn = (int)Math.Ceiling( Math.Max( 0, _respawnCooldown - timeSinceDeath ) );
			}

			if ( Input.Pressed( "Jump" ) )
			{
				if ( !IsDead ) return;
				if ( !_canRespawn ) return;
				// handle respawn logic
				Respawn();
			}
		}

		[Rpc.Broadcast]
		public void TakeDamage(int damage, string source)
		{
			Health -= damage;
			if (Health <= 0)
			{
				KilledByName = source;
				Health = 0;
			}
		}

		[Rpc.Broadcast]
		public void Die()
		{
			IsDead = true;
			Ragdoll();

			PlayerMovement.UseInputControls = false;
			PlayerMovement.UseCameraControls = false;

			_timeOfDeath = RealTime.Now;

			_playerStats.AddDeath();
		}

		[Rpc.Broadcast]
		public void Respawn()
		{
			UnRagdoll();
			Health = 100;
			IsDead = false;

			Random random = new Random();
			var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToList();
			var randomSpawnPoint = spawnPoints[random.Next( 0, spawnPoints.Count )];

			this.GameObject.LocalTransform = randomSpawnPoint.LocalTransform;

			PlayerMovement.ThirdPerson = false;
			PlayerMovement.UseInputControls = true;
		}

		private void Ragdoll()
		{
			_ragdoll = PlayerMovement.AddComponent<ModelPhysics>();
			_ragdoll.Renderer = PlayerMovement.Renderer;
			_ragdoll.Model = PlayerMovement.Renderer.Model;
		}

		private void UnRagdoll()
		{
			if ( _ragdoll != null )
			{
				_ragdoll.Destroy();
			}
		}
	}
}
