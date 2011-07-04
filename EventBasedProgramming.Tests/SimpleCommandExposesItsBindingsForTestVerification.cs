using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NUnit.Framework;

namespace EventBasedProgramming.Tests
{
	[TestFixture]
	public class SimpleCommandExposesItsBindingsForTestVerification
	{
		[Test]
		public void CanVerifyWithDisposesToConstraint()
		{
			_testSubject = new SimpleCommand(_CorrectCanExecuteInstanceMethod, _CorrectExecuteInstanceMethod);
			Assert.That(_testSubject, Command.DelegatesTo(() => _CorrectCanExecuteInstanceMethod(), () => _CorrectExecuteInstanceMethod()));
		}

		[Test]
		public void CanVerifyWhenIsBoundToInstanceMethods()
		{
			_testSubject = new SimpleCommand(_CorrectCanExecuteInstanceMethod, _CorrectExecuteInstanceMethod);
			Assert.That(_testSubject.MethodHandlingCanExecute, Calls.To(() => _CorrectCanExecuteInstanceMethod()));
			Assert.That(_testSubject.MethodHandlingExecute, Calls.To(() => _CorrectExecuteInstanceMethod()));
		}

		[Test]
		public void AssertionFiresWhenBindingIsNotAsExpected()
		{
			var other = new SimpleCommandExposesItsBindingsForTestVerification();
			_testSubject = new SimpleCommand(_CorrectCanExecuteInstanceMethod, other._CorrectExecuteInstanceMethod);
			var actualBinding = Extract.BindingInfoFrom(() => other._CorrectExecuteInstanceMethod()).ToString();
			_AssertionShouldFail(() => _CorrectExecuteStaticMethod(), "bound to incorrect method", actualBinding);
			_AssertionShouldFail(() => _CorrectExecuteInstanceMethod(), "bound to incorrect object instance", actualBinding);
		}

		[Test]
		public void CanVerifyWhenIsBoundToInstanceMethodsOnAnotherObject()
		{
			var other = new SimpleCommandExposesItsBindingsForTestVerification();
			_testSubject = new SimpleCommand(other._CorrectCanExecuteInstanceMethod, other._CorrectExecuteInstanceMethod);
			Assert.That(_testSubject.MethodHandlingCanExecute, Calls.To(() => other._CorrectCanExecuteInstanceMethod()));
			Assert.That(_testSubject.MethodHandlingExecute, Calls.To(() => other._CorrectExecuteInstanceMethod()));
		}

		[Test]
		public void CanVerifyWhenIsBoundToStaticMethods()
		{
			_testSubject = new SimpleCommand(_CorrectCanExecuteStaticMethod, _CorrectExecuteStaticMethod);
			Assert.That(_testSubject.MethodHandlingCanExecute, Calls.To(() => _CorrectCanExecuteStaticMethod()));
			Assert.That(_testSubject.MethodHandlingExecute, Calls.To(() => _CorrectExecuteStaticMethod()));
		}

		private void _AssertionShouldFail([NotNull] Expression<Action> assertionToMake, string expectedMessage,
			string actualValue)
		{
			Assert.That(
				Assert.Throws<AssertionException>(
					() => Assert.That(_testSubject.MethodHandlingExecute, Calls.To(assertionToMake))),
				Has.Property("Message").EqualTo(string.Format(@"  {0}.
  Expected: calling <{1}>
  But was:  <{2}>
", expectedMessage,
					Extract.BindingInfoFrom(assertionToMake), actualValue)));
		}

		private bool _CorrectCanExecuteInstanceMethod()
		{
			Assert.Fail("The test should not need to call the method just to verify the binding.");
			_testSubject.CanExecute(null);
			return false;
		}

		private static bool _CorrectCanExecuteStaticMethod()
		{
			Assert.Fail("The test should not need to call the method just to verify the binding.");
			return false;
		}

		private void _CorrectExecuteInstanceMethod()
		{
			Assert.Fail("The test should not need to call the method just to verify the binding.");
			_testSubject.Execute(null);
		}

		private static void _CorrectExecuteStaticMethod()
		{
			Assert.Fail("The test should not need to call the method just to verify the binding.");
		}

		private SimpleCommand _testSubject;
	}
}
