using UnityEngine;

public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID, bool sendAsReliable = false)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		if (sendAsReliable)
			PacketManager.Instance.SendReliablePacket(textPacket, objectID);
		else
			PacketManager.Instance.SendPacket(textPacket, objectID);
    }

	public void SendVelocity(Vector3 velocity, uint objectID, bool sendAsReliable = false)
	{

	}
}

