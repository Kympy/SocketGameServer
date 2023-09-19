using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocketGameServer
{
	public class Singleton<T> where T : class, new()
	{
		private static volatile T? instance = null;
		private static object lockObject = new object();

		public static T Instance
		{
			get
			{
				lock(lockObject)
				{
					instance ??= new T();
					return instance;
				}
			}
		}
	}
}
