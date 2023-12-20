using OneOf;
// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");
OneOf<string, (int? Id, string, char?)> oneOf = (1, "test", 'c');