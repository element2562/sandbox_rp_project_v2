using Sandbox;

public sealed class CameraMovement : Component
{
	[Property] public float Distance { get; set; } = 0f;
	public bool IsFirstPerson => Distance == 0f;

	private Vector3 _currentOffset = Vector3.Zero;
	private CameraComponent _camera;
	private PlayerMovement _player;
	private GameObject _head;

	// Recoil properties
	private Vector2 _recoilOffset = Vector2.Zero;
	private const float RecoilRecoverySpeed = 5f; // Adjust for faster/slower recovery
	private float _lastAttackTime;

	protected override void OnAwake()
	{
		_camera = Components.Get<CameraComponent>();
		EnsurePlayerReference();
	}

	protected override void OnUpdate()
	{
		EnsurePlayerReference();

		UpdateEyeAngles();
		ApplyRecoilRecovery();
		FollowTarget();
	}

	private void EnsurePlayerReference()
	{
		if ( _player == null || !_player.IsValid )
		{
			_player = Game.ActiveScene.GetAllComponents<PlayerMovement>().FirstOrDefault( x => x.Network.IsOwner );
			_head = _player?.Head;
		}
	}

	private void UpdateEyeAngles()
	{
		if ( _head == null ) return;

		var eyeAngles = _head.WorldRotation.Angles();

		// Adjust based on mouse input and recoil
		eyeAngles.pitch = (eyeAngles.pitch + Input.MouseDelta.y * 0.1f + _recoilOffset.y).Clamp( -89.9f, 89.9f );
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f - _recoilOffset.x;
		eyeAngles.roll = 0f;

		_head.WorldRotation = eyeAngles.ToRotation();
	}

	private void ApplyRecoilRecovery()
	{
		// Gradually reduce recoil offset
		_recoilOffset.x = Lerp( _recoilOffset.x, 0, Time.Delta * RecoilRecoverySpeed );
		_recoilOffset.y = Lerp( _recoilOffset.y, 0, Time.Delta * RecoilRecoverySpeed );
	}

	private void FollowTarget()
	{
		if ( _camera == null || _head == null ) return;

		var camPos = _head.WorldPosition + GetCameraOffset();

		if ( _player.ThirdPerson )
		{
			Distance = 100f;

			var camForward = _head.WorldRotation.Forward;
			var targetCamPos = camPos + Distance;
			var camTrace = Scene.Trace.Ray( camPos, targetCamPos )
				.WithoutTags( "player" )
				.Run();

			camPos = camTrace.Hit ? camTrace.HitPosition + camTrace.Normal : camTrace.EndPosition;
		}

		_camera.WorldPosition = Vector3.Lerp( _camera.WorldPosition, camPos, Time.Delta * 20f );
		_camera.WorldRotation = _head.WorldRotation;
	}

	private Vector3 GetCameraOffset()
	{
		Vector3 targetOffset = Vector3.Zero;
		if ( _player.IsCrouching ) targetOffset += Vector3.Down * 20f;

		_currentOffset = Vector3.Lerp( _currentOffset, targetOffset, Time.Delta * 10f );
		return _currentOffset;
	}

	/// <summary>
	/// Adds a recoil effect to the camera.
	/// </summary>
	public void AddRecoil( float verticalRecoil, float horizontalRecoil )
	{
		// Randomize horizontal recoil direction
		float horizontalDirection = Game.Random.Float( -1f, 1f ); // Random float between -1 and 1
		float horizontalAmount = horizontalRecoil * horizontalDirection;

		// Apply the recoil
		_recoilOffset.x += horizontalAmount; // Horizontal (yaw)
		_recoilOffset.y -= verticalRecoil;  // Vertical (pitch)

		// Record the last time recoil was added
		_lastAttackTime = Time.Now;
	}

	private float Lerp( float a, float b, float t )
	{
		return a + (b - a) * t;
	}
}
