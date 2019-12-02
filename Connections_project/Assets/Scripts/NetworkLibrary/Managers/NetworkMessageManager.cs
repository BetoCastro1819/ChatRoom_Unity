using UnityEngine;

public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID, bool sendAsReliable = false)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		SendPacket(textPacket, objectID);
    }

	public void SendVelocity(ushort entityID, Vector3 velocity, uint objectID, bool sendAsReliable = false)
	{
		VelocityPacket velocityPacket = new VelocityPacket();

		velocityPacket.payload.enitityID = entityID;
		velocityPacket.payload.velocity.x = velocity.x;
		velocityPacket.payload.velocity.y = velocity.y;
		velocityPacket.payload.velocity.z = velocity.z;
		
		SendPacket(velocityPacket, objectID);
	}

	private void SendPacket<T>(NetworkPacket<T> packet, uint objectID,  bool isReliable = false)
	{
		if (isReliable)
			PacketManager.Instance.SendReliablePacket(packet, objectID);
		else
			PacketManager.Instance.SendPacket(packet, objectID);
	} 
}

