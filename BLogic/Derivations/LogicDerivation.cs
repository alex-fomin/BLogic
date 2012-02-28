using System;
using System.Collections.Generic;
using System.Text;

namespace BLogic.Derivations
{
	public class LogicDerivation
	{
		private readonly String _initialText;
		private readonly List<LogicStep> _steps;
		private int _cnfNumberOfElements;
		private int _dnfNumberOfElements;

		public LogicDerivation(String paramString, LogicExpression paramLogicExpression)
		{
			_initialText = paramString;

			_steps = new List<LogicStep>();

			AddStep(paramLogicExpression, "Initial parsed expression");
		}

		public void AddStep(LogicExpression paramLogicExpression, String paramString)
		{
			LogicalForm logicalForm = paramLogicExpression.GetLogicalForm();

			Next = paramLogicExpression.Clone();

			var localLogicStep = new LogicStep {Expression = paramLogicExpression, Comment = paramString, Form = logicalForm};

			_steps.Add(localLogicStep);

			int j = paramLogicExpression.GetNumberOfElements();

			if (logicalForm.HasFlag(LogicalForm.LogicCNF))
			{
				if ((_cnf == null) || (j < _cnfNumberOfElements))
				{
					_cnf = paramLogicExpression;
					_cnfNumberOfElements = j;
				}
			}
			if (logicalForm.HasFlag( LogicalForm.LogicDNF ))
			{
				if ((_dnf == null) || (j < _dnfNumberOfElements))
				{
					_dnf = paramLogicExpression;
					_dnfNumberOfElements = j;
				}
			}
		}

		public LogicExpression Next { get; private set; }

		private LogicExpression _cnf;

		private LogicExpression _dnf;

// ReSharper disable InconsistentNaming
		public bool CNFandDNF
// ReSharper restore InconsistentNaming
		{
			get { return (_cnf != null) && (_dnf != null); }
		}

		public void PrintTrace(LogicSyntax logicSyntax, NegationSyntax negationSyntax)
		{
			int i = _initialText.Length;
			var localStringBuilder = new StringBuilder();

			for (int j = 0; j < i; j++)
			{
				localStringBuilder.Append(" ");
			}
			String str = localStringBuilder.ToString();
			int k = 1;

			foreach (LogicStep localLogicStep in _steps)
			{
				Console.WriteLine(
					new StringBuilder().Append(k != 0 ? _initialText : str).Append(" |=| ").Append(
						localLogicStep.Expression.ToString(logicSyntax, negationSyntax)).Append(" - ").Append(localLogicStep.Comment).Append(
							" - ").Append(LogicHandler.GetFormName(localLogicStep.Form)).ToString());

				k = 0;
			}

			Console.WriteLine(new StringBuilder().Append("CNF: ").Append(_cnf.ToString(logicSyntax, negationSyntax)).ToString());
			Console.WriteLine(new StringBuilder().Append("DNF: ").Append(_dnf.ToString(logicSyntax, negationSyntax)).ToString());
		}
	}
}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.LogicDerivation
 * JD-Core Version:    0.6.0
 */