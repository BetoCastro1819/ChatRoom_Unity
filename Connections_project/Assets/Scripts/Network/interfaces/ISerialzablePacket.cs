using System.IO;

public interface ISerialzablePacket
{
	void Serialize(Stream stream);
	void Deserialize(Stream stream);
}
