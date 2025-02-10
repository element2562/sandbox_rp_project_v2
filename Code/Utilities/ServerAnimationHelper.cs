using Sandbox;
namespace Sandbox.Utilities
{
	public static class ServerAnimationHelper
	{
		[Rpc.Broadcast]
		public static void BroadcastAnimationSet(SkinnedModelRenderer renderer, string parameter, bool value)
		{
			if ( renderer == null ) return;

			renderer.Set(parameter, value);
		}

		[Rpc.Broadcast]
		public static void BroadcastAnimationSet(SkinnedModelRenderer renderer, string parameter, int value)
		{
			if ( renderer == null ) return;

			renderer.Set(parameter, value);
		}

		[Rpc.Broadcast]
		public static void BroadcastAnimationSet(SkinnedModelRenderer renderer, string parameter, float value)
		{
			if ( renderer == null ) return;

			renderer.Set(parameter, value);
		}
	}
}
