using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Roslyn_Extract_Methods.Database;

namespace Roslyn_Extract_Methods {
    internal class ApiSequenceExtractor : CSharpSyntaxWalker {
        private readonly SemanticModel _model;
        private string _lastCalledMethodReturnType;

        public ApiSequenceExtractor(SemanticModel model) {
            _model = model;
        }

        public List<ApiCall> Calls { get; } = new List<ApiCall>();
        public List<MethodParameter> MethodParameters { get; } = new List<MethodParameter>();

        public string GetFullMethodName(MethodDeclarationSyntax methodNode) {
            var declaredSymbol = _model.GetDeclaredSymbol(methodNode);
            return declaredSymbol.ContainingSymbol.ToDisplayString() + "." + methodNode.Identifier.ToString();
        }

        public override void VisitParameter(ParameterSyntax node) {
            var parameterSymbol = _model.GetDeclaredSymbol(node);
            var parameterSymbolType = parameterSymbol.Type;
            var properTypeName = GetProperTypeName(parameterSymbolType);
            MethodParameters.Add(new MethodParameter(properTypeName, node.Identifier.Text));
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
            try {
                var ctorSymbol = _model.GetTypeInfo(node).Type;
                if (node.ArgumentList != null) {
                    foreach (var argumentSyntax in node.ArgumentList.Arguments) {
                        argumentSyntax.Accept(this);
                    }
                }
                var properTypeName = GetProperTypeName(ctorSymbol);
                Calls.Add(ApiCall.OfConstructor(properTypeName));
                _lastCalledMethodReturnType = properTypeName;
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
            }
        }

        public override void VisitUsingStatement(UsingStatementSyntax node) {
            Visit(node.Declaration);
            var typeInUsing = _lastCalledMethodReturnType;
            Visit(node.Expression);
            Visit(node.Statement);
            Calls.Add(ApiCall.OfMethodInvocation(typeInUsing, "Dispose"));
        }

        private static string GetProperTypeName(ISymbol type) {
            var displayFormat = new SymbolDisplayFormat(
                SymbolDisplayGlobalNamespaceStyle.Omitted,
                SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
                SymbolDisplayGenericsOptions.IncludeTypeConstraints,
                miscellaneousOptions: SymbolDisplayMiscellaneousOptions.ExpandNullable
            );
            var displayString = type.ToDisplayString(displayFormat);
            if (!displayString.Contains(".")) {
                displayString = type.ContainingSymbol.ToDisplayString();
            }

            return displayString;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
            foreach (var argumentSyntax in node.ArgumentList.Arguments) {
                argumentSyntax.Accept(this);
            }
            if (node.Expression is IdentifierNameSyntax) {
                var methodName = (IdentifierNameSyntax) node.Expression;
                var method = _model.GetSymbolInfo(methodName).Symbol;
                if (method == null || method.Name.StartsWith("_")) return;
                Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(method), method.Name));
                UpdateLastCalledMethod(method);
            }
            else node.Expression.Accept(this);
            UpdateLastCalledMethod(_model.GetSymbolInfo(node).Symbol);
        }

        public override void VisitIfStatement(IfStatementSyntax node) {
            node.Condition.Accept(this);
            node.Statement.Accept(this);
            node.Else?.Accept(this);
        }

        public override void VisitWhileStatement(WhileStatementSyntax node) {
            node.Condition.Accept(this);
            node.Statement.Accept(this);
        }

        private void UpdateLastCalledMethod(ISymbol method) {
            if (method == null) {
                _lastCalledMethodReturnType = null;
                return;
            }
            if (method is IMethodSymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((IMethodSymbol) method).ReturnType);
                return;
            }
            if (method is IPropertySymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((IPropertySymbol) method).Type);
                return;
            }
            if (method is IFieldSymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((IFieldSymbol) method).Type);
                return;
            }
            if (method is IEventSymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((IEventSymbol) method).Type);
                return;
            }
            if (method is IParameterSymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((IParameterSymbol) method).Type);
                return;
            }
            if (method is ILocalSymbol) {
                _lastCalledMethodReturnType = GetProperTypeName(((ILocalSymbol) method).Type);
                return;
            }
            _lastCalledMethodReturnType = "__oops__";
            //throw new NotImplementedException("Function called is something unaccounted for.");
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
            var method = _model.GetSymbolInfo(node.Name).Symbol;
            if (method == null) return;
            //TODO: I don't know if there'a any PROPER way to differentiate between parameterless method call and property access.
            if (method.Name.StartsWith("_")) return;

            if (node.Expression is IdentifierNameSyntax) {
                // The target is a simple identifier, the code being analysed is of the form
                // "command.ExecuteReader()" and memberAccess.Expression is the "command"
                // node
                var variable = (IdentifierNameSyntax) node.Expression;
                var variableTypeSymbol = _model.GetTypeInfo(variable).Type;
                var variableType = GetProperTypeName(variableTypeSymbol);
                
                if (variableTypeSymbol == null || variableType == null) return; //throw new ArgumentNullException(nameof(method));
                Calls.Add(ApiCall.OfMethodInvocation(variableType, method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is InvocationExpressionSyntax) {
                // The target is another invocation, the code being analysed is of the form
                // "GetCommand().ExecuteReader()" and memberAccess.Expression is the
                // "GetCommand()" node
                var invocationSyntax = (InvocationExpressionSyntax) node.Expression;
                invocationSyntax.Accept(this);
                //TODO: DANGER! I assume that the true last called method is stored
                if (_lastCalledMethodReturnType == null) return;
                Calls.Add(ApiCall.OfMethodInvocation(_lastCalledMethodReturnType, method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is LiteralExpressionSyntax) {
                var literalSyntax = (LiteralExpressionSyntax) node.Expression;
                var literalType = _model.GetTypeInfo(literalSyntax).Type;
                
                if (literalType == null) return;
                Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(literalType), method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is PredefinedTypeSyntax) {
                var typeSyntax = (PredefinedTypeSyntax) node.Expression;
                var typeName = GetProperTypeName(_model.GetTypeInfo(typeSyntax).Type);
                Calls.Add(ApiCall.OfMethodInvocation(typeName, method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is MemberAccessExpressionSyntax) {
                var memberSyntax = (MemberAccessExpressionSyntax) node.Expression;
                var tryType = _model.GetTypeInfo(memberSyntax);
                string type;
                if (tryType.Equals(null)) {
                    memberSyntax.Accept(this);
                    type = _lastCalledMethodReturnType;
                }
                else type = GetProperTypeName(tryType.Type);
                
                Calls.Add(ApiCall.OfMethodInvocation(type, method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is ObjectCreationExpressionSyntax) {
                var objSyntax = (ObjectCreationExpressionSyntax) node.Expression;
                var type = _model.GetTypeInfo(objSyntax).Type;
                if (type == null) return;
                Calls.Add(ApiCall.OfConstructor(GetProperTypeName(type)));
                if (node.Name != null) {
                    Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(type), method.Name));
                }
                objSyntax.ArgumentList?.Accept(this);
                objSyntax.Initializer?.Accept(this);
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is InstanceExpressionSyntax) {
                var instSyntax = (InstanceExpressionSyntax) node.Expression;
                var type = _model.GetTypeInfo(instSyntax).Type;
                
                if (type == null) return;
                Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(type), method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is ParenthesizedExpressionSyntax) {
                var parenSyntax = (ParenthesizedExpressionSyntax) node.Expression;
                parenSyntax.Expression.Accept(this);
                //TODO: danger with last called method
                var type = _lastCalledMethodReturnType;
                
                Calls.Add(ApiCall.OfMethodInvocation(type, method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            if (node.Expression is TypeOfExpressionSyntax) {
                var typeofSyntax = (TypeOfExpressionSyntax) node.Expression;
                var type = _model.GetTypeInfo(typeofSyntax.Type).Type;
                
                if (type == null) return;
                Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(type), method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
//            if (node.Expression is ElementAccessExpressionSyntax) {
//                var accessSyntax = (ElementAccessExpressionSyntax)node.Expression;
//                var type = _model.GetTypeInfo(accessSyntax.).Type;
//                var method = _model.GetSymbolInfo(node.Name).Symbol;
//                Calls.Add(ApiCall.OfMethodInvocation(type.Name, method.Name));
//                updateLastCalledMethod(method);
//                return;
//            }
            if (node.Expression is ElementAccessExpressionSyntax) return;//not interested.
            if (node.Expression is GenericNameSyntax) {
                var genericSyntax = (GenericNameSyntax)node.Expression;
                var type = _model.GetTypeInfo(genericSyntax).Type;
                
                if (type == null) return;
                Calls.Add(ApiCall.OfMethodInvocation(GetProperTypeName(type), method.Name));
                UpdateLastCalledMethod(method);
                return;
            }
            Console.WriteLine("Missed something!");
        }
    }
}