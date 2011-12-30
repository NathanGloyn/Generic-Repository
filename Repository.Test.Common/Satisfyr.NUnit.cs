/*
Microsoft Public License (Ms-PL)

This license governs use of the accompanying software. If you use the software, you accept this license. If you do not accept the license, do not use the software.

1. Definitions

The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same meaning here as under U.S. copyright law.

A "contribution" is the original software, or any additions or changes to the software.

A "contributor" is any person that distributes its contribution under this license.

"Licensed patents" are a contributor's patent claims that read directly on its contribution.

2. Grant of Rights

(A) Copyright Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free copyright license to reproduce its contribution, prepare derivative works of its contribution, and distribute its contribution or any derivative works that you create.

(B) Patent Grant- Subject to the terms of this license, including the license conditions and limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or otherwise dispose of its contribution in the software or derivative works of the contribution in the software.

3. Conditions and Limitations

(A) No Trademark License- This license does not grant you rights to use any contributors' name, logo, or trademarks.

(B) If you bring a patent claim against any contributor over patents that you claim are infringed by the software, your patent license from such contributor to the software ends automatically.

(C) If you distribute any portion of the software, you must retain all copyright, patent, trademark, and attribution notices that are present in the software.

(D) If you distribute any portion of the software in source code form, you may do so only under this license by including a complete copy of this license with your distribution. If you distribute any portion of the software in compiled or object code form, you may only do so under a license that complies with this license.

(E) The software is licensed "as-is." You bear the risk of using it. The contributors give no express warranties, guarantees or conditions. You may have additional consumer rights under your local laws which this license cannot change. To the extent permitted under your local laws, the contributors exclude the implied warranties of merchantability, fitness for a particular purpose and non-infringement.
*/
// In order for this alias to compile, you must set the "Aliases" property 
// for the reference (from the Properties Window) to "global, NUnitAlias"
extern alias NUnitAlias;
/*
 *	All the code in this file was developed by Fabio Maulo for the Sharp Tests Ex library (http://sharptestex.codeplex.com/)
 *	All I did was reduce the API surface to just the Satisfies extension method, deleted everything else 
 *	that was not in used by that code path, and made everything a single file for much easier reuse 
 *	and to avoid taking yet another dependency on an external binary.
 *	
 *	So, if there are bugs, please report them to Fabio :)
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Satisfyr
{
    /// <summary>
    /// Provides the Satisfes extension method over any object, which allows to perform asserts using plain .NET code.
    /// </summary>
    /// <example>
    /// The following example verifies a couple conditions on a list:
    /// <code>
    /// List&lt;Customer&gt; customers = GetCustomers();
    /// customers.Satisfies(c =&gt; c != null && c.Count != 0 && c[0] == expectedCustomer);
    /// </code>
    /// The following example validates that a result code is within a given range:
    /// <code>
    /// int status = GetStatus();
    /// status.Satisfies(s =&gt; s &gt;= 0 && s &lt;= 5);
    /// </code>
    /// </example>
    ///	<version>1.0.0.0</version>
    /// <authored-by>Fabio Maulo</authored-by>
    /// <forked-by>Daniel Cazzulino</forked-by>
    [DebuggerStepThrough]
    public static class Extensions
    {
        /// <summary>
        /// Checks that the given actual value satisfies the expression, optionally specifying the 
        ///	failure message to use in the assertion.
        /// </summary>
        public static void Satisfies<T>(this T actual, Expression<Func<T, bool>> assertion)
        {
            Satisfies(actual, null, assertion);
        }

        /// <summary>
        /// Checks that the given actual value satisfies the expression, optionally specifying the 
        ///	failure message to use in the assertion.
        /// </summary>
        public static void Satisfies<T>(this T actual, string failureMessage, Expression<Func<T, bool>> assertion)
        {
            var a = new SatisfyAssertion<T>(assertion);
            a.Assert(actual, failureMessage);
        }

        #region Implementation

        private static class AssertExceptionFactory
        {
            public static Exception CreateException(string message)
            {
                return new NUnitAlias::NUnit.Framework.AssertionException(message);
            }
        }

        #region Extension Methods

        private static string DisplayName(this Type source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }
            var builder = new StringBuilder(100);
            builder.Append(source.Name.Split('`').First());
            if (source.IsGenericType)
            {
                builder.Append("<");
                builder.Append(source.GetGenericArguments().Select(t => t.Name).AsCommaSeparatedValues());
                builder.Append(">");
            }
            return builder.ToString();
        }

        private static string[] Lines(this string source)
        {
            return source.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
        }

        private static string AsCommaSeparatedValues(this IEnumerable<string> source)
        {
            if (source == null)
            {
                return string.Empty;
            }
            var result = new StringBuilder(100);
            bool appendComma = false;
            foreach (var value in source)
            {
                if (appendComma)
                {
                    result.Append(", ");
                }
                result.Append(value);
                appendComma = true;
            }
            return result.ToString();
        }

        /// <summary>
        /// Find the first position where two sequence differ
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of the input sequences.</typeparam>
        /// <param name="first">An <see cref="IEnumerable{T}"/> to compare to second</param>
        /// <param name="second">An <see cref="IEnumerable{T}"/> to compare to the first sequence. </param>
        /// <returns>The position of the first difference; otherwise -1 where the two sequences has the same sequence.</returns>
        private static int PositionOfFirstDifference<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return PositionOfFirstDifference(first, second, EqualityComparer<TSource>.Default);
        }

        private static int PositionOfFirstDifference<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second, IEqualityComparer<TSource> comparer)
        {
            if (first == null)
            {
                throw new ArgumentNullException("first");
            }
            if (second == null)
            {
                throw new ArgumentNullException("second");
            }
            if (comparer == null)
            {
                throw new ArgumentNullException("comparer");
            }

            int diffPos = -1;
            using (IEnumerator<TSource> firstEnumerator = first.GetEnumerator())
            {
                using (IEnumerator<TSource> secondEnumerator = second.GetEnumerator())
                {
                    while (firstEnumerator.MoveNext())
                    {
                        diffPos++;
                        if (!secondEnumerator.MoveNext() || !comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                        {
                            return diffPos;
                        }
                    }
                    if (secondEnumerator.MoveNext())
                    {
                        return diffPos == -1 ? 0 : ++diffPos;
                    }
                }
            }
            return -1;
        }

        #endregion

        #region Interfaces

        private interface IAssertion<TActual>
        {
            void Assert(TActual actual, string customMessage);
        }

        private interface IMessageComposer<TA>
        {
            string GetMessage(TA actual, string customMessage);
        }

        private interface IAssertionMatcher<TActual>
        {
            Func<TActual, bool> Matcher { get; }
        }

        private interface IFailureMagnifier
        {
            string Message();
        }

        #endregion

        #region Assertions

        private class SatisfyAssertion<TA> : IAssertion<TA>
        {
            private readonly Expression<Func<TA, bool>> assertionExpression;

            public SatisfyAssertion(Expression<Func<TA, bool>> expression)
            {
                assertionExpression = expression;
            }

            public void Assert(TA actual, string customMessage)
            {
                var ev = new ExpressionVisitor<TA>(actual, assertionExpression);
                var assertion = ev.Visit();
                assertion.Assert(actual, customMessage);
            }
        }

        private class ExpressionVisitor<TA>
        {
            private const string InvalidOperandsMessageTemplate = "The expression ({0}) is invalid; none of the operands includes the value under test.";
            private const string ConstantExpressionMessageTemplate = "The expression ({0}) is a constant; you may test something else.";

            private readonly TA actualValue;
            private readonly ParameterExpression actual;

            public ExpressionVisitor(TA actualValue, Expression<Func<TA, bool>> expression)
            {
                this.actualValue = actualValue;
                TestExpression = expression;
                actual = expression.Parameters.Single();
            }

            public Expression<Func<TA, bool>> TestExpression { get; private set; }

            public IAssertion<TA> Visit()
            {
                var result = Visit(TestExpression.Body);
                if (!ContainsActualParameter(TestExpression.Body))
                {
                    throw new InvalidOperationException(string.Format(InvalidOperandsMessageTemplate, new ExpressionStringBuilder(TestExpression.Body)));
                }
                return result;
            }

            private UnaryAssertion<TA> Visit(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                        return Visit(expression as BinaryExpression);
                    case ExpressionType.Constant:
                        throw new InvalidOperationException(string.Format(ConstantExpressionMessageTemplate, expression));
                    case ExpressionType.Not:
                        return Visit(expression as UnaryExpression);
                    case ExpressionType.MemberAccess:
                        return new ExpressionAssertion<TA>(Expression.Lambda<Func<TA, bool>>(expression, actual));
                    case ExpressionType.Call:
                        return Visit(expression as MethodCallExpression);
                    case ExpressionType.Parameter:
                        return new ExpressionAssertion<TA>(Expression.Lambda<Func<TA, bool>>(expression, actual));
                    default:
                        throw new ArgumentOutOfRangeException("expression");
                }
            }

            private bool ContainsActualParameter(Expression expression)
            {
                if (expression == null)
                {
                    return false;
                }
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.OrElse:
                    case ExpressionType.AndAlso:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.Modulo:
                        return GetExpressions(expression as BinaryExpression).Any(ContainsActualParameter);
                    case ExpressionType.Constant:
                        return false;
                    case ExpressionType.Not:
                        return ContainsActualParameter(((UnaryExpression)expression).Operand);
                    case ExpressionType.MemberAccess:
                        return ContainsActualParameter(((MemberExpression)expression).Expression);
                    case ExpressionType.Call:
                        return GetExpressions(expression as MethodCallExpression).Any(ContainsActualParameter);
                    case ExpressionType.Parameter:
                        return expression == actual;
                    default:
                        return false;
                }
            }

            private UnaryAssertion<TA> Visit(MethodCallExpression expression)
            {
                IFailureMagnifier magnifier;
                if (expression.Method.Name == "SequenceEqual" && expression.Method.DeclaringType == typeof(System.Linq.Enumerable))
                {
                    magnifier = GetSequenceEqualMagnifier(expression);
                }
                else
                {
                    magnifier = new EmptyMagnifier();
                }
                var lambda = Expression.Lambda<Func<TA, bool>>(expression, actual);
                return new ExpressionAssertion<TA>(lambda, new ExpressionMessageComposer<TA>(lambda, magnifier));
            }

            private IFailureMagnifier GetSequenceEqualMagnifier(MethodCallExpression expression)
            {
                Type genericType = expression.Method.GetGenericArguments().First();
                var concreteType = typeof(SameSequenceAsFailureMagnifier<>).MakeGenericType(genericType);
                return (IFailureMagnifier)Activator.CreateInstance(concreteType, expression.Arguments.Select(arg => GetAsValue(arg)).ToArray());
            }

            private IEnumerable<Expression> GetExpressions(MethodCallExpression methodCallExpression)
            {
                yield return methodCallExpression.Object;
                foreach (var argument in methodCallExpression.Arguments)
                {
                    yield return argument;
                }
            }

            private UnaryAssertion<TA> Visit(UnaryExpression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Not:
                        return new ExpressionAssertion<TA>(Expression.Lambda<Func<TA, bool>>(expression, actual));
                    default: throw new ArgumentOutOfRangeException("expression");
                }
            }

            private UnaryAssertion<TA> Visit(BinaryExpression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Equal:
                        return GetEqualOperatorGenericAssertion(expression);
                    case ExpressionType.NotEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.LessThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LessThanOrEqual:
                        var lambda = Expression.Lambda<Func<TA, bool>>(expression, actual);
                        if (expression.Left.Type.IsPrimitive || expression.Right.Type.IsPrimitive)
                            return new ExpressionAssertion<TA>(lambda, new ExpressionMessageComposer<TA>(lambda, new BinaryFailureMagnifier(expression, GetAsValue)));
                        else
                            return new ExpressionAssertion<TA>(lambda, new ExpressionMessageComposer<TA>(lambda));
                    case ExpressionType.AndAlso:
                        return new AndAssertion<TA>(Visit(expression.Left), Visit(expression.Right));
                    case ExpressionType.OrElse:
                        return new OrAssertion<TA>(Visit(expression.Left), Visit(expression.Right));
                    default:
                        throw new ArgumentOutOfRangeException("expression");
                }
            }

            private UnaryAssertion<TA> GetEqualOperatorGenericAssertion(BinaryExpression expression)
            {
                IFailureMagnifier magnifier = new EmptyMagnifier();
                if (expression.Left.Type == typeof(string) || expression.Right.Type == typeof(string))
                {
                    var left = GetAsValue(expression.Left);
                    var rigth = GetAsValue(expression.Right);
                    magnifier = new StringEqualityFailureMagnifier(left as string, rigth as string);
                }
                else if (expression.Left.Type.IsPrimitive || expression.Right.Type.IsPrimitive)
                {
                    magnifier = new BinaryFailureMagnifier(expression, GetAsValue);
                }

                var lambda = Expression.Lambda<Func<TA, bool>>(expression, actual);
                return new ExpressionAssertion<TA>(lambda, new ExpressionMessageComposer<TA>(lambda, magnifier));
            }

            private object GetAsValue(Expression expression)
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Constant:
                        return ((ConstantExpression)expression).Value;
                    case ExpressionType.Parameter:
                        return actualValue;
                    case ExpressionType.Call:
                    case ExpressionType.MemberAccess:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Multiply:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.Add:
                    case ExpressionType.Subtract:
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.New:
                        var unaryExpression = Expression.Convert(expression, expression.Type);
                        return Expression.Lambda(unaryExpression, actual).Compile().DynamicInvoke(actualValue);
                    default:
                        throw new ArgumentOutOfRangeException("expression");
                }
            }

            private IEnumerable<Expression> GetExpressions(BinaryExpression expression)
            {
                yield return expression.Left;
                yield return expression.Right;
            }
        }

        private class ExpressionAssertion<TA> : UnaryAssertion<TA>
        {
            private readonly Func<TA, bool> compiledMatcher;

            public ExpressionAssertion(Expression<Func<TA, bool>> expression)
                : this(expression, new ExpressionMessageComposer<TA>(expression)) { }

            public ExpressionAssertion(Expression<Func<TA, bool>> expression, IMessageComposer<TA> messageComposer)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }
                if (messageComposer == null)
                {
                    throw new ArgumentNullException("messageComposer");
                }
                MessageComposer = messageComposer;
                compiledMatcher = expression.Compile();
            }

            public IMessageComposer<TA> MessageComposer { get; set; }

            public override Func<TA, bool> Matcher
            {
                get { return compiledMatcher; }
            }

            public override string GetMessage(TA actual, string customMessage)
            {
                return MessageComposer.GetMessage(actual, customMessage);
            }
        }

        private class AndAssertion<TA> : BitwiseAssertion<TA>
        {
            public AndAssertion(UnaryAssertion<TA> left, UnaryAssertion<TA> right) : base("And", left, right) { }

            public override Func<TA, bool> Matcher
            {
                get { return a => Left.Matcher(a) && Right.Matcher(a); }
            }

            public override void Assert(TA actual, string customMessage)
            {
                // To prevent double execution of the matcher the this method does not use base.Assert nor base.GetMessage(TA,string)
                var leftMatch = Left.Matcher(actual);
                var rigthMatch = Right.Matcher(actual);
                if (leftMatch && rigthMatch)
                {
                    return;
                }
                throw AssertExceptionFactory.CreateException(GetMessage(leftMatch, rigthMatch, actual, customMessage));
            }

            private string GetMessage(bool leftMatch, bool rigthMatch, TA actual, string customMessage)
            {
                var sb = new StringBuilder(500);

                if (!leftMatch)
                {
                    sb.AppendLine(GetUnaryFailureMessage(Left, actual));
                }

                if (!leftMatch && !rigthMatch)
                {
                    sb.AppendLine(Operator);
                }

                var hasCustomMessage = string.IsNullOrEmpty(customMessage);
                if (!rigthMatch)
                {
                    if (hasCustomMessage)
                    {
                        sb.Append(GetUnaryFailureMessage(Right, actual));
                    }
                    else
                    {
                        sb.AppendLine(string.Format("{0}", Right.GetMessage(actual, string.Empty)));
                    }
                }
                if (hasCustomMessage)
                {
                    sb.Append(customMessage);
                }
                return sb.ToString();
            }

            public override string GetMessage(TA actual, string customMessage)
            {
                return GetMessage(Left.Matcher(actual), Right.Matcher(actual), actual, customMessage);
            }
        }

        private abstract class BitwiseAssertion<TA> : UnaryAssertion<TA>
        {
            private readonly string @operator;
            private readonly UnaryAssertion<TA> left;
            private readonly UnaryAssertion<TA> right;

            protected BitwiseAssertion(string @operator, UnaryAssertion<TA> left, UnaryAssertion<TA> right)
            {
                if (left == null)
                {
                    throw new ArgumentNullException("left");
                }
                if (right == null)
                {
                    throw new ArgumentNullException("right");
                }
                this.left = left;
                this.right = right;
                this.@operator = @operator;
            }

            public UnaryAssertion<TA> Left
            {
                get { return left; }
            }

            public UnaryAssertion<TA> Right
            {
                get { return right; }
            }

            public string Operator
            {
                get { return @operator; }
            }

            public override string GetMessage(TA actual, string customMessage)
            {
                var sb = new StringBuilder(500);
                sb.AppendLine(GetUnaryFailureMessage(left, actual));
                sb.AppendLine(@operator);
                if (string.IsNullOrEmpty(customMessage))
                {
                    sb.Append(GetUnaryFailureMessage(right, actual));
                }
                else
                    if (!string.IsNullOrEmpty(customMessage))
                    {
                        sb.AppendLine(GetUnaryFailureMessage(right, actual));
                        sb.Append(customMessage);
                    }
                return sb.ToString();
            }

            protected string GetUnaryFailureMessage(UnaryAssertion<TA> unaryAssertion, TA actual)
            {
                return string.Format("{0}", unaryAssertion.GetMessage(actual, string.Empty));
            }
        }

        private class UnaryAssertion<TA> : IAssertion<TA>, IMessageComposer<TA>, IAssertionMatcher<TA>
        {
            protected UnaryAssertion()
            {
                Predicate = string.Empty;
            }

            public UnaryAssertion(string predicate, Func<TA, bool> matcher)
            {
                if (matcher == null)
                {
                    throw new ArgumentNullException("matcher");
                }
                Predicate = predicate ?? string.Empty;
                Matcher = matcher;
            }

            public virtual string Predicate { get; set; }

            #region IAssertionMatcher<TA> Members

            public virtual Func<TA, bool> Matcher { get; private set; }

            #endregion

            #region Implementation of IAssertion<TA>

            public virtual void Assert(TA actual, string customMessage)
            {
                if (Matcher(actual))
                {
                    return;
                }

                throw AssertExceptionFactory.CreateException(GetMessage(actual, customMessage));
            }

            #endregion

            public static IAssertion<TA> operator !(UnaryAssertion<TA> source)
            {
                return new NegateAssertion<TA>(source);
            }

            public static IAssertion<TA> operator |(UnaryAssertion<TA> x, UnaryAssertion<TA> y)
            {
                return new OrAssertion<TA>(x, y);
            }

            public static IAssertion<TA> operator &(UnaryAssertion<TA> x, UnaryAssertion<TA> y)
            {
                return new AndAssertion<TA>(x, y);
            }

            public virtual string GetMessage(TA actual, string customMessage)
            {
                return string.Format("{0} {1} {2}.{3}", Messages.FormatValue(actual), Resources.AssertionVerb, Predicate, customMessage);
            }
        }

        private class NegateAssertion<TA> : UnaryAssertion<TA>
        {
            private readonly UnaryAssertion<TA> source;

            public NegateAssertion(UnaryAssertion<TA> source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException("source");
                }
                this.source = source;
                source.Predicate = Resources.Negation + " " + source.Predicate;
            }

            public override string Predicate
            {
                get
                {
                    return source.Predicate;
                }
            }

            public override Func<TA, bool> Matcher
            {
                get
                {
                    return a => !source.Matcher(a);
                }
            }

            public override string GetMessage(TA actual, string customMessage)
            {
                return source.GetMessage(actual, customMessage);
            }
        }

        private class OrAssertion<TA> : BitwiseAssertion<TA>
        {
            public OrAssertion(UnaryAssertion<TA> left, UnaryAssertion<TA> right)
                : base("Or", left, right) { }

            public override Func<TA, bool> Matcher
            {
                get
                {
                    return a => Left.Matcher(a) || Right.Matcher(a);
                }
            }
        }

        private class BinaryFailureMagnifier : IFailureMagnifier
        {
            private BinaryExpression comparison;
            private Func<Expression, object> valueGetter;

            public BinaryFailureMagnifier(BinaryExpression comparison, Func<Expression, object> valueGetter)
            {
                this.comparison = comparison;
                this.valueGetter = valueGetter;
            }

            public string Message()
            {
                return string.Format(Resources.FailureMsgBinary,
                    this.valueGetter(this.comparison.Left),
                    ExpressionStringBuilder.ToStringOperator(this.comparison.NodeType),
                    this.valueGetter(this.comparison.Right));
            }
        }

        private class StringEqualityFailureMagnifier : IFailureMagnifier
        {
            const int MagnifierMaxAcceptableLength = 40;
            const int MagnifierLength = 40;
            const string Ellipsis = "...";

            private readonly string actual;
            private readonly string expected;

            public StringEqualityFailureMagnifier(string actual, string expected)
            {
                this.actual = actual;
                this.expected = expected;
            }

            #region Implementation of IFailureMagnifier

            public string Message()
            {
                if (ReferenceEquals(null, actual) || ReferenceEquals(null, expected))
                {
                    return string.Empty;
                }

                var firstDifference = actual.PositionOfFirstDifference(expected);
                if (firstDifference < 0)
                {
                    return string.Empty;
                }
                var sb = new StringBuilder(200);
                sb.AppendLine(string.Format(Resources.FailureMsgStringDiffPosTmpl, firstDifference + 1));
                sb.AppendLine(GetStringToShow(actual, firstDifference));
                sb.AppendLine(GetStringToShow(expected, firstDifference));

                var magnifier = (new string('_', Math.Max(actual.Length, expected.Length))).ToCharArray();
                magnifier[firstDifference] = '^';

                sb.AppendLine(GetStringToShow(new string(magnifier), firstDifference));
                return sb.ToString();
            }

            #endregion

            public string GetStringToShow(string original, int magnifierPos)
            {
                const int stringMiddle = (MagnifierLength / 2);

                if (original.Length <= MagnifierMaxAcceptableLength)
                {
                    return original;
                }

                var start = (magnifierPos - stringMiddle) < 0
                                            ? 0
                                            : (magnifierPos + stringMiddle) > original.Length
                                                    ? original.Length - MagnifierLength
                                                    : magnifierPos - stringMiddle;

                var sb = new StringBuilder(MagnifierMaxAcceptableLength);
                if (start > 0)
                {
                    sb.Append(Ellipsis);
                }
                sb.Append(original.Substring(start, MagnifierLength));
                if ((start + MagnifierLength) < original.Length)
                {
                    sb.Append(Ellipsis);
                }
                return sb.ToString();
            }
        }

        private class EmptyMagnifier : IFailureMagnifier
        {
            public string Message()
            {
                return null;
            }
        }

        private class SameSequenceAsFailureMagnifier<T> : IFailureMagnifier
        {
            private readonly IEnumerable<T> actual;
            private readonly IEnumerable<T> expected;
            private readonly IEqualityComparer<T> comparer;

            public SameSequenceAsFailureMagnifier(IEnumerable<T> actual, IEnumerable<T> expected)
                : this(actual, expected, EqualityComparer<T>.Default)
            {
            }

            public SameSequenceAsFailureMagnifier(IEnumerable<T> actual, IEnumerable<T> expected, IEqualityComparer<T> comparer)
            {
                this.actual = actual;
                this.expected = expected;
                this.comparer = comparer;
            }

            #region Implementation of IFailureMagnifier

            public string Message()
            {
                if (ReferenceEquals(null, actual) || ReferenceEquals(null, expected))
                {
                    return string.Empty;
                }

                int firstDifference = actual.PositionOfFirstDifference(expected, comparer);
                if (firstDifference < 0)
                {
                    return string.Empty;
                }

                var sb = new StringBuilder(100);
                sb.AppendLine(string.Format(Resources.FailureMsgEnumerableDiffPosTmpl,
                                                                        firstDifference == 0 ? firstDifference.ToString() : firstDifference + " (zero based)"));
                sb.AppendLine(string.Format(Resources.ExpectedTmpl,
                                                                        Messages.FormatValue(expected.ElementAtOrDefault(firstDifference))));
                sb.AppendLine(string.Format(Resources.FoundTmpl,
                                                                        Messages.FormatValue(actual.ElementAtOrDefault(firstDifference))));
                return sb.ToString();
            }

            #endregion
        }

        #endregion

        #region Utils

        private static class StackTraceFilter
        {
            private static readonly Regex stackTraceFilter = new Regex(@"Satisfyr(?!\.Tests)\.");

            public static string FilterStackTrace(string stackTrace)
            {
                var sb = new StringBuilder();
                var sr = new StringReader(stackTrace);

                string line;

                while ((line = sr.ReadLine()) != null)
                {
                    if (!stackTraceFilter.IsMatch(line))
                    {
                        sb.AppendLine(line);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// The intention of <see cref="ExpressionStringBuilder"/> is to create a more readable 
        /// string representation for the failure message.
        /// </summary>
        private class ExpressionStringBuilder
        {
            private readonly Expression expression;
            private StringBuilder builder;

            public ExpressionStringBuilder(Expression expression)
            {
                this.expression = expression;
            }

            public override string ToString()
            {
                builder = new StringBuilder();
                this.ToString(expression);
                return builder.ToString();
            }

            public void ToString(Expression exp)
            {
                if (exp == null)
                {
                    builder.Append(Resources.NullValue);
                    return;
                }
                switch (exp.NodeType)
                {
                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                    case ExpressionType.Not:
                    case ExpressionType.Convert:
                    case ExpressionType.ConvertChecked:
                    case ExpressionType.ArrayLength:
                    case ExpressionType.Quote:
                    case ExpressionType.TypeAs:
                        ToStringUnary((UnaryExpression)exp);
                        return;
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                    case ExpressionType.Divide:
                    case ExpressionType.Modulo:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Coalesce:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.RightShift:
                    case ExpressionType.LeftShift:
                    case ExpressionType.ExclusiveOr:
                        ToStringBinary((BinaryExpression)exp);
                        return;
                    case ExpressionType.TypeIs:
                        ToStringTypeIs((TypeBinaryExpression)exp);
                        return;
                    case ExpressionType.Conditional:
                        ToStringConditional((ConditionalExpression)exp);
                        return;
                    case ExpressionType.Constant:
                        ToStringConstant((ConstantExpression)exp);
                        return;
                    case ExpressionType.Parameter:
                        ToStringParameter((ParameterExpression)exp);
                        return;
                    case ExpressionType.MemberAccess:
                        ToStringMemberAccess((MemberExpression)exp);
                        return;
                    case ExpressionType.Call:
                        ToStringMethodCall((MethodCallExpression)exp);
                        return;
                    case ExpressionType.Lambda:
                        ToStringLambda((LambdaExpression)exp);
                        return;
                    case ExpressionType.New:
                        ToStringNew((NewExpression)exp);
                        return;
                    case ExpressionType.NewArrayInit:
                    case ExpressionType.NewArrayBounds:
                        ToStringNewArray((NewArrayExpression)exp);
                        return;
                    case ExpressionType.Invoke:
                        ToStringInvocation((InvocationExpression)exp);
                        return;
                    case ExpressionType.MemberInit:
                        ToStringMemberInit((MemberInitExpression)exp);
                        return;
                    case ExpressionType.ListInit:
                        ToStringListInit((ListInitExpression)exp);
                        return;
                    default:
                        throw new Exception(string.Format("Unhandled expression type: '{0}'", exp.NodeType));
                }
            }

            private void ToStringBinding(MemberBinding binding)
            {
                switch (binding.BindingType)
                {
                    case MemberBindingType.Assignment:
                        ToStringMemberAssignment((MemberAssignment)binding);
                        return;
                    case MemberBindingType.MemberBinding:
                        ToStringMemberMemberBinding((MemberMemberBinding)binding);
                        return;
                    case MemberBindingType.ListBinding:
                        ToStringMemberListBinding((MemberListBinding)binding);
                        return;
                    default:
                        throw new Exception(string.Format("Unhandled binding type '{0}'", binding.BindingType));
                }
            }

            private void ToStringElementInitializer(ElementInit initializer)
            {
                builder.Append("{");
                ToStringExpressionList(initializer.Arguments);
                builder.Append("}");
                return;
            }

            private void ToStringUnary(UnaryExpression u)
            {
                switch (u.NodeType)
                {
                    case ExpressionType.ArrayLength:
                        ToString(u.Operand);
                        builder.Append(".Length");
                        return;

                    case ExpressionType.Negate:
                    case ExpressionType.NegateChecked:
                        builder.Append("-");
                        ToString(u.Operand);
                        return;

                    case ExpressionType.Not:
                        builder.Append("!(");
                        ToString(u.Operand);
                        builder.Append(")");
                        return;

                    case ExpressionType.Quote:
                        ToString(u.Operand);
                        return;

                    case ExpressionType.TypeAs:
                        builder.Append("(");
                        ToString(u.Operand);
                        builder.Append(" as ");
                        builder.Append(u.Type.DisplayName());
                        builder.Append(")");
                        return;
                }
                return;
            }

            private void ToStringBinary(BinaryExpression b)
            {
                if (b.NodeType == ExpressionType.ArrayIndex)
                {
                    ToString(b.Left);
                    builder.Append("[");
                    ToString(b.Right);
                    builder.Append("]");
                }
                else
                {
                    string @operator = ToStringOperator(b.NodeType);
                    if (NeedEncloseInParen(b.Left))
                    {
                        builder.Append("(");
                        ToString(b.Left);
                        builder.Append(")");
                    }
                    else
                    {
                        ToString(b.Left);
                    }
                    builder.Append(" ");
                    builder.Append(@operator);
                    builder.Append(" ");
                    if (NeedEncloseInParen(b.Right))
                    {
                        builder.Append("(");
                        ToString(b.Right);
                        builder.Append(")");
                    }
                    else
                    {
                        ToString(b.Right);
                    }
                }
            }

            private bool NeedEncloseInParen(Expression operand)
            {
                return operand.NodeType == ExpressionType.AndAlso || operand.NodeType == ExpressionType.OrElse;
            }

            private void ToStringTypeIs(TypeBinaryExpression b)
            {
                ToString(b.Expression);
                return;
            }

            private void ToStringConstant(ConstantExpression c)
            {
                var value = c.Value;
                if (value != null)
                {
                    if (value is string)
                    {
                        builder.Append("\"").Append(value).Append("\"");
                    }
                    else if (value.ToString() == value.GetType().ToString())
                    {
                        // Perhaps is better without nothing (at least for local variables)
                        //builder.Append("<value>");
                    }
                    else if (c.Type.IsEnum)
                    {
                        builder.Append(c.Type.DisplayName()).Append(".").Append(value);
                    }
                    else
                    {
                        builder.Append(value);
                    }
                }
                else
                {
                    builder.Append(Resources.NullValue);
                }
            }

            private void ToStringConditional(ConditionalExpression c)
            {
                ToString(c.Test);
                ToString(c.IfTrue);
                ToString(c.IfFalse);
                return;
            }

            private void ToStringParameter(ParameterExpression p)
            {
                if (p.Name != null)
                {
                    builder.Append(p.Name);
                }
                else
                {
                    builder.Append("<param>");
                }
            }

            private void ToStringMemberAccess(MemberExpression m)
            {
                if (m.Expression != null)
                {
                    ToString(m.Expression);
                }
                else
                {
                    builder.Append(m.Member.DeclaringType.DisplayName());
                }
                builder.Append(".");
                builder.Append(m.Member.Name);
                return;
            }

            private void ToStringMethodCall(MethodCallExpression m)
            {
                int analizedParam = 0;
                Expression exp = m.Object;
                if (Attribute.GetCustomAttribute(m.Method, typeof(ExtensionAttribute)) != null)
                {
                    analizedParam = 1;
                    exp = m.Arguments[0];
                }
                if (exp != null)
                {
                    ToString(exp);
                    builder.Append(".");
                }
                else if (m.Method.IsStatic)
                {
                    builder.Append(m.Method.DeclaringType.DisplayName());
                    builder.Append(".");
                }
                builder.Append(m.Method.Name);
                builder.Append("(");
                AsCommaSeparatedValues(m.Arguments.Skip(analizedParam), ToString);
                builder.Append(")");
                return;
            }

            private void ToStringExpressionList(ReadOnlyCollection<Expression> original)
            {
                AsCommaSeparatedValues(original, ToString);
                return;
            }

            private void ToStringMemberAssignment(MemberAssignment assignment)
            {
                builder.Append(assignment.Member.Name);
                builder.Append("= ");
                ToString(assignment.Expression);
                return;
            }

            private void ToStringMemberMemberBinding(MemberMemberBinding binding)
            {
                ToStringBindingList(binding.Bindings);
                return;
            }

            private void ToStringMemberListBinding(MemberListBinding binding)
            {
                ToStringElementInitializerList(binding.Initializers);
                return;
            }

            private void ToStringBindingList(IEnumerable<MemberBinding> original)
            {
                bool appendComma = false;
                foreach (var exp in original)
                {
                    if (appendComma)
                    {
                        builder.Append(", ");
                    }
                    ToStringBinding(exp);
                    appendComma = true;
                }
                return;
            }

            private void ToStringElementInitializerList(ReadOnlyCollection<ElementInit> original)
            {
                for (int i = 0, n = original.Count; i < n; i++)
                {
                    ToStringElementInitializer(original[i]);
                }
                return;
            }

            private void ToStringLambda(LambdaExpression lambda)
            {
                if (lambda.Parameters.Count == 1)
                {
                    ToStringParameter(lambda.Parameters[0]);
                }
                else
                {
                    builder.Append("(");
                    AsCommaSeparatedValues(lambda.Parameters, ToStringParameter);
                    builder.Append(")");
                }
                builder.Append(" => ");
                ToString(lambda.Body);
                return;
            }

            private void ToStringNew(NewExpression nex)
            {
                Type type = (nex.Constructor == null) ? nex.Type : nex.Constructor.DeclaringType;
                builder.Append("new ");
                builder.Append(type.DisplayName());
                builder.Append("(");
                AsCommaSeparatedValues(nex.Arguments, ToString);
                builder.Append(")");
                return;
            }

            private void ToStringMemberInit(MemberInitExpression init)
            {
                ToStringNew(init.NewExpression);
                builder.Append(" {");
                ToStringBindingList(init.Bindings);
                builder.Append("}");
                return;
            }

            private void ToStringListInit(ListInitExpression init)
            {
                ToStringNew(init.NewExpression);
                builder.Append(" {");
                bool appendComma = false;
                foreach (var initializer in init.Initializers)
                {
                    if (appendComma)
                    {
                        builder.Append(", ");
                    }
                    ToStringElementInitializer(initializer);
                    appendComma = true;
                }
                builder.Append("}");
                return;
            }

            private void ToStringNewArray(NewArrayExpression na)
            {
                switch (na.NodeType)
                {
                    case ExpressionType.NewArrayInit:
                        builder.Append("new[] {");
                        AsCommaSeparatedValues(na.Expressions, ToString);
                        builder.Append("}");
                        return;
                    case ExpressionType.NewArrayBounds:
                        builder.Append("new ");
                        builder.Append(na.Type.GetElementType().DisplayName());
                        builder.Append("[");
                        AsCommaSeparatedValues(na.Expressions, ToString);
                        builder.Append("]");
                        return;
                }
            }

            private void AsCommaSeparatedValues<T>(IEnumerable<T> source, Action<T> toStringAction) where T : Expression
            {
                bool appendComma = false;
                foreach (var exp in source)
                {
                    if (appendComma)
                    {
                        builder.Append(", ");
                    }
                    toStringAction(exp);
                    appendComma = true;
                }
            }

            private void ToStringInvocation(InvocationExpression iv)
            {
                ToStringExpressionList(iv.Arguments);
                return;
            }

            public static string ToStringOperator(ExpressionType nodeType)
            {
                switch (nodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.AddChecked:
                        return "+";

                    case ExpressionType.And:
                        return "&";

                    case ExpressionType.AndAlso:
                        return "&&";

                    case ExpressionType.Coalesce:
                        return "??";

                    case ExpressionType.Divide:
                        return "/";

                    case ExpressionType.Equal:
                        return "==";

                    case ExpressionType.ExclusiveOr:
                        return "^";

                    case ExpressionType.GreaterThan:
                        return ">";

                    case ExpressionType.GreaterThanOrEqual:
                        return ">=";

                    case ExpressionType.LeftShift:
                        return "<<";

                    case ExpressionType.LessThan:
                        return "<";

                    case ExpressionType.LessThanOrEqual:
                        return "<=";

                    case ExpressionType.Modulo:
                        return "%";

                    case ExpressionType.Multiply:
                    case ExpressionType.MultiplyChecked:
                        return "*";

                    case ExpressionType.NotEqual:
                        return "!=";

                    case ExpressionType.Or:
                        return "|";

                    case ExpressionType.OrElse:
                        return "||";

                    case ExpressionType.Power:
                        return "^";

                    case ExpressionType.RightShift:
                        return ">>";

                    case ExpressionType.Subtract:
                    case ExpressionType.SubtractChecked:
                        return "-";
                }
                return nodeType.ToString();
            }
        }

        private class Messages
        {
            public static string FormatValue(object value)
            {
                if (ReferenceEquals(null, value))
                {
                    return Resources.NullValue;
                }
                if (value.GetType() == typeof(string))
                {
                    return string.Format("\"{0}\"", value);
                }
                else
                {
                    var v = value as IEnumerable;
                    if (v != null)
                    {
                        return FormatEnumerable(v);
                    }
                }
                return value.ToString();
            }

            public static string FormatEnumerable(IEnumerable enumerable)
            {
                if (ReferenceEquals(null, enumerable))
                {
                    return Resources.NullValue;
                }
                var result = new StringBuilder(200);
                result.Append('[');
                bool appendComma = false;
                foreach (var element in enumerable)
                {
                    if (appendComma)
                    {
                        result.Append(", ");
                    }
                    result.Append(FormatValue(element));
                    appendComma = true;
                }
                if (!appendComma)
                {
                    // is empty
                    result.Append(Resources.EmptyEnumerable);
                }
                result.Append(']');
                return result.ToString();
            }
        }

        private class ExpressionMessageComposer<TA> : IMessageComposer<TA>
        {
            private static readonly IFailureMagnifier Empty = new EmptyMagnifier();
            private readonly Expression<Func<TA, bool>> expression;
            private readonly IFailureMagnifier magnifier;

            public ExpressionMessageComposer(Expression<Func<TA, bool>> expression) : this(expression, Empty) { }

            public ExpressionMessageComposer(Expression<Func<TA, bool>> expression, IFailureMagnifier magnifier)
            {
                if (expression == null)
                {
                    throw new ArgumentNullException("expression");
                }
                if (magnifier == null)
                {
                    throw new ArgumentNullException("magnifier");
                }
                this.expression = expression;
                this.magnifier = magnifier;
            }

            #region Implementation of IMessageComposer<TA>

            public string GetMessage(TA actual, string customMessage)
            {
                string baseMessage;
                if (!string.IsNullOrEmpty(customMessage))
                {
                    baseMessage = string.Format("{3}: {0} {1} {2}", Messages.FormatValue(actual), Resources.AssertionVerb,
                                                string.Format("Satisfy ({0})", new ExpressionStringBuilder(expression)), customMessage);
                }
                else
                {
                    baseMessage = string.Format("{0} {1} {2}", Messages.FormatValue(actual), Resources.AssertionVerb,
                                                string.Format("Satisfy ({0})", new ExpressionStringBuilder(expression)));
                }
                string magnMessage = magnifier.Message();
                return string.IsNullOrEmpty(magnMessage) ? baseMessage : string.Concat(baseMessage, Environment.NewLine, magnMessage);
            }

            #endregion
        }

        private static class Resources
        {
            public const string AssertionVerb = "Should";
            public const string Be = "Be";
            public const string EmptyEnumerable = "<Empty>";
            public const string ExceptionMsgAccessToField = "Can't access to a field of a null value.";
            public const string ExceptionMsgFieldNameTmpl = "The class {0} does not contain a field named {1}.";
            public const string ExceptionMsgInvalidCastTmpl = "The class {0} does contain a field named {1} but its type is {2} and not {3}.";
            public const string ExceptionMsgSerializableNull = "Can't check serialization for (null) value.";
            public const string ExpectedTmpl = "Expected: {0}";
            public const string FailureMsgDifferences = "Differences :";
            public const string FailureMsgEnumerableDiffPosTmpl = "Values differ at position {0}.";
            public const string FailureMsgNotThrow = "Not expected exception message:";
            public const string FailureMsgStringDiffPosTmpl = "Strings differ at position {0}.";
            public const string FailureMsgBinary = "Compared {0} {1} {2}.";
            public const string FoundTmpl = "Found   : {0}";
            public const string Negation = "Not";
            public const string NullValue = "(null)";
        }

        #endregion

        #endregion
    }
}
