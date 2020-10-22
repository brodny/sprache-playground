// https://github.com/sprache/Sprache/blob/master/samples/LinqyCalculator/ExpressionParser.cs

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sprache;

namespace SprachePlayground
{
    public static class ExpressionParser
    {
        public static Expression<Func<double>> ParseExpression(string text)
        {
            return Lambda.Parse(text);
        }

        private static Parser<ExpressionType> Operator(string op, ExpressionType opType)
        {
            return Parse.String(op).Token().Return(opType);
        }

        private static Expression CallFunction(string name, Expression[] parameters)
        {
            MethodInfo methodInfo = typeof(Math).GetTypeInfo()
                .GetMethod(name, parameters.Select(e => e.Type).ToArray());

            if (methodInfo == null)
            {
                throw new ParseException(string.Format("Function '{0}({1})' does not exist.",
                    name, string.Join(",", parameters.Select(e => e.Type.Name))));
            }

            return Expression.Call(methodInfo, parameters);
        }

        private static readonly Parser<ExpressionType> Add = Operator("+", ExpressionType.AddChecked);
        private static readonly Parser<ExpressionType> Subtract = Operator("-", ExpressionType.SubtractChecked);
        private static readonly Parser<ExpressionType> Multiply = Operator("*", ExpressionType.MultiplyChecked);
        private static readonly Parser<ExpressionType> Divide = Operator("/", ExpressionType.Divide);
        private static readonly Parser<ExpressionType> Modulo = Operator("%", ExpressionType.Modulo);
        private static readonly Parser<ExpressionType> Power = Operator("^", ExpressionType.Power);

        private static readonly Parser<Expression> Expr =
            Parse.ChainOperator(Add.Or(Subtract), Term, Expression.MakeBinary);

        private static readonly Parser<Expression> Term =
            Parse.ChainOperator(Multiply.Or(Divide).Or(Modulo), InnerTerm, Expression.MakeBinary);

        private static readonly Parser<Expression> InnerTerm =
            Parse.ChainRightOperator(Power, Operand, Expression.MakeBinary);

        private static readonly Parser<Expression> Operand =
            (
                (
                    from sign in Parse.Char('-')
                    from factor in Factor
                    select Expression.Negate(factor)
                )
                .XOr(Factor)
            ).Token();
        
        private static readonly Parser<Expression> Factor =
            (
                from lParenthesis in Parse.Char('(')
                from expr in Parse.Ref(() => Expr)
                from rParenthesis in Parse.Char(')')
                select expr
            ).Named("expression")
            .XOr(Constant)
            .XOr(Function);
        
        private static readonly Parser<Expression> Constant =
            Parse.Decimal.Select(x => Expression.Constant(double.Parse(x))).Named("number");
        
        private static readonly Parser<Expression> Function =
            from name in Parse.Letter.AtLeastOnce().Text()
            from lParenthesis in Parse.Char('(')
            from expr in Parse.Ref(() => Expr).DelimitedBy(Parse.Char(',').Token())
            from rParenthesis in Parse.Char(')')
            select CallFunction(name, expr.ToArray());

        private static readonly Parser<Expression<Func<double>>> Lambda =
            Expr.End().Select(body => Expression.Lambda<Func<double>>(body));
    }
}