using Sandbox;

public sealed class Bullet : Component
{
	[Property] public int Damage { get; set; } = 10;
	[Property] public float Speed { get; set; } = 1000.0f;
	[Property] public float Lifetime { get; set; } = 1.5f; // Bullet lifetime in seconds

	public GameObject Owner { get; set; }

	private Vector3 _velocity;
	private float _spawnTime;

	protected override void OnAwake()
	{
		_velocity = GameObject.LocalTransform.Forward * Speed; // Set initial velocity
		_spawnTime = RealTime.Now;
	}

	protected override void OnUpdate()
	{
		if( Network.IsProxy ) return;
		// Destroy the bullet if it exceeds its lifetime
		if (RealTime.Now - _spawnTime > Lifetime)
		{
			GameObject.Destroy();
			return;
		}

		// Update bullet position
		GameObject.WorldPosition += _velocity * Time.Delta;

		// Check for collisions
		PerformCollisionCheck();
	}

	private void PerformCollisionCheck()
	{
		var trace = Scene.Trace.Ray( GameObject.WorldPosition, GameObject.WorldPosition + _velocity * Time.Delta )
			.Radius( 1.0f ) // Adjust for the size of the bullet
			.IgnoreGameObjectHierarchy( Owner )
			.WithAnyTags("world", "player") // Define tags to collide with
			.UseHitboxes()
			.Run();

		if (trace.Hit)
		{
			HandleCollision(trace);
		}
	}

	private async void HandleCollision(SceneTraceResult trace)
	{
		var hitbox = trace.Hitbox;
		if (hitbox != null)
		{
			var damageable = trace.GameObject.GetComponent<PlayerHealthController>();
			if (damageable != null && !damageable.IsDead)
			{
				var ownerAttackController = Owner.GetComponent<PlayerAttackController>();
				ownerAttackController.HandleHit();

				var damage = CalculateDamage(hitbox);
				damageable.TakeDamage( damage, Owner.Network.Owner.DisplayName ); // Apply damage to the object
				
				var healthAfterDamage = damageable.Health - damage; // the method is broadcasted and doesn't update the health in time, so we'll do it ourselves

				if (healthAfterDamage <= 0 )
				{
					ownerAttackController.HandleKill();
				}
			}

			// Create impact effects, if any
			CreateImpactEffects(trace);

			// Destroy the bullet
			GameObject.Destroy();
		}
	}

	private void CreateImpactEffects(SceneTraceResult trace)
	{
		// Add logic to spawn impact effects (particles, decals, etc.)
	}

	private int CalculateDamage( Hitbox hitbox )
	{
		var damage = 0;

		var hitboxTag = hitbox.Tags.FirstOrDefault();
		switch ( hitboxTag )
		{
			case "head":
				damage = Damage * 2;
				break;
			case "torso":
				damage = Damage;
				break;
			case "leg":
				damage = Damage / 2;
				break;
			case "arm":
				damage = Damage / 2;
				break;
			default:
				damage = Damage;
				break;
		}

		return damage;
	}
}
