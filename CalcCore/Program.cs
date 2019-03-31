using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CalcCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Tokenizer tokenizer = new Tokenizer();
            //List<Token> tokens = tokenizer.Tokenize("1mod2+6-sin(21)*3*(21+(21+2))*6");
            //tokens = tokenizer.Tokenize(".258*3");
            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("3*-1"));
            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("-1-1+-2"));
            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("-x"));
            //tokenizer.Variables.Add(Tokenizer.CreateVariable("x", 2));
            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("-(2)"));
            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("-(2)/4*8/.5"));

            //List<Token> tokens = tokenizer.resolveNegatives(tokenizer.Tokenize("50 - 10 * (4 - 2) + 6"));
            //Console.WriteLine(tokenizer.tokensToExpression(tokenizer.resolveNegatives(tokens)));
            //Console.WriteLine(tokenizer.Parse(tokenizer.resolveNegatives(tokens)));
            //Console.ReadLine();

            tokenizer.Variables.Add(Tokenizer.CreateVariable("PI", Math.PI));
            while (true)
            {
                string line = Console.ReadLine();
                List<Token> tokens = tokenizer.Tokenize(line);
                double res = tokenizer.Parse(tokens);
                Console.WriteLine(tokenizer.tokensToExpression(tokens) + "=" + res);
            }
        }
    }
}
