using Sandbox;
using Sandbox.Interfaces;
using Sandbox.Utilities;

public sealed class PickupComponent : Component, IInteractable
{
	[Property] public InventoryItem InventoryItem { get; set; }
	public string InteractionMessage => $"Press [E] to pick up {InventoryItem.Name}";

	[Sync]
	private Vector3 _targetPosition { get; set; }
	[Sync]
	private Rotation _targetRotation { get; set; }
	[Sync]
	private PlayerMovement _player { get; set; }

	[Sync]
	public bool BeingCarried { get; set; } = false;

	// Offset applied for SlotPrev/SlotNext adjustments
	private float _offsetDistance = 0f; // Tracks the distance offset from the player's head

	public void OnCarry( PlayerMovement player )
	{
		if ( _player == null )
			_player = player;

		_targetRotation = _player.Head.WorldRotation.Inverse * GameObject.WorldRotation;
		_targetPosition = _player.Head.WorldPosition + _player.Head.LocalTransform.Forward * 75f;
	}


	public void OnInteract( PlayerMovement player )
	{
		var inventory = player.Components.Get<InventoryManager>();
		if ( inventory != null )
		{
			inventory.AddItem( InventoryItem );
			ServerObjectRemover.DeleteObject( InventoryItem.GameObject );
		}
	}

	private void UpdatePosition()
	{
		var forward = _player.Head.LocalTransform.Forward;
		var offset = forward * _offsetDistance;

		var desiredRotation = _player.Head.WorldRotation * _targetRotation;
		var desiredPosition = _targetPosition + offset;

		float lerpSpeed = 20f; 

		GameObject.WorldPosition = Vector3.Lerp(
			GameObject.WorldPosition,
			desiredPosition,
			Time.Delta * lerpSpeed
		);

		GameObject.WorldRotation = _player.Head.WorldRotation * _targetRotation;
	}

	protected override void OnUpdate()
	{
		if ( BeingCarried )
		{
			if ( Input.Pressed( "SlotPrev" ) )
			{
				_offsetDistance -= 5f;
			}

			if ( Input.Pressed( "SlotNext" ) )
			{
				_offsetDistance += 5f;
			}

			UpdatePosition();
		}
	}
}
