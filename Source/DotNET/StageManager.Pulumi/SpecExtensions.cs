// Copyright (c) Cratis. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Cratis.Showrunner.StageManager.Pulumi;

/// <summary>
/// Builds the spec of a custom resource as plain dictionaries, leaving out what was not set so the operator's own defaults apply.
/// </summary>
internal static class SpecExtensions
{
    /// <summary>
    /// Creates an empty spec map.
    /// </summary>
    /// <returns>The map.</returns>
    public static Dictionary<string, object> NewSpec() => new(StringComparer.Ordinal);

    /// <summary>
    /// Sets a value, unless it is null.
    /// </summary>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name, in the camel case the custom resource uses.</param>
    /// <param name="value">The value, which may be a Pulumi input.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> Put(this Dictionary<string, object> spec, string key, object? value)
    {
        if (value is not null)
        {
            spec[key] = value;
        }

        return spec;
    }

    /// <summary>
    /// Sets a boolean, only when it is true.
    /// </summary>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">The flag.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutWhenTrue(this Dictionary<string, object> spec, string key, bool value) =>
        value ? spec.Put(key, true) : spec;

    /// <summary>
    /// Sets an enumeration value by its name, unless it is null.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">The value.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutEnum<TEnum>(this Dictionary<string, object> spec, string key, TEnum? value)
        where TEnum : struct, Enum =>
        value is null ? spec : spec.PutEnum(key, value.Value);

    /// <summary>
    /// Sets an enumeration value by its name.
    /// </summary>
    /// <typeparam name="TEnum">The type of the enumeration.</typeparam>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="value">The value.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutEnum<TEnum>(this Dictionary<string, object> spec, string key, TEnum value)
        where TEnum : struct, Enum =>
        spec.Put(key, value.ToString());

    /// <summary>
    /// Sets a list, unless it is empty.
    /// </summary>
    /// <typeparam name="T">The type of the items.</typeparam>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="items">The items, which may be Pulumi inputs.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutList<T>(this Dictionary<string, object> spec, string key, IEnumerable<T> items)
        where T : notnull
    {
        var list = items.Cast<object>().ToList();
        return list.Count == 0 ? spec : spec.Put(key, list);
    }

    /// <summary>
    /// Sets a map of strings, unless it is empty.
    /// </summary>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="values">The values, which may be Pulumi inputs.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutMap(this Dictionary<string, object> spec, string key, IEnumerable<KeyValuePair<string, object>> values)
    {
        var map = NewSpec();
        foreach (var (name, value) in values)
        {
            map[name] = value;
        }

        return map.Count == 0 ? spec : spec.Put(key, map);
    }

    /// <summary>
    /// Sets a nested object, unless it has no properties.
    /// </summary>
    /// <param name="spec">The map to set it in.</param>
    /// <param name="key">The property name.</param>
    /// <param name="nested">The nested object.</param>
    /// <returns>The same map.</returns>
    public static Dictionary<string, object> PutObject(this Dictionary<string, object> spec, string key, Dictionary<string, object> nested) =>
        nested.Count == 0 ? spec : spec.Put(key, nested);

    /// <summary>
    /// Describes a reference to a key of a Secret.
    /// </summary>
    /// <param name="name">The name of the Secret.</param>
    /// <param name="key">The key, or null for the operator's default.</param>
    /// <returns>The reference.</returns>
    public static Dictionary<string, object> SecretReference(string name, string? key) => Reference(name, key);

    /// <summary>
    /// Describes a reference to a key of a Secret or a ConfigMap.
    /// </summary>
    /// <param name="name">The name of the Secret or ConfigMap.</param>
    /// <param name="key">The key, or null for the operator's default.</param>
    /// <returns>The reference.</returns>
    public static Dictionary<string, object> Reference(string name, string? key) => NewSpec().Put("name", name).Put("key", key);
}
