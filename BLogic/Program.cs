using System;
using System.Text;
using BLogic.Derivations;

namespace BLogic
{
	static class Program
	{
		public static void Main(String[] args)
		{
			Console.WriteLine("# LogicHandler v1.0 beta by Isaac Turner");
			Console.WriteLine("# isaac.turner@student.manchester.ac.uk");

			if (args.Length != 1)
			{
				Console.WriteLine("USAGE: java -jar boolLogic.jar <expression>");
				args = new[]
				                     	{
				                     		//"A <=> B <=> C",
											"(A . B . C) + (B' . A' . C) + (A' . B . C') + (B' . A . C')",
				                     	};
			}

			try
			{
				ParsedExpression expression = LogicParser.ParseLogic(args[0]);

				LogicDerivation localLogicDerivation = LogicHandler.LocalLogicDerivation(expression);
				PrintTrace(expression, localLogicDerivation, args[0]);
			}
			catch (Exception localException)
			{
				Console.Error.WriteLine(args[0]);
				Console.Error.WriteLine(localException.Message);

				return;
			}
		}

		private static void PrintTrace(ParsedExpression expression, LogicDerivation derivation, string initialText)
		{
			LogicSyntax logicSyntax = expression.LogicSyntax;
			NegationSyntax negationSyntax = expression.NegationSyntax;
			int i = initialText.Length;
			var localStringBuilder = new StringBuilder();

			for (int j = 0; j < i; j++)
			{
				localStringBuilder.Append(" ");
			}
			String str = localStringBuilder.ToString();
			int k = 1;

			foreach (LogicStep localLogicStep in derivation.Steps)
			{
				Console.WriteLine(
					new StringBuilder().Append(k != 0 ? initialText : str).Append(" |=| ").Append(
						localLogicStep.Expression.ToString(logicSyntax, negationSyntax)).Append(" - ").Append(localLogicStep.Comment).Append(
							" - ").Append(LogicHandler.GetFormName(localLogicStep.Form)).ToString());

				k = 0;
			}

			Console.WriteLine(new StringBuilder().Append("CNF: ").Append(derivation.CNF.ToString(logicSyntax, negationSyntax)).ToString());
			Console.WriteLine(new StringBuilder().Append("DNF: ").Append(derivation.DNF.ToString(logicSyntax, negationSyntax)).ToString());
		}
	}
}