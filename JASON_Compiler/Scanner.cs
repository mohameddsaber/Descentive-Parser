using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public enum Token_Class
{
    Else, If, Int, Float, String, Endl,
    Read, Then, Until, Write, Repeat,LCurly,RCurly,ElseIf,Return,Main,
    Dot, Semicolon, Comma, LParanthesis, RParanthesis, EqualOp, LessThanOp,
    GreaterThanOp, NotEqualOp, PlusOp, MinusOp, MultiplyOp, DivideOp, AssignmentOP,
    Identifier, AndOp, OrOp, Number, QuotedString, AndSymbol, OrSymbol, Colon, End
}
namespace JASON_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();




        public Scanner()
        {
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("elseif", Token_Class.ElseIf);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("end", Token_Class.End);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("main", Token_Class.Main);




            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);


            Operators.Add(".", Token_Class.Dot);
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurly);
            Operators.Add("}", Token_Class.RCurly);
            Operators.Add(":", Token_Class.Colon);
            Operators.Add("|", Token_Class.OrSymbol);
            Operators.Add("&", Token_Class.AndSymbol);


            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);

            Operators.Add(":=", Token_Class.AssignmentOP);


            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);

            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("/", Token_Class.DivideOp);



        }

        public void StartScanning(string SourceCode)
        {
            SourceCode = SourceCode.ToLower();

            for (int i = 0; i < SourceCode.Length; i++)
            {

                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = "";

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n' || CurrentChar =='\t')
                    continue;

                if ((CurrentChar >= 'A' && CurrentChar <= 'Z') || (CurrentChar >= 'a' && CurrentChar <= 'z'))
                {
                    while (j < SourceCode.Length && ((SourceCode[j] >= 'A' && SourceCode[j] <= 'Z') || (SourceCode[j] >= 'a' && SourceCode[j] <= 'z') || (SourceCode[j] >= '0' && SourceCode[j] <= '9')))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    i = j - 1;
                    FindTokenClass(CurrentLexeme);
                }

                else if (CurrentChar >= '0' && CurrentChar <= '9')
                {
                    bool hasDecimalPoint = false;
                    bool isMalformed = false;

                    while (j < SourceCode.Length && SourceCode[j] >= '0' && SourceCode[j] <= '9')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    if (j < SourceCode.Length && SourceCode[j] == '.')
                    {
                        hasDecimalPoint = true;
                        CurrentLexeme += '.';
                        j++;

                        if (j < SourceCode.Length && SourceCode[j] >= '0' && SourceCode[j] <= '9')
                        {
                            while (j < SourceCode.Length && SourceCode[j] >= '0' && SourceCode[j] <= '9')
                            {
                                CurrentLexeme += SourceCode[j];
                                j++;
                            }
                        }
                        else
                        {isMalformed = true; }
                    }
                    if (hasDecimalPoint && CurrentLexeme.Count(c => c == '.') > 1)
                    { isMalformed = true; }

                    i = j - 1;

                    if (isMalformed)
                    {
                        Errors.Error_List.Add($"Malformed float literal: {CurrentLexeme}");
                    }
                    else
                    {
                        FindTokenClass(CurrentLexeme);
                    }
                }

                else if (CurrentChar == '"')
                {
                    i++;
                    while (i < SourceCode.Length && SourceCode[i] != '"')
                    {
                        CurrentLexeme += SourceCode[i];
                        i++;
                    }
                    if (i < SourceCode.Length && SourceCode[i] == '"')
                    {
                        FindTokenClass($"\"{CurrentLexeme}\"");
                    }
                    else
                    {
                        Errors.Error_List.Add("Unclosed string literal");
                    }
                }

                else if (CurrentChar == '/' && i + 1 < SourceCode.Length && SourceCode[i + 1] == '*')
                {
                    i += 2;
                    while (i + 1 < SourceCode.Length && !(SourceCode[i] == '*' && SourceCode[i + 1] == '/'))
                    {
                        i++;
                    }
                    if (i + 1 < SourceCode.Length)
                        i++; 
                    else
                        Errors.Error_List.Add("Unclosed comment");
                }
                else
                {
                    CurrentLexeme += CurrentChar;

                    if (CurrentChar == ':' || CurrentChar == '|' || CurrentChar == '&')
                    {
                        if (i + 1 < SourceCode.Length)
                        {
                            char NextChar = SourceCode[i + 1];
                            string CombinedLexeme = CurrentLexeme + NextChar;

                            if (Operators.ContainsKey(CombinedLexeme))
                            {
                                CurrentLexeme = CombinedLexeme;
                                i++; 
                                FindTokenClass(CurrentLexeme); 
                            }
                            else
                            {
                                Errors.Error_List.Add($"{CurrentChar}{NextChar} is not a valid operator.");
                            }
                        }
                        else
                        {
                            Errors.Error_List.Add($"{CurrentChar} is not a valid operator.");
                        }
                    }
                    else if (CurrentChar == '<')
                    {
                        if (i + 1 < SourceCode.Length)
                        {
                            char NextChar = SourceCode[i + 1];
                            string CombinedLexeme = CurrentLexeme + NextChar;

                            if (Operators.ContainsKey(CombinedLexeme))
                            {
                                CurrentLexeme = CombinedLexeme;
                                i++;
                            }
                        }
                        FindTokenClass(CurrentLexeme);
                    }
                    else
                    {
                        if (Operators.ContainsKey(CurrentLexeme))
                        {
                            FindTokenClass(CurrentLexeme); 
                        }
                        else
                        {
                            Errors.Error_List.Add($"{CurrentChar} is not a valid operator.");
                        }
                    }
                }
            }

            JASON_Compiler.TokenStream = Tokens;
        }

        void FindTokenClass(string Lex)
        {



            Token Tok = new Token();
            Tok.lex = Lex;

            if (ReservedWords.ContainsKey(Lex))
                Tok.token_type = ReservedWords[Lex];


            else if (isIdentifier(Lex))
                Tok.token_type = Token_Class.Identifier;

            else if (IsInteger(Lex) || IsFloat(Lex))
                Tok.token_type = Token_Class.Number;



            else if (IsString(Lex))
                Tok.token_type = Token_Class.QuotedString;

            else if (Operators.ContainsKey(Lex))
                Tok.token_type = Operators[Lex];




            else
            { Errors.Error_List.Add(Lex); }

            Tokens.Add(Tok);
        }

        bool isIdentifier(string lex)
        {
            return Regex.IsMatch(lex, @"^[A-Za-z][A-Za-z0-9_]*$");

        }

        bool IsInteger(string lex)
        {

            return Regex.IsMatch(lex, @"^[0-9]+$");
        }

        bool IsFloat(string lex)
        {

            return Regex.IsMatch(lex, @"^[0-9]+\.[0-9]+$");
        }

        bool IsString(string lex)
        {
            return Regex.IsMatch(lex, @"^""[^""]*""$");
        }


    }
}
