using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

namespace Content.Client._BRatbite.TrackingHud.MarkerMonitor;

public sealed partial class ShaderTextureButton : TextureButton
{
    private ShaderInstance _shader;

    public ShaderTextureButton(ShaderInstance shader)
    {
        _shader = shader;
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        handle.UseShader(_shader);
        base.Draw(handle);
    }
}
