using System;
using System.Linq.Expressions;
using BLogic.Derivations;

namespace BLogic.Expressions
{
	public class ExpressionTranslator : ExpressionVisitor
	{
		public static LogicExpression Convert(LambdaExpression e)
		{
			return Convert(e.Body);
		}

		public static LogicExpression Convert(Expression e)
		{
			if (e is LambdaExpression)
				return Convert(e as LambdaExpression);
			var translator = new ExpressionTranslator();
			translator.Visit(e);
			return translator._expression;
		}

		private ExpressionTranslator()
		{
		}

		private LogicExpression _expression;

		protected override Expression VisitUnary(UnaryExpression node)
		{
			Visit(node.Operand);
			if (node.NodeType == ExpressionType.Not)
			{
				_expression.Negated = !_expression.Negated;
			}
			else
			{
				throw new NotSupportedException(node.ToString());
			}
			return node;
		}

		protected override Expression VisitConstant(ConstantExpression node)
		{
			if (node.Value is bool)
			{
				var value = (bool) node.Value;
				_expression = new LogicValue(value);
			}
			else
			{
				throw new NotSupportedException(node.ToString());
			}
			return node;
		}

		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (node.Type != typeof(bool))
				throw new NotSupportedException(node.ToString());

			_expression = new ParameterLogicExpression(node.Name);
			return node;
		}

		protected override Expression VisitBinary(BinaryExpression node)
		{
			Visit(node.Left);
			var left = _expression;
			Visit(node.Right);
			var right = _expression;

			Operator @operator;
			switch (node.NodeType)
			{
				case ExpressionType.And:
				case ExpressionType.AndAlso:
					@operator = Operator.And;
					break;
				case ExpressionType.Or:
				case ExpressionType.OrElse:
					@operator = Operator.Or;
					break;
				case ExpressionType.ExclusiveOr:
					@operator = Operator.OperatorXor;
					break;
				default:
					throw new NotSupportedException(node.ToString());
			}

			_expression = new OperatorExpression(@operator) { Branches = new[]{left,right}};

			
			
			return node;

		}
	}
}