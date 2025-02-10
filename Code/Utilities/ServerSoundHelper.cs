using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Utilities
{
	public static class ServerSoundHelper
	{
		[Rpc.Broadcast]
		public static void BroadcastSound( SoundEvent sound )
		{
			Sound.Play( sound );
		}

		[Rpc.Broadcast]
		public static void BroadcastSoundOnGameObject( SoundEvent sound, GameObject gameObject )
		{
			gameObject.PlaySound( sound );
		}

		public static void BroadcastSoundOnGameObject( SoundEvent sound, GameObject gameObject, float volume )
		{
			var soundEvent = gameObject.PlaySound( sound );

			if( soundEvent == null ) return;
			soundEvent.Volume = volume;
		}
	}
}
