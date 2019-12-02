using System.IO;
using UnityEngine;

public class ServerShip : NetworkEntity
{
	protected override void Start()
	{
		base.Start();

		objectID = ObjectIDs.ServerShip;
		PacketManager.Instance.AddListener((uint)objectID, OnReceiveDataEvent);
	}

	protected override void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
	{
		Debug.Log("Applying velocity received to " + gameObject.name);

		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Velocity:

				//VelocityPacket velocityPacket = new VelocityPacket();
				//velocityPacket.Deserialize(stream);
//
				//Debug.Log("Applying velocity received to " + gameObject.name);
				//Vector3 velocityReceived = new Vector3(
				//	velocityPacket.payload.x,
				//	velocityPacket.payload.y,
				//	velocityPacket.payload.z
				//);
				//rb.velocity = velocityReceived;
				break;
			case (ushort)UserPacketType.Position:
				PositionPacket positionPacket = new PositionPacket();
				positionPacket.Deserialize(stream);

				Vector3 position = new Vector3(
					positionPacket.payload.x,
					positionPacket.payload.y,
					positionPacket.payload.z
				);
				rb.position = position;
				break;
		}
	}

    void FixedUpdate()
    {
		if (!NetworkManager.Instance.isServer) return;

		float horizontal = Input.GetAxis("Horizontal");
		float vertical = Input.GetAxis("Vertical");

		Vector3 velocity = new Vector3(horizontal, 0, vertical);
		rb.velocity = velocity * speed;

		//Debug.Log("Sending packet from " + gameObject.name);
		//NetworkMessageManager.Instance.SendVelocity(velocity * speed, (uint)objectID);
		NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID);
	}
}
