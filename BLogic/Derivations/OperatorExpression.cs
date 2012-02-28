using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BLogic.Derivations
{
	public class OperatorExpression : LogicExpression
	{
		private LogicExpression[] _logicBranches;

		public OperatorExpression(Operator @operator, bool negated = false)
			: base(negated)
		{
			Operator = @operator;
		}

		public Operator Operator { get; set; }

		public override bool Evaluate(Dictionary<String, Mutablebool> paramDictionary)
		{
			switch (Operator)
			{
				case Operator.And:
					return !_logicBranches.Any(t => !t.Evaluate(paramDictionary));
				case Operator.Or:
					return _logicBranches.Any(t => t.Evaluate(paramDictionary));
				case Operator.OperatorImplies:
					return (!_logicBranches[0].Evaluate(paramDictionary)) || (_logicBranches[1].Evaluate(paramDictionary));
				case Operator.OperatorBiimplies:
					return _logicBranches[0].Evaluate(paramDictionary) == _logicBranches[1].Evaluate(paramDictionary);
				case Operator.OperatorXor:
					return _logicBranches[0].Evaluate(paramDictionary) != _logicBranches[1].Evaluate(paramDictionary);
			}
			Console.Error.WriteLine("Software Error: Unknown operator");
			return false;
		}

		public override void GetLeafs(HashSet<String> paramLinkedHashSet)
		{
			foreach (LogicExpression t in _logicBranches)
				t.GetLeafs(paramLinkedHashSet);
		}

		public override int GetDepth(int depth = 0)
		{
			depth++;

			int i = depth;

			foreach (LogicExpression t in _logicBranches)
			{
				int j;
				if ((j = t.GetDepth(depth)) > i)
				{
					i = j;
				}
			}
			return i;
		}

		public override int GetNumberOfElements(int paramInt = 0)
		{
			return _logicBranches.Aggregate(paramInt, (current, t) => t.GetNumberOfElements(current));
		}

		public bool IsNormalForm()
		{
			if (Negated)
			{
				return false;
			}

			if ((Operator != Operator.And) && (Operator != Operator.Or))
			{
				return false;
			}

			return _logicBranches.All(t => t.GetLogicalForm() != 0);
		}

// ReSharper disable InconsistentNaming
		public LogicalForm IsCNFOrDNF()
		{
			if (_logicBranches.OfType<OperatorExpression>().Any(t => (t).Operator == Operator))
			{
				return LogicalForm.LogicNnf;
			}

			return (Operator == Operator.And ? LogicalForm.LogicCNF : LogicalForm.LogicDNF);
		}
		// ReSharper restore InconsistentNaming

		public override LogicExpression GetSubExpression(int paramInt1, int paramInt2, MutableInteger paramMutableInteger)
		{
			int i = GetDepth();

			if ((i == 1) && (paramInt1 == 1))
			{
				if (paramMutableInteger.Value > paramInt2)
				{
					return this;
				}

				paramMutableInteger.Value += 1;
				return null;
			}

			if (paramInt1 == i)
			{
				if (paramMutableInteger.Value > paramInt2)
				{
					return this;
				}

				paramMutableInteger.Value += 1;
				return null;
			}

			return
				_logicBranches.Select(t => t.GetSubExpression(paramInt1, paramInt2, paramMutableInteger)).FirstOrDefault(
					localLogicExpression => localLogicExpression != null);
		}

		public LogicExpression[] Branches
		{
			get { return _logicBranches; }
			set
			{
				_logicBranches = value;

				if ((value.Length != 2) && (Operator != Operator.And) && (Operator != Operator.Or))
				{
					Console.Error.WriteLine("Software Error: Invalid number of operators");
				}

				for (int i = 0; i < value.Length; i++)
					value[i].SetParent(this, i);
			}
		}

		public void SetBranch(LogicExpression paramLogicExpression, int paramInt)
		{
			_logicBranches[paramInt] = paramLogicExpression;
			paramLogicExpression.SetParent(this, paramInt);
		}

		public override bool HasNegatedBranches()
		{
			return Negated || _logicBranches.Any(t => t.HasNegatedBranches());
		}

		public override void ToString(StringBuilder paramStringBuilder, LogicSyntax paramInt1, NegationSyntax paramInt2)
		{
			String str = GetOperatorString(paramInt1, Operator);

			if (Negated)
			{
				if (paramInt2 == NegationSyntax.Written)
					paramStringBuilder.Append("NOT ");
				else if (paramInt2 == NegationSyntax.Before)
				{
					paramStringBuilder.Append("~");
				}
			}
			paramStringBuilder.Append("(");
			for (int i = 0; i < _logicBranches.Length; i++)
			{
				if (i > 0)
				{
					paramStringBuilder.Append(" ");
					paramStringBuilder.Append(str);
					paramStringBuilder.Append(" ");
				}

				_logicBranches[i].ToString(paramStringBuilder, paramInt1, paramInt2);
			}

			paramStringBuilder.Append(")");
			if ((Negated) && (paramInt2 == NegationSyntax.After))
				paramStringBuilder.Append("'");
		}

		private String GetOperatorString(LogicSyntax paramInt1, Operator paramInt2)
		{
			switch (paramInt2)
			{
				case Operator.And:
					return LogicParser.GetAndString(paramInt1);
				case Operator.Or:
					return LogicParser.GetOrString(paramInt1);
				case Operator.OperatorImplies:
					return LogicParser.GetImpliesString(paramInt1);
				case Operator.OperatorBiimplies:
					return LogicParser.GetBiimpliesString(paramInt1);
				case Operator.OperatorXor:
					return LogicParser.GetXorString(paramInt1);
			}
			Console.Error.WriteLine("Software Error: Unknown operator passed");
			return LogicParser.GetAndString(paramInt1);
		}

		public override LogicExpression Clone()
		{
			var localLogicBranch = new OperatorExpression(Operator, Negated);

			var arrayOfLogicExpression = new LogicExpression[_logicBranches.Length];

			for (int i = 0; i < _logicBranches.Length; i++)
			{
				arrayOfLogicExpression[i] = _logicBranches[i].Clone();
			}
			localLogicBranch.Branches = arrayOfLogicExpression;
			return localLogicBranch;
		}
	}

	public enum Operator
	{
		NonOperator = -1,
		And = 0,
		Or = 1,
		OperatorImplies = 2,
		OperatorBiimplies = 3,
		OperatorXor = 4
	}
}

/* Location:           D:\Downloads\boolLogicApplet.jar
 * Qualified Name:     com.izyt.boolLogic.LogicBranch
 * JD-Core Version:    0.6.0
 */