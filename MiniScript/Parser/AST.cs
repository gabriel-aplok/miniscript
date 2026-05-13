using MiniScript.Lexer;

namespace MiniScript.Parser;

// expressions
public abstract record Expr;

public record AssignExpr(Token Name, Expr Value) : Expr;

public record BinaryExpr(Expr Left, Token Operator, Expr Right) : Expr;

public record CallExpr(Expr Callee, Token Paren, List<Expr> Arguments) : Expr;

public record LiteralExpr(object? Value) : Expr;

public record InterpolatedStringExpr(List<Expr> parts) : Expr;

public record LogicalExpr(Expr Left, Token Operator, Expr Right) : Expr;

public record UnaryExpr(Token Operator, Expr Right) : Expr;

public record VariableExpr(Token Name) : Expr;

public record ArrayExpr(List<Expr> Elements) : Expr;

public record IndexExpr(Expr Callee, Token Bracket, Expr Index) : Expr;

public record DictionaryExpr(Dictionary<Expr, Expr> Entries) : Expr;

public record SetExpr(Expr Callee, Token Bracket, Expr Index, Expr Value) : Expr;

// statements
public abstract record Stmt;

public record BlockStmt(List<Stmt> Statements) : Stmt;

public record ExpressionStmt(Expr Expression) : Stmt;

public record FunctionStmt(Token Name, List<Token> Params, Stmt Body) : Stmt;

public record IfStmt(Expr Condition, Stmt ThenBranch, Stmt? ElseBranch) : Stmt;

public record ReturnStmt(Token Keyword, Expr? Value) : Stmt;

public record VarStmt(Token Name, Expr? Initializer) : Stmt;

public record WhileStmt(Expr Condition, Stmt Body) : Stmt;
