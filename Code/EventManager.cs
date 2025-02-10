using Sandbox;
using System;
using System.Collections.Generic;

public class EventManager : Component
{
	// Singleton instance
	public static EventManager Instance { get; private set; }

	// Event message queue
	private Queue<string> _eventMessages = new();

	public EventManager()
	{
		Instance = this;
	}

	/// <summary>
	/// Broadcast a message to all players.
	/// </summary>
	/// <param name="message">The message to display.</param>
	[Rpc.Broadcast]
	public void BroadcastMessage( string message )
	{
		_eventMessages.Enqueue( message );

		// Optionally, limit the queue size
		if ( _eventMessages.Count > 10 )
		{
			_eventMessages.Dequeue();
		}

		// Trigger client-side message display
		DisplayMessage( message );
	}

	/// <summary>
	/// Called on the client to display the message.
	/// </summary>
	private void DisplayMessage( string message )
	{
		MessageDisplay.Instance?.ShowMessage( message );
	}
}
