using System;
using BLogic.Exceprions;

namespace BLogic.Derivations
{
	public static class LogicParser
	{

		public static ParsedExpression ParseLogic(string paramString)
			// EmptyLogicException, DifferentSyntaxesUsedException, UnexpectedSymbolException, InvalidVariableNameException, UnclosedBracketException
		{
			var localParserSyntax = new ParserSyntax();
			LogicExpression localLogicExpression = Parse(paramString, localParserSyntax);

			LogicSyntax i = localParserSyntax.LogicSyntax;
			NegationSyntax j = localParserSyntax.NegationSyntax;

			if (i == LogicSyntax.Unknown)
			{
				if (j == NegationSyntax.Unknown)
				{
					i = LogicSyntax.LogicSyntaxDotPlus;
					j = GetNegationSyntax(i);
				}
				else
				{
					switch (j)
					{
						case NegationSyntax.Written:
							i = LogicSyntax.Written;
							break;
						case NegationSyntax.Before:
							i = LogicSyntax.LogicSyntaxDotPlus;
							break;
						case NegationSyntax.After:
							i = LogicSyntax.LogicSyntaxDotPlus;
							break;
					}
				}

			}

			return new ParsedExpression(paramString, localLogicExpression, i, j);
		}

		private static LogicExpression Parse(String paramString, ParserSyntax paramParserSyntax)
		{
			if (paramString.Length == 0)
			{
				throw new EmptyLogicException("No expression entered");
			}
			paramString = StripDown(paramString);

			if (paramString.Length == 0)
			{
				throw new EmptyLogicException("Only whitespace or brackets entered");
			}

			String[] arrayOfString = BreakDownInToWords(paramString);

			return ParseWords(arrayOfString, paramParserSyntax);
		}

		private static LogicExpression ParseWords(String[] paramArrayOfString, ParserSyntax paramParserSyntax)
		{
			if (paramArrayOfString.Length == 1)
			{
				String[] arrayOfString = BreakDownInToWords(paramArrayOfString[0]);

				if (arrayOfString.Length == 1)
				{
					return paramParserSyntax.GenerateLiteral(paramArrayOfString[0]);
				}
				paramArrayOfString = arrayOfString;
			}
			for (int j = paramArrayOfString.Length - 1; j >= 0; j--)
			{
				var i = GetOperatorValue(paramArrayOfString[j]);

				LogicSyntax k;
				switch (i)
				{
					case (Operator.OperatorImplies):
						k = GetLogicSyntax(paramArrayOfString[j]);
						if (k != 0)
						{
							paramParserSyntax.UpdateLogicSyntax(k);
						}
						return SplitWordsByOperator(paramArrayOfString, j, Operator.OperatorImplies, paramParserSyntax);
					case Operator.OperatorBiimplies:
						k = GetLogicSyntax(paramArrayOfString[j]);
						if (k != 0)
						{
							paramParserSyntax.UpdateLogicSyntax(k);
						}
						return SplitWordsByOperator(paramArrayOfString, j, Operator.OperatorBiimplies, paramParserSyntax);
				}
			}
			{
				for (int j = paramArrayOfString.Length - 1; j >= 0; j--)
				{
					var i = GetOperatorValue(paramArrayOfString[j]);

					if (i == Operator.Or)
					{
						paramParserSyntax.UpdateLogicSyntax(GetLogicSyntax(paramArrayOfString[j]));

						return SplitWordsByOperator(paramArrayOfString, j, Operator.Or, paramParserSyntax);
					}
					if (paramArrayOfString[j].Equals("!="))
					{
						return SplitWordsByOperator(paramArrayOfString, j, Operator.OperatorXor, paramParserSyntax);
					}
				}
			}
			for (int j = paramArrayOfString.Length - 1; j >= 0; j--)
			{
				if (GetOperatorValue(paramArrayOfString[j]) != 0)
					continue;
				paramParserSyntax.UpdateLogicSyntax(GetLogicSyntax(paramArrayOfString[j]));

				return SplitWordsByOperator(paramArrayOfString, j, 0, paramParserSyntax);
			}

			if (paramParserSyntax.NegationSyntax == NegationSyntax.Unknown)
			{
				if (paramArrayOfString[0].Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
					paramParserSyntax.UpdateNegationSyntax(NegationSyntax.Written);
				else if (paramArrayOfString[0].Equals("~"))
					paramParserSyntax.UpdateNegationSyntax(NegationSyntax.Before);
				else if (paramArrayOfString[(paramArrayOfString.Length - 1)].Equals("'"))
				{
					paramParserSyntax.UpdateNegationSyntax(NegationSyntax.After);
				}

			}

			bool negated = false;
			var kk = paramArrayOfString.Length - 1;
			LogicExpression localLogicExpression = null;
			int m;
			switch (paramParserSyntax.NegationSyntax)
			{
				case NegationSyntax.Written:
					for (m = 0; m < kk; m++)
					{
						if (!paramArrayOfString[m].Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
						{
							throw new UnexpectedSymbolException("Expected 'NOT' instead of (or operator after) '" + paramArrayOfString[m] + "'");
						}

						negated = negated == false;
					}
					localLogicExpression = Parse(paramArrayOfString[kk], paramParserSyntax);
					break;
				case NegationSyntax.Before:
					for (m = 0; m < kk; m++)
					{
						if (!paramArrayOfString[m].Equals("~", StringComparison.InvariantCultureIgnoreCase))
						{
							throw new UnexpectedSymbolException("Expected '~' instead of (or operator after) '" + paramArrayOfString[m] + "'");
						}

						negated = negated == false;
					}
					localLogicExpression = Parse(paramArrayOfString[kk], paramParserSyntax);
					break;
				case NegationSyntax.After:
					for (m = kk; m > 0; m--)
					{
						if (!paramArrayOfString[m].Equals("'", StringComparison.InvariantCultureIgnoreCase))
						{
							throw new UnexpectedSymbolException("Expected \"'\" instead of (or operator before) '" + paramArrayOfString[m] +
							                                    "'");
						}

						negated = !negated;
					}
					localLogicExpression = Parse(paramArrayOfString[0], paramParserSyntax);
					break;
			}

			localLogicExpression.Negated = negated;
			return localLogicExpression;
		}

		private static LogicExpression SplitWordsByOperator(String[] paramArrayOfString, int paramInt1, Operator paramInt2,
		                                                    ParserSyntax paramParserSyntax)
			// EmptyLogicException, DifferentSyntaxesUsedException, UnexpectedSymbolException, InvalidVariableNameException, UnclosedBracketException
		{
			if (paramInt1 == 0)
				throw new UnexpectedSymbolException("Operator at beginning of list of arguments");
			if (paramInt1 == paramArrayOfString.Length - 1)
			{
				throw new UnexpectedSymbolException("Operator at end of list of arguments");
			}
			int m;
			if ((paramInt2 == Operator.OperatorXor) || (paramInt2 == Operator.OperatorImplies) || (paramInt2 == Operator.OperatorBiimplies))
			{
				var arrayOfLogicExpression1 = new LogicExpression[2];

				var arrayOfString1 = new String[paramInt1];
				var arrayOfString2 = new String[paramArrayOfString.Length - paramInt1 - 1];

				for (var mm = 0; mm < arrayOfString1.Length; mm++)
				{
					arrayOfString1[mm] = paramArrayOfString[mm];
				}

				m = paramInt1 + 1;

				for (int n = 0; n < arrayOfString2.Length; n++)
				{
					arrayOfString2[n] = paramArrayOfString[(m++)];
				}
				arrayOfLogicExpression1[0] = ParseWords(arrayOfString1, paramParserSyntax);
				arrayOfLogicExpression1[1] = ParseWords(arrayOfString2, paramParserSyntax);

				var localLogicBranch1 = new OperatorExpression(paramInt2)
				                        	{
				                        		Branches = arrayOfLogicExpression1
				                        	};
				return localLogicBranch1;
			}

			int i = 2;
			for (int k = paramInt1 - 1; k >= 0; k--)
			{
				var j = GetOperatorValue(paramArrayOfString[k]);

				if (j == Operator.NonOperator)
					continue;
				if (j != paramInt2)
				{
					break;
				}
				paramParserSyntax.UpdateLogicSyntax(GetLogicSyntax(paramArrayOfString[k]));

				i++;
			}

			var arrayOfLogicExpression2 = new LogicExpression[i];

			m = paramArrayOfString.Length - 1;

			for (int i2 = arrayOfLogicExpression2.Length - 1; i2 >= 0; i2--)
			{
				int i1 = 0;

				int i3;
				for (i3 = m; i3 >= 0; i3--)
				{
					var j = GetOperatorValue(paramArrayOfString[i3]);

					if (j != paramInt2)
						i1++;
					else if (j == paramInt2)
					{
						break;
					}
				}
				var arrayOfString3 = new String[i1];

				i3 = m - i1;
				int i4 = i1 - 1;

				for (int i5 = m; i5 > i3; i5--)
				{
					arrayOfString3[(i4--)] = paramArrayOfString[i5];
				}

				m = m - i1 - 1;

				arrayOfLogicExpression2[i2] = ParseWords(arrayOfString3, paramParserSyntax);
			}

			var localLogicBranch2 = new OperatorExpression(paramInt2)
			                        	{
			                        		Branches = arrayOfLogicExpression2
			                        	};
			return localLogicBranch2;
		}

		public static LogicExpression GenerateLiteral(String paramString)
			// InvalidVariableNameException
		{
			switch (GetValidBoolValue(paramString))
			{
				case BoolValue.BoolTrue:
					return new LogicValue(true);
				case BoolValue.BoolFalse:
					return new LogicValue(false);
			}
			if (IsValidVariableName(paramString))
			{
				return new ParameterLogicExpression(paramString);
			}
			throw new InvalidVariableNameException("Variable name not allowed: '" + paramString + "'");

		}

		private static BoolValue GetValidBoolValue(String paramString)
		{
			if (paramString.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase))
				return BoolValue.BoolTrue;
			if (paramString.Equals("FALSE", StringComparison.InvariantCultureIgnoreCase))
			{
				return BoolValue.BoolFalse;
			}
			return BoolValue.BoolInvalid;
		}

		private static bool IsValidVariableName(String paramString)
		{
			if (paramString.Length == 0)
			{
				return false;
			}
			char c = paramString[0];

			if (GetOperatorValue(paramString) != (Operator) (-1))
			{
				return false;
			}
			if (!IsAlphaChar(c))
			{
				return false;
			}
			int i = paramString.Length;

			for (int j = 1; j < i; j++)
			{
				c = paramString[j];

				if ((!IsAlphaChar(c)) && (!IsNumChar(c)) && (c != '-') && (c != '_'))
				{
					return false;
				}
			}
			return true;
		}

		private static LogicSyntax GetLogicSyntax(String paramString)
		{
			if (paramString.Equals("AND", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("OR", StringComparison.InvariantCultureIgnoreCase) ||
			    paramString.Equals("XOR", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("IMPLIES", StringComparison.InvariantCultureIgnoreCase) ||
			    paramString.Equals("BIIMPLIES", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("NOT", StringComparison.InvariantCultureIgnoreCase))
			{
				return LogicSyntax.Written;
			}
			if (paramString.Equals("n", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("u", StringComparison.InvariantCultureIgnoreCase))
				return LogicSyntax.LogicSyntaxNu;
			if (paramString.Equals("^") || paramString.Equals("v", StringComparison.InvariantCultureIgnoreCase))
				return LogicSyntax.LogicSyntaxUpDown;
			if (paramString.Equals("/\\") || paramString.Equals("\\/"))
				return LogicSyntax.LogicSyntaxSlashes;
			if (paramString.Equals(".") || paramString.Equals("+"))
				return LogicSyntax.LogicSyntaxDotPlus;
			if (paramString.Equals("=>") || paramString.Equals("<=>") || paramString.Equals("!="))
			{
				return LogicSyntax.Unknown;
			}
			return LogicSyntax.Invalid;
		}

		public static NegationSyntax GetNegationSyntax(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return NegationSyntax.Written;
				case LogicSyntax.LogicSyntaxNu:
					return NegationSyntax.Before;
				case LogicSyntax.LogicSyntaxUpDown:
					return NegationSyntax.Before;
				case LogicSyntax.LogicSyntaxSlashes:
					return NegationSyntax.Before;
				case LogicSyntax.LogicSyntaxDotPlus:
					return NegationSyntax.After;
				case LogicSyntax.Unknown:
					return NegationSyntax.Unknown;
			}
			Console.Error.WriteLine("Software error: unknown negationSyntax; logicSyntax: " + paramInt);

			return NegationSyntax.Unknown;
		}

		private static Operator GetOperatorValue(String paramString)
		{
			if (paramString.Equals("AND", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("n", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("^") ||
			    paramString.Equals("/\\") || paramString.Equals("."))
			{
				return Operator.And;
			}
			if (paramString.Equals("OR", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("u", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("v", StringComparison.InvariantCultureIgnoreCase) ||
			    paramString.Equals("\\/") || paramString.Equals("+"))
			{
				return Operator.Or;
			}
			if (paramString.Equals("XOR", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("!="))
				return Operator.OperatorXor;
			if (paramString.Equals("IMPLIES", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("=>"))
				return Operator.OperatorImplies;
			if (paramString.Equals("BIIMPLIES", StringComparison.InvariantCultureIgnoreCase) || paramString.Equals("<=>"))
			{
				return Operator.OperatorBiimplies;
			}
			return Operator.NonOperator;
		}

		private static String StripDown(String paramString)
		{
			paramString = paramString.Trim();

			while ((paramString.Length > 0) && (paramString[0] == '('))
			{
				if (FindMatchingBracket(paramString, 0) != paramString.Length - 1)
					break;
				paramString = paramString.Substring(1, paramString.Length - 1);
				paramString = paramString.Trim();
			}

			return paramString;
		}

		private static int FindMatchingBracket(String paramString, int paramInt)
			// UnclosedBracketException
		{
			int i = 1;
			int j = paramInt;

			while (i > 0)
			{
				int k = paramString.IndexOf('(', j + 1);
				int m = paramString.IndexOf(')', j + 1);

				if (m == -1)
					throw new UnclosedBracketException();
				if ((m < k) || (k == -1))
				{
					j = m;
					i--;
					continue;
				}

				j = k;
				i++;
			}

			return j;
		}

		private static String[] BreakDownInToWords(String paramString)
		{
			var arrayOfString = new String[0];

			for (int i = 0; i < 2; i++)
			{
				int j = i == 1 ? 1 : 0;
				int k = 0;

				for (int m = 0; m < paramString.Length; m++)
				{
					char c = paramString[m];
					int n;
					if (c == '(')
					{
						n = FindMatchingBracket(paramString, m);

						if (j != 0)
						{
							int startIndex = m + 1;
							arrayOfString[(k++)] = StripDown(paramString.Substring(startIndex, n  - startIndex));
						}
						else
						{
							k++;
						}
						m = n;
					}
					else
					{
						if (IsWhiteSpace(c))
						{
							n = paramString.Length;

							for (var i1 = m + 1; i1 < paramString.Length; i1++)
							{
								if (IsWhiteSpace(paramString[i1]))
									continue;
								n = i1 - 1;
								break;
							}

							m = n;
						}
						else if ((c == '^') || (c == 'v') || (c == '.') || (c == '+') ||
						         (((c == 'n') || (c == 'u')) && (m + 1 < paramString.Length) && (IsWhiteSpace(paramString[m + 1]))))
						{
							if (j != 0)
							{
								arrayOfString[(k++)] = paramString.Substring(m, 1);
							}
							else
								k++;
						}
						else if (c == '/')
						{
							if (m + 2 >= paramString.Length)
							{
								throw new UnexpectedSymbolException("Unexpected forward-slash (/) found at end of expression");
							}

							if (paramString[m + 1] == '\\')
							{
								if (j != 0)
								{
									arrayOfString[(k++)] = paramString.Substring(m, 2);
								}
								else
								{
									k++;
								}
								m++;
							}
							else
							{
								throw new UnexpectedSymbolException("Incomplete AND (/\\) operator: " + paramString.Substring(m, 2));
							}

						}
						else if (c == '\\')
						{
							if (m + 2 >= paramString.Length)
							{
								throw new UnexpectedSymbolException("Unexpected back-slash (\\) found at end of expression");
							}

							if (paramString[m + 1] == '/')
							{
								if (j != 0)
								{
									arrayOfString[(k++)] = paramString.Substring(m, 2);
								}
								else
								{
									k++;
								}
								m++;
							}
							else
							{
								throw new UnexpectedSymbolException("Incomplete OR (\\/) operator: " + paramString.Substring(m, 2));
							}

						}
						else if (c == '=')
						{
							if (m + 2 >= paramString.Length)
							{
								throw new UnexpectedSymbolException("Unexpected equals (=) found at end of expression");
							}

							if (paramString[m + 1] == '>')
							{
								if (j != 0)
								{
									arrayOfString[(k++)] = paramString.Substring(m, 2);
								}
								else
								{
									k++;
								}
								m++;
							}
							else
							{
								throw new UnexpectedSymbolException("Incomplete => (IMPLIES) operator: " + paramString.Substring(m, 2));
							}

						}
						else if (c == '<')
						{
							if (m + 3 >= paramString.Length)
							{
								throw new UnexpectedSymbolException("Unexpected equals (<) found at end of expression");
							}

							if ((paramString[m + 1] == '=') && (paramString[m + 2] == '>'))
							{
								if (j != 0)
								{
									arrayOfString[(k++)] = paramString.Substring(m, 3);
								}
								else
								{
									k++;
								}
								m += 2;
							}
							else
							{
								throw new UnexpectedSymbolException("Incomplete <=> (BI-IMPLICATION) operator: " +
								                                    paramString.Substring(m, 3));
							}

						}
						else if (c == '!')
						{
							if (m + 2 >= paramString.Length)
							{
								throw new UnexpectedSymbolException("Unexpected exclamation (!) found at end of expression");
							}

							if (paramString[m + 1] == '=')
							{
								if (j != 0)
								{
									arrayOfString[(k++)] = paramString.Substring(m, 3);
								}
								else
								{
									k++;
								}
								m++;
							}
							else
							{
								throw new UnexpectedSymbolException("Incomplete != (XOR) operator: " + paramString.Substring(m, 3));
							}

						}
						else if ((c == '~') || (c == '\''))
						{
							if (j != 0)
								arrayOfString[(k++)] = char.ToString(c);
							else
							{
								k++;
							}
						}
						else
						{
							if ((!IsAlphaChar(c)) && (!IsNumChar(c)))
							{
								throw new UnexpectedSymbolException("Invalid char: '" + c + "'");
							}
							n = paramString.Length;

							for (int i2 = m + 1; i2 < paramString.Length; i2++)
							{
								char i11 = paramString[i2];

								if ((IsAlphaChar(i11)) || (IsNumChar(i11)) || (i11 == 45) || (i11 == 95))
								{
									continue;
								}
								n = i2;
								break;
							}

							if (j != 0)
							{
								arrayOfString[(k++)] = paramString.Substring(m, n- m);
							}
							else
							{
								k++;
							}
							m = n - 1;
						}
					}
				}
				if (j == 0)
				{
					arrayOfString = new String[k];
				}
			}
			return arrayOfString;
		}

		private static bool IsAlphaChar(char paramChar)
		{
			return ((paramChar >= 'a') && (paramChar <= 'z')) || ((paramChar >= 'A') && (paramChar <= 'Z'));
		}

		private static bool IsNumChar(char paramChar)
		{
			return (paramChar >= '0') && (paramChar <= '9');
		}

		private static bool IsWhiteSpace(char paramChar)
		{
			return (paramChar == ' ') || (paramChar == '\t');
		}

		public static String GetAndString(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return "AND";
				case LogicSyntax.LogicSyntaxNu:
					return "n";
				case LogicSyntax.LogicSyntaxUpDown:
					return "^";
				case LogicSyntax.LogicSyntaxSlashes:
					return "/\\";
				case LogicSyntax.LogicSyntaxDotPlus:
					return ".";
			}
			return ".";
		}

		public static String GetOrString(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return "OR";
				case (LogicSyntax) 2:
					return "u";
				case (LogicSyntax) 3:
					return "v";
				case (LogicSyntax) 4:
					return "\\/";
				case (LogicSyntax) 5:
					return "+";
			}
			return "+";
		}

		public static String GetXorString(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return "XOR";
			}
			return "!=";
		}

		public static String GetImpliesString(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return "IMPLIES";
			}
			return "=>";
		}

		public static String GetBiimpliesString(LogicSyntax paramInt)
		{
			switch (paramInt)
			{
				case LogicSyntax.Written:
					return "BIIMPLIES";
			}
			return "<=>";
		}
	}

	internal enum BoolValue
	{
		BoolInvalid = 20,
		BoolFalse = 21,
		BoolTrue = 22,
	}
}