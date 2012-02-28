using System;
using BLogic.Exceprions;

namespace BLogic.Derivations
{
	public class ParserSyntax
	{
		public ParserSyntax(LogicSyntax paramInt1 = LogicSyntax.Unknown, NegationSyntax paramInt2=NegationSyntax.Unknown)
		{
			LogicSyntax = paramInt1;
			NegationSyntax = paramInt2;
		}

		public LogicSyntax LogicSyntax { get; private set; }
		public NegationSyntax NegationSyntax { get; private set; }

		public void UpdateLogicSyntax(LogicSyntax paramInt)
		{
			NegationSyntax negationSyntax = LogicParser.GetNegationSyntax(paramInt);

			if ((LogicSyntax    == LogicSyntax.Unknown || (LogicSyntax == paramInt)) &&
			    (NegationSyntax == NegationSyntax.Unknown || (negationSyntax == NegationSyntax)))
			{
				LogicSyntax = paramInt;
				NegationSyntax = negationSyntax;
			}
			else
			{
				throw new DifferentSyntaxesUsedException("Change in logic syntax");
			}
		}

		public void UpdateNegationSyntax(NegationSyntax negationSyntax)
		{
			if ((NegationSyntax == NegationSyntax.Unknown || (NegationSyntax == negationSyntax)) &&
			    (LogicSyntax == LogicSyntax.Unknown || (LogicParser.GetNegationSyntax(LogicSyntax) == negationSyntax)))
			{
				NegationSyntax = negationSyntax;
			}
			else
			{
				throw new DifferentSyntaxesUsedException("Change in logic syntax");
			}
		}

		public LogicExpression GenerateLiteral(String paramString)
		{
			if (NegationSyntax == NegationSyntax.Unknown)
			{
				if (paramString[0] == '~')
					UpdateNegationSyntax(NegationSyntax.Before);
				else if (paramString[paramString.Length - 1] == '\'')
				{
					UpdateNegationSyntax(NegationSyntax.After);
				}
			}
			bool negated = false;
			int j;
			if (NegationSyntax == NegationSyntax.Before)
			{
				if (paramString[paramString.Length - 1] == '\'')
				{
					throw new DifferentSyntaxesUsedException("Incorrect negation");
				}

				for (j = 0; (j < paramString.Length) && (paramString[j] == '~'); j++)
				{
					negated = !negated;
				}
				if ((j == paramString.Length - 1) && (paramString[j] == '~'))
				{
					throw new UnexpectedSymbolException("Negator(s) without literal");
				}
			}
			else if (NegationSyntax == NegationSyntax.After)
			{
				if (paramString[0] == '~')
				{
					throw new DifferentSyntaxesUsedException("Incorrect negation");
				}

				for (j = paramString.Length - 1; (j >= 0) && (paramString[j] == '\''); j--)
				{
					negated = !negated;
				}
				if ((j == 0) && (paramString[0] == '\''))
				{
					throw new UnexpectedSymbolException("Negator(s) without literal");
				}
			}

			LogicExpression localLogicExpression = LogicParser.GenerateLiteral(paramString);
			localLogicExpression.Negated = negated;
			return localLogicExpression;
		}
	}

	public enum NegationSyntax
	{
		Unknown = 0,
		Written = 11,
		Before = 12,
		After = 13,
	}

	public enum LogicSyntax
	{
		Invalid = -1,
		Unknown = 0,
		Written = 1,
		LogicSyntaxNu = 2,
		LogicSyntaxUpDown = 3,
		LogicSyntaxSlashes = 4,
		LogicSyntaxDotPlus = 5,

	}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.ParserSyntax
 * JD-Core Version:    0.6.0
 */
}