namespace Sandbox.Interfaces
{
	public interface IInteractable
	{
		/// <summary>
		/// The message that should be displayed when the player looks at the object.
		/// Example: "Press [E] to pick up"
		/// </summary>
		string InteractionMessage { get; }

		/// <summary>
		/// Called when the player interacts with the object (presses E).
		/// </summary>
		/// <param name="player">The player interacting with the object.</param>
		void OnInteract( PlayerMovement player );
		void OnCarry( PlayerMovement player );
	}
}
