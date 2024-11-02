using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NUnit.Framework;

namespace HomeExercises
{
    public class NumberValidatorTests
    {
        [Test]
        public void NumberValidator_Throws_WhenNegativePrecision()
        {
            Action action = () => new NumberValidator(-1, 2, true);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void NumberValidator_Throws_WhenNegativeScale()
        {
            Action action = () => new NumberValidator(2, -1, true);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void NumberValidator_Throws_WhenScaleGreaterThanPrecision()
        {
            Action action = () => new NumberValidator(2, 3, true);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void NumberValidator_Throws_WhenScaleEqualPrecision()
        {
            Action action = () => new NumberValidator(2, 2, true);
            action.Should().Throw<ArgumentException>();
        }

        [Test]
        public void NumberValidator_DontThrows_WhenPrecisionIsPositive()
        {
            Action action = () => new NumberValidator(2, 1, true);
            action.Should().NotThrow<Exception>();
        }

        [Test]
        public void NumberValidator_DontThrows_WhenScaleLessThanPrecision()
        {
            Action action = () => new NumberValidator(2, 1, true);
            action.Should().NotThrow<Exception>();
        }

        [TestCase(1, true, "1")]
        [TestCase(2, true, "21")]
        [TestCase(3, true, "13")]
        [TestCase(5, true, "514")]
        [TestCase(10, false, "-314")]
        [TestCase(2, false, "-3")]
        [TestCase(3, false, "-99")]
        public void NumberValidator_ReturnsTrue_WithValidIntegerNumbers_WithScaleZero(int precision, bool onlyPositive,
            string number) => TestWithValidParameters(precision, 0, onlyPositive, number);

        [TestCase(1, true, "11")]
        [TestCase(2, true, "001")]
        [TestCase(3, true, "4134124")]
        [TestCase(10, false, "-031442424324243")]
        [TestCase(2, false, "-13")]
        [TestCase(3, false, "-993")]
        public void NumberValidator_ReturnsFalse_WithInvalidIntegerNumbers_WithScaleZero(int precision,
            bool onlyPositive, string number) => TestWithInvalidParameters(precision, 0, onlyPositive, number);

        [TestCase(2, 1, true, "1.1")]
        [TestCase(4, 3, true, "2.21")]
        [TestCase(4, 3, true, "0.000")]
        [TestCase(4, 3, true, "00.00")]
        [TestCase(5, 1, true, "51.4")]
        [TestCase(10, 5, false, "-31.414")]
        [TestCase(3, 1, false, "-3.2")]
        [TestCase(2, 1, false, "-1")]
        public void NumberValidator_ReturnsTrue_WithValidFloatNumbers(int precision, int scale, bool onlyPositive,
            string number) => TestWithValidParameters(precision, scale, onlyPositive, number);

        [TestCase(2, 1, true, "1.12")]
        [TestCase(4, 3, true, "2.2112")]
        [TestCase(5, 1, true, "51.43")]
        [TestCase(10, 5, false, "-31.41431231")]
        [TestCase(3, 1, false, "-3.21")]
        [TestCase(2, 1, false, "-1.1")]
        public void NumberValidator_ReturnsFalse_WithInvalidFloatNumbers(int precision, int scale, bool onlyPositive,
            string number) => TestWithInvalidParameters(precision, scale, onlyPositive, number);

        [TestCase("a")]
        [TestCase("seven")]
        [TestCase("one.five")]
        [TestCase("   ")]
        [TestCase("")]
        [TestCase(null)]
        public void NumberValidator_ReturnsFalse_WithNonNumber(string number)
            => TestWithInvalidParameters(5, 4, false, number);

        [TestCase("IV")]
        [TestCase("1 . 1")]
        [TestCase("1. 1")]
        [TestCase("1 .1")]
        [TestCase("10_000")]
        [TestCase("10 000")]
        [TestCase("10.")]
        [TestCase(".1")]
        [TestCase("+.1")]
        [TestCase("-.1")]
        [TestCase("5*3")]
        public void NumberValidator_ReturnsFalse_WithWrongFormat(string number)
            => TestWithInvalidParameters(5, 4, false, number);

        [TestCase("1,1")]
        [TestCase("1.1")]
        [TestCase("11")]
        public void NumberValidator_CorrectWork_WithCorrectFormat(string number)
            => TestWithValidParameters(5, 4, false, number);

        [TestCase("+11")]
        [TestCase("+1111")]
        [TestCase("+1.111")]
        [TestCase("-1111")]
        [TestCase("-1.111")]
        [TestCase("-1.1")]
        [TestCase("-11")]
        [TestCase("+1.1")]
        [TestCase("1.1")]
        [TestCase("11")]
        public void NumberValidator_CorrectWork_WithAndWithoutSign(string number)
            => TestWithValidParameters(5, 3, false, number);

        private static void TestWithValidParameters(int precision, int scale, bool onlyPositive,
            string number)
        {
            var validator = new NumberValidator(precision, scale, onlyPositive);
            validator.IsValidNumber(number).Should().BeTrue();
        }

        private static void TestWithInvalidParameters(int precision, int scale, bool onlyPositive,
            string number)
        {
            var validator = new NumberValidator(precision, scale, onlyPositive);
            validator.IsValidNumber(number).Should().BeFalse();
        }
    }

    public class NumberValidator
    {
        private readonly Regex numberRegex;
        private readonly bool onlyPositive;
        private readonly int precision;
        private readonly int scale;

        public NumberValidator(int precision, int scale = 0, bool onlyPositive = false)
        {
            this.precision = precision;
            this.scale = scale;
            this.onlyPositive = onlyPositive;
            if (precision <= 0)
                throw new ArgumentException("precision must be a positive number");
            if (scale < 0 || scale >= precision)
                throw new ArgumentException("scale must be a non-negative number less or equal than precision");
            numberRegex = new Regex(@"^([+-]?)(\d+)([.,](\d+))?$", RegexOptions.IgnoreCase);
        }

        public bool IsValidNumber(string value)
        {
            // Проверяем соответствие входного значения формату N(m,k), в соответствии с правилом, 
            // описанным в Формате описи документов, направляемых в налоговый орган в электронном виде по телекоммуникационным каналам связи:
            // Формат числового значения указывается в виде N(m.к), где m – максимальное количество знаков в числе, включая знак (для отрицательного числа), 
            // целую и дробную часть числа без разделяющей десятичной точки, k – максимальное число знаков дробной части числа. 
            // Если число знаков дробной части числа равно 0 (т.е. число целое), то формат числового значения имеет вид N(m).

            if (string.IsNullOrEmpty(value))
                return false;

            var match = numberRegex.Match(value);
            if (!match.Success)
                return false;

            // Знак и целая часть
            var intPart = match.Groups[1].Value.Length + match.Groups[2].Value.Length;
            // Дробная часть
            var fracPart = match.Groups[4].Value.Length;

            if (intPart + fracPart > precision || fracPart > scale)
                return false;

            return !onlyPositive || match.Groups[1].Value != "-";
        }
    }
}