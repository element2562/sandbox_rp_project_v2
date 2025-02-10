using Sandbox.Entities;
using Sandbox.Utilities;
using Sandbox.Enums;
using System.Threading.Tasks;

public sealed class PlayerAttackController : Component
{
	[Property] private InventoryManager _inventoryManager;
	[Property] private PlayerMovement _playerMovement;
	[Property] private PlayerStats _playerStats;
	[Property] private GameObject _hitmarkerPrefab;

	public bool Hit { get; private set; }

	private const float HitResetTime = 0.2f;
	private float _hitResetTimer = 0f;

	private bool _jumpAnimPlayed = false;

	protected override async void OnUpdate()
	{
		if ( Network.IsProxy ) return;

		var equippedItem = _inventoryManager.EquippedItem;
		if ( equippedItem is Gun weapon )
		{
			_hitResetTimer += Time.Delta;

			HandleAttack( weapon );
			HandleIronSights( weapon );
			await HandleReload( weapon );
			HandleFireModeChange( weapon );
			UpdateWeaponAnimations( weapon );
			ResetHitIfNecessary();
		}
	}

	private void HandleAttack( Gun weapon )
	{
		if( _playerMovement.IsSprinting ) return;

		if ( Input.Pressed( "Attack1" ) && weapon.CurrentFireType == WeaponFireTypeEnum.Single )
		{
			weapon.Attack();
		}
		else if ( Input.Down( "Attack1" ) && weapon.CurrentFireType == WeaponFireTypeEnum.Full_Auto )
		{
			weapon.Attack();
		}
	}

	private void HandleIronSights(Gun weapon)
	{
		bool shouldAim = Input.Down("Attack2") && !_playerMovement.IsSprinting;
		weapon.SetIronSights(shouldAim);

		var viewModelRenderer = weapon.ViewModel.GetComponent<SkinnedModelRenderer>();

		viewModelRenderer.Set("move_bob", !_playerMovement.IsMoving ? 0.0f : (_playerMovement.IsSprinting ? 0.5f : 0.2f));
	}

	private async Task HandleReload( Gun weapon )
	{
		if ( Input.Pressed( "Reload" ) )
		{
			await weapon.Reload();
		}
	}

	private void HandleFireModeChange( Gun weapon )
	{
		if ( Input.Pressed( "ChangeFireType" ) )
		{
			weapon.ChangeFireType();
		}
	}

	private void UpdateWeaponAnimations(Gun weapon)
	{
		ServerAnimationHelper.BroadcastAnimationSet(_playerMovement.Renderer, "holdtype", (int)weapon.HoldType);

		var viewModelRenderer = weapon.ViewModel.GetComponent<SkinnedModelRenderer>();
		viewModelRenderer.Set("b_sprint", _playerMovement.IsSprinting);

		if ( _playerMovement.IsJumping && !_jumpAnimPlayed )
		{
			viewModelRenderer.Set("b_jump", true);
			_jumpAnimPlayed = true;
		}
		else if (!_playerMovement.IsJumping && _jumpAnimPlayed )
		{
			_jumpAnimPlayed = false;
		}
	}

	private void ResetHitIfNecessary()
	{
		if ( _hitResetTimer > HitResetTime )
		{
			Hit = false;
		}
	}

	public void HandleHit()
	{
		Sound.Play( _hitmarkerPrefab.GetComponent<SoundPointComponent>()?.SoundEvent );
		_hitResetTimer = 0;
		Hit = true;
	}

	public void HandleKill()
	{
		_playerStats.AddKill();
	}
}
