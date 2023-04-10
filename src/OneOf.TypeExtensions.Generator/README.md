## OneOf.TypeExtensions

An incremental source generator that creates extension methods for OneOf<> types for more readable code.

### Important

Source generated files can have very long names and will likely exceed the path length limit of 256 if you have not disabled it on your computer. See [this article](https://www.autodesk.com/support/technical/article/caas/sfdcarticles/sfdcarticles/The-Windows-10-default-path-length-limitation-MAX-PATH-is-256-characters.html) for instructions on increasing the limit on Windows.

### Installation

Via nuget `NuGet\Install-Package OneOf.TypeExtensions`

### Breaking changes

This project is still in beta and therefore breaking changes may occur. The only thing I forsee changing is how generated functions will be named when there are generics. Currently, `OneOf<int, Dictionary<string, int>>` will create extension methods like so `oneOf.IsDictionaryOfString_Int`. This looks fine but it can get a little ugly when there are multiple nested generics.

Also, generated code currently has no namespace which is fine with me but may be changed based on feedback.

### Performance

I have tried to make the generator somewhat performant, but I am by no means an expert at incremental source generators. Feel free to submit a PR improving it's efficiency.

### Example Generated Code

For the type `OneOf<int, string>`, these extension methods will be generated.

`OneOfExtensions_String_Int.g.cs`
```csharp
// <auto-generated>

using OneOf;

public static partial class OneOfTypeExtensions
{
    public static bool IsString(this OneOf<string, int> oneOf)
    {
        return oneOf.IsT0;
    }
    public static string AsString(this OneOf<string, int> oneOf)
    {
        return oneOf.AsT0;
    }
    public static OneOf<TResult, int> MapString<TResult>(this OneOf<string, int> oneOf, Func<string, TResult> mapFunc)
    {
        return oneOf.MapT0(mapFunc);
    }
    public static bool TryPickString(this OneOf<string, int> oneOf, out string value, out int remainder)
    {
        return oneOf.TryPickT0(out value, out remainder);
    }
    public static bool IsInt(this OneOf<string, int> oneOf)
    {
        return oneOf.IsT1;
    }
    public static int AsInt(this OneOf<string, int> oneOf)
    {
        return oneOf.AsT1;
    }
    public static OneOf<string, TResult> MapInt<TResult>(this OneOf<string, int> oneOf, Func<int, TResult> mapFunc)
    {
        return oneOf.MapT1(mapFunc);
    }
    public static bool TryPickInt(this OneOf<string, int> oneOf, out int value, out string remainder)
    {
        return oneOf.TryPickT1(out value, out remainder);
    }
}
```

### Supported Types

I believe the generator should support practically any type, if you find a type or combination of types that causes an error, feel free to create an issue or submit a PR.

Special consideration was given to support these types
- [X] Nullable value types `int?`
- [X] Nullable annotated reference types `string?`
- [X] Value Tuples `(int?, string?)`
- [X] Generics `List<int?>`
- [X] Multiple Generics `Dictionary<string?, int?>`
- [X] Nested generics `List<List<int?>>`
- [X] Nested types `Class1.Class2`
- [X] Any combination of the above

### Roadmap

The project is still very much at it's infancy. I have a few ideas for future features.
- Control what extension methods get generated through csproj properties
- Improve efficiency of generator
- Better extension method names with nested generics
- Keep hint names shorter and more understandable while ensuring they are unique