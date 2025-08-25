using PaintDotNet.Effects;
using PaintDotNet.Imaging;

namespace PdnKeyMapper;

[EffectCategory(EffectCategory.Adjustment)]
public class DummyEffect : BitmapEffect<KeybindDialogue.ConfigToken>
{
	/// <inheritdoc />
	public DummyEffect() : base("Keybinds", (IBitmapSource)null, null, BitmapEffectOptions.Create() with { IsConfigurable = true })
	{
		KeybindManager.HookForm();
	}

	protected override void OnInitializeRenderInfo(IBitmapEffectRenderInfo renderInfo)
	{
		renderInfo.Flags = BitmapEffectRenderingFlags.SingleThreaded;
		renderInfo.OutputPixelFormat = PixelFormats.Bgra32;
		renderInfo.Schedule = BitmapEffectRenderingSchedule.None;
	}

	protected override IEffectConfigForm OnCreateConfigForm()
	{
		return new KeybindDialogue();
	}

	protected override void OnRender(IBitmapEffectOutput output)
	{
		using var input = Environment.GetSourceBitmapBgra32();
		using var outputLock = output.LockBgra32();
		using var inputLock = input.Lock(output.Bounds);
		
		// If we don't do this, the current image will look corrupted
		// while the keybind window is open. It actually isn't, since
		// this window never commits anything (closing it is always
		// the "cancel" action), but it looks nicer this way.
		inputLock.AsRegionPtr().CopyTo(outputLock.AsRegionPtr());
	}
}