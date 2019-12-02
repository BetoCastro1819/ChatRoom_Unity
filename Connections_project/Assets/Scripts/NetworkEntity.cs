using System.IO;
using UnityEngine;

public class NetworkEntity : MonoBehaviour
{
	[SerializeField] float speed = 10;
	[SerializeField] NetworkShip networkShip;

	private enum NetworkShip
	{
		Server = 0,
		Client = 1
	}

	Rigidbody rb;
	uint objectID = 2;

	void Start()
    {
		rb = GetComponent<Rigidbody>();

		PacketManager.Instance.AddListener(objectID, OnReceiveDataEvent);
    }

    void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
	{
		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Velocity:
				
				VelocityPacket velocityPacket = new VelocityPacket();
				velocityPacket.Deserialize(stream);

				Debug.Log("Velocity packet received for ship with ID:  " + velocityPacket.payload.enitityID);
				Debug.Log("Current ship ID:  " + (int)networkShip);
				Debug.Log("ID checks out:  " + (velocityPacket.payload.enitityID == (ushort)networkShip));

				if (velocityPacket.payload.enitityID == (ushort)networkShip)
				{
					Debug.Log("Applying velocity received to " + gameObject.name);
					Vector3 velocityReceived = new Vector3(
						velocityPacket.payload.velocity.x,
						velocityPacket.payload.velocity.y,
						velocityPacket.payload.velocity.z
					);
					rb.velocity = velocityReceived;

					//if (NetworkManager.Instance.isServer && velocityPacket.payload.enitityID == (ushort)networkShip)
					//{
					//	NetworkMessageManager.Instance.SendVelocity(
					//		velocityPacket.payload.enitityID, 
					//		velocityPacket.payload.velocity, 
					//		objectID
					//	);
					//}
				}
				break;
		}
	}

    void FixedUpdate()
    {
		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 velocity = new Vector3(horizontal, 0, vertical);

		if (!NetworkManager.Instance.isServer)
		{
			if (networkShip == NetworkShip.Client)
			{
				//rb.velocity = velocity * speed;
				Debug.Log("Sending packet from " + gameObject.name);
				NetworkMessageManager.Instance.SendVelocity((ushort)NetworkShip.Client, velocity * speed, objectID);
			}
		}
		else
		{
			if (networkShip == NetworkShip.Server)
			{
				rb.velocity = velocity * speed;
				NetworkMessageManager.Instance.SendVelocity((ushort)NetworkShip.Server, velocity * speed, objectID);
			}
		}
    }
}