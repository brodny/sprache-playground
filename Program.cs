using System;
using System.Linq.Expressions;

namespace SprachePlayground
{
    public class Program
    {
        public static void Main(string[] args)
        {
            string line;

            do
            {
                Console.Write("Enter expression: ");
                line = Console.ReadLine();

                if (!string.IsNullOrEmpty(line))
                {
                    Expression<Func<double>> expression = ExpressionParser.ParseExpression(line);

                    Func<double> func = expression.Compile();

                    System.Console.WriteLine("Result: {0}", func());
                }
            }
            while (!string.IsNullOrEmpty(line));
        }
    }
}
