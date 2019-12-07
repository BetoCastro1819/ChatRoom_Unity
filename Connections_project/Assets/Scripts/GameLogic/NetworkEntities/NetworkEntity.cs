using System.IO;
using UnityEngine;

public enum ObjectIDs
{
	ServerShip = 2,
	ClientShip = 3,
}

public abstract class NetworkEntity : MonoBehaviour
{
	[SerializeField] protected float speed = 10;
	[SerializeField] protected GameObject bulletPrefab;
	[SerializeField] protected Transform shootPosition;
	[SerializeField] protected int health;

	protected ObjectIDs objectID;

	protected virtual void Start()
    {
    }

    protected abstract void OnReceiveDataEvent(uint packetID, ushort packetTypeID, Stream stream);

	public int GetHealth() { return health; }
}