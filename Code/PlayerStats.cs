using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class PlayerStats : Component
	{
		[Property] public PlayerJobController JobController;

		public string DisplayName { get; set; }
		public string JobName { get { return JobController?.CurrentJob?.Name; } }
		[Sync] public int Kills { get; set; }
		[Sync] public int Deaths { get; set; }
		public int Ping { get; set; }

		protected override void OnStart()
		{
			DisplayName = Network.Owner.DisplayName;
			Kills = 0;
			Deaths = 0;
			Ping = Network.Owner.Ping.CeilToInt();
		}

		public void AddKill()
		{
			Log.Info( "adding kill" );
			Kills++;
		}

		public void AddDeath()
		{
			Deaths++;
		}
	}
}
