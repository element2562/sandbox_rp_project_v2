using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Entities;

namespace Sandbox
{
	public class PlayerJobController : Component
	{
		public Job CurrentJob { get; set; }

		[Rpc.Broadcast]
		public void SetJob( Job job )
		{
			if ( job == CurrentJob )
				return;

			if(!Network.IsProxy)
			{
				EventManager.Instance.BroadcastMessage( $"{Connection.Local.DisplayName} is now a {job.Name}" );
				CurrentJob = job;
			}
		}

		[Rpc.Broadcast]
		public void QuitJob()
		{
			CurrentJob = null;
		}
	}
}
