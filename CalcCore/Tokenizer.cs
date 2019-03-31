using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace CalcCore
{
    public struct Operator
    {
        public string Name;
        public int Priority;
    }

    public struct Function
    {
        public string Name;
        public int NArgs;
    }

    public enum TokenType
    {
        Number,
        Variable,
        Function,
        Parentheses,
        Operator,

        Start,
        End,
    }

    public struct Token
    {
        public string Value;
        public TokenType Type;
        public int Priority;
        public bool NumEval;
        public double Number;
    }

    //public struct Variable
    //{
    //    public string Name;
    //    public double Value;
    //}

    public class Tokenizer
    {
        public List<Operator> Operators = new List<Operator>();
        public List<string> Functions = new List<string>();
        public Dictionary<string, double> Variables = new Dictionary<string, double>();
        //TokenType.Function isn't here because it's always followed by a Parentheses
        //It has been added to make sure that if there is a fucntion after an operator it's still valid
        public static readonly TokenType[] NumericalTokens = { TokenType.Parentheses, TokenType.Number, TokenType.Variable, TokenType.Function };

        public Tokenizer()
        {
            Operators.Add(CreateOperator("+", 0));
            Operators.Add(CreateOperator("-", 0));
            Operators.Add(CreateOperator("*", 1));
            Operators.Add(CreateOperator("/", 1));
            Operators.Add(CreateOperator("mod", 1));
            Operators.Add(CreateOperator("^", 2));

            Functions.Add("floor");
            Functions.Add("ceil");
            Functions.Add("round");
            Functions.Add("pow");
            Functions.Add("sqrt");
            Functions.Add("root");
            Functions.Add("sin");
            Functions.Add("cos");
            Functions.Add("tan");
            Functions.Add("asin");
            Functions.Add("acos");
            Functions.Add("atan");
        }

        public static Operator CreateOperator(string Name, int Priority)
        {
            Operator op = new Operator();
            op.Name = Name;
            op.Priority = Priority;
            return op;
        }

        public static Token CreateToken(string Value, TokenType Type, int Priority)
        {
            Token token = new Token();
            token.Value = Value;
            token.Type = Type;
            token.Priority = Priority;
            token.NumEval = false;
            token.Number = double.NaN;
            return token;
        }

        public static Token CreateNumberToken(double Value)
        {
            Token token = new Token();
            token.Value = Value.ToString();
            token.Type = TokenType.Number;
            token.Priority = -1;
            token.NumEval = true;
            token.Number = Value;
            return token;
        }

        public double ParseFunction(string name, string expression)
        {
            string[] parts = expression.Split(',');
            double[] parsedParts = parts.Select((x) => Parse(Tokenize(x))).ToArray();
            switch (name)
            {
                case "floor":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Floor(parsedParts[0]);
                case "ceil":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Ceiling(parsedParts[0]);
                case "round":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Round(parsedParts[0]);
                case "pow":
                    if (parts.Length < 2) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 2) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Pow(parsedParts[0], parsedParts[1]);
                case "root":
                    if (parts.Length < 2) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 2) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Pow(parsedParts[0], 1/parsedParts[1]);
                case "sqrt":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Sqrt(parsedParts[0]);
                case "sin":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Sin(parsedParts[0] * Math.PI / 180);
                case "asin":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Asin(parsedParts[0]) * 180 / Math.PI;
                case "cos":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Cos(parsedParts[0] * Math.PI / 180);
                case "acos":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Acos(parsedParts[0]) * 180 / Math.PI;
                case "tan":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Tan(parsedParts[0] * Math.PI / 180);
                case "atan":
                    if (parts.Length < 1) throw new Exception("Not enough arguments in " + name + "(" + expression + ")");
                    if (parts.Length > 1) throw new Exception("Too many arguments in " + name + "(" + expression + ")");
                    return Math.Atan(parsedParts[0]) * 180 / Math.PI;
                default:
                    throw new Exception("No '" + name + "' function exists in " + name + "(" + expression + ")");
            }
        }

        public double TokenizeAndParse(string Expression)
        {
            return Parse(Tokenize(Expression));
        }

        public int[] OrderTokens(List<Token> tokens)
        {
            List<Tuple<int, int>> tokenIndex = new List<Tuple<int, int>>();
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Operator) tokenIndex.Add(new Tuple<int, int>(i, tokens[i].Priority));
            }
            return tokenIndex.OrderByDescending((x) => x.Item2).Select((x) => x.Item1).ToArray();
        }

        public double parseOperator(string op, double x, double y)
        {
            if (op == "+")
            {
                return x + y;
            }
            else if (op == "-")
            {
                return x - y;
            }
            else if (op == "*")
            {
                return x * y;
            }
            else if (op == "/")
            {
                return x / y;
            }
            else if (op == "mod")
            {
                return x % y;
            }
            else if (op == "^")
            {
                return Math.Pow(x, y);
            }
            else
            {
                throw new Exception("No operator '" + op + "'");
            }
        }

        public int resolveOperatorPriority(string op)
        {
            return Operators.Where((x) => x.Name == op).Select((x) => x.Priority).ToList()[0];
        }

        public double resolveVariable(string varname)
        {
            //if (!Variables.Any((x) => x.Name == varname)) throw new Exception("Variable with name '" + varname + "' was not defined");
            //return Variables.Where((x) => x.Name == varname).Select((x) => x.Value).ToArray()[0];
            if (!Variables.ContainsKey(varname)) throw new Exception("Variable with name '" + varname + "' was not defined");
            return Variables[varname];
        }

        public double resolveNumber(string number)
        {
            return double.Parse(number, System.Globalization.CultureInfo.InvariantCulture);
        }

        public string tokensToExpression(List<Token> tokens)
        {
            string exp = "";
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i].Type == TokenType.Start) continue;
                else if (tokens[i].Type == TokenType.End) continue;
                else if (tokens[i].Type == TokenType.Parentheses) exp += "(" + tokens[i].Value + ")";
                else exp += tokens[i].Value;
            }
            return exp;
        }

        public List<Token> resolveNegatives(List<Token> tokens)
        {
            List<Token> retTokens = new List<Token>();
            bool lastOperator = false;
            int i = -1;

            for (int j = 0; j < tokens.Count; j++)
            {
                if (j == 0) continue; //So we can access token before
                if (tokens[j - 1].Type == TokenType.Operator && tokens[j - 1].Value == "-" && tokens[j].Type == TokenType.Operator && tokens[j].Value == "-")
                    throw new Exception("Multiple - signs before number.");// x--2 is invalid while  x+-2 is valid
            }

            while (true)
            {
                i++;
                if (i >= tokens.Count) break;
                if (lastOperator && tokens[i].Type == TokenType.Operator)
                {
                    //check if current operator is a -
                    if (tokens[i].Value == "-")
                    {
                        //Check if next token is a number
                        if (tokens.Count > i + 1)
                        {
                            //TODO: CHECK FOR MULTIPLE - OPERATORS IN A ROW
                            //Hack: put all the load on the parser
                            if (tokens[i + 1].Type == TokenType.Number)
                            {
                                retTokens.Add(CreateToken("-" + tokens[i + 1].Value, TokenType.Number, -1));
                                //Skip one token because we consumed it
                                i++;
                            }
                            else if (tokens[i + 1].Type == TokenType.Parentheses)
                            {
                                retTokens.Add(CreateToken("0-(" + tokens[i + 1].Value + ")", TokenType.Parentheses, -1));
                                //Skip one token because we consumed it
                                i++;
                            }
                            else if (tokens[i + 1].Type == TokenType.Variable)
                            {
                                retTokens.Add(CreateToken("0-(" + tokens[i + 1].Value + ")", TokenType.Parentheses, -1));
                                //Skip one token because we consumed it
                                i++;
                            }
                            else if (tokens[i + 1].Type == TokenType.Function)
                            {
                                if (tokens.Count > i + 2)
                                {
                                    //HACK: we assume that tokens[i + 2] is a parentheses token
                                    retTokens.Add(CreateToken("0-(" + tokens[i + 1].Value + "(" + tokens[i + 2].Value + ")" + ")", TokenType.Parentheses, -1));
                                    //Skip two token because we consumed them
                                    i++;
                                    i++;
                                }
                            }
                            else
                            {
                                retTokens.Add(tokens[i]);
                            }
                        }
                        else
                        {
                            retTokens.Add(tokens[i]);
                        }
                    }
                    else
                    {
                        retTokens.Add(tokens[i]);
                    }
                }
                else
                {
                    retTokens.Add(tokens[i]);
                }
                if (tokens[i].Type == TokenType.Operator || tokens[i].Type == TokenType.Start) lastOperator = true;
                else lastOperator = false;
            }

            return retTokens;
        }

        public double Parse(List<Token> tokens)
        {
            //throw new NotImplementedException();
            //Fix numbers so that negative numbers are allowed with -5+6

            //We only need a shallow clone, plus we need to remove the start and end tokens, as were already done useing them and they complicate everything
            List<Token> Parsed = tokens.Where((x) => x.Type != TokenType.Start && x.Type != TokenType.End).ToList();
            //Implementing order of operations
            int[] OperatorIndexOrdered = OrderTokens(Parsed);

            for (int i = 0; i < OperatorIndexOrdered.Length; i++)
            {
                int ci = OperatorIndexOrdered[i];
                Token curOperator = Parsed[ci];

                double x = double.NaN;
                double y = double.NaN;

                int tokenCountX = 0;
                int tokenCountY = 0;

                //Make sure there is a token before and after
                if (ci == 0)
                {
                    //Since this is index 0, nothing comes before it
                    throw new Exception("No token before operator 0");
                }

                if (ci >= Parsed.Count)
                {
                    //Since this is index 0, nothing comes before it
                    throw new Exception("No token before operator at " + ci);
                }

                //Check if the tokens are 'numerical' tokens
                if (!NumericalTokens.Contains(Parsed[ci - 1].Type))
                {
                    throw new Exception("No numerical token before token " + (ci - 1));
                }

                if (!NumericalTokens.Contains(Parsed[ci + 1].Type))
                {
                    throw new Exception("No numerical token before token " + (ci + 1));
                }

                //X
                if (Parsed[ci - 1].Type == TokenType.Parentheses)
                {
                    //Check for function token before
                    if (ci - 2 >= 0)
                    {
                        //could be a function
                        if (Parsed[ci - 2].Type == TokenType.Function)
                        {
                            //It's a function
                            x = ParseFunction(Parsed[ci - 2].Value, Parsed[ci - 1].Value);
                            tokenCountX += 2;
                        }
                        else
                        {
                            //parse as a normal set of parentheses
                            x = TokenizeAndParse(Parsed[ci - 1].Value);
                            tokenCountX += 1;
                        }
                    }
                    else
                    {
                        //parse as a normal set of parentheses
                        x = TokenizeAndParse(Parsed[ci - 1].Value);
                        tokenCountX += 1;
                    }
                }

                if (Parsed[ci - 1].Type == TokenType.Variable)
                {
                    x = resolveVariable(Parsed[ci - 1].Value);
                    tokenCountX += 1;
                }

                if (Parsed[ci - 1].Type == TokenType.Number)
                {
                    if (Parsed[ci - 1].NumEval) x = Parsed[ci - 1].Number;
                    else x = resolveNumber(Parsed[ci - 1].Value);
                    tokenCountX += 1;
                }

                //Y

                if (Parsed[ci + 1].Type == TokenType.Parentheses)
                {
                    //Can't be a fucntion becuase it would not be valid since a function token is always followed by a parenthese token
                    y = TokenizeAndParse(Parsed[ci + 1].Value);
                    tokenCountY += 1;
                }

                if (Parsed[ci + 1].Type == TokenType.Function)
                {
                    //It's  a funciton
                    //We can assume that parentheses follow
                    y = ParseFunction(Parsed[ci + 1].Value, Parsed[ci + 2].Value);
                    tokenCountY += 2;
                }

                if (Parsed[ci + 1].Type == TokenType.Variable)
                {
                    y = resolveVariable(Parsed[ci + 1].Value);
                    tokenCountY += 1;
                }

                if (Parsed[ci + 1].Type == TokenType.Number)
                {
                    if (Parsed[ci + 1].NumEval) y = Parsed[ci + 1].Number;
                    else y = resolveNumber(Parsed[ci + 1].Value);
                    tokenCountY += 1;
                }

                //Apply the operator
                double z = parseOperator(curOperator.Value, x, y);

                //The hard part, inserting and shifting indexes... ugh

                //Start accounting for the decrease in length at the resolved tokens
                //If there are functions on both sides, we remove 5 tokens
                //But since we add one back: the evaluated number token it becomes 4

                //int shiftBack = tokenCountX + tokenCountY;

                //x is on the left, y is on the right

                //Check for i + 1
                for (int j = 0; j < OperatorIndexOrdered.Length; j++)
                {
                    if (OperatorIndexOrdered[j] <= ci) continue;
                    //need to shift back depending on position
                    OperatorIndexOrdered[j] -= tokenCountY + tokenCountX;
                }

                //for (int j = 01; j < i; j++)
                //{
                //    //need to shift back depending on position
                //    OperatorIndexOrdered[j] -= tokenCountY;
                //}

                //Actually preform the operation
                Parsed.RemoveRange(OperatorIndexOrdered[i] - tokenCountX, tokenCountY + tokenCountX + 1);
                Parsed.Insert(OperatorIndexOrdered[i] - tokenCountX, CreateNumberToken(z));
            }
            //SUCH A HACK
            if (Parsed.Count == 1)
            {
                if (Parsed[0].Type == TokenType.Number)
                {
                    if (Parsed[0].NumEval) return Parsed[0].Number;
                    return resolveNumber(Parsed[0].Value);
                }
                else if (Parsed[0].Type == TokenType.Parentheses)
                {
                    return TokenizeAndParse(Parsed[0].Value);
                }
                else if (Parsed[0].Type == TokenType.Variable)
                {
                    return resolveVariable(Parsed[0].Value);
                }
            }
            else if (Parsed.Count == 2)
            {
                //function?
                if (Parsed[0].Type == TokenType.Function)
                {
                    return ParseFunction(Parsed[0].Value, Parsed[1].Value);
                }
            }
            throw new Exception("AFHDakfdhah");
        }

        public bool isVarSet(string Expression)
        {
            string[] parts = Expression.Split('=');
            if (parts.Length == 1) return false;
            if (parts.Length > 2) throw new Exception("Too many '=' in expression");
            return true;
        }

        public List<Token> parseVariable(string Expression)
        {
            List<Token> tokens = new List<Token>();
            string[] parts = Expression.Split('=');
            //Parts[0] should be a variable
            //Parts[1] should be the expression
            if (!parts[0].All((x) => Char.IsLetter(x))) throw new Exception("Invalid variable name");
            if (Variables.ContainsKey(parts[0]))
            {
                Variables[parts[0]] = TokenizeAndParse(parts[1]);
            }
            else
            {
                Variables.Add(parts[0], TokenizeAndParse(parts[1]));
            }
            tokens.Add(CreateToken(parts[0], TokenType.Variable, -1));
            return tokens;
        }

        public List<Token> Tokenize(string Expression)
        {
            Expression = Expression.Replace(" ", "");

            //Implement variables
            if (isVarSet(Expression))
            {
                return parseVariable(Expression);
            }

            List<Token> tokens = new List<Token>();
            tokens.Add(CreateToken("", TokenType.Start, -1));

            //TokenType ParserMode = TokenType.Start;
            string Buffer = "";

            int i = 0;

            while (true)
            {
                //if (ParserMode == TokenType.Start)
                //{
                //3 Cases: number, letter, parentheses, or maybe '=', but we will worry about = later
                if (i >= Expression.Length)
                {
                    break;
                }
                if (Char.IsLetter(Expression[i]))
                {
                    //Could be Operator, Function, Variable 
                    //Since it's the start we only have to check: Function, Variable
                    bool Done = false;
                    foreach (string funcName in Functions)
                    {
                        //Changed from: (!(A>B)), to: A<=B
                        if (!(funcName.Length > Expression.Substring(i).Length))
                        {
                            if (Expression.Substring(i, funcName.Length) == funcName)
                            {
                                //Function was found, Check for parentheses
                                if (Expression.Substring(i).Length >= (funcName.Length + 1))
                                {
                                    if (Expression[funcName.Length + i] == '(')
                                    {
                                        //We can now select the internal expression(s)
                                        i = funcName.Length + i;
                                        string ArgBuffer = "";
                                        //Grab stuff inbetween parentheses

                                        //DONE
                                        //TODO: pay attention to multiple parentheses inside of function call arguments
                                        int numOpenParenth = 1;
                                        while (true)
                                        {
                                            i++;
                                            if (i >= Expression.Length)
                                            {
                                                throw new Exception("No closing parentheses present for function call at " + i);
                                            }
                                            if (Expression[i] == ')')
                                            {
                                                numOpenParenth--;
                                                if (numOpenParenth == 0)
                                                {
                                                    //i++; //Account for closing parentheses
                                                    i++; //Account for closing parentheses
                                                    break;
                                                }
                                                if (numOpenParenth < 0)
                                                {
                                                    throw new Exception("Too many closing parentheses at " + i);
                                                }
                                                //Optimization
                                                ArgBuffer += Expression[i]; //if (numOpenParenth > 0) ArgBuffer += Expression[i];
                                            }
                                            else if (Expression[i] == '(')
                                            {
                                                numOpenParenth++;
                                                ArgBuffer += Expression[i];
                                            }
                                            else
                                            {
                                                ArgBuffer += Expression[i];
                                            }
                                        }

                                        //Do function call //DON'T DO FUNCTION CALL! MAKE A TOKEN
                                        //Technically we should make a parentheses token and make a function token
                                        //Let's do it!
                                        tokens.Add(CreateToken(funcName, TokenType.Function, -1)); //A priority of -1 means that it's not an operator
                                        tokens.Add(CreateToken(ArgBuffer, TokenType.Parentheses, -1)); //Because of this we can always assume that a function token will be followed by a parentheses token
                                        Done = true;
                                        break;
                                    }
                                    //Removed this because if there are two functions: AA() and A(), If it checks for A where there is AA it will say there are no parentheses
                                    //else
                                    //{
                                    //    throw new Exception("No parentheses present for function call");
                                    //}
                                }
                                //else
                                //{
                                //    throw new Exception("No parentheses present for function call");
                                //}
                            }

                        }
                    }

                    if (Done) continue;

                    //Check for variables
                    foreach (string varName in Variables.Keys)
                    {
                        if (varName.Length <= Expression.Substring(i).Length)
                        {
                            if (Expression.Substring(i, varName.Length) == varName)
                            {
                                tokens.Add(CreateToken(varName, TokenType.Variable, -1));
                                Done = true;
                                i += varName.Length; //"Move over play head"
                                break;
                            }
                        }
                    }

                    if (Done) continue;

                    //Check for operators
                    foreach (string opName in Operators.Select((x) => x.Name))
                    {
                        if (opName.Length <= Expression.Substring(i).Length)
                        {
                            if (Expression.Substring(i, opName.Length) == opName)
                            {
                                tokens.Add(CreateToken(opName, TokenType.Operator, resolveOperatorPriority(opName)));
                                Done = true;
                                i += opName.Length; //"Move over play head"
                                break;
                            }
                        }
                    }

                    if (!Done) throw new Exception("Could not parse token begining at " + i);
                    continue;
                }
                else if (Char.IsNumber(Expression[i]) || Expression[i] == '.') //Include . for numbers like ".5"
                {
                    string numberBuffer = "";
                    string curString = Expression.Substring(i);
                    for (int j = 0; j < curString.Length; j++)
                    {
                        //if (curString.Length < j) 
                        if ((i + j) >= Expression.Length - 1)
                        {
                            numberBuffer += Expression[i + j];
                            i = Expression.Length + 1;
                            break;
                        }
                        if (Char.IsNumber(Expression[i + j]) || Expression[i + j] == '.')
                        {
                            Debug.WriteLine("{0}:{1},{2},{3}", j + i, Expression[i + j], Char.IsNumber(Expression[i + j]), Expression[i + j] == '.');
                            numberBuffer += Expression[i + j];
                        }
                        else
                        {
                            i = i + j;
                            break;
                        }
                    }
                    //i++;
                    //Check for only one '.'
                    //Find where it is and check for a number after
                    string[] numberParts = numberBuffer.Split('.');
                    if (numberParts.Length - 1 != 1 && numberParts.Length - 1 != 0) throw new Exception("To many '.' in number at " + i);
                    if (numberParts.Length - 1 == 1 && !Char.IsNumber(numberParts[1][0])) throw new Exception("[0-9] must follow after a '.' at " + i);
                    tokens.Add(CreateToken(numberBuffer, TokenType.Number, -1));
                    continue;
                }
                else if (Expression[i] == '(')
                {
                    string ArgBuffer = "";
                    //Grab stuff inbetween parentheses

                    //DONE
                    //TODO: pay attention to multiple parentheses inside of function call arguments
                    int numOpenParenth = 1;
                    while (true)
                    {
                        i++;
                        if (i > Expression.Length)
                        {
                            throw new Exception("No closing parentheses present for function call at " + i);
                        }
                        if (Expression[i] == ')')
                        {
                            numOpenParenth--;
                            if (numOpenParenth == 0)
                            {
                                i++; //Account for closing parentheses
                                break;
                            }
                            if (numOpenParenth < 0)
                            {
                                throw new Exception("Too many closing parentheses at " + i);
                            }
                            //Optimization
                            ArgBuffer += Expression[i]; //if (numOpenParenth > 0) ArgBuffer += Expression[i];
                        }
                        else if (Expression[i] == '(')
                        {
                            numOpenParenth++;
                            ArgBuffer += Expression[i];
                        }
                        else
                        {
                            ArgBuffer += Expression[i];
                        }
                    }
                    tokens.Add(CreateToken(ArgBuffer, TokenType.Parentheses, 3)); //TODO: CHANGE PARENTHESE PRIORITY
                    continue;
                }
                else
                {
                    bool found = false;
                    //Check again for non letter begining operators
                    foreach (string opName in Operators.Select((x) => x.Name))
                    {
                        if (opName.Length <= Expression.Substring(i).Length)
                        {
                            if (Expression.Substring(i, opName.Length) == opName)
                            {
                                tokens.Add(CreateToken(opName, TokenType.Operator, resolveOperatorPriority(opName)));
                                found = true;
                                i += opName.Length; //"Move over play head"
                                break;
                            }
                        }
                    }
                    //Console.WriteLine(found);
                    if (!found) throw new Exception("Could not tokenize token at " + i);
                }
                //}
            }
            tokens.Add(CreateToken("", TokenType.End, -1));
            return resolveNegatives(tokens);
        }
    }

    public static class CalcCoreEX
    {
        public static void ReplaceRange<T>(this List<T> ts, int startIndex, List<T> insert)
        {
            int removeCount = insert.Count;
            ts.RemoveRange(startIndex, removeCount);
            ts.InsertRange(startIndex, insert);
        }
    }
}
