using UnityEngine;

public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID, bool sendAsReliable = false)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		SendPacket(textPacket, objectID);
    }

	public void SendVelocity(Vector3 velocity, uint objectID, bool sendAsReliable = false)
	{
		VelocityPacket velocityPacket = new VelocityPacket();

		velocityPacket.payload.x = velocity.x;
		velocityPacket.payload.y = velocity.y;
		velocityPacket.payload.z = velocity.z;
		
		SendPacket(velocityPacket, objectID);
	}

	public void SendPosition(Vector3 position, uint objectID, bool sendAsReliable = false)
	{
		PositionPacket positionPacket = new PositionPacket();

		positionPacket.payload.x = position.x;
		positionPacket.payload.y = position.y;
		positionPacket.payload.z = position.z;

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

