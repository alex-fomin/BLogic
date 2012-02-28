using System;
using System.Collections;
using System.Linq.Expressions;
using BLogic.Derivations;
using BLogic.Expressions;
using NUnit.Framework;

namespace BLogic.Tests
{
	[TestFixture]
	public class ExpressionConverterTest
	{
		[Test, TestCaseSource(typeof(ExpressionConverterTest),"TestData")]
		public string ParseTest(Expression actual)
		{
			LogicExpression logicExpression = ExpressionTranslator.Convert(actual);
			return logicExpression.ToString();
		}

// ReSharper disable UnusedMember.Global
		public static IEnumerable TestData()
// ReSharper restore UnusedMember.Global
		{
			yield return TestData((a, b) => a && b, "(a . b)");
			yield return TestData((a, b) => !(a && b), "(a . b)'");
			yield return TestData((a, b) => (a && b) && a, "((a . b) . a)");
			yield return TestData((a, b) => a && (b && a), "(a . (b . a))");
			yield return TestData((a, b) => a || true, "(a + True)");
		}

		private static TestCaseData TestData(Expression<Func<bool, bool, bool>> expression, string aB)
		{
			return new TestCaseData(E(expression)).Returns(aB);
		}


		private static 
			Expression<Func<bool, bool, bool>> E
		   (Expression<Func<bool, bool, bool>> func)
		{
			return func;
		}
	}
}
