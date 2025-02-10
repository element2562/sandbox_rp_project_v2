using Sandbox;

namespace Sandbox
{
	public class InventoryItem : Component
	{
		[Property] public string Name { get; set; }
		[Property] public string Description { get; set; }
		[Property] public GameObject WorldModel { get; set; }
		[Property, ImageAssetPath] public string Icon { get; set; }
	}
}
