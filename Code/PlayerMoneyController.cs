using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
	public class PlayerMoneyController : Component
	{
		public int Money { get; set; }

		public void AddMoney( int amount )
		{
			Money += amount;
		}
		public void RemoveMoney( int amount )
		{
			Money -= amount;
		}
	}
}
