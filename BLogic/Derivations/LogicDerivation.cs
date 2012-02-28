using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLogic.Derivations
{
	public class LogicDerivation
	{
		private readonly List<LogicStep> _steps;
		private int _cnfNumberOfElements;
		private int _dnfNumberOfElements;

		public LogicDerivation(LogicExpression paramLogicExpression)
		{
			_steps = new List<LogicStep>();

			_next = paramLogicExpression.Clone();

			AddStep(paramLogicExpression, "Initial parsed expression");
		}

		private void AddStep(LogicExpression paramLogicExpression, string paramString)
		{
			LogicalForm logicalForm = paramLogicExpression.GetLogicalForm();

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
			if (logicalForm.HasFlag(LogicalForm.LogicDNF))
			{
				if ((_dnf == null) || (j < _dnfNumberOfElements))
				{
					_dnf = paramLogicExpression;
					_dnfNumberOfElements = j;
				}
			}
		}

		private LogicExpression _next;

		private LogicExpression _cnf;

		private LogicExpression _dnf;

// ReSharper disable InconsistentNaming
		public bool CNFandDNF
// ReSharper restore InconsistentNaming
		{
			get { return (_cnf != null) && (_dnf != null); }
		}

		public IEnumerable<LogicStep> Steps
		{
			get { return _steps; }
		}

		public LogicExpression CNF
		{
			get { return _cnf; }
		}

		public LogicExpression DNF
		{
			get { return _dnf; }
		}

		public void CarryOutAbsorbtion()
		{
			var localObject = _next;
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

					_next = localObject.Clone();

					AddStep(localObject, "Absorbtion");
					localObject = _next;
					continue;
				}

				i++;
			}
		}

		public void CarryOutNonPrimaryOperatorReplacement()
		{
			LogicExpression localLogicExpression1 = _next;

			for (int k = 2; k <= localLogicExpression1.GetDepth(); k++)
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
		
							_next = localLogicExpression1.Clone();
							AddStep(localLogicExpression1, "Replaced IMPLIES operator");
							localLogicExpression1 = _next;

							break;
						case Operator.OperatorBiimplies:
							LogicHandler.ReplaceBIIMPLIESOperator(localLogicBranch);
							j++;
							_next = localLogicExpression1.Clone();

							AddStep(localLogicExpression1, "Replaced BIIMPLIES operator");

							localLogicExpression1 = _next;
							break;
						case Operator.OperatorXor:
							LogicHandler.ReplaceXOROperator(localLogicBranch);
							j++;

							_next = localLogicExpression1.Clone();
							AddStep(localLogicExpression1, "Replaced XOR operator");
							localLogicExpression1 = _next;
		
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
			var expression = _next;

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
							_next = localLogicValue.Clone();

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
				_next = expression.Clone();

				AddStep(expression, "Removed redundant bool values");
				expression = _next;
			}
		}

		public void CarryOutAssociativity()
		{
			LogicExpression localLogicExpression1 = _next;

			int i = localLogicExpression1.GetDepth();

			for (int k = 3; k <= i; k++)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j)) != null)
				{
					if (LogicHandler.Associativity((OperatorExpression) localLogicExpression2))
					{
						_next = localLogicExpression1.Clone();

						AddStep(localLogicExpression1, "Associativity");
						localLogicExpression1 = _next;
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

					_next = localLogicExpression1.Clone();

					AddStep(localLogicExpression1, "Associativity");
				}
			}
		}

		public void CarryOutDeMorgans()
		{
			LogicExpression expression = _next;

			int i = expression.GetDepth();

			for (int k = i; k >= 2; k--)
			{
				int j = 0;
				LogicExpression logicExpression;
				while ((logicExpression = expression.GetSubExpression(k, j)) != null)
				{
					if (LogicHandler.DeMorgans((OperatorExpression) logicExpression))
					{
						_next = expression.Clone();

						AddStep(expression, "De Morgan's");
						expression = _next;
						continue;
					}

					j++;
				}
			}
		}

		public void CarryOutIdempotency()
		{

			LogicExpression localLogicExpression1 = _next;
			{
				bool i = false;

				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(2, j)) != null)
				{
					if (((OperatorExpression)localLogicExpression2).Idempotency())
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

				_next = localLogicExpression1.Clone();

				AddStep(localLogicExpression1, "Idempotency");
			}
		}

		public void CarryOutDistributivity()
		{
			LogicExpression localLogicExpression1 = _next;

			int i = 0;
			LogicExpression localLogicExpression2;
			while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(3, i++)) != null)
			{
				if (!LogicHandler.Distributivity((OperatorExpression) localLogicExpression2))
					continue;
				_next = localLogicExpression1.Clone();

				AddStep(localLogicExpression1, "Distributivity");
				localLogicExpression1 = _next;
			}
		}
	}
}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.LogicDerivation
 * JD-Core Version:    0.6.0
 */