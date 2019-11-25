using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		PacketManager.Instance.SendPacket(textPacket, objectID);
    }
}

