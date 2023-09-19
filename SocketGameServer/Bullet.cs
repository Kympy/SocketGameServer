using System.Numerics;

namespace SocketGameServer
{
	[System.Serializable]
	public class Bullet
	{
		public string name = "";
		public Vector3 position;

		public static Bullet FromByteArray(byte[] input)
		{
			using (MemoryStream ms = new MemoryStream(input))
			{
				using (BinaryReader br = new BinaryReader(ms))
				{

					Bullet bullet = new Bullet();
					bullet.name = br.ReadString();
					bullet.position.X = br.ReadSingle();
					bullet.position.Y = br.ReadSingle();
					bullet.position.Z = br.ReadSingle();

					return bullet;
				}
			}
		}

		public static byte[] ToByteArray(Bullet bullet)
		{
			using (MemoryStream ms = new MemoryStream())
			{
				using (BinaryWriter bw = new BinaryWriter(ms))
				{
					bw.Write(bullet.name);
					bw.Write(bullet.position.X);
					bw.Write(bullet.position.Y);
					bw.Write(bullet.position.Z);

					return ms.ToArray();
				}
			}
		}
	}
}
