using System;
using BLogic.Derivations;

namespace BLogic
{
	static class Program
	{
		public static void Main(String[] paramArrayOfString)
		{
			Console.WriteLine("# LogicHandler v1.0 beta by Isaac Turner");
			Console.WriteLine("# isaac.turner@student.manchester.ac.uk");

			if (paramArrayOfString.Length != 1)
			{
				Console.WriteLine("USAGE: java -jar boolLogic.jar <expression>");
				paramArrayOfString = new[]
				                     	{
				                     		//"A <=> B <=> C",
											"(A . B . C) + (B' . A' . C) + (A' . B . C') + (B' . A . C')",
				                     	};
			}

			try
			{
				ParsedExpression localParsedExpression = LogicParser.ParseLogic(paramArrayOfString[0]);

				LogicDerivation localLogicDerivation = LogicHandler.ManipulateLogic(localParsedExpression.InitialText, localParsedExpression.Expression);
				localLogicDerivation.PrintTrace(localParsedExpression.LogicSyntax, localParsedExpression.NegationSyntax);
			}
			catch (Exception localException)
			{
				Console.Error.WriteLine(paramArrayOfString[0]);
				Console.Error.WriteLine(localException.Message);

				return;
			}
		}
	}
}