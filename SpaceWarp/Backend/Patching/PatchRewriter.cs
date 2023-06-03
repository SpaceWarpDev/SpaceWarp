using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SpaceWarp.Backend.Patching;

public class PatchRewriter : CSharpSyntaxRewriter
{
    public override SyntaxNode Visit(SyntaxNode n)
    {
        var classes = n.ChildNodes().OfType<ClassDeclarationSyntax>().Where(node =>
            (from attrList in node.AttributeLists
                from attr in attrList.Attributes
                where attr.Name.ToString() == "DependsOn"
                select attr).Any(attr =>
                (from arg in attr.ArgumentList?.Arguments
                    where arg.Expression.Kind() == SyntaxKind.StringLiteralExpression
                    select (LiteralExpressionSyntax)arg.Expression
                    into lit
                    select lit.Token.ValueText)
                .Any(value => SpaceWarpManager.SpaceWarpPlugins.All(x => x.Guid != value)))).ToArray();
        if (classes.Any())
        {
            n = n.RemoveNodes(classes, SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.KeepTrailingTrivia);
        }
        classes = n!.ChildNodes().OfType<ClassDeclarationSyntax>().Where(node =>
            (from attrList in node.AttributeLists
                from attr in attrList.Attributes
                where attr.Name.ToString() == "SkipOn"
                select attr).Any(attr =>
                (from arg in attr.ArgumentList?.Arguments
                    where arg.Expression.Kind() == SyntaxKind.StringLiteralExpression
                    select (LiteralExpressionSyntax)arg.Expression
                    into lit
                    select lit.Token.ValueText)
                .Any(value => SpaceWarpManager.SpaceWarpPlugins.Any(x => x.Guid == value)))).ToArray();
        // Now lets add the "RemoveOn" attribute
        return base.Visit(n);
    }

    public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax n)
    {
        var methods = n.ChildNodes().OfType<MethodDeclarationSyntax>().Where(node =>
            (from attrList in node.AttributeLists
                from attr in attrList.Attributes
                where attr.Name.ToString() == "DependsOn"
                select attr).Any(attr =>
                (from arg in attr.ArgumentList?.Arguments
                    where arg.Expression.Kind() == SyntaxKind.StringLiteralExpression
                    select (LiteralExpressionSyntax)arg.Expression
                    into lit
                    select lit.Token.ValueText)
                .Any(value => SpaceWarpManager.SpaceWarpPlugins.All(x => x.Guid != value)))).ToArray();
        if (methods.Any())
        {
            n = n.RemoveNodes(methods, SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.KeepTrailingTrivia)!;
        }
        methods = n!.ChildNodes().OfType<MethodDeclarationSyntax>().Where(node =>
            (from attrList in node.AttributeLists
                from attr in attrList.Attributes
                where attr.Name.ToString() == "SkipOn"
                select attr).Any(attr =>
                (from arg in attr.ArgumentList?.Arguments
                    where arg.Expression.Kind() == SyntaxKind.StringLiteralExpression
                    select (LiteralExpressionSyntax)arg.Expression
                    into lit
                    select lit.Token.ValueText)
                .Any(value => SpaceWarpManager.SpaceWarpPlugins.Any(x => x.Guid == value)))).ToArray();
        if (methods.Any())
        {
            n = n!.RemoveNodes(methods, SyntaxRemoveOptions.KeepLeadingTrivia | SyntaxRemoveOptions.KeepTrailingTrivia)!;
        }
        return base.VisitClassDeclaration(n!);
    }
}