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
		//Debug.Log("Applying velocity received to " + gameObject.name);

		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Position:
				PositionPacket positionPacket = new PositionPacket();
				positionPacket.Deserialize(stream);

				Vector3 position = new Vector3(
					positionPacket.payload.position.x,
					positionPacket.payload.position.y,
					positionPacket.payload.position.z
				);
				rb.position = position;
				break;
		}
	}

    void FixedUpdate()
    {
		if (!NetworkManager.Instance.isServer) return;

		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");

		Vector3 velocity = new Vector3(-horizontal, 0, -vertical);
		rb.position += velocity * speed * Time.fixedDeltaTime;

		//Debug.Log("Sending packet from " + gameObject.name);
		//NetworkMessageManager.Instance.SendVelocity(velocity * speed, (uint)objectID);
		NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID);
	}
}
