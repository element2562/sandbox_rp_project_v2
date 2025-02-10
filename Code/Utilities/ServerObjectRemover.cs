using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox.Utilities
{
	public static class ServerObjectRemover
	{
		[Rpc.Broadcast]
		public static void DeleteObject( GameObject obj )
		{
			obj.Destroy();
		}
	}
}
