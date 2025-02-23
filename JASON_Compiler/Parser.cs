using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JASON_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();

        public string Name;
        public Node(string N)
        {
            this.Name = N;
        }
    }
    public class Parser
    {
        private int InputPointer = 0;
        private List<Token> TokenStream;
        public Node root;

        public Node StartParsing(List<Token> tokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = tokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        private Token CurrentToken => InputPointer < TokenStream.Count ? TokenStream[InputPointer] : null;

        private bool IsEndOfStream => CurrentToken == null;

        private bool MatchToken(Token_Class tokenClass)
        {
            return CurrentToken != null && CurrentToken.token_type == tokenClass;
        }

        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }

        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }

        private Node Program()
        {
            var programNode = new Node("Program");
            programNode.Children.Add(Functions());
            programNode.Children.Add(MainFunction());
            return programNode;
        }

        private Node Functions()
        {
            var functionsNode = new Node("Functions");
            if (IsEndOfStream || CheckMainFunction()) return functionsNode;

            functionsNode.Children.Add(FunctionStatement());
            functionsNode.Children.Add(Functions());
            return functionsNode;
        }

        private bool CheckMainFunction()
        {
            if (InputPointer + 3 >= TokenStream.Count) return false;

            int tempPointer = InputPointer;
            return MatchSequence(tempPointer, Token_Class.Int, Token_Class.Main, Token_Class.LParanthesis, Token_Class.RParanthesis);
        }

        private bool MatchSequence(int start, params Token_Class[] sequence)
        {
            for (int i = 0; i < sequence.Length; i++)
            {
                if (start + i >= TokenStream.Count || TokenStream[start + i].token_type != sequence[i])
                    return false;
            }
            return true;
        }

        private Node MainFunction()
        {
            var mainNode = new Node("MainFunction");
            mainNode.Children.Add(MatchAndConsume(Token_Class.Int));
            mainNode.Children.Add(MatchAndConsume(Token_Class.Main));
            mainNode.Children.Add(MatchAndConsume(Token_Class.LParanthesis));
            mainNode.Children.Add(MatchAndConsume(Token_Class.RParanthesis));
            mainNode.Children.Add(MainFunctionBody());
            return mainNode;
        }

        private Node FunctionStatement()
        {
            var functionNode = new Node("Function");
            functionNode.Children.Add(FunctionDeclaration());
            functionNode.Children.Add(FunctionBody());
            return functionNode;
        }

        private Node FunctionDeclaration()
        {
            var declarationNode = new Node("Declaration");

            if (MatchToken(Token_Class.Int)) declarationNode.Children.Add(MatchAndConsume(Token_Class.Int));
            else if (MatchToken(Token_Class.Float)) declarationNode.Children.Add(MatchAndConsume(Token_Class.Float));
            else if (MatchToken(Token_Class.String)) declarationNode.Children.Add(MatchAndConsume(Token_Class.String));

            declarationNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            declarationNode.Children.Add(MatchAndConsume(Token_Class.LParanthesis));
            declarationNode.Children.Add(Arguments());
            declarationNode.Children.Add(MatchAndConsume(Token_Class.RParanthesis));

            return declarationNode;
        }

        private Node Arguments()
        {
            var argsNode = new Node("Arguments");
            if (!IsEndOfStream &&
        (MatchToken(Token_Class.Int) || MatchToken(Token_Class.Float) || MatchToken(Token_Class.String)))
            {
                argsNode.Children.Add(ParameterList());
            }

            return argsNode;
        }

        private Node ParameterList()
        {
            var paramsNode = new Node("Parameters");
            if (IsEndOfStream) return paramsNode;

            paramsNode.Children.Add(Parameter());
            while (!IsEndOfStream && MatchToken(Token_Class.Comma))
            {
                paramsNode.Children.Add(MatchAndConsume(Token_Class.Comma));
                paramsNode.Children.Add(Parameter());
            }

            return paramsNode;
        }

        private Node Parameter()
        {
            var paramNode = new Node("Parameter");
            if (MatchToken(Token_Class.Int)) paramNode.Children.Add(MatchAndConsume(Token_Class.Int));
            else if (MatchToken(Token_Class.Float)) paramNode.Children.Add(MatchAndConsume(Token_Class.Float));
            else if (MatchToken(Token_Class.String)) paramNode.Children.Add(MatchAndConsume(Token_Class.String));
            paramNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            return paramNode;
        }


        private Node MainFunctionBody()
        {
            Node mainFB = new Node("Main Function Body");
            mainFB.Children.Add(MatchAndConsume(Token_Class.LCurly));
            mainFB.Children.Add(Statements());
            mainFB.Children.Add(MainReturnStatement());
            mainFB.Children.Add(MatchAndConsume(Token_Class.RCurly));
            return mainFB;
        }
        private Node MainReturnStatement()
        {
            Node mainReturn = new Node("Main Return");
            mainReturn.Children.Add(MatchAndConsume(Token_Class.Return));
            mainReturn.Children.Add(MatchAndConsume(Token_Class.Number));
            mainReturn.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            return mainReturn;
        }


        private Node FunctionBody()
        {
            var bodyNode = new Node("Function Body");
            bodyNode.Children.Add(MatchAndConsume(Token_Class.LCurly));
            bodyNode.Children.Add(Statements());
            bodyNode.Children.Add(ReturnStatement());
            bodyNode.Children.Add(MatchAndConsume(Token_Class.RCurly));
            return bodyNode;
        }

        private Node Statements()
        {
            var statementsNode = new Node("Statements");
            while (!IsEndOfStream && !MatchToken(Token_Class.Return) && !MatchToken(Token_Class.RCurly))
            {
                statementsNode.Children.Add(Statement());
            }
            return statementsNode;
        }

        private Node Statement()
        {
            var statementNode = new Node("Statement");

            if (MatchToken(Token_Class.Int) || MatchToken(Token_Class.Float) || MatchToken(Token_Class.String))
                statementNode.Children.Add(DeclarationStatement());
            else if (MatchToken(Token_Class.Write))
                statementNode.Children.Add(WriteStatement());
            else if (MatchToken(Token_Class.Read))
                statementNode.Children.Add(ReadStatement());
            else if (MatchToken(Token_Class.If))
                statementNode.Children.Add(IfStatement());
            else if (MatchToken(Token_Class.Repeat))
                statementNode.Children.Add(RepeatStatement());
            else if (MatchToken(Token_Class.Identifier))
                statementNode.Children.Add(AssignmentStatement());

            else
                Errors.Error_List.Add($"Unexpected token: {CurrentToken?.token_type}");

            return statementNode;
        }

        private Node DeclarationStatement()
        {
            var declNode = new Node("DeclarationStatement");

            if (MatchToken(Token_Class.Int) || MatchToken(Token_Class.Float) || MatchToken(Token_Class.String))
            {
                declNode.Children.Add(MatchAndConsume(CurrentToken.token_type)); // Consume the datatype
            }
            else
            {
                Errors.Error_List.Add($"Expected DataType but found: {CurrentToken?.token_type}");
                return declNode;
            }
            var idListNode = IdentifierList();
            if (idListNode.Children.Count == 0)
            {
                Errors.Error_List.Add("Expected at least one Identifier in DeclarationStatement.");
            }
            declNode.Children.Add(idListNode);

            if (MatchToken(Token_Class.Semicolon))
            {
                declNode.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            }
            else
            {
                Errors.Error_List.Add($"Expected ';' but found: {CurrentToken?.token_type}");
            }

            return declNode;
        }

        private Node IdentifierList()
        {
            var idListNode = new Node("IdentifierList");

            var firstIdNode = IdentifierWithOptionalAssignment();
            if (firstIdNode == null)
            {
                Errors.Error_List.Add("Expected Identifier in IdentifierList.");
                return idListNode;
            }
            idListNode.Children.Add(firstIdNode);

            while (MatchToken(Token_Class.Comma))
            {
                idListNode.Children.Add(MatchAndConsume(Token_Class.Comma));
                var nextIdNode = IdentifierWithOptionalAssignment();
                if (nextIdNode == null)
                {
                    Errors.Error_List.Add("Expected Identifier after ',' in IdentifierList.");
                    break;
                }
                idListNode.Children.Add(nextIdNode);
            }

            return idListNode;
        }

        private Node IdentifierWithOptionalAssignment()
        {
            var idNode = new Node("IdentifierWithAssignment");
            if (MatchToken(Token_Class.Identifier))
            {
                idNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            }
            else
            {
                Errors.Error_List.Add($"Expected Identifier but found: {CurrentToken?.token_type}");
                return null;
            }
            if (MatchToken(Token_Class.AssignmentOP))
            {
                idNode.Children.Add(MatchAndConsume(Token_Class.AssignmentOP));
                var exprNode = Expression();

                if (exprNode == null)
                {
                    Errors.Error_List.Add("Expected Expression after ':=' in IdentifierWithOptionalAssignment.");
                }
                else
                {
                    idNode.Children.Add(exprNode);
                }
            }

            return idNode;
        }


        private Node AssignmentStatement()
        {
            var assignmentNode = new Node("AssignmentStatement");
            assignmentNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            assignmentNode.Children.Add(MatchAndConsume(Token_Class.AssignmentOP));
            assignmentNode.Children.Add(Expression());
            assignmentNode.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            return assignmentNode;
        }

        private Node WriteStatement()
        {
            var writeNode = new Node("WriteStatement");
            writeNode.Children.Add(MatchAndConsume(Token_Class.Write));
            if (MatchToken(Token_Class.Endl)) { MatchAndConsume(Token_Class.Endl); MatchAndConsume(Token_Class.Semicolon); return writeNode; } //Write_Params
            writeNode.Children.Add(Expression());
            writeNode.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            return writeNode;
        }


        private Node ReadStatement()
        {
            var readNode = new Node("ReadStatement");
            readNode.Children.Add(MatchAndConsume(Token_Class.Read));
            readNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            readNode.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            return readNode;
        }
        private Node ReturnStatement()
        {
            var returnNode = new Node("Return");

            if (MatchToken(Token_Class.Return))
            {
                returnNode.Children.Add(MatchAndConsume(Token_Class.Return));
            }
            else
            {
                Errors.Error_List.Add($"Expected 'return' but found: {CurrentToken?.token_type}");
                return returnNode;
            }
            var exprNode = Expression();
            if (exprNode != null)
            {
                returnNode.Children.Add(exprNode);
            }
            else
            {
                Errors.Error_List.Add($"Invalid expression in ReturnStatement");
            }

            if (MatchToken(Token_Class.Semicolon))
            {
                returnNode.Children.Add(MatchAndConsume(Token_Class.Semicolon));
            }
            else
            {
                Errors.Error_List.Add($"Expected ';' but found: {CurrentToken?.token_type}");
            }

            return returnNode;
        }

        private Token PeekToken(int offset)
        {
            int peekIndex = InputPointer + offset;
            if (peekIndex >= 0 && peekIndex < TokenStream.Count)
            {
                return TokenStream[peekIndex];
            }
            return null;
        }


        private Node RepeatStatement()
        {
            var repeatNode = new Node("RepeatStatement");

            try
            {
                if (MatchToken(Token_Class.Repeat))
                {
                    repeatNode.Children.Add(MatchAndConsume(Token_Class.Repeat));
                    var stNode = new Node("Statements");

                    while (!MatchToken(Token_Class.Until) && !MatchToken(Token_Class.Return) && !MatchToken(Token_Class.RCurly))
                    {
                        stNode.Children.Add(Statement());
                    }
                    repeatNode.Children.Add(stNode);

                    if (MatchToken(Token_Class.Until) )
                    {
                        repeatNode.Children.Add(MatchAndConsume(Token_Class.Until));
                        repeatNode.Children.Add(Condition());
                    }
                    else
                    {
                        Errors.Error_List.Add("Error: Missing 'until' in repeat statement.");
                    }
                }
                else
                {
                    Errors.Error_List.Add("Error: Invalid repeat statement structure.");
                }
            }
            catch (Exception ex)
            {
                Errors.Error_List.Add($"Critical Error in RepeatStatement: {ex.Message}");
                return new Node("Error");
            }

            return repeatNode;
        }

        private Node IfStatement()
        {
            var ifNode = new Node("IfStatement");

            try
            {
                if (MatchToken(Token_Class.If))
                {
                    ifNode.Children.Add(MatchAndConsume(Token_Class.If));
                    ifNode.Children.Add(Condition());

                    if (MatchToken(Token_Class.Then))
                    {
                        ifNode.Children.Add(MatchAndConsume(Token_Class.Then));
                        var statementsNode = new Node("Statements");
                        while (!MatchToken(Token_Class.ElseIf) && !MatchToken(Token_Class.Else) && !MatchToken(Token_Class.End) && !MatchToken(Token_Class.Return) && !MatchToken(Token_Class.RCurly))
                        {
                            statementsNode.Children.Add(Statement());
                        }
                        ifNode.Children.Add(statementsNode);

                    }
                    else
                    {
                        Errors.Error_List.Add("Error: Missing 'then' in if statement.");
                    }

                    while (MatchToken(Token_Class.ElseIf))
                    {
                        ifNode.Children.Add(ElseIfClause());
                    }

                    if (MatchToken(Token_Class.Else))
                    {
                        ifNode.Children.Add(ElseClause());
                    }

                    if (MatchToken(Token_Class.End))
                    {
                        ifNode.Children.Add(MatchAndConsume(Token_Class.End));
                    }
                    else
                    {
                        Errors.Error_List.Add("Error: Missing 'end' in if statement.");
                    }
                }
                else
                {
                    Errors.Error_List.Add("Error: Invalid if statement structure.");
                }
            }
            catch (Exception ex)
            {
                Errors.Error_List.Add($"Critical Error in IfStatement: {ex.Message}");
                return new Node("Error");
            }

            return ifNode;
        }

        private Node ElseIfClause()
        {
            var elseifNode = new Node("ElseIfClause");

            try
            {
                if (MatchToken(Token_Class.ElseIf))
                {
                    elseifNode.Children.Add(MatchAndConsume(Token_Class.ElseIf));
                    elseifNode.Children.Add(Condition());

                    if (MatchToken(Token_Class.Then))
                    {
                        elseifNode.Children.Add(MatchAndConsume(Token_Class.Then));
                        var statementsNode = new Node("Statements");
                        while (!MatchToken(Token_Class.ElseIf) && !MatchToken(Token_Class.Else) && !MatchToken(Token_Class.End) && !MatchToken(Token_Class.Return) && !MatchToken(Token_Class.RCurly))
                        {
                            statementsNode.Children.Add(Statement());
                        }
                        elseifNode.Children.Add(statementsNode);
                    }
                    else
                    {
                        Errors.Error_List.Add("Error: Missing 'then' in elseif clause.");
                    }
                }
                else
                {
                    Errors.Error_List.Add("Error: Invalid elseif clause structure.");
                }
            }
            catch (Exception ex)
            {
                Errors.Error_List.Add($"Critical Error in ElseIfClause: {ex.Message}");
            }

            return elseifNode;
        }

        private Node ElseClause()
        {
            var elseNode = new Node("ElseClause");

            try
            {
                if (MatchToken(Token_Class.Else))
                {
                    elseNode.Children.Add(MatchAndConsume(Token_Class.Else));
                    var statementsNode = new Node("Statements");
                    while (!MatchToken(Token_Class.End) && !MatchToken(Token_Class.Return) && !MatchToken(Token_Class.RCurly))
                    {
                        statementsNode.Children.Add(Statement());
                    }
                    elseNode.Children.Add(statementsNode);
                }
                else
                {
                    Errors.Error_List.Add("Error: Invalid else clause structure.");
                }
            }
            catch (Exception ex)
            {
                Errors.Error_List.Add($"Critical Error in ElseClause: {ex.Message}");
            }

            return elseNode;
        }

        public Node Condition()
        {
            var conditionNode = new Node("Condition");

            try
            {
                if (MatchToken(Token_Class.Identifier))
                {
                    conditionNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
                }
                else if (MatchToken(Token_Class.Number))
                {
                    conditionNode.Children.Add(MatchAndConsume(Token_Class.Number));
                }
                else
                {
                    Errors.Error_List.Add($"Error: Expected Identifier or Number at the start of Condition, but found {CurrentToken?.token_type}");
                    return new Node("Error");
                }

                if (MatchToken(Token_Class.LessThanOp) || MatchToken(Token_Class.GreaterThanOp) ||
                    MatchToken(Token_Class.NotEqualOp) || MatchToken(Token_Class.EqualOp))
                {
                    conditionNode.Children.Add(MatchAndConsume(CurrentToken.token_type));
                }
                else
                {
                    Errors.Error_List.Add($"Error: Expected comparison operator (<, >, !=, ==), but found {CurrentToken?.token_type}");
                    return new Node("Error");
                }
                conditionNode.Children.Add(Term());
                while (MatchToken(Token_Class.AndOp) || MatchToken(Token_Class.OrOp))
                {

                    conditionNode.Children.Add(MatchAndConsume(CurrentToken.token_type));
                    conditionNode.Children.Add(Condition());
                }
            }
            catch (Exception ex)
            {
                Errors.Error_List.Add($"Critical Error in Condition: {ex.Message}");
                return new Node("Error");
            }

            return conditionNode;
        }

        private Node FunctionCall()
        {
            var funcCallNode = new Node("FunctionCall");

            if (MatchToken(Token_Class.Identifier))
            {
                funcCallNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            }
            else
            {
                Errors.Error_List.Add($"Expected function name but found: {CurrentToken?.token_type}");
                return funcCallNode;
            }

            if (MatchToken(Token_Class.LParanthesis))
            {
                funcCallNode.Children.Add(MatchAndConsume(Token_Class.LParanthesis));
            }
            else
            {
                Errors.Error_List.Add($"Expected '(' after function name but found: {CurrentToken?.token_type}");
                return funcCallNode;
            }

            var argsNode = new Node("CallArguments");
            if (MatchToken(Token_Class.Identifier) || MatchToken(Token_Class.Number) || MatchToken(Token_Class.String))
            {
                argsNode.Children.Add(ParseArgument());

                while (MatchToken(Token_Class.Comma))
                {
                    argsNode.Children.Add(MatchAndConsume(Token_Class.Comma));

                    if (MatchToken(Token_Class.Identifier) || MatchToken(Token_Class.Number) || MatchToken(Token_Class.String))
                    {
                        argsNode.Children.Add(ParseArgument());
                    }
                    else
                    {
                        Errors.Error_List.Add($"Expected argument after ',' but found: {CurrentToken?.token_type}");
                        break;
                    }
                }
            }
            funcCallNode.Children.Add(argsNode);

            if (MatchToken(Token_Class.RParanthesis))
            {
                funcCallNode.Children.Add(MatchAndConsume(Token_Class.RParanthesis));
            }
            else
            {
                Errors.Error_List.Add($"Expected ')' at the end of function call but found: {CurrentToken?.token_type}");
            }

            return funcCallNode;
        }
        private Node ParseArgument()
        {
            var argNode = new Node("Argument");

            if (MatchToken(Token_Class.Identifier))
            {
                argNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
            }
            else if (MatchToken(Token_Class.Number))
            {
                argNode.Children.Add(MatchAndConsume(Token_Class.Number));
            }
            else if (MatchToken(Token_Class.String))
            {
                argNode.Children.Add(MatchAndConsume(Token_Class.String));
            }
            else
            {
                Errors.Error_List.Add($"Invalid argument type: {CurrentToken?.token_type}");
            }

            return argNode;
        }


        private Node MatchAndConsume(Token_Class tokenClass)
        {
            if (MatchToken(tokenClass))
            {
                var tokenNode = new Node(CurrentToken.token_type.ToString());
                InputPointer++;
                return tokenNode;
            }
            Errors.Error_List.Add($"Expected {tokenClass} but found {CurrentToken?.token_type}");
            return new Node("Error");
        }

        private Node Expression()
        {
            var exprNode = new Node("Expression");

            if (MatchToken(Token_Class.QuotedString))
            {
                exprNode.Children.Add(MatchAndConsume(Token_Class.QuotedString)); // Matches a String
            }
            else if (MatchToken(Token_Class.Number) || MatchToken(Token_Class.Identifier) || MatchToken(Token_Class.LParanthesis))
            {
                var termOrEquationNode = TryParseEquationOrTerm();
                if (termOrEquationNode != null)
                {
                    exprNode.Children.Add(termOrEquationNode);
                }
                else
                {
                    Errors.Error_List.Add($"Unexpected token in Expression: {CurrentToken?.token_type}");
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add($"Unexpected token in Expression: {CurrentToken?.token_type}");
                return null;
            }

            return exprNode;
        }
        private Node TryParseEquationOrTerm()
        {
            if (MatchToken(Token_Class.LParanthesis) || PeekNextArithmeticOp())
            {
                return Equation();
            }
            else
            {
                return Term();
            }
        }
        private bool PeekNextArithmeticOp()
        {
            var nextToken = PeekToken(1);
            return nextToken != null &&
                   (nextToken.token_type == Token_Class.PlusOp ||
                    nextToken.token_type == Token_Class.MinusOp ||
                    nextToken.token_type == Token_Class.MultiplyOp ||
                    nextToken.token_type == Token_Class.DivideOp);
        }

        private Node Term()
        {
            var termNode = new Node("Term");

            if (MatchToken(Token_Class.Identifier))
            {
                if (PeekToken(1)?.token_type == Token_Class.LParanthesis)
                {
                    termNode.Children.Add(FunctionCall());
                }
                else
                {
                    termNode.Children.Add(MatchAndConsume(Token_Class.Identifier));
                }
            }
            else if (MatchToken(Token_Class.Number))
            {
                termNode.Children.Add(MatchAndConsume(Token_Class.Number));
            }
            else {
                Errors.Error_List.Add($"Parsing Error:Expected term but found {CurrentToken?.token_type}");
            }

            return termNode;
        }

        private Node Equation()
        {
            var equationNode = new Node("Equation");

            if (MatchToken(Token_Class.Number) || MatchToken(Token_Class.Identifier))
            {
                equationNode.Children.Add(Term());
                equationNode.Children.Add(EquationTail());
            }
            else if (MatchToken(Token_Class.LParanthesis))
            {
                equationNode.Children.Add(MatchAndConsume(Token_Class.LParanthesis));
                var innerEquation = Equation();
                if (innerEquation != null)
                {
                    equationNode.Children.Add(innerEquation);
                }
                else
                {
                    Errors.Error_List.Add($"Unexpected token after '(' in Equation: {CurrentToken?.token_type}");
                    return null;
                }

                if (MatchToken(Token_Class.RParanthesis))
                {
                    equationNode.Children.Add(MatchAndConsume(Token_Class.RParanthesis));
                }
                else
                {
                    Errors.Error_List.Add("Missing closing parenthesis ')' in Equation.");
                    return null;
                }

                equationNode.Children.Add(EquationTail());
            }
            else
            {
                Errors.Error_List.Add($"Unexpected token in Equation: {CurrentToken?.token_type}");
                return null;
            }

            return equationNode;
        }

        private Node EquationTail()
        {
            var tailNode = new Node("Equation_Tail");

            if (MatchToken(Token_Class.PlusOp) ||
                MatchToken(Token_Class.MinusOp) ||
                MatchToken(Token_Class.MultiplyOp) ||
                MatchToken(Token_Class.DivideOp))
            {
                tailNode.Children.Add(MatchAndConsume(CurrentToken.token_type));

                var termNode = Term();
                if (termNode != null)
                {
                    tailNode.Children.Add(termNode);
                }
                else
                {
                    Errors.Error_List.Add("Expected Term after arithmetic operator in Equation_Tail.");
                    return null;
                }
                var nextTailNode = EquationTail();
                if (nextTailNode != null)
                {
                    tailNode.Children.Add(nextTailNode);
                }
            }
            else
            { return null; }

            return tailNode;
        }
    }
}