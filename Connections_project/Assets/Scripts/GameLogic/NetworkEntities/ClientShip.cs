using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ClientShip : NetworkEntity
{
	Dictionary<uint, Vector3> inputs = new Dictionary<uint, Vector3>();
	uint sequence = 0;

    protected override void Start()
    {
		base.Start();

		objectID = ObjectIDs.ClientShip;
		PacketManager.Instance.AddListener((uint)objectID, OnReceiveDataEvent);
    }

	protected override void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream)
	{
		switch (packetTypeID)
		{
			case (ushort)UserPacketType.Velocity:

				VelocityPacket velocityPacket = new VelocityPacket();
				velocityPacket.Deserialize(stream);

				//Debug.Log("Applying velocity received to " + gameObject.name);
				Vector3 velocityReceived = new Vector3(
					velocityPacket.payload.x,
					velocityPacket.payload.y,
					velocityPacket.payload.z
				);
				rb.velocity = velocityReceived;

				if (NetworkManager.Instance.isServer)
					NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID);

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

				if (NetworkManager.Instance.isServer)
					NetworkMessageManager.Instance.SendPosition(rb.position, (uint)objectID);

				break;
		}
	}

    void FixedUpdate()
    {
		if (NetworkManager.Instance.isServer) return;
		
		float horizontal = Input.GetAxisRaw("Horizontal");
		float vertical = Input.GetAxisRaw("Vertical");


		Vector3 movPosition = Vector3.zero;
		if (Input.GetKeyDown(KeyCode.LeftArrow))
		{
			movPosition += -Vector3.right;
			NetworkMessageManager.Instance.SendPosition(rb.position + movPosition, (uint)objectID);
		}
		else if (Input.GetKeyDown(KeyCode.RightArrow))
		{
			movPosition += Vector3.right;
			NetworkMessageManager.Instance.SendPosition(rb.position + movPosition, (uint)objectID);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{
			movPosition += -Vector3.forward;
			NetworkMessageManager.Instance.SendPosition(rb.position + movPosition, (uint)objectID);
		}
		else if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			movPosition += Vector3.forward;
			NetworkMessageManager.Instance.SendPosition(rb.position + movPosition, (uint)objectID);
		}
		//rb.position += movPosition;

		//Debug.Log("Sending packet from " + gameObject.name);
		//NetworkMessageManager.Instance.SendVelocity(rb.velocity, (uint)objectID);
		//inputs.Add(sequence++, rb.velocity);
	}
}
