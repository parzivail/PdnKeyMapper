using System.Windows.Forms;

namespace PdnKeyMapper;

public record KeybindAction(string Name, Keys Shortcut, ToolStripMenuItem Item);