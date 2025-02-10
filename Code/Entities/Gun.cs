using System;
using System.Threading.Tasks;
using Sandbox.Entities;
using Sandbox.Utilities;
using Coroutines;
using Sandbox.Enums;

namespace Sandbox.Entities
{
	public class Gun : WeaponItem
	{
		[Property] public int MagazineSize;
		[Property] public int RoundsPerMinute;
		[Property] public SoundEvent FireSound;
		[Property] public SoundEvent ReloadSound;
		[Property] public GameObject BulletPrefab;
		[Property] public GameObject WeaponMuzzleFlash;
		[Property( Name = "Reload Time (in seconds)" )] public float ReloadTime;
		[Property] public float Recoil;

		[Property] private Vector3 _viewModelIronSightsPosition;
		[Property] private Rotation _viewModelIronSightsRotation;

		[Property] private List<WeaponFireTypeEnum> _weaponFireTypes;

		private MuzzleFlashController _viewModelMuzzleFlashController { get; set; }
		private MuzzleFlashController _worldModelMuzzleFlashController { get; set; }

		public bool IsShooting { get; set; }
		public WeaponFireTypeEnum CurrentFireType => _currentFireType;
		public GameObject ViewModel => _viewModel;

		private int _currentAmmo;
		private float _timeBetweenShots; // Cooldown time in seconds
		private float _lastShotTime; // Timestamp of the last shot
		private bool _isReloading;
		private WeaponFireTypeEnum _currentFireType;

		protected override void OnAwake()
		{
			_currentAmmo = MagazineSize;
			_timeBetweenShots = 60f / RoundsPerMinute; // Calculate the time between shots
			_lastShotTime = 0f; // Initialize the last shot time
			_currentFireType = _weaponFireTypes[0];
		}

		public override async void Attack()
		{
			if ( _isReloading ) return;
			if ( Time.Now - _lastShotTime < _timeBetweenShots )
				return; // Still in cooldown, don't fire

			if ( _currentAmmo > 0 )
			{
				_currentAmmo--;
				_lastShotTime = Time.Now;

				ViewModelRenderer.Set( "b_attack", true );
				ServerAnimationHelper.BroadcastAnimationSet( _playerModelRenderer, "b_attack", true );
				ServerSoundHelper.BroadcastSoundOnGameObject( FireSound, _viewModel, 0.2f);

				_viewModelMuzzleFlashController ??= _viewModel.Components.Get<MuzzleFlashController>();
				_viewModelMuzzleFlashController.PlayMuzzleFlash();

				_worldModelMuzzleFlashController ??= _worldModel.Components.Get<MuzzleFlashController>();
				_worldModelMuzzleFlashController?.PlayMuzzleFlashServer();
				_worldModelMuzzleFlashController?.StopMuzzleFlashClient();

				SpawnBullet();

				var cameraMovement = Game.ActiveScene.Camera.GetComponent<CameraMovement>();
				cameraMovement?.AddRecoil( Recoil, Recoil * 0.5f );
			}
			else
			{
				await Reload();
			}
		}

		public async Task Reload()
		{
			if ( _isReloading || _currentAmmo == MagazineSize )
				return;

			_isReloading = true;
			Log.Info( WorldModelRenderer );
			ViewModelRenderer.Set( "b_reload", true );
			ServerAnimationHelper.BroadcastAnimationSet( _playerModelRenderer, "b_reload", true );
			ServerSoundHelper.BroadcastSoundOnGameObject( ReloadSound, WorldModelRenderer.GameObject );

			await GameTask.DelaySeconds( ReloadTime );

			_currentAmmo = MagazineSize;
			_isReloading = false;
		}

		public void SetIronSights( bool isIronSights )
		{
			if ( _isReloading ) return;

			// smoothly set the iron sights positions

			if ( isIronSights )
			{
				ViewModelRenderer.Set( "ironsights", 1 );
				_viewModel.LocalPosition = Vector3.Lerp(_viewModel.LocalPosition, _viewModelIronSightsPosition, 0.5f);
				_viewModel.LocalRotation = Rotation.Lerp(_viewModel.LocalRotation, _viewModelIronSightsRotation, 0.5f );
			}
			else
			{
				ViewModelRenderer.Set( "ironsights", 0 );
				_viewModel.LocalPosition = Vector3.Lerp( _viewModel.LocalPosition, _viewModelLocalPosition, 0.5f );
				_viewModel.LocalRotation = Rotation.Lerp( _viewModel.LocalRotation, Rotation.Identity, 0.5f );
			}
		}

		private void SpawnBullet()
		{
			if ( BulletPrefab == null ) return;

			var camera = Game.ActiveScene.Camera;
			var cameraTransform = camera.LocalTransform;

			// Create a new instance of the bullet prefab
			var bullet = BulletPrefab.Clone( new CloneConfig
			{
				StartEnabled = true,
				Parent = null, // No parent for the bullet
				Transform = cameraTransform
			} );

			// Position the bullet slightly forward from the camera
			bullet.GetComponent<Bullet>().Owner = PlayerMovement.Local.GameObject;
			bullet.LocalPosition += cameraTransform.Forward * 10f;

			bullet.NetworkSpawn(); // Spawn the bullet on the network
		}

		public void ChangeFireType()
		{
			if( _weaponFireTypes.Count == 1 ) return;

			var currentIndex = _weaponFireTypes.IndexOf( _currentFireType );
			currentIndex++;

			if ( currentIndex >= _weaponFireTypes.Count )
			{
				currentIndex = 0;
			}

			_currentFireType = _weaponFireTypes[currentIndex];
			ViewModelRenderer.Set( "firing_mode", (int)_currentFireType );
		}

		public void SetSprinting(bool value)
		{
			ViewModelRenderer.Set( "sprinting", value );
		}
	}
}
