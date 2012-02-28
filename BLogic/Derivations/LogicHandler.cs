#region

using System;
using System.Linq;

namespace BLogic.Derivations
{

	#endregion

	public static class LogicHandler
	{
		public static LogicDerivation ManipulateLogic(String paramString, LogicExpression paramLogicExpression)
		{
			var derivation = new LogicDerivation(paramString, paramLogicExpression);

			if (derivation.CNFandDNF)
			{
				return derivation;
			}

			derivation.CarryOutNonPrimaryOperatorReplacement();
			derivation.CarryOutBoolValues();

			if (derivation.CNFandDNF)
			{
				return derivation;
			}
			derivation.CarryOutAssociativity();
			derivation.CarryOutDeMorgans();
			derivation.CarryOutAssociativity();
			derivation.CarryOutIdempotency();
			derivation.CarryOutBoolValues();
			derivation.CarryOutAbsorbtion(derivation.Next);

			if (derivation.CNFandDNF)
			{
				return derivation;
			}

			do
			{
				derivation.CarryOutDistributivity();
				derivation.CarryOutAssociativity();
				derivation.CarryOutIdempotency();
				derivation.CarryOutBoolValues();
				derivation.CarryOutAbsorbtion(derivation.Next);
			} while (!derivation.CNFandDNF);
			return derivation;
		}

		public static String GetFormName(LogicalForm paramInt)
		{
			switch (paramInt)
			{
				case LogicalForm.LogicUnclassified:
					return "Not any Normal Form";
				case LogicalForm.LogicNnf:
					return "Negation Normal Form";
				case LogicalForm.LogicCNF:
					return "Conjunctive Normal Form (CNF)";
				case LogicalForm.LogicDNF:
					return "Disjunctive Normal Form (DNF)";
				case LogicalForm.LogicCNF | LogicalForm.LogicDNF:
					return "Conjunctive and Disjunctive Normal Form (CNF & DNF)";
			}

			return "Invalid Form passed";
		}

		public static BoolResolution GetBoolResolution(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression = paramOperatorExpression.Branches;

			bool i = false;
			bool j = false;

			foreach (bool b in arrayOfLogicExpression.OfType<LogicValue>().Select(t => t.EqualsTrue()))
			{
				if (b)
					i = true;
				else
					j = true;
			
				if (i && j)
				{
					break;
				}
			}
			if (i || j)
			{
				var k = paramOperatorExpression.Operator;

				switch (k)
				{
					case Operator.And:
						return j ? BoolResolution.BoolResolveFalse : BoolResolution.BoolRemoveBoolValues;
					case Operator.Or:
						return i ? BoolResolution.BoolResolveTrue : BoolResolution.BoolRemoveBoolValues;
				}

				Console.Error.WriteLine("Software Error: Operator other than AND, OR found when looking at bool values");

				return BoolResolution.BoolNotFound;
			}
			return BoolResolution.BoolNotFound;
		}

		public static void ReplaceIMPLIESOperator(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression = paramOperatorExpression.Branches;

			arrayOfLogicExpression[0].Negated = !arrayOfLogicExpression[0].Negated;

			paramOperatorExpression.Operator = (Operator) 1;
		}

		public static void ReplaceBIIMPLIESOperator(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression1 = paramOperatorExpression.Branches;

			LogicExpression[] arrayOfLogicExpression2 = {
			                                            	arrayOfLogicExpression1[0].Clone(),
			                                            	arrayOfLogicExpression1[1].Clone()
			                                            };

			LogicExpression[] arrayOfLogicExpression3 = {
			                                            	arrayOfLogicExpression1[0].Clone(),
			                                            	arrayOfLogicExpression1[1].Clone()
			                                            };

			arrayOfLogicExpression3[0].Negated = !arrayOfLogicExpression3[0].Negated;
			arrayOfLogicExpression3[1].Negated = !arrayOfLogicExpression3[1].Negated;

			var localLogicBranch1 = new OperatorExpression(Operator.And) { Branches = arrayOfLogicExpression2 };

			var localLogicBranch2 = new OperatorExpression(Operator.And) { Branches = arrayOfLogicExpression3 };

			LogicExpression[] arrayOfLogicExpression4 = {localLogicBranch1, localLogicBranch2};

			paramOperatorExpression.Branches = arrayOfLogicExpression4;
			paramOperatorExpression.Operator = Operator.Or;
		}

		public static void ReplaceXOROperator(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] branches = paramOperatorExpression.Branches;

			LogicExpression[] expressions = {
			                                            	branches[0].Clone(),
			                                            	branches[1].Clone()
			                                            };

			expressions[0].Negated = !expressions[0].Negated;

			LogicExpression[] arrayOfLogicExpression3 = {
			                                            	branches[0].Clone(),
			                                            	branches[1].Clone()
			                                            };

			arrayOfLogicExpression3[1].Negated = !arrayOfLogicExpression3[1].Negated;

			LogicExpression[] arrayOfLogicExpression4 = {
			                                            	new OperatorExpression(Operator.And)
			                                            		{
			                                            			Branches = expressions
			                                            		},
			                                            	new OperatorExpression(Operator.And)
			                                            		{
			                                            			Branches = arrayOfLogicExpression3
			                                            		}
			                                            };

			paramOperatorExpression.Branches = arrayOfLogicExpression4;
			paramOperatorExpression.Operator = Operator.Or;
		}

		public static bool DeMorgans(OperatorExpression paramOperatorExpression)
		{
			Operator i = paramOperatorExpression.Operator;

			if (!paramOperatorExpression.Negated)
			{
				return false;
			}
			LogicExpression[] arrayOfLogicExpression = paramOperatorExpression.Branches;

			foreach (LogicExpression t in arrayOfLogicExpression)
			{
				t.Negated = !t.Negated;
			}

			paramOperatorExpression.Operator = i == Operator.And ? Operator.Or : Operator.And;
			paramOperatorExpression.Negated = false;

			return true;
		}

		public static bool Associativity(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression1 = paramOperatorExpression.Branches;

			int i = arrayOfLogicExpression1.OfType<OperatorExpression>()
				.Where(
					branch =>
					(branch.Operator == paramOperatorExpression.Operator) && (!branch.Negated))
				.Sum(branch => branch.Branches.Length - 1);

			if (i == 0)
			{
				return false;
			}

			var arrayOfLogicExpression2 = new LogicExpression[arrayOfLogicExpression1.Length + i];

			int k = 0;

			foreach (LogicExpression t in arrayOfLogicExpression1)
			{
				var operatorExpression = t as OperatorExpression;
				if (operatorExpression != null)
				{
					var localLogicBranch2 = operatorExpression;

					if ((localLogicBranch2.Operator == paramOperatorExpression.Operator) && (!localLogicBranch2.Negated))
					{
						LogicExpression[] arrayOfLogicExpression3 = localLogicBranch2.Branches;

						foreach (LogicExpression t1 in arrayOfLogicExpression3)
							arrayOfLogicExpression2[k++] = t1;
					}
					else
					{
						arrayOfLogicExpression2[k++] = t;
					}
				}
				else
				{
					arrayOfLogicExpression2[(k++)] = t;
				}
			}
			paramOperatorExpression.Branches = arrayOfLogicExpression2;

			return true;
		}

		public static bool Distributivity(OperatorExpression paramOperatorExpression)
		{
			Operator @operator = paramOperatorExpression.Operator;

			LogicExpression[] branches = paramOperatorExpression.Branches;
			var arrayOfLogicExpression = new LogicExpression[branches.Length][];

			int j = 0;
			int k = 1;

			foreach (LogicExpression expression in branches)
			{
				if (((expression is ParameterLogicExpression)) || ((expression is LogicValue)))
				{
					arrayOfLogicExpression[(j++)] = new[] {expression};
				}
				else
				{
					arrayOfLogicExpression[j] = ((OperatorExpression) expression).Branches;
					k *= arrayOfLogicExpression[j].Length;
					j++;
				}
			}

			if (k*branches.Length == j)
			{
				return false;
			}
			var arrayOfInt = new int[arrayOfLogicExpression.Length];
			arrayOfInt[arrayOfInt.Length - 1] = -1;
			var expressions = new LogicExpression[k];

			for (int n = 0; n < expressions.Length; n++)
			{
				arrayOfInt[(arrayOfInt.Length - 1)] += 1;

				for (int i1 = arrayOfInt.Length - 1; i1 > 0; i1--)
				{
					if (arrayOfInt[i1] != arrayOfLogicExpression[i1].Length)
						break;
					arrayOfInt[i1] = 0;
					arrayOfInt[(i1 - 1)] += 1;
				}

				var arrayOfLogicExpression3 = new LogicExpression[arrayOfLogicExpression.Length];

				for (int i2 = 0; i2 < arrayOfLogicExpression3.Length; i2++)
				{
					arrayOfLogicExpression3[i2] = arrayOfLogicExpression[i2][arrayOfInt[i2]];
				}
				if (arrayOfLogicExpression3.Length == 1)
				{
					expressions[n] = arrayOfLogicExpression3[0];
				}
				else
				{
					expressions[n] = new OperatorExpression(@operator)
					                             	{
					                             		Branches = arrayOfLogicExpression3
					                             	};
				}
			}

			var nn = (Operator) (@operator == 0 ? 1 : 0);

			paramOperatorExpression.Branches = expressions;
			paramOperatorExpression.Operator = nn;

			return true;
		}

		public static int Absorbtion(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression1 = paramOperatorExpression.Branches;

			int j = 0;
			int m;
			for (int k = 0; k < arrayOfLogicExpression1.Length - 1; k++)
			{
				for (m = k + 1; (m < arrayOfLogicExpression1.Length) && (arrayOfLogicExpression1[k] != null); m++)
				{
					if (arrayOfLogicExpression1[m] == null)
						continue;
					var q = IsAbsorbtion(arrayOfLogicExpression1[k], arrayOfLogicExpression1[m]);
					switch (q)
					{
						case Absorption.AbsorbedRight:
							arrayOfLogicExpression1[m] = null;
							j++;
							break;
						case Absorption.AbsorbedLeft:
							arrayOfLogicExpression1[k] = null;
							j++;
							break;
					}
				}
			}

			if (j > 0)
			{
				var arrayOfLogicExpression2 = new LogicExpression[arrayOfLogicExpression1.Length - j];

				m = 0;

				foreach (LogicExpression t in arrayOfLogicExpression1.Where(t => t != null))
				{
					arrayOfLogicExpression2[m++] = t;
				}
				paramOperatorExpression.Branches = arrayOfLogicExpression2;
			}

			return j;
		}

		private static Absorption IsAbsorbtion(LogicExpression left, LogicExpression right)
		{
			LogicExpression[] arrayOfLogicExpression1;
			if ((left is OperatorExpression))
			{
				arrayOfLogicExpression1 = ((OperatorExpression) left).Branches;
			}
			else
			{
				arrayOfLogicExpression1 = new LogicExpression[1];
				arrayOfLogicExpression1[0] = left;
			}
			LogicExpression[] arrayOfLogicExpression2;
			if ((right is OperatorExpression))
			{
				arrayOfLogicExpression2 = ((OperatorExpression) right).Branches;
			}
			else
			{
				arrayOfLogicExpression2 = new LogicExpression[1];
				arrayOfLogicExpression2[0] = right;
			}

			bool i = arrayOfLogicExpression1.Length < arrayOfLogicExpression2.Length;
			LogicExpression[] arrayOfLogicExpression3;
			LogicExpression[] arrayOfLogicExpression4;
			if (i)
			{
				arrayOfLogicExpression3 = arrayOfLogicExpression1;
				arrayOfLogicExpression4 = arrayOfLogicExpression2;
			}
			else
			{
				arrayOfLogicExpression3 = arrayOfLogicExpression2;
				arrayOfLogicExpression4 = arrayOfLogicExpression1;
			}

			if (arrayOfLogicExpression3.Any(t => arrayOfLogicExpression4.Any(t.Equals) == false))
			{
				return Absorption.AbsorbtionNotDone;
			}
			return (i ? Absorption.AbsorbedRight : Absorption.AbsorbedLeft);
		}

		public static bool Idempotency(OperatorExpression paramOperatorExpression)
		{
			Operator i = paramOperatorExpression.Operator;

			LogicExpression[] arrayOfLogicExpression = paramOperatorExpression.Branches;

			int j = arrayOfLogicExpression.Length;

			for (var k = 0; k < arrayOfLogicExpression.Length; k++)
			{
				if ((arrayOfLogicExpression[k] == null) || (!(arrayOfLogicExpression[k] is ParameterLogicExpression)))
					continue;
				var localLogicLeaf1 = (ParameterLogicExpression) arrayOfLogicExpression[k];

				for (int n = k + 1; n < arrayOfLogicExpression.Length; n++)
				{
					if ((arrayOfLogicExpression[n] == null) || (!(arrayOfLogicExpression[n] is ParameterLogicExpression)))
						continue;
					var localLogicLeaf2 = (ParameterLogicExpression) arrayOfLogicExpression[n];

					if (!localLogicLeaf1.Name.Equals(localLogicLeaf2.Name))
						continue;
					if (arrayOfLogicExpression[k].Negated != arrayOfLogicExpression[n].Negated)
					{
						switch (i)
						{
							case Operator.And:
								arrayOfLogicExpression[k] = new LogicValue(false);
								break;
							case Operator.Or:
								arrayOfLogicExpression[k] = new LogicValue(true);
								break;
							default:
								Console.Error.WriteLine("Software Error: Unimplemented operator: " + i);
								break;
						}
					}

					arrayOfLogicExpression[n] = null;
					j--;
				}
			}

			if (j == arrayOfLogicExpression.Length)
				return false;
			if (j == 1)
			{
				var localObject1 = paramOperatorExpression.Parent;

				if (localObject1 != null)
				{
					localObject1.SetBranch(arrayOfLogicExpression[0], paramOperatorExpression.GetPositionInParent());
				}
			}
			else
			{

				var localObject = new LogicExpression[j];
				j = 0;

				foreach (LogicExpression t in arrayOfLogicExpression.Where(t => t != null))
				{
					localObject[j++] = t;
				}
				paramOperatorExpression.Branches = localObject;
			}
			return true;
		}
	}

	internal enum Absorption
	{
		AbsorbtionNotDone = 20,
		AbsorbedRight = 21,
		AbsorbedLeft = 22,

	}

	public enum BoolResolution
	{
		BoolRemoveBoolValues = 31,
		BoolResolveTrue = 32,
		BoolResolveFalse = 33,
		BoolNotFound = 30
	}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.LogicHandler
 * JD-Core Version:    0.6.0
 */
}