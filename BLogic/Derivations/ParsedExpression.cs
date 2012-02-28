using System;

namespace BLogic.Derivations
{
	public class ParsedExpression
	{
		public ParsedExpression(String initialText, LogicExpression expression, LogicSyntax logicSyntax,
		                        NegationSyntax negationSyntax)
		{
			InitialText = initialText;
			Expression = expression;
			LogicSyntax = logicSyntax;
			NegationSyntax = negationSyntax;
		}

		public string InitialText { get; private set; }

		public LogicExpression Expression { get; private set; }

		public LogicSyntax LogicSyntax { get; private set; }

		public NegationSyntax NegationSyntax { get; private set; }
	}
}
