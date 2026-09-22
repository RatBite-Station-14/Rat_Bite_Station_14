using Content.Shared._BRatbite.Access;
using Robust.Client.UserInterface;

namespace Content.Client._BRatbite.Access;

public sealed partial class LockBoundUserInterface : BoundUserInterface
{
    private LockMenu? _menu;

    public LockBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<LockMenu>();
        _menu.OnEnterButtonPressed += (password) =>
        {
            SendMessage(new LockableIDSendPasswordMessage(password));
        };
    }
}
