using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;

namespace ConsoleApp1
{
   public class MyData
	{
		public MyData()
		{
		}



		
	}

   class Program
   {
		static void Main(string[] args)
		{
			Console.ReadLine();
			var MyClass = new MyData();
			Console.ReadLine();
			var harmony = HarmonyInstance.Create("de.kopffaren");
			Console.ReadLine();
	  }
   }
}
