using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SocketGameServer
{
	internal class MainClass
	{
		[DllImport("User32.dll")]
		public static extern short GetAsyncKeyState(int inputKey);
		private static int lastTick = 0;
		private static void Main()
		{
			Console.WriteLine("======= SERVER START =======");
			Console.WriteLine();

			Server.Instance.CreateSocket();

			while(true)
			{
				if (Console.KeyAvailable == true)
				{
					if ((GetAsyncKeyState((int)ConsoleKey.Escape) & 0x8000) == 0x8000)
					{
						Server.Instance.CloseSocket();
						break;
					}
				}
				if ((Environment.TickCount & Int32.MaxValue) - lastTick < 1000)
				{
					continue;
				}
				lastTick = (Environment.TickCount & Int32.MaxValue);

				Server.Instance.Update();
			}
			Console.WriteLine();
			Console.WriteLine("======= SERVER DOWN =======");
		}
	}
}
