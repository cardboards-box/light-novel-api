namespace LightNovelCore.DataSet;

/// <summary>
/// Indicates that a property corresponds to an element in an array
/// </summary>
/// <param name="index">The index of the item in the array</param>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ArrayIndexAttribute(int index) : Attribute
{
	/// <summary>
	/// The index of the item in the array
	/// </summary>
	public int Index { get; } = index;
}

/// <summary>
/// A converter for getting class instances from JSON arrays
/// </summary>
/// <typeparam name="T">The type of class</typeparam>
public class ArrayConverter<T> : JsonConverter<T>
{
	/// <inheritdoc />
	public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		if (reader.TokenType != JsonTokenType.StartArray)
			throw new JsonException($"Expected StartArray token, but got {reader.TokenType}");

		var properties = ArrayConverterHelper.GetProperties(typeof(T));
		if (properties.Length == 0)
			throw new JsonException($"Type {typeof(T).FullName} does not have any properties with {nameof(ArrayIndexAttribute)}");

		using var doc = JsonDocument.ParseValue(ref reader);
		if (doc.RootElement.ValueKind != JsonValueKind.Array)
			throw new JsonException($"Expected JSON array for {typeof(T).FullName}");

		var items = doc.RootElement.EnumerateArray().ToArray();

		var instance = Activator.CreateInstance<T>() 
			?? throw new JsonException($"Unable to create instance of {typeof(T).FullName}");

		foreach (var ap in properties)
		{
			if (!ap.Property.CanWrite)
				continue;

			if (ap.Index < 0 || ap.Index >= items.Length)
				continue;

			var element = items[ap.Index];

			if (element.ValueKind == JsonValueKind.Null)
			{
				if (!ap.Property.PropertyType.IsValueType ||
					Nullable.GetUnderlyingType(ap.Property.PropertyType) is not null)
					ap.Property.SetValue(instance, null);
				continue;
			}

			object? value;
			try
			{
				value = element.Deserialize(ap.Property.PropertyType, options);
			}
			catch (Exception ex)
			{
				throw new JsonException(
					$"Failed to deserialize array index {ap.Index} into property {ap.Property.DeclaringType?.FullName}.{ap.Property.Name} ({ap.Property.PropertyType.FullName})",
					ex);
			}

			if (value is null &&
				ap.Property.PropertyType.IsValueType &&
				Nullable.GetUnderlyingType(ap.Property.PropertyType) is null)
				continue;

			ap.Property.SetValue(instance, value);
		}

		return instance;
	}

	/// <inheritdoc />
	public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
	{
		var properties = ArrayConverterHelper.GetProperties(typeof(T));
		if (properties.Length == 0)
			throw new JsonException($"Type {typeof(T).FullName} does not have any properties with {nameof(ArrayIndexAttribute)}");

		var maxIndex = properties.Max(p => p.Index);
		if (maxIndex < 0)
		{
			writer.WriteStartArray();
			writer.WriteEndArray();
			return;
		}

		var byIndex = properties.ToDictionary(p => p.Index, p => p);
		writer.WriteStartArray();

		for (int i = 0; i <= maxIndex; i++)
		{
			if (!byIndex.TryGetValue(i, out var ap))
			{
				writer.WriteNullValue();
				continue;
			}

			var prop = ap.Property;
			var propValue = prop.GetValue(value);

			if (propValue is null)
			{
				writer.WriteNullValue();
				continue;
			}

			JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
		}

		writer.WriteEndArray();
	}
}

internal static class ArrayConverterHelper
{
	public record class ArrayProperty(PropertyInfo Property, int Index);

	private readonly static ConcurrentDictionary<Type, ArrayProperty[]> _propertyCache = [];

	public static ArrayProperty[] GetProperties(Type type)
	{
		return _propertyCache.GetOrAdd(type, t => [..t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
			.Select(t => (Property: t, Attribute: t.GetCustomAttribute<ArrayIndexAttribute>()))
			.Where(t => t.Attribute is not null)
			.Select(t => new ArrayProperty(t.Property, t.Attribute!.Index))
			.OrderBy(t => t.Index)]);
	}
}