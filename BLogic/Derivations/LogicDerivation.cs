using System;
using System.Collections.Generic;
using System.Linq;
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

		public void CarryOutAbsorbtion(LogicExpression localObject)
		{
			int i = 0;
			LogicExpression localLogicExpression1;
			while ((localLogicExpression1 = localObject.GetSubExpression(3, i)) != null)
			{
				var localLogicBranch1 = (OperatorExpression) localLogicExpression1;
				int j = localLogicBranch1.Branches.Length;
				int k = LogicHandler.Absorbtion(localLogicBranch1);

				if (k > 0)
				{
					if (j - k == 1)
					{
						OperatorExpression localLogicBranch2 = localLogicExpression1.Parent;

						LogicExpression[] arrayOfLogicExpression = localLogicBranch1.Branches;
						LogicExpression localLogicExpression2 = arrayOfLogicExpression[0];

						if (localLogicBranch2 == null)
						{
							localObject = localLogicExpression2;
							localObject.SetParent(null, -1);
						}
						else
						{
							localLogicBranch2.SetBranch(localLogicExpression2, localLogicBranch1.GetPositionInParent());
						}

						i--;
					}

					AddStep(localObject, "Absorbtion");
					localObject = Next;
					continue;
				}

				i++;
			}
		}

		public void CarryOutNonPrimaryOperatorReplacement()
		{
			LogicExpression localLogicExpression1 = Next;

			int i = localLogicExpression1.GetDepth();

			for (int k = 2; k <= i; k++)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j++)) != null)
				{
					var localLogicBranch = (OperatorExpression) localLogicExpression2;

					switch (localLogicBranch.Operator)
					{
						case Operator.OperatorImplies:
							LogicHandler.ReplaceIMPLIESOperator(localLogicBranch);
							j += 2;
							AddStep(localLogicExpression1, "Replaced IMPLIES operator");

							localLogicExpression1 = Next;
							break;
						case Operator.OperatorBiimplies:
							LogicHandler.ReplaceBIIMPLIESOperator(localLogicBranch);
							j++;
							AddStep(localLogicExpression1, "Replaced BIIMPLIES operator");

							localLogicExpression1 = Next;
							break;
						case Operator.OperatorXor:
							LogicHandler.ReplaceXOROperator(localLogicBranch);
							j++;
							AddStep(localLogicExpression1, "Replaced XOR operator");

							localLogicExpression1 = Next;
							break;
						default:
							j++;
							break;
					}
				}
			}
		}

		public void CarryOutBoolValues()
		{
			var expression = Next;

			int i = expression.GetDepth();

			for (int m = 2; m <= i; m++)
			{
				int k = 0;
				int j = 0;
				LogicExpression localLogicExpression;
				while ((localLogicExpression = expression.GetSubExpression(m, j)) != null)
				{
					var localLogicBranch1 = (OperatorExpression) localLogicExpression;
					BoolResolution n = LogicHandler.GetBoolResolution(localLogicBranch1);
					if (n == BoolResolution.BoolRemoveBoolValues)
					{
						LogicExpression[] arrayOfLogicExpression = localLogicBranch1.Branches;
						int i1 = arrayOfLogicExpression.Length;

						foreach (LogicExpression t in arrayOfLogicExpression.Where(t => (t is LogicValue)))
						{
							i1--;
						}
						if (i1 == 1)
						{
							var localObject2 = arrayOfLogicExpression.FirstOrDefault(t => (!(t is LogicValue)));

							OperatorExpression localLogicBranch2 = localLogicExpression.Parent;

							if (localLogicBranch2 == null)
							{
								expression = localObject2;
								localObject2.SetParent(null, -1);
							}
							else
							{
								localLogicBranch2.SetBranch(localObject2, localLogicExpression.GetPositionInParent());
							}
						}
						else
						{
							var expressions = new LogicExpression[i1];

							int i4 = 0;

							foreach (LogicExpression t in arrayOfLogicExpression.Where(t => !(t is LogicValue)))
							{
								expressions[i4++] = t;
							}
							localLogicBranch1.Branches = expressions;

							j++;
						}

						k = 1;
					}
					else if ((n == BoolResolution.BoolResolveTrue) || (n == BoolResolution.BoolResolveFalse))
					{
						bool @bool = n == BoolResolution.BoolResolveTrue;
						var localLogicValue = new LogicValue(@bool);

						var localObject2 = localLogicBranch1.Parent;

						if (localObject2 == null)
						{
							localLogicValue.SetParent(null, -1);
							AddStep(localLogicValue, "Resolved bool values");
							break;
						}

						localObject2.SetBranch(localLogicValue, localLogicBranch1.GetPositionInParent());

						k = 1;
					}
					else
					{
						j++;
					}
				}

				if (k == 0)
					continue;
				AddStep(expression, "Removed redundant bool values");
				expression = Next;
			}
		}

		public void CarryOutAssociativity()
		{
			LogicExpression localLogicExpression1 = Next;

			int i = localLogicExpression1.GetDepth();

			for (int k = 3; k <= i; k++)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j)) != null)
				{
					if (LogicHandler.Associativity((OperatorExpression) localLogicExpression2))
					{
						AddStep(localLogicExpression1, "Associativity");
						localLogicExpression1 = Next;
						continue;
					}

					j++;
				}
			}

			i = localLogicExpression1.GetDepth();

			if (i == 2)
			{
				var localLogicBranch = (OperatorExpression) localLogicExpression1;
				LogicExpression[] arrayOfLogicExpression = localLogicBranch.Branches;

				if (arrayOfLogicExpression.Length == 1)
				{
					localLogicExpression1 = arrayOfLogicExpression[0];
					localLogicExpression1.SetParent(null, -1);

					AddStep(localLogicExpression1, "Associativity");
				}
			}
		}

		public void CarryOutDeMorgans()
		{
			LogicExpression localLogicExpression1 = Next;

			int i = localLogicExpression1.GetDepth();

			for (int k = i; k >= 2; k--)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j)) != null)
				{
					if (LogicHandler.DeMorgans((OperatorExpression) localLogicExpression2))
					{
						AddStep(localLogicExpression1, "De Morgan's");
						localLogicExpression1 = Next;
						continue;
					}

					j++;
				}
			}
		}

		public void CarryOutIdempotency()
		{

			LogicExpression localLogicExpression1 = Next;
			{
				bool i = false;

				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(2, j)) != null)
				{
					if (LogicHandler.Idempotency((OperatorExpression)localLogicExpression2))
					{
						i = true;
						continue;
					}
					j++;
				}
				if (!i)
					return;
			}
			{
				if ((localLogicExpression1 is OperatorExpression))
				{
					LogicExpression[] arrayOfLogicExpression = ((OperatorExpression) localLogicExpression1).Branches;

					if (arrayOfLogicExpression.Length == 1)
					{
						localLogicExpression1 = arrayOfLogicExpression[0];
						localLogicExpression1.SetParent(null, -1);
					}
				}

				AddStep(localLogicExpression1, "Idempotency");
			}
		}

		public void CarryOutDistributivity()
		{
			LogicExpression localLogicExpression1 = Next;

			int i = 0;
			LogicExpression localLogicExpression2;
			while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(3, i++)) != null)
			{
				if (!LogicHandler.Distributivity((OperatorExpression) localLogicExpression2))
					continue;
				AddStep(localLogicExpression1, "Distributivity");
				localLogicExpression1 = Next;
			}
		}
	}
}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.LogicDerivation
 * JD-Core Version:    0.6.0
 */