#region

using System;
using System.Collections.Generic;
using System.Text;

#endregion

namespace BLogic.Derivations
{
	public abstract class LogicExpression
	{
		private OperatorExpression _parent;

		private int _positionInParent = -1;

		protected LogicExpression(bool negated=false)
		{
			Negated = negated;
		}

		public OperatorExpression Parent
		{
			get { return _parent; }
		}

		public bool Negated { get; set; }

		public void SetParent(OperatorExpression paramOperatorExpression, int paramInt)
		{
			_parent = paramOperatorExpression;
			_positionInParent = paramInt;
		}

		public int GetPositionInParent()
		{
			return _positionInParent;
		}


		public LogicalForm GetLogicalForm()
		{
			var logicBranch = this as OperatorExpression;
			if (logicBranch != null)
			{
				if (!logicBranch.IsNormalForm())
				{
					return 0;
				}
				int i = GetDepth();

				if (i != 2)
				{
					return i == 3 ? logicBranch.IsCNFOrDNF() : LogicalForm.LogicNnf;
				}
			}

			return LogicalForm.LogicCNF | LogicalForm.LogicDNF;
		}

		public LogicExpression GetSubExpression(int paramInt1, int paramInt2)
		{
			return GetSubExpression(paramInt1, paramInt2, new MutableInteger(1));
		}

		public virtual LogicExpression GetSubExpression(int paramInt1, int paramInt2, MutableInteger paramMutableInteger)
		{
			if ((paramInt1 == 1) && (GetDepth() == 1))
			{
				if (paramMutableInteger.Value > paramInt2)
				{
					return this;
				}

				paramMutableInteger.Value += 1;
			}

			return null;
		}

		public override String ToString()
		{
			return ToString(LogicSyntax.LogicSyntaxDotPlus, NegationSyntax.After);
		}

		public String ToString(LogicSyntax paramInt1, NegationSyntax paramInt2)
		{
			var localStringBuilder = new StringBuilder();

			ToString(localStringBuilder, paramInt1, paramInt2);

			return localStringBuilder.ToString();
		}

		public virtual bool Equals(LogicExpression paramLogicExpression)
		{
			return false;
		}

		public virtual void GetLeafs(HashSet<string> paramLinkedHashSet)
		{
		}

		public virtual int GetDepth(int depth = 0)
		{
			return depth + 1;
		}

		public virtual int GetNumberOfElements(int paramInt = 0)
		{
			return paramInt + 1;
		}

		public virtual bool HasNegatedBranches()
		{
			return false;
		}

		public abstract void ToString(StringBuilder paramStringBuilder, LogicSyntax paramInt1, NegationSyntax paramInt2);

		public abstract bool Evaluate(Dictionary<String, Mutablebool> paramDictionary);

		public abstract LogicExpression Clone();
	}

	[Flags]
	public enum LogicalForm
	{
		LogicUnclassified = 0,
		LogicNnf = 1,
		LogicCNF = 2,
		LogicDNF = 4,
	}
}
