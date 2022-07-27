using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using BrightIdeasSoftware;
using PaintDotNet.Effects;

namespace PdnKeyMapper;

public class KeybindDialogue : EffectConfigDialog
{
	private Dictionary<string, string> _itemIdentifiers = new();
	private TreeListView _keybindTree;

	public KeybindDialogue()
	{
		SuspendLayout();

		Size = new Size(700, 500);

		_keybindTree = new TreeListView()
		{
			Dock = DockStyle.Fill,
			BorderStyle = BorderStyle.None
		};
		Controls.Add(_keybindTree);

		ResumeLayout(true);

		_keybindTree.CanExpandGetter += CanExpandGetter;
		_keybindTree.ChildrenGetter += ChildrenGetter;
		_keybindTree.FullRowSelect = true;
		_keybindTree.CellEditActivation = ObjectListView.CellEditActivateMode.DoubleClick;
		_keybindTree.CellEditStarting += KeybindTreeOnCellEditStarting;
		_keybindTree.CellEditFinishing += KeybindTreeOnCellEditFinishing;

		_keybindTree.AllColumns.Add(new OLVColumn("Name", "Name") { Width = 450, AspectGetter = ActionNameFormatter });
		_keybindTree.AllColumns.Add(new OLVColumn("Shortcut", "Shortcut") { FillsFreeSpace = true, AspectGetter = ShortcutFormatter });
		_keybindTree.RebuildColumns();

		_keybindTree.SetObjects(PdnInternal.MainMenu.Items);
		_keybindTree.ExpandAll();

		CollectIdentifiers(PdnInternal.MainMenu, _itemIdentifiers);
	}

	private void KeybindTreeOnCellEditFinishing(object sender, CellEditEventArgs e)
	{
		if (e.Cancel)
			return;

		if (e.RowObject is not KeybindAction ka)
			return;

		var keys = Keys.None;

		if (!string.IsNullOrWhiteSpace((string)e.NewValue) && !KeybindManager.TryParseKeys((string)e.NewValue, out keys))
			return;

		if (keys != Keys.None && !ToolStripManager.IsValidShortcut(keys))
			return;

		PdnInternal.PdnMenuItemShortcutKeysSet(ka.Item, keys);

		var itemId = _itemIdentifiers[ka.Item.Name];
		var config = KeybindManager.ReadConfig();
		config[itemId] = KeybindManager.StringifyKeys(keys);
		KeybindManager.WriteConfig(config);

		_keybindTree.SetObjects(PdnInternal.MainMenu.Items);
	}

	private static void KeybindTreeOnCellEditStarting(object sender, CellEditEventArgs e)
	{
		if (e.Column.AspectName != "Shortcut")
			e.Cancel = true;
	}

	private static void CollectIdentifiers(object control, Dictionary<string, string> actions, string path = "")
	{
		switch (control)
		{
			case ToolStrip strip:
			{
				foreach (var item in strip.Items)
					CollectIdentifiers(item, actions, path);
				break;
			}
			case ToolStripMenuItem dropDownItem:
			{
				if (path.Length > 0)
					path += ".";
				path += dropDownItem.Name;
				foreach (var item in dropDownItem.DropDownItems)
					CollectIdentifiers(item, actions, path);

				if (dropDownItem.Name.Length > 0)
					actions[dropDownItem.Name] = path;

				break;
			}
		}
	}

	private object ActionNameFormatter(object rowobject)
	{
		var name = rowobject switch
		{
			ToolStripMenuItem control => control.Name,
			KeybindAction tsmi => tsmi.Name,
			_ => null
		};

		if (name == null)
			return "";

		if (!_itemIdentifiers.TryGetValue(name, out var translated))
			return name;
		return PdnInternal.PdnResourcesGetString(translated + ".Text").Replace("&", "");
	}

	private static object ShortcutFormatter(object rowobject)
	{
		if (rowobject is not KeybindAction tsmi)
			return "";

		return KeybindManager.StringifyKeys(tsmi.Shortcut);
	}

	private static IEnumerable ChildrenGetter(object model)
	{
		return model switch
		{
			ToolStrip strip => strip.Items
				.Cast<object>()
				.Where(o => o is ToolStripMenuItem)
				.Cast<ToolStripMenuItem>()
				.Where(item => item.Name.Length > 0)
				.Select(item => new KeybindAction(item.Name, item.ShortcutKeys, item)),
			ToolStripMenuItem dropDownItem => dropDownItem.DropDownItems.Cast<object>()
				.Where(o => o is ToolStripMenuItem)
				.Cast<ToolStripMenuItem>()
				.Where(item => item.Name.Length > 0)
				.Select(item => new KeybindAction(item.Name, item.ShortcutKeys, item)),
			_ => throw new InvalidOperationException()
		};
	}

	private static bool CanExpandGetter(object model)
	{
		return model switch
		{
			ToolStrip strip => strip.Items.Count > 0,
			ToolStripMenuItem dropDownItem => dropDownItem.DropDownItems.Count > 0,
			_ => false
		};
	}
}