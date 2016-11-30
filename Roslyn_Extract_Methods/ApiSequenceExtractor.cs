using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Roslyn_Extract_Methods {
    internal class ApiSequenceExtractor : CSharpSyntaxWalker {
        private string _lastCalledMethod;
        private readonly SemanticModel _model;

        public ApiSequenceExtractor(SemanticModel model) {
            this._model = model;
        }

        public List<ApiCall> Calls { get; } = new List<ApiCall>();
//        public override void VisitVariableDeclarator(VariableDeclaratorSyntax node) {
//            var type = model.GetTypeInfo(node.Initializer.Value).Type;
//            Calls.Add(ApiCall.ofConstructor(type.Name));
//        }
        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node) {
            try {
                var ctorSymbol = _model.GetTypeInfo(node).Type;
                foreach (var argumentSyntax in node.ArgumentList?.Arguments ?? new SeparatedSyntaxList<ArgumentSyntax>()) {
                    argumentSyntax.Accept(this);
                }
                Calls.Add(ApiCall.OfConstructor(ctorSymbol.Name));
            }
            catch (Exception e) {
                Console.WriteLine(e.Message);
                Console.ReadLine();
            }
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node) {
            foreach (var argumentSyntax in node.ArgumentList.Arguments) {
                argumentSyntax.Accept(this);
            }
            node.Expression.Accept(this);
            updateLastCalledMethod(_model.GetSymbolInfo(node).Symbol);
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

        private void updateLastCalledMethod(ISymbol method) {
            if (method is IMethodSymbol) {
                _lastCalledMethod = ((IMethodSymbol)method).ReturnType.Name;
            } else {
                if (method is IPropertySymbol) {
                    _lastCalledMethod = ((IPropertySymbol)method).Type.Name;
                } else {
                    if (method is IFieldSymbol) {
                        _lastCalledMethod = ((IFieldSymbol) method).Type.Name;
                    }
                    else {
                        if (method is IEventSymbol) {
                            _lastCalledMethod = ((IEventSymbol) method).Type.Name;
                        }
                        else throw new NotImplementedException("Function called is nor method, nor property, but something unaccounted for.");
                    }
                        
                }
            }
        }

        public override void VisitMemberAccessExpression(MemberAccessExpressionSyntax node) {
            if (node.Expression is IdentifierNameSyntax) {
                // The target is a simple identifier, the code being analysed is of the form
                // "command.ExecuteReader()" and memberAccess.Expression is the "command"
                // node
                var variable = (IdentifierNameSyntax) node.Expression;
                var variableType = _model.GetTypeInfo(variable).Type.Name;
                var method = _model.GetSymbolInfo(node.Name).Symbol;
                if (method == null) throw new ArgumentNullException(nameof(method));
                Calls.Add(ApiCall.OfMethodInvocation(variableType, method.Name));
                updateLastCalledMethod(method);
                return;
            }
            if (node.Expression is InvocationExpressionSyntax) {
                var invocSyntax = (InvocationExpressionSyntax) node.Expression;
                var method = _model.GetSymbolInfo(node.Name).Symbol;
                invocSyntax.Accept(this);
                //TODO: DANGER! I assume that the true last called method is stored
                if (_lastCalledMethod == null) {
                    Console.WriteLine("Mistake!");
                }
                Calls.Add(ApiCall.OfMethodInvocation(_lastCalledMethod, method.Name));
                updateLastCalledMethod(method);
                // The target is another invocation, the code being analysed is of the form
                // "GetCommand().ExecuteReader()" and memberAccess.Expression is the
                // "GetCommand()" node
                return;
            }
            if (node.Expression is LiteralExpressionSyntax) {
                var litSyntax = (LiteralExpressionSyntax)node.Expression;
                var litType = _model.GetTypeInfo(litSyntax).Type;
                var method = _model.GetSymbolInfo(node.Name).Symbol;
                Calls.Add(ApiCall.OfMethodInvocation(litType.Name, method.Name));
                updateLastCalledMethod(method);
                return;
            }
            if (node.Expression is PredefinedTypeSyntax) {
                var typeSyntax = (PredefinedTypeSyntax) node.Expression;
                var typeName = _model.GetTypeInfo(typeSyntax).Type.Name;
                var method = _model.GetSymbolInfo(node.Name).Symbol;
                Calls.Add(ApiCall.OfMethodInvocation(typeName, method.Name));
                updateLastCalledMethod(method);
                return;
            }
            if (node.Expression is MemberAccessExpressionSyntax) {
                Console.WriteLine("NOT IMPLEMENTED");
//                var memberSyntax = (MemberAccessExpressionSyntax) node.Expression;
//                var type = model.GetTypeInfo(memberSyntax.Expression).Type;
//                var method = model.GetSymbolInfo(node.Name).Symbol;
//                Calls.Add(ApiCall.ofMethodInvocation(type.Name, method.Name));
//                updateLastCalledMethod(method);

                // The target is a member access, the code being analysed is of the form
                // "x.Command.ExecuteReader()" and memberAccess.Expression is the "x.Command"
                // node
                return;
            }
            if (node.Expression is ObjectCreationExpressionSyntax) {
                var objSyntax = (ObjectCreationExpressionSyntax) node.Expression;
                var type = _model.GetTypeInfo(objSyntax).Type;
                Calls.Add(ApiCall.OfConstructor(type.Name));
                if (node.Name != null) {
                    var method = _model.GetSymbolInfo(node.Name).Symbol;
                    Calls.Add(ApiCall.OfMethodInvocation(type.Name, method.Name));
                }
                objSyntax.ArgumentList?.Accept(this);
                objSyntax.Initializer?.Accept(this);
                return;
            }
            if (node.Expression is InstanceExpressionSyntax) {
                var instSyntax = (InstanceExpressionSyntax) node.Expression;
                var type = _model.GetTypeInfo(instSyntax).Type;
                var method = _model.GetSymbolInfo(node.Name).Symbol;
                Calls.Add(ApiCall.OfMethodInvocation(type.Name, method.Name));
                updateLastCalledMethod(method);
                return;
            }
            Console.WriteLine("Missed something!");
        }
    }
}