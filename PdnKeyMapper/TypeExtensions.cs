using System.Reflection;

namespace PdnKeyMapper;

public static class TypeExtensions
{
	public static object GetPrivateField(this Type t, object? instance, string name)
	{
		var flags = BindingFlags.NonPublic;

		if (instance == null)
			flags |= BindingFlags.Static;
		else
			flags |= BindingFlags.Instance;

		return t.GetField(name, flags).GetValue(instance);
	}

	public static T GetPrivateField<T>(this Type t, object instance, string name)
	{
		if (t.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(instance) is not T value)
			throw new InvalidCastException();
		return value;
	}
}