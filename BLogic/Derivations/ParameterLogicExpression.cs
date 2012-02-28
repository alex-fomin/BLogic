#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BLogic.Derivations
{
	internal class ParameterLogicExpression : LogicExpression
	{
		public ParameterLogicExpression(string name, bool negated = false) : base(negated)
		{
			Name = name;
		}

		public override bool Evaluate(Dictionary<String, Mutablebool> paramDictionary)
		{
			bool @bool = (paramDictionary[Name]).value;

			return Negated ? !@bool : @bool;
		}

		public override void GetLeafs(HashSet<String> paramLinkedHashSet)
		{
			paramLinkedHashSet.Add(Name);
		}

		public string Name { get; private set; }

		public override void ToString(StringBuilder paramStringBuilder, LogicSyntax paramInt1, NegationSyntax paramInt2)
		{
			if (Negated)
			{
				switch (paramInt2)
				{
					case NegationSyntax.Written:
						paramStringBuilder.Append("NOT " + Name);
						break;
					case NegationSyntax.Before:
						paramStringBuilder.Append("~" + Name);
						break;
					case NegationSyntax.After:
						paramStringBuilder.Append(Name + "'");
						break;
					default:
						Console.Error.WriteLine("Software Error: Unknown negationSyntax passed: " + paramInt2);
						break;
				}
			}
			else
				paramStringBuilder.Append(Name);
		}

		public override LogicExpression Clone()
		{
			return new ParameterLogicExpression(Name, Negated);
		}

		public override bool Equals(LogicExpression paramLogicExpression)
		{
			return ((paramLogicExpression is ParameterLogicExpression)) && (((ParameterLogicExpression) paramLogicExpression).Name.Equals(Name)) &&
			       (paramLogicExpression.Negated == Negated);
		}
	}
}
