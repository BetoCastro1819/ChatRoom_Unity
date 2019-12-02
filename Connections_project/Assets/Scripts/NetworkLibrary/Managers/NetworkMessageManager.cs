﻿using UnityEngine;

public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID, bool sendAsReliable = false)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		SendPacket(textPacket, objectID);
    }

	public void SendVelocity(Vector3 velocity, uint objectID, uint sequence = 0, bool sendAsReliable = false)
	{
		VelocityPacket velocityPacket = new VelocityPacket();

		velocityPacket.payload.sequence = sequence;
		velocityPacket.payload.velocity.x = velocity.x;
		velocityPacket.payload.velocity.y = velocity.y;
		velocityPacket.payload.velocity.z = velocity.z;
		
		SendPacket(velocityPacket, objectID);
	}

	public void SendPosition(Vector3 position, uint objectID, uint sequence = 0, bool sendAsReliable = false)
	{
		PositionPacket positionPacket = new PositionPacket();

		positionPacket.payload.sequence = sequence;
		positionPacket.payload.position.x = position.x;
		positionPacket.payload.position.y = position.y;
		positionPacket.payload.position.z = position.z;

		SendPacket(positionPacket, objectID);
	}

	private void SendPacket<T>(NetworkPacket<T> packet, uint objectID,  bool isReliable = false)
	{
		if (isReliable)
			PacketManager.Instance.SendReliablePacket(packet, objectID);
		else
			PacketManager.Instance.SendPacket(packet, objectID);
	} 
}

