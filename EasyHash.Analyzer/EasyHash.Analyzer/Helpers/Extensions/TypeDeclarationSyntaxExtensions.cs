namespace EasyHash.Analyzer.Helpers.Extensions
{
    using System;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    internal static class TypeDeclarationSyntaxExtensions
    {
        public static TypeDeclarationSyntax AddMembers(
           this TypeDeclarationSyntax node, params MemberDeclarationSyntax[] members)
        {
            switch (node.Kind())
            {
                case SyntaxKind.ClassDeclaration:
                    return ((ClassDeclarationSyntax)node).AddMembers(members);
                case SyntaxKind.InterfaceDeclaration:
                    return ((InterfaceDeclarationSyntax)node).AddMembers(members);
                case SyntaxKind.StructDeclaration:
                    return ((StructDeclarationSyntax)node).AddMembers(members);
                default:
                    throw new InvalidOperationException($"Can't add members to {node.Kind()}");
            }
        }
    }
}
