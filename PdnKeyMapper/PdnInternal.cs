using System.Reflection;
using System.Windows.Forms;

namespace PdnKeyMapper;

public class PdnInternal
{
	private static readonly Lazy<Assembly> _pdnAssembly = new(() => AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.ManifestModule.Name == "paintdotnet.dll"));
	private static readonly Lazy<Assembly> _pdnResourcesAssembly = new(() => AppDomain.CurrentDomain.GetAssemblies().First(assembly => assembly.ManifestModule.Name == "PaintDotNet.Resources.dll"));

	private static readonly Lazy<Type> _pdnResourcesType = new(() => PdnResourcesAssembly.GetType("PaintDotNet.Resources.PdnResources"));
	private static readonly Lazy<MethodInfo> _pdnResourcesGetString = new(() => PdnResourcesType.GetMethod("GetString", new[] { typeof(string) }));

	private static readonly Lazy<Type> _programType = new(() => PdnAssembly.GetType("PaintDotNet.Program"));
	private static readonly Lazy<object> _programInstance = new(() => ProgramType.GetPrivateField(null, "instance"));

	private static readonly Lazy<Type> _formType = new(() => PdnAssembly.GetType("PaintDotNet.Dialogs.MainForm"));
	private static readonly Lazy<Form> _formInstance = new(() => (Form)ProgramType.GetPrivateField(Program, "mainForm"));

	private static readonly Lazy<Type> _appWorkspaceType = new(() => PdnAssembly.GetType("PaintDotNet.Controls.AppWorkspace"));
	private static readonly Lazy<Control> _appWorkspaceInstance = new(() => (Control)MainFormType.GetPrivateField(MainForm, "appWorkspace"));

	private static readonly Lazy<Type> _pdnToolBarType = new(() => PdnAssembly.GetType("PaintDotNet.Controls.PdnToolBar"));
	private static readonly Lazy<Control> _pdnToolBar = new(() => AppWorkspaceType.GetPrivateField<Control>(AppWorkspace, "toolBar"));

	private static readonly Lazy<Type> _pdnMenuItemType = new(() => PdnAssembly.GetType("PaintDotNet.Menus.PdnMenuItem"));
	private static readonly Lazy<PropertyInfo> _pdnMenuItemShortcutKeys = new(() => PdnMenuItemType.GetProperty("ShortcutKeys", BindingFlags.Public | BindingFlags.Instance));

	private static readonly Lazy<ToolStrip> _pdnMainMenu = new(() => PdnToolBarType.GetPrivateField<ToolStrip>(PdnInternal.ToolBar, "mainMenu"));

	private static readonly Lazy<Control> _activeDocumentWorkspaceInstance =
		new(() => (Control)AppWorkspaceType.GetProperty("ActiveDocumentWorkspace", BindingFlags.Instance | BindingFlags.Public).GetValue(AppWorkspace));

	public static Assembly PdnAssembly => _pdnAssembly.Value;
	public static Assembly PdnResourcesAssembly => _pdnResourcesAssembly.Value;

	public static Type PdnResourcesType => _pdnResourcesType.Value;
	public static string PdnResourcesGetString(string value) => (string)_pdnResourcesGetString.Value.Invoke(null, new object[] { value });

	public static Type ProgramType => _programType.Value;
	public static object Program => _programInstance.Value;

	public static Type MainFormType => _formType.Value;
	public static Form MainForm => _formInstance.Value;

	public static Type PdnMenuItemType => _pdnMenuItemType.Value;
	public static void PdnMenuItemShortcutKeysSet(ToolStripMenuItem source, Keys value) => _pdnMenuItemShortcutKeys.Value.SetValue(source, value);

	public static Type PdnToolBarType => _pdnToolBarType.Value;
	public static Control ToolBar => _pdnToolBar.Value;

	public static ToolStrip MainMenu => _pdnMainMenu.Value;

	public static Type AppWorkspaceType => _appWorkspaceType.Value;
	public static Control AppWorkspace => _appWorkspaceInstance.Value;

	public static Control ActiveDocumentWorkspace => _activeDocumentWorkspaceInstance.Value;
}