using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace OneOf.TypeExtensions.CodeFix;

public static class HelperExtensions
{
    private static string Indent(int indentLevel)
    {
        return new string(' ', indentLevel * 4);
    }
    public static StringBuilder AppendIndentedLine(this StringBuilder sb, int indentLevel, string str)
    {
        sb.AppendLine(Indent(indentLevel) + str);
        return sb;
    }

    public static string Capitalize(this string str)
    {
        if (!char.IsUpper(str[0]))
        {
            return char.ToUpperInvariant(str[0]) + str.Substring(1);
        }
        return str;
    }
    private static readonly SymbolDisplayFormat FullyQualifiedNameFormat = new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces);

    public static TypeArgument GetTypeArgument(ITypeSymbol typeSymbol)
    {
        var isNullableValueType = typeSymbol is { IsValueType: true, NullableAnnotation: NullableAnnotation.Annotated };
        var isNullableAnnotated = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        var isTupleType = typeSymbol.IsTupleType;
        var readableName = typeSymbol.Name;
        var fullyQualifiedName = typeSymbol.ToDisplayString(FullyQualifiedNameFormat);

        TypeArgument[] nestedTypeArguments = Array.Empty<TypeArgument>();
        // If the tuple elements have default names then we just want to treat them as generic arguments
        if (typeSymbol.IsTupleType && typeSymbol is INamedTypeSymbol tuple 
                && !tuple.TupleElements.All(x => x.Name.StartsWith("Item") && char.IsDigit(x.Name.LastOrDefault())))
        {
            nestedTypeArguments = tuple.TupleElements
                .Select(GetTypeArgument)
                .ToArray();
        }
        else if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
        {
            nestedTypeArguments = namedTypeSymbol.TypeArguments
                .OfType<INamedTypeSymbol>()
                .Select(GetTypeArgument)
                .ToArray();
        }
        return new TypeArgument(readableName, fullyQualifiedName, nestedTypeArguments, isNullableValueType, isNullableAnnotated, isTupleType, null);
    }

    private static TypeArgument GetTypeArgument(IFieldSymbol fieldSymbol)
    {
        var typeSymbol = fieldSymbol.Type;
        var isNullableValueType = typeSymbol is { IsValueType: true, NullableAnnotation: NullableAnnotation.Annotated };
        var isNullableAnnotated = typeSymbol.NullableAnnotation == NullableAnnotation.Annotated;
        var isTupleType = typeSymbol.IsTupleType;
        var readableName = typeSymbol.Name;
        var fullyQualifiedName = typeSymbol.ToDisplayString(FullyQualifiedNameFormat);

        TypeArgument[] nestedTypeArguments = Array.Empty<TypeArgument>();
        if (typeSymbol.IsTupleType && typeSymbol is INamedTypeSymbol tuple
                && !tuple.TupleElements.All(x => x.Name.StartsWith("Item") && char.IsDigit(x.Name.LastOrDefault())))
        {
            nestedTypeArguments = tuple.TupleElements
                .Select(GetTypeArgument)
                .ToArray();
        }
        else if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
        {
            nestedTypeArguments = namedTypeSymbol.TypeArguments
                .OfType<INamedTypeSymbol>()
                .Select(GetTypeArgument)
                .ToArray();
        }
        return new TypeArgument(readableName, fullyQualifiedName, nestedTypeArguments, isNullableValueType, isNullableAnnotated, isTupleType, fieldSymbol.Name);
    }
}