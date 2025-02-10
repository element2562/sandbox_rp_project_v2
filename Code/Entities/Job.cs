using System.Text.Json.Serialization;

namespace Sandbox.Entities
{
	public class Job
	{
		[JsonPropertyName( "name" )]
		public string Name { get; set; }
		[JsonPropertyName( "description" )]
		public string Description { get; set; }
		[JsonPropertyName( "salary" )]
		public int Salary { get; set; }
	}
}
