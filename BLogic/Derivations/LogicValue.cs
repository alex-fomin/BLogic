#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BLogic.Derivations
{
	internal class LogicValue : LogicExpression
	{
		private readonly bool _value;
		public LogicValue(bool value, bool negated = false)
			: base(negated)
		{
			_value = value;
		}


		public bool EqualsTrue()
		{
			return Negated != _value;
		}

		public override void ToString(StringBuilder paramStringBuilder, LogicSyntax paramInt1, NegationSyntax paramInt2)
		{
			if (Negated)
			{
				switch (paramInt2)
				{
					case NegationSyntax.Written:
						paramStringBuilder.Append("NOT " + _value);
						break;
					case NegationSyntax.Before:
						paramStringBuilder.Append("~" + _value);
						break;
					case NegationSyntax.After:
						paramStringBuilder.Append(_value + "'");
						break;
					default:
						Console.Error.WriteLine("Software Error: Unknown negationSyntax passed: " + paramInt2);
						break;
				}
			}
			else
				paramStringBuilder.Append(_value);
		}

		public override bool Evaluate(Dictionary<String, Mutablebool> paramDictionary)
		{
			return Negated != _value;
		}

		public override LogicExpression Clone()
		{
			return new LogicValue(_value, Negated);
		}

		public override bool Equals(LogicExpression paramLogicExpression)
		{
			var logicValue = paramLogicExpression as LogicValue;
			if (logicValue == null)
				return false;
			return logicValue.EqualsTrue() == EqualsTrue();
		}
	}
}