namespace Sandbox
{
	public class MuzzleFlashController : Component
	{
		[Property] private GameObject _muzzleFlash { get; set; }

		public async void PlayMuzzleFlash()
		{
			_muzzleFlash.Enabled = true;
			await Task.Delay( 100 );
			_muzzleFlash.Enabled = false;
		}

		[Rpc.Broadcast]
		public async void PlayMuzzleFlashServer()
		{
			_muzzleFlash.Enabled = true;
			await Task.Delay( 100 );
			_muzzleFlash.Enabled = false;
		}

		[Rpc.Owner]
		public void StopMuzzleFlashClient()
		{
			_muzzleFlash.Enabled = false;
		}
	}
}
