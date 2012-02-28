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
			var localLogicDerivation = new LogicDerivation(paramString, paramLogicExpression);

			if (localLogicDerivation.CNFandDNF)
			{
				return localLogicDerivation;
			}

			CarryOutNonPrimaryOperatorReplacement(localLogicDerivation);
			CarryOutNonPrimaryOperatorReplacement(localLogicDerivation);
			CarryOutboolValues(localLogicDerivation);

			if (localLogicDerivation.CNFandDNF)
			{
				return localLogicDerivation;
			}
			CarryOutAssociativity(localLogicDerivation);
			CarryOutDeMorgans(localLogicDerivation);
			CarryOutAssociativity(localLogicDerivation);
			CarryOutIdempotency(localLogicDerivation);
			CarryOutboolValues(localLogicDerivation);
			CarryOutAbsorbtion(localLogicDerivation);

			if (localLogicDerivation.CNFandDNF)
			{
				return localLogicDerivation;
			}

			do
			{
				CarryOutDistributivity(localLogicDerivation);
				CarryOutAssociativity(localLogicDerivation);
				CarryOutIdempotency(localLogicDerivation);
				CarryOutboolValues(localLogicDerivation);
				CarryOutAbsorbtion(localLogicDerivation);
			} while (!localLogicDerivation.CNFandDNF);
			return localLogicDerivation;
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

		private static void CarryOutNonPrimaryOperatorReplacement(LogicDerivation paramLogicDerivation)
		{
			LogicExpression localLogicExpression1 = paramLogicDerivation.Next;

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
							ReplaceIMPLIESOperator(localLogicBranch);
							j += 2;
							paramLogicDerivation.AddStep(localLogicExpression1, "Replaced IMPLIES operator");

							localLogicExpression1 = paramLogicDerivation.Next;
							break;
						case Operator.OperatorBiimplies:
							ReplaceBIIMPLIESOperator(localLogicBranch);
							j++;
							paramLogicDerivation.AddStep(localLogicExpression1, "Replaced BIIMPLIES operator");

							localLogicExpression1 = paramLogicDerivation.Next;
							break;
						case Operator.OperatorXor:
							ReplaceXOROperator(localLogicBranch);
							j++;
							paramLogicDerivation.AddStep(localLogicExpression1, "Replaced XOR operator");

							localLogicExpression1 = paramLogicDerivation.Next;
							break;
						default:
							j++;
							break;
					}
				}
			}
		}

		private static void CarryOutDistributivity(LogicDerivation paramLogicDerivation)
		{
			LogicExpression localLogicExpression1 = paramLogicDerivation.Next;

			int i = 0;
			LogicExpression localLogicExpression2;
			while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(3, i++)) != null)
			{
				if (!Distributivity((OperatorExpression) localLogicExpression2))
					continue;
				paramLogicDerivation.AddStep(localLogicExpression1, "Distributivity");
				localLogicExpression1 = paramLogicDerivation.Next;
			}
		}

		private static void CarryOutAbsorbtion(LogicDerivation paramLogicDerivation)
		{
			Object localObject = paramLogicDerivation.Next;

			int i = 0;
			LogicExpression localLogicExpression1;
			while ((localLogicExpression1 = ((LogicExpression) localObject).GetSubExpression(3, i)) != null)
			{
				var localLogicBranch1 = (OperatorExpression) localLogicExpression1;
				int j = localLogicBranch1.Branches.Length;
				int k = Absorbtion(localLogicBranch1);

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
							((LogicExpression) localObject).SetParent(null, -1);
						}
						else
						{
							localLogicBranch2.SetBranch(localLogicExpression2, localLogicBranch1.GetPositionInParent());
						}

						i--;
					}

					paramLogicDerivation.AddStep((LogicExpression) localObject, "Absorbtion");
					localObject = paramLogicDerivation.Next;
					continue;
				}

				i++;
			}
		}

		private static void CarryOutDeMorgans(LogicDerivation paramLogicDerivation)
		{
			LogicExpression localLogicExpression1 = paramLogicDerivation.Next;

			int i = localLogicExpression1.GetDepth();

			for (int k = i; k >= 2; k--)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j)) != null)
				{
					if (DeMorgans((OperatorExpression) localLogicExpression2))
					{
						paramLogicDerivation.AddStep(localLogicExpression1, "De Morgan's");
						localLogicExpression1 = paramLogicDerivation.Next;
						continue;
					}

					j++;
				}
			}
		}

		private static void CarryOutAssociativity(LogicDerivation paramLogicDerivation)
		{
			LogicExpression localLogicExpression1 = paramLogicDerivation.Next;

			int i = localLogicExpression1.GetDepth();

			for (int k = 3; k <= i; k++)
			{
				int j = 0;
				LogicExpression localLogicExpression2;
				while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(k, j)) != null)
				{
					if (Associativity((OperatorExpression) localLogicExpression2))
					{
						paramLogicDerivation.AddStep(localLogicExpression1, "Associativity");
						localLogicExpression1 = paramLogicDerivation.Next;
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

					paramLogicDerivation.AddStep(localLogicExpression1, "Associativity");
				}
			}
		}

		private static void CarryOutIdempotency(LogicDerivation paramLogicDerivation)
		{
			LogicExpression localLogicExpression1 = paramLogicDerivation.Next;

			int i = 0;

			int j = 0;
			LogicExpression localLogicExpression2;
			while ((localLogicExpression2 = localLogicExpression1.GetSubExpression(2, j)) != null)
			{
				if (Idempotency((OperatorExpression) localLogicExpression2))
				{
					i = 1;
					continue;
				}
				j++;
			}

			if (i != 0)
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

				paramLogicDerivation.AddStep(localLogicExpression1, "Idempotency");
			}
		}

		private static void CarryOutboolValues(LogicDerivation paramLogicDerivation)
		{
			var expression = paramLogicDerivation.Next;

			int i = expression.GetDepth();

			for (int m = 2; m <= i; m++)
			{
				int k = 0;
				int j = 0;
				LogicExpression localLogicExpression;
				while ((localLogicExpression = expression.GetSubExpression(m, j)) != null)
				{
					var localLogicBranch1 = (OperatorExpression) localLogicExpression;
					BoolResolution n = GetBoolResolution(localLogicBranch1);
					if (n == BoolResolution.BoolRemoveBoolValues)
					{
						LogicExpression[] arrayOfLogicExpression = localLogicBranch1.Branches;
						int i1 = arrayOfLogicExpression.Length;

						foreach (LogicExpression t in arrayOfLogicExpression)
						{
							if ((t is LogicValue))
							{
								i1--;
							}
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
							paramLogicDerivation.AddStep(localLogicValue, "Resolved bool values");
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
				paramLogicDerivation.AddStep(expression, "Removed redundant bool values");
				expression = paramLogicDerivation.Next;
			}
		}

		private static BoolResolution GetBoolResolution(OperatorExpression paramOperatorExpression)
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

		private static void ReplaceIMPLIESOperator(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression = paramOperatorExpression.Branches;

			arrayOfLogicExpression[0].Negated = !arrayOfLogicExpression[0].Negated;

			paramOperatorExpression.Operator = (Operator) 1;
		}

		private static void ReplaceBIIMPLIESOperator(OperatorExpression paramOperatorExpression)
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

			var localLogicBranch1 = new OperatorExpression(0) {Branches = arrayOfLogicExpression2};

			var localLogicBranch2 = new OperatorExpression(0) {Branches = arrayOfLogicExpression3};

			LogicExpression[] arrayOfLogicExpression4 = {localLogicBranch1, localLogicBranch2};

			paramOperatorExpression.Branches = arrayOfLogicExpression4;
			paramOperatorExpression.Operator = (Operator) 1;
		}

		private static void ReplaceXOROperator(OperatorExpression paramOperatorExpression)
		{
			LogicExpression[] arrayOfLogicExpression1 = paramOperatorExpression.Branches;

			LogicExpression[] arrayOfLogicExpression2 = {
			                                            	arrayOfLogicExpression1[0].Clone(),
			                                            	arrayOfLogicExpression1[1].Clone()
			                                            };

			arrayOfLogicExpression2[0].Negated = !arrayOfLogicExpression2[0].Negated;

			LogicExpression[] arrayOfLogicExpression3 = {
			                                            	arrayOfLogicExpression1[0].Clone(),
			                                            	arrayOfLogicExpression1[1].Clone()
			                                            };

			arrayOfLogicExpression3[1].Negated = !arrayOfLogicExpression3[1].Negated;

			var localLogicBranch1 = new OperatorExpression(0) {Branches = arrayOfLogicExpression2};

			var localLogicBranch2 = new OperatorExpression(0) {Branches = arrayOfLogicExpression3};

			LogicExpression[] arrayOfLogicExpression4 = {localLogicBranch1, localLogicBranch2};

			paramOperatorExpression.Branches = arrayOfLogicExpression4;
			paramOperatorExpression.Operator = Operator.Or;
		}

		private static bool DeMorgans(OperatorExpression paramOperatorExpression)
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
			var j = i == Operator.And ? Operator.Or : Operator.And;

			paramOperatorExpression.Operator = j;
			paramOperatorExpression.Negated = false;

			return true;
		}

		private static bool Associativity(OperatorExpression paramOperatorExpression)
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

		private static bool Distributivity(OperatorExpression paramOperatorExpression)
		{
			Operator i = paramOperatorExpression.Operator;

			LogicExpression[] arrayOfLogicExpression1 = paramOperatorExpression.Branches;
			var arrayOfLogicExpression = new LogicExpression[arrayOfLogicExpression1.Length][];
			LogicExpression[] arrayOfLogicExpression2;

			int j = 0;
			int k = 1;

			foreach (LogicExpression t in arrayOfLogicExpression1)
			{
				if (((t is ParameterLogicExpression)) || ((t is LogicValue)))
				{
					arrayOfLogicExpression2 = new[] {t};
					arrayOfLogicExpression[(j++)] = arrayOfLogicExpression2;
				}
				else
				{
					arrayOfLogicExpression[j] = ((OperatorExpression) t).Branches;
					k *= arrayOfLogicExpression[j].Length;
					j++;
				}
			}

			if (k*arrayOfLogicExpression1.Length == j)
			{
				return false;
			}
			var arrayOfInt = new int[arrayOfLogicExpression.Length];
			arrayOfInt[(arrayOfInt.Length - 1)] = -1;
			arrayOfLogicExpression2 = new LogicExpression[k];

			for (int n = 0; n < arrayOfLogicExpression2.Length; n++)
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
					arrayOfLogicExpression2[n] = arrayOfLogicExpression3[0];
				}
				else
				{
					arrayOfLogicExpression2[n] = new OperatorExpression(i)
					                             	{
					                             		Branches = arrayOfLogicExpression3
					                             	};
				}
			}

			var nn = (Operator) (i == 0 ? 1 : 0);

			paramOperatorExpression.Branches = arrayOfLogicExpression2;
			paramOperatorExpression.Operator = nn;

			return true;
		}

		private static int Absorbtion(OperatorExpression paramOperatorExpression)
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

		private static bool Idempotency(OperatorExpression paramOperatorExpression)
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

	internal enum BoolResolution
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