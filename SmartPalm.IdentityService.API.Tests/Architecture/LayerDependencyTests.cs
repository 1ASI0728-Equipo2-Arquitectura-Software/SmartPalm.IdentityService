using System.Reflection;
using SmartPalm.IdentityService.Domain.Users;

namespace SmartPalm.IdentityService.API.Tests.Architecture;

public class LayerDependencyTests
{
    [Fact]
    public void DomainPublicApi_DoesNotExposeOuterLayers()
    {
        var forbiddenPrefixes = new[]
        {
            "SmartPalm.IdentityService.Application",
            "SmartPalm.IdentityService.Infrastructure",
            "SmartPalm.IdentityService.Interfaces",
            "SmartPalm.IdentityService.Api"
        };

        var violations = typeof(User).Assembly.GetTypes()
            .Where(type => type.Namespace?.StartsWith("SmartPalm.IdentityService.Domain", StringComparison.Ordinal) == true)
            .SelectMany(GetExposedTypes)
            .Where(type => forbiddenPrefixes.Any(prefix =>
                type.Namespace?.StartsWith(prefix, StringComparison.Ordinal) == true))
            .Select(type => type.FullName)
            .Distinct()
            .ToArray();

        Assert.Empty(violations);
    }

    [Fact]
    public void CommandAndQueryTypes_UseDomainModelNamespaces()
    {
        var domainTypes = typeof(User).Assembly.GetTypes();
        var commands = domainTypes.Where(type => type.Name.EndsWith("Command", StringComparison.Ordinal)).ToArray();
        var queries = domainTypes.Where(type => type.Name.EndsWith("Query", StringComparison.Ordinal)).ToArray();

        Assert.NotEmpty(commands);
        Assert.NotEmpty(queries);
        Assert.All(commands, type => Assert.Equal("SmartPalm.IdentityService.Domain.Model.Commands", type.Namespace));
        Assert.All(queries, type => Assert.Equal("SmartPalm.IdentityService.Domain.Model.Queries", type.Namespace));
    }

    private static IEnumerable<Type> GetExposedTypes(Type type)
    {
        foreach (var constructor in type.GetConstructors(BindingFlags.Instance | BindingFlags.Public))
        foreach (var parameter in constructor.GetParameters())
            yield return Unwrap(parameter.ParameterType);

        foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
        {
            yield return Unwrap(method.ReturnType);
            foreach (var parameter in method.GetParameters())
                yield return Unwrap(parameter.ParameterType);
        }

        foreach (var property in type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly))
            yield return Unwrap(property.PropertyType);
    }

    private static Type Unwrap(Type type)
    {
        if (type.IsArray) return type.GetElementType() ?? type;
        if (type.IsGenericType)
            return type.GetGenericArguments().FirstOrDefault() ?? type;
        return type;
    }
}
