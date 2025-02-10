using Sandbox;
using Sandbox.Enums;
using Sandbox.Interfaces;
using Sandbox.Utilities;

public sealed class PlayerInteractionController : Component
{
	[Property] private PlayerMovement _player { get; set; }
	[Property] private InventoryManager _inventoryManager { get; set; }

	private const float InteractionDistance = 150f;
	private const float InteractionRadius = 10f;

	private IInteractable _currentInteractable;

	[Sync] private IInteractable _carriedObject { get; set; }
	[Sync] private Rigidbody _carriedObjectRigidBody { get; set; }
	[Sync] private PickupComponent _carriedItemPickupComponent { get; set; }
	[Sync] private bool _isCarrying { get; set; } = false;
	[Sync] public bool ShowInteractionMessage { get; private set; } = false;
	[Sync] public string InteractionText { get; private set; } = "Press [E] to pick up";

	protected override void OnUpdate()
	{
		if ( Network.IsProxy ) return;

		DetectInteractable();
		HandleInteraction();
	}

	/// <summary>
	/// Detects an interactable object within range and updates the interaction message.
	/// </summary>
	private void DetectInteractable()
	{
		if(_isCarrying && _carriedObject != null )
		{
			_currentInteractable = _carriedObject;
			return;
		}

		var playerPosition = _player.Head.WorldPosition;
		var playerDirection = _player.Head.WorldRotation.Forward;

		var trace = Scene.Trace.Ray( playerPosition, playerPosition + playerDirection * InteractionDistance )
			.Radius( InteractionRadius )
			.IgnoreGameObject( _player.GameObject )
			.Run();

		if ( trace.Hit )
		{
			_currentInteractable = trace.GameObject.GetComponent<IInteractable>();

			if ( _currentInteractable is PickupComponent pickupItem )
			{
				ShowInteractionMessage = true;
				InteractionText = pickupItem.InteractionMessage;
				return;
			}
		}

		ShowInteractionMessage = false;
		_currentInteractable = null;
	}

	/// <summary>
	/// Handles interaction logic, including picking up and carrying objects.
	/// </summary>
	private void HandleInteraction()
	{
		if ( _currentInteractable == null ) return;

		// Handle interaction input
		if ( ShowInteractionMessage && Input.Pressed( "use" ) )
		{
			_currentInteractable?.OnInteract( _player );
			ShowInteractionMessage = false;

			if ( _isCarrying )
			{
				_isCarrying = false;
				StopCarrying();
				ResetCarriedObject();
			}
		}

		if ( _inventoryManager?.EquippedItem != null ) return;

		if ( Input.Pressed( "Attack2" ) )
		{
			ToggleCarry();
		}

		// Update carried object position if carrying
		if ( _isCarrying && _carriedObject != null )
		{
			_carriedObject?.OnCarry( _player );
		}
		else
		{
			ResetCarriedObject();
		}
	}

	/// <summary>
	/// Toggles the carry state for the currently detected interactable object.
	/// </summary>
	private void ToggleCarry()
	{
		_isCarrying = !_isCarrying;

		if ( _isCarrying )
		{
			StartCarrying();
		}
		else
		{
			StopCarrying();
			ResetCarriedObject();
		}
	}

	/// <summary>
	/// Starts carrying the currently detected interactable object.
	/// </summary>
	private void StartCarrying()
	{
		_carriedObject = _currentInteractable;

		if ( _carriedObject is PickupComponent pickup )
		{
			_carriedItemPickupComponent = pickup;
			_carriedItemPickupComponent.Network.AssignOwnership( Connection.Local );
			_carriedItemPickupComponent.BeingCarried = true;

			var prop = _carriedItemPickupComponent.GameObject.Components.Get<Prop>();
			_carriedObjectRigidBody = prop.GetComponent<Rigidbody>();
			_carriedObjectRigidBody.Gravity = false;

			ServerAnimationHelper.BroadcastAnimationSet( _player.Renderer, "holdtype", (int)HoldTypeEnum.HoldItem );
		}
	}

	/// <summary>
	/// Stops carrying the current object and resets its state.
	/// </summary>
	private void StopCarrying()
	{
		if ( _carriedItemPickupComponent != null )
		{
			_carriedItemPickupComponent.BeingCarried = false;
		}

		if ( _carriedObjectRigidBody != null )
		{
			_carriedObjectRigidBody.Gravity = true;
		}

		ServerAnimationHelper.BroadcastAnimationSet( _player.Renderer, "holdtype", (int)HoldTypeEnum.None );
	}

	/// <summary>
	/// Resets the carried object and its related state.
	/// </summary>
	private void ResetCarriedObject()
	{
		_carriedObject = null;
		_carriedItemPickupComponent = null;
		_carriedObjectRigidBody = null;
	}
}
