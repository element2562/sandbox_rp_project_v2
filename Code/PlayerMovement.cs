using Sandbox;
using Sandbox.Citizen;
using Sandbox.Utilities;

public sealed class PlayerMovement : Component
{
	[Property( Name = "Ground Control" )] public float GroundControl { get; set; } = 4.0f;
	[Property( Name = "Air Control" )] public float AirControl { get; set; } = 0.1f;
	[Property( Name = "Max Force" )] public float MaxForce { get; set; } = 50f;
	[Property( Name = "Speed" )] public float Speed { get; set; } = 160f;
	[Property( Name = "Run Speed" )] public float RunSpeed { get; set; } = 290f;
	[Property( Name = "Crouch Speed" )] public float CrouchSpeed { get; set; } = 90f;
	[Property( Name = "Jump Force" )] public float JumpForce { get; set; } = 400f;

	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public SkinnedModelRenderer Renderer { get; set; }

	[Property] private GameObject _footstepPrefab;
	private SoundPointComponent _currentFootstepSound;

	public static PlayerMovement Local { get; private set; }
	public bool ThirdPerson { get; set; } = false;
	public Vector3 CameraOffset = new Vector3( -200, 0, 50 );
	public bool UseInputControls = true;
	public bool UseCameraControls = true;

	public Vector3 WishVelocity = Vector3.Zero;
	[Sync]
	public bool IsMoving
	{
		get
		{
			if ( _characterController == null ) return false;

			const float moveThreshold = 1f;
			float speed = _characterController.Velocity.WithZ( 0 ).Length;
			return speed > moveThreshold;
		}
	}


	[Sync] public bool IsCrouching { get; set; } = false;
	[Sync] public bool IsSprinting { get; set; } = false;
	[Sync]
	public bool IsJumping
	{
		get
		{
			if ( _characterController == null ) return false;
			return !_characterController.IsOnGround;
		}
	}

	[Sync] public Angles TargetAngle { get; set; } = Angles.Zero;

	private CharacterController _characterController;
	private CitizenAnimationHelper _animationHelper;

	private float _footstepTimer = 0f;
	private float _footstepInterval = 0.3f;

	protected override void OnStart()
	{
		if ( Network.IsOwner )
			Local = this;

		_characterController = Components.Get<CharacterController>();
		_animationHelper = Components.Get<CitizenAnimationHelper>();
		_currentFootstepSound = _footstepPrefab.Components.Get<SoundPointComponent>();

		if ( _characterController == null )
			Log.Error( "CharacterController not found. Make sure it's added as a component." );
	}

	protected override void OnUpdate()
	{
		if ( !Network.IsProxy )
		{
			if ( !UseInputControls ) return;

			UpdateMovement();
			if ( Input.Pressed( "Jump" ) ) Jump();

			TargetAngle = new Angles( 0, Head.WorldRotation.Yaw(), 0 ).ToRotation();
			HandleFootsteps();
		}

		Body.LocalPosition = Vector3.Zero;
		RotateBody();
		UpdateAnimations();
	}

	protected override void OnFixedUpdate()
	{
		if ( !Network.IsProxy )
		{
			BuildWishVelocity();
			ApplyMovement();
		}
	}

	private void UpdateMovement()
	{
		Vector3 direction = GetMovementDirection();
		bool movingBackward = direction.Dot( Head.WorldRotation.Forward ) < -0.1f;

		IsSprinting = Input.Down( "Run" ) && !movingBackward && direction.LengthSquared > 0.1f;
	}

	private Vector3 GetMovementDirection()
	{
		Vector3 direction = Vector3.Zero;
		var rotation = Head.WorldRotation;

		if ( Input.Down( "Forward" ) ) direction += rotation.Forward;
		if ( Input.Down( "Backward" ) ) direction -= rotation.Forward;
		if ( Input.Down( "Left" ) ) direction += rotation.Left;
		if ( Input.Down( "Right" ) ) direction += rotation.Right;

		return direction.WithZ( 0 ).Normal;
	}

	private void BuildWishVelocity()
	{
		if ( !UseInputControls ) return;

		var inputDirection = GetMovementDirection();
		float speedMultiplier = IsCrouching ? CrouchSpeed : (IsSprinting ? RunSpeed : Speed);

		WishVelocity = Vector3.Lerp( WishVelocity, inputDirection * speedMultiplier, Time.Delta * 20f );
	}


	private void ApplyMovement()
	{
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( _characterController.IsOnGround )
		{
			// Reset vertical velocity when on the ground
			_characterController.Velocity = _characterController.Velocity.WithZ( 0 );

			// Apply grounded movement
			_characterController.Accelerate( WishVelocity );
			_characterController.ApplyFriction( GroundControl );
		}
		else
		{
			// Apply gravity and limit horizontal acceleration when in the air
			_characterController.Velocity += gravity * Time.Delta;
			_characterController.Accelerate( WishVelocity.ClampLength( MaxForce ) );
			_characterController.ApplyFriction( AirControl );
		}

		_characterController.Move();
	}

	private void HandleFootsteps()
	{
		if ( !_characterController.IsOnGround || WishVelocity.Length <= 0.1f ) return;

		_footstepInterval = IsSprinting ? 0.25f : (IsCrouching ? 0.7f : 0.5f);
		_footstepTimer += Time.Delta;

		if ( _footstepTimer >= _footstepInterval )
		{
			_footstepTimer = 0f;
			PlayFootstepSound();
		}
	}
	private void PlayFootstepSound()
	{
		var sound = _currentFootstepSound.SoundEvent;
		sound.Volume = IsSprinting ? 0.4f : 0.15f;
		ServerSoundHelper.BroadcastSoundOnGameObject( sound, Body );
	}

	private void RotateBody()
	{
		if ( Body == null ) return;

		float rotationDiff = Body.WorldRotation.Distance( TargetAngle );

		Body.WorldRotation = rotationDiff > 50f || _characterController.Velocity.Length > 10f
			? Rotation.Lerp( Body.WorldRotation, TargetAngle, Time.Delta * 10f )
			: TargetAngle;
	}

	private void Jump()
	{
		if ( !_characterController.IsOnGround ) return;

		// Apply the original jump force multiplier logic
		float jumpForceMultiplier = Input.Down( "Backward" ) ? 0.6f : 1.0f;

		// Apply upward force for the jump
		_characterController.Punch( Vector3.Up * JumpForce * jumpForceMultiplier );

		// Trigger jump animations to sync across clients
		BroadcastJumpAnimation();
	}

	private void UpdateAnimations()
	{
		if ( _animationHelper == null ) return;

		foreach ( var renderer in Body.Components.GetAll<ModelRenderer>() )
		{
			renderer.RenderType = Network.IsProxy || (!Network.IsProxy && ThirdPerson)
				? ModelRenderer.ShadowRenderType.On
				: ModelRenderer.ShadowRenderType.ShadowsOnly;
		}

		_animationHelper.WithWishVelocity( WishVelocity );
		_animationHelper.WithVelocity( _characterController.Velocity );
		_animationHelper.AimAngle = TargetAngle;
		_animationHelper.IsGrounded = _characterController.IsOnGround;
		_animationHelper.WithLook( TargetAngle.Forward, 1f, 0.75f, 0.5f );
		_animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Run;
		_animationHelper.DuckLevel = IsCrouching ? 1f : 0f;
	}

	[Rpc.Broadcast]
	private void BroadcastJumpAnimation()
	{
		_animationHelper?.TriggerJump();
	}
}
