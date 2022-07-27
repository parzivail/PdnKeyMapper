using System.Drawing;
using PaintDotNet;
using PaintDotNet.Effects;

namespace PdnKeyMapper;

[EffectCategory(EffectCategory.Adjustment)]
public class DummyEffect : Effect
{
	/// <inheritdoc />
	public DummyEffect() : base("Keybinds", null, null, new EffectOptions
	{
		Flags = EffectFlags.Configurable,
		RenderingSchedule = EffectRenderingSchedule.None
	})
	{
		KeybindManager.HookForm();
	}

	/// <inheritdoc />
	public override EffectConfigDialog CreateConfigDialog()
	{
		return new KeybindDialogue();
	}

	/// <inheritdoc />
	public override void Render(EffectConfigToken parameters, RenderArgs dstArgs, RenderArgs srcArgs, Rectangle[] rois, int startIndex, int length)
	{
	}
}