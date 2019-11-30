public class NetworkMessageManager : MonoBehaviourSingleton<NetworkMessageManager>
{
    public void SendTextMessage(string message, uint objectID)
    {
        TextPacket textPacket = new TextPacket();
		textPacket.payload = message;

		//PacketManager.Instance.SendPacket(textPacket, objectID);
		PacketManager.Instance.SendReliablePacket(textPacket, objectID);
    }
}

