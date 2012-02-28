using System;
using BLogic.Derivations;
using FluentAssertions;
using NUnit.Framework;

namespace BLogic.Tests.Derivations
{
	[TestFixture]
	public class CarryOutDeMorganTest
	{
		[Test]
		public void Test()
		{
			ParsedExpression expression = LogicParser.ParseLogic("not(a and b)");

			LogicDerivation localLogicDerivation = LogicHandler.LocalLogicDerivation(expression);
			localLogicDerivation.CNF.ToString().Should().Be("(a' + b')");
			localLogicDerivation.DNF.ToString().Should().Be("(a' + b')");
		}
	}
}