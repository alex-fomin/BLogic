 using System;

namespace BLogic.Derivations
{
	public class Mutablebool
	{
		public static  int TRUE_FALSE = 1;
		public static  int ONE_ZERO = 2;
		public static int boolSyntax = 1;
		public bool value;
 
		public Mutablebool(bool parambool)
		{
			this.value = parambool;
		}
 
		public override string ToString()
		{
			switch (boolSyntax)
			{
				case 1:
					return this.value ? "true" : "false";
				case 2:
					return this.value ? "1" : "0";
			}
			Console.Error.WriteLine("Software Error: unknown boolSyntax: " + boolSyntax);
			return this.value ? "true" : "false";
		}
 
		public Mutablebool clone()
		{
			return new Mutablebool(this.value);
		}
	}
}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.Mutablebool
 * JD-Core Version:    0.6.0
 */