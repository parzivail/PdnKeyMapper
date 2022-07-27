using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows.Forms;

namespace PdnKeyMapper;

public record KeybindAction(string Name, Keys Shortcut, ToolStripMenuItem Item);

public class KeybindManager
{
	public static void HookForm()
	{
		var mainFormField = PdnInternal.ProgramType.GetField("mainForm", BindingFlags.Instance | BindingFlags.NonPublic);

		if (mainFormField == null)
			return;

		new Thread(async () =>
		{
			await TaskWaiter.WaitUntil(() => mainFormField.GetValue(PdnInternal.Program) != null, 500);

			var activeDocumentWorkspace = PdnInternal.AppWorkspaceType.GetProperty("ActiveDocumentWorkspace", BindingFlags.Instance | BindingFlags.Public);
			await TaskWaiter.WaitUntil(() => activeDocumentWorkspace.GetValue(PdnInternal.AppWorkspace) != null, 500);

			PdnInternal.MainForm.Invoke(Initialize);
		}).Start();
	}

	public static Dictionary<string, string> ReadConfig()
	{
		if (!File.Exists("keymap.json"))
			return new Dictionary<string, string>();
		return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText("keymap.json"));
	}

	public static void WriteConfig(Dictionary<string, string> dict)
	{
		File.WriteAllText("keymap.json", JsonSerializer.Serialize(dict, new JsonSerializerOptions()
		{
			WriteIndented = true
		}));
	}

	private static void Initialize()
	{
		var map = new Dictionary<string, ToolStripMenuItem>();
		CollectItems(PdnInternal.MainMenu, map);

		var dict = ReadConfig();
		foreach (var (item, value) in dict)
		{
			if (!map.ContainsKey(item))
				continue;

			if (!TryParseKeys(value, out var keys) || !ToolStripManager.IsValidShortcut(keys))
				continue;

			PdnInternal.PdnMenuItemShortcutKeysSet(map[item], keys);
		}
	}

	private static void CollectItems(object control, Dictionary<string, ToolStripMenuItem> actions, string path = "")
	{
		switch (control)
		{
			case ToolStrip strip:
			{
				foreach (var item in strip.Items)
					CollectItems(item, actions, path);
				break;
			}
			case ToolStripMenuItem dropDownItem:
			{
				if (path.Length > 0)
					path += ".";
				path += dropDownItem.Name;
				foreach (var item in dropDownItem.DropDownItems)
					CollectItems(item, actions, path);

				if (dropDownItem.Name.Length > 0)
					actions[path] = dropDownItem;

				break;
			}
		}
	}

	public static string StringifyKeys(Keys keys)
	{
		return TypeDescriptor.GetConverter(typeof(Keys)).ConvertToString(keys);
	}

	public static bool TryParseKeys(string value, out Keys keys)
	{
		try
		{
			keys = (Keys)(TypeDescriptor.GetConverter(typeof(Keys)).ConvertFromString(value) ?? Keys.None);
			return true;
		}
		catch
		{
			keys = Keys.None;
			return false;
		}
	}
}

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