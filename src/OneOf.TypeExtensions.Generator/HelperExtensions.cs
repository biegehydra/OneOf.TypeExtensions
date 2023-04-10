using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text;

namespace OneOf.TypeExtensions.Generator;

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
        if (typeSymbol is INamedTypeSymbol { IsGenericType: true } namedTypeSymbol)
        {
            nestedTypeArguments = namedTypeSymbol.TypeArguments
                .OfType<INamedTypeSymbol>()
                .Select(GetTypeArgument)
                .ToArray();
        }
        return new TypeArgument(readableName, fullyQualifiedName, nestedTypeArguments, isNullableValueType, isNullableAnnotated, isTupleType);
    }
}