using System.Collections.Generic;
using System.Linq;

namespace OneOf.TypeExtensions.CodeFix;

public class OneOfArgumentsInfo(TypeArgument[] typeArguments)
{
    public TypeArgument[] TypeArguments { get; } = typeArguments;
}

public class TypeArgument(string readableName, string fullyQualified, TypeArgument[] typeArguments, bool isNullableValueType, bool isNullableAnnotated, bool isTupleType, string? fieldName)
{
    private readonly string _readableName = ReadableTypeAlias(readableName).Capitalize();
    public string ReadableName
    {
        get
        {
        if (!NestedTypeArguments.Any())
        {
            return _readableName;
        }

        if (isNullableValueType)
        {
            // _readableName will be "Nullable", no need for "Of"
                return $"{_readableName}{string.Join("_", NestedTypeArguments.Select(x => x.ReadableName))}";
        }
        return $"{_readableName}Of{string.Join("_", NestedTypeArguments.Select(x => x.ReadableName))}";
    }
    }
    private readonly string _fullyQualified = ReadableFullyQualifiedTypeAlias(fullyQualified);
    public string FullyQualifiedName()
    {
        if (fieldName == null)
        {
            return FullyQualifiedNameNoFieldName();
        }
        return $"{FullyQualifiedNameNoFieldName()} {fieldName}";
    }

    private string FullyQualifiedNameNoFieldName()
    {
        if (isNullableValueType)
        {
            return _fullyQualified;
        }
        if (!NestedTypeArguments.Any())
        {
            if (isNullableAnnotated)
            {
                return $"{_fullyQualified}?";
            }
            return _fullyQualified;
        }

        if (isTupleType)
        {
            return $"({string.Join(", ", NestedTypeArguments.Select(x => x.FullyQualifiedName()))})";
        }

        return $"{_fullyQualified}<{string.Join(", ", NestedTypeArguments.Select(x => x.FullyQualifiedName()))}>";
    }

    public string HintName()
    {
        if (!NestedTypeArguments.Any())
        {
            if (isNullableAnnotated)
            {
                return $"Nullable{_fullyQualified}";
            }
            return _fullyQualified;
        }
        if (isNullableValueType || isTupleType)
        {
            return ReadableName;
        }
        return $"{_fullyQualified}<{string.Join("_", NestedTypeArguments.Select(x => x.FullyQualifiedName()))}>";
    }

    public HashSet<string> SystemNamespaces()
    {
        var systemNamespaces = new HashSet<string>();
        if (fullyQualified.StartsWith("System."))
        {
            var lastDotIndex = fullyQualified.LastIndexOf('.');
            var namespaceName = fullyQualified.Substring(0, lastDotIndex);
            if (namespaceName != "System")
            {
                systemNamespaces.Add(namespaceName);
            }
        }

        foreach (var systemNamespace in NestedTypeArguments.SelectMany(x => x.SystemNamespaces()))
        {
            systemNamespaces.Add(systemNamespace);
        }

        return systemNamespaces;
    }

    public bool IsNullableReferenceType()
    {
        return isNullableAnnotated && !isNullableValueType;
    }

    public readonly TypeArgument[] NestedTypeArguments = typeArguments;

    private static readonly Dictionary<string, string> FullyQualifiedTypeAliases = new()
    {
        {"System.Int32", "int"},
        {"System.String", "string"},
        {"System.Double", "double"},
        {"System.Int64", "long"},
        {"System.Int16", "short"},
        {"System.Boolean", "bool"},
        {"System.Object", "object"},
        {"System.Decimal", "decimal"},
        {"System.Single", "float"},
        {"System.Byte", "byte"},
        {"System.SByte", "sbyte"},
        {"System.UInt16", "ushort"},
        {"System.UInt32", "uint"},
        {"System.UInt64", "ulong"},
        {"System.Char", "char"},
        {"System.Int32?", "int?"},
        {"System.String?", "string?"},
        {"System.Double?", "double?"},
        {"System.Int64?", "long?"},
        {"System.Int16?", "short?"},
        {"System.Boolean?", "bool?"},
        {"System.Decimal?", "decimal?"},
        {"System.Single?", "float?"},
        {"System.Byte?", "byte?"},
        {"System.SByte?", "sbyte?"},
        {"System.UInt16?", "ushort?"},
        {"System.UInt32?", "uint?"},
        {"System.UInt64?", "ulong?"},
        {"System.Char?", "char?"},
    };

    private static readonly Dictionary<string, string> ReadableTypeAliases = new()
    {
        {"Int32", "int"},
        {"String", "string"},
        {"Double", "double"},
        {"Int64", "long"},
        {"Int16", "short"},
        {"Boolean", "bool"},
        {"Object", "object"},
        {"Decimal", "decimal"},
        {"Single", "float"},
        {"Byte", "byte"},
        {"SByte", "sbyte"},
        {"UInt16", "ushort"},
        {"UInt32", "uint"},
        {"UInt64", "ulong"},
        {"Char", "char"},
    };

    private static string ReadableFullyQualifiedTypeAlias(string fullName)
    {
        if (FullyQualifiedTypeAliases.TryGetValue(fullName, out var alias))
        {
            return alias;
        }

        if (fullName.StartsWith("System."))
        {
            var lastDotIndex = fullName.LastIndexOf('.');
            return fullName.Substring(lastDotIndex + 1);
        }
        return fullName;
    }
    private static string ReadableTypeAlias(string simpleName)
    {
        return ReadableTypeAliases.TryGetValue(simpleName, out var alias) ? alias : simpleName;
    }
}

public class OneOfArgumentsComparer : IEqualityComparer<OneOfArgumentsInfo?>
{
    public bool Equals(OneOfArgumentsInfo? x, OneOfArgumentsInfo? y)
    {
        if (x is null || y is null)
            return false;

        if (ReferenceEquals(x, y))
            return true;

        return TypeArgumentsEqual(x.TypeArguments, y.TypeArguments);
    }

    public int GetHashCode(OneOfArgumentsInfo? obj)
    {
        if (obj is null)
            return 0;

        unchecked
        {
            int hash = 19;
            foreach (var arg in obj.TypeArguments)
            {
                hash = hash * 31 + TypeArgumentHashCode(arg);
            }
            return hash;
        }
    }

    private bool TypeArgumentsEqual(TypeArgument[] xArgs, TypeArgument[] yArgs)
    {
        if (xArgs.Length != yArgs.Length)
            return false;

        for (int i = 0; i < xArgs.Length; i++)
        {
            if (!TypeArgumentEqual(xArgs[i], yArgs[i]))
                return false;
        }

        return true;
    }

    private bool TypeArgumentEqual(TypeArgument x, TypeArgument y)
    {
        if (x.ReadableName != y.ReadableName || x.FullyQualifiedName() != y.FullyQualifiedName())
            return false;

        return TypeArgumentsEqual(x.NestedTypeArguments, y.NestedTypeArguments);
    }

    private int TypeArgumentHashCode(TypeArgument arg)
    {
        unchecked
        {
            int hash = 19;
            hash = hash * 31 + arg.ReadableName.GetHashCode();
            hash = hash * 31 + arg.FullyQualifiedName().GetHashCode();
            foreach (var nestedArg in arg.NestedTypeArguments)
            {
                hash = hash * 31 + TypeArgumentHashCode(nestedArg);
            }
            return hash;
        }
    }
}
