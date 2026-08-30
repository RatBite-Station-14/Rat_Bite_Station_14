// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Content.Client.Administration.Managers;
using Content.Client.Administration.Systems;
using Content.Client.Administration.UI.Bwoink;
using Content.Client.Gameplay;
using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Client.Stylesheets;
using Content.Client.Stylesheets.Sheetlets;
using Content.Client.UserInterface.Controls;
using Content.Client.UserInterface.Systems.MenuBar.Widgets;
using Content.Shared._BRatbite.CCVar;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Shared.Input;
using JetBrains.Annotations;
using Robust.Client.Audio;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Configuration;
using Robust.Shared.Input.Binding;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using static Content.Shared.Administration.SharedBwoinkSystem;

namespace Content.Client.UserInterface.Systems.Bwoink;

[UsedImplicitly]
public sealed class AHelpUIController: UIController, IOnSystemChanged<BwoinkSystem>, IOnStateChanged<GameplayState>, IOnStateChanged<LobbyState>
{
    [Dependency] private readonly IClientAdminManager _adminManager = default!;
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IUserInterfaceManager _uiManager = default!;
    [Dependency] private readonly IInputManager _input = default!;
    [UISystemDependency] private readonly AudioSystem _audio = default!;

    private BwoinkSystem? _bwoinkSystem;
    private MenuButton? GameAHelpButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.AHelpButton;
    private MenuButton? GameRPHelpButton => UIManager.GetActiveUIWidgetOrNull<GameTopMenuBar>()?.RPHelpButton;
    private Button? LobbyAHelpButton => (UIManager.ActiveScreen as LobbyGui)?.AHelpButton;
    public IAHelpUIHandler? UIHelper;
    private bool _discordRelayActive;
    private bool _hasUnreadAHelp;
    private bool _bwoinkSoundEnabled;
    private string? _aHelpSound;
    private string? _rpHelpSound;

    protected override string SawmillName => "c.s.go.es.bwoink";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<BwoinkDiscordRelayUpdated>(DiscordRelayUpdated);
        SubscribeNetworkEvent<BwoinkPlayerTypingUpdated>(PeopleTypingUpdated);

        _adminManager.AdminStatusUpdated += OnAdminStatusUpdated;
        _config.OnValueChanged(CCVars.AHelpSound, v => _aHelpSound = v, true);
        _config.OnValueChanged(RatbiteCVars.RPHelpSound, v => _rpHelpSound = v, true);
        _config.OnValueChanged(CCVars.BwoinkSoundEnabled, v => _bwoinkSoundEnabled = v, true);
    }

    public void UnloadButton()
    {
        if (GameAHelpButton != null)
            GameAHelpButton.OnPressed -= AHelpButtonPressed;

        // Ratbite
        if (GameRPHelpButton != null)
            GameRPHelpButton.OnPressed -= RPHelpButtonPressed;

        if (LobbyAHelpButton != null)
            LobbyAHelpButton.OnPressed -= AHelpButtonPressed;
    }

    public void LoadButton()
    {
        if (GameAHelpButton != null)
            GameAHelpButton.OnPressed += AHelpButtonPressed;

        // Ratbite
        if (GameRPHelpButton != null)
        {
            GameRPHelpButton.Disabled = _adminManager.IsActive();
            GameRPHelpButton.OnPressed += RPHelpButtonPressed;
        }

        if (LobbyAHelpButton != null)
            LobbyAHelpButton.OnPressed += AHelpButtonPressed;
    }

    private void OnAdminStatusUpdated()
    {
        GameRPHelpButton?.Disabled = _adminManager.IsActive();
        if (UIHelper is not { IsAnyOpen: true })
            return;
        EnsureUIHelper();
    }

    private void AHelpButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        EnsureUIHelper();
        UIHelper!.ToggleWindow(BwoinkType.AHelp);
    }

    private void RPHelpButtonPressed(BaseButton.ButtonEventArgs obj)
    {
        EnsureUIHelper();
        UIHelper!.ToggleWindow(BwoinkType.RPHelp);
    }

    public void OnSystemLoaded(BwoinkSystem system)
    {
        _bwoinkSystem = system;
        _bwoinkSystem.OnBwoinkTextMessageRecieved += ReceivedBwoink;

        _input.SetInputCommand(ContentKeyFunctions.OpenAHelp,
            InputCmdHandler.FromDelegate(_ => ToggleWindow(BwoinkType.AHelp)));
        // Ratbite
        _input.SetInputCommand(ContentKeyFunctions.OpenRPHelp,
            InputCmdHandler.FromDelegate(_ => ToggleWindow(BwoinkType.RPHelp)));
    }

    public void OnSystemUnloaded(BwoinkSystem system)
    {
        _input.SetInputCommand(ContentKeyFunctions.OpenAHelp, null);

        DebugTools.Assert(_bwoinkSystem != null);
        _bwoinkSystem!.OnBwoinkTextMessageRecieved -= ReceivedBwoink;
        _bwoinkSystem = null;
    }

    private void UpdateButtonPressed()
    {
        if (UIHelper is null) return;
        SetAHelpPressed(UIHelper.IsOpen(BwoinkType.AHelp));
        SetRPHelpPressed(UIHelper.IsOpen(BwoinkType.RPHelp));
    }

    private void SetAHelpPressed(bool pressed)
    {
        if (GameAHelpButton != null)
        {
            GameAHelpButton.Pressed = pressed;
        }

        if (LobbyAHelpButton != null)
        {
            LobbyAHelpButton.Pressed = pressed;
        }

        UIManager.ClickSound();
        UnreadAHelpRead();
    }

    private void SetRPHelpPressed(bool pressed)
    {
        GameRPHelpButton?.Pressed = pressed;
        UIManager.ClickSound();
    }

    private void ReceivedBwoink(object? sender, SharedBwoinkSystem.BwoinkTextMessage message)
    {
        Log.Info($"@{message.UserId}: {message.Text}");
        var localPlayer = _playerManager.LocalSession;
        if (localPlayer == null)
        {
            return;
        }
        if (message.PlaySound && localPlayer.UserId != message.TrueSender)
        {
            var sound = message.Type == BwoinkType.RPHelp ? _rpHelpSound : _aHelpSound;
            if (sound != null && (_bwoinkSoundEnabled || !_adminManager.IsActive()))
                _audio.PlayGlobal(sound, Filter.Local(), false);
            _clyde.RequestWindowAttention();
        }

        EnsureUIHelper();

        if (!UIHelper!.IsOpen(message.Type))
        {
            UnreadAHelpReceived();
        }

        UIHelper!.Receive(message);
    }

    private void DiscordRelayUpdated(BwoinkDiscordRelayUpdated args, EntitySessionEventArgs session)
    {
        _discordRelayActive = args.DiscordRelayEnabled;
        UIHelper?.DiscordRelayChanged(_discordRelayActive);
    }

    private void PeopleTypingUpdated(BwoinkPlayerTypingUpdated args, EntitySessionEventArgs session)
    {
        UIHelper?.PeopleTypingUpdated(args);
    }

    public void EnsureUIHelper()
    {
        var isAdmin = _adminManager.HasFlag(AdminFlags.Adminhelp);

        if (UIHelper != null && UIHelper.IsAdmin == isAdmin)
            return;

        var ownerUserId = _playerManager.LocalUser!.Value;
        UIHelper = isAdmin ? new AdminAHelpUIHandler(ownerUserId) : new UserAHelpUIHandler(ownerUserId);
        UIHelper.DiscordRelayChanged(_discordRelayActive);

        UIHelper.SendMessageAction = (type, userId, textMessage, playSound, adminOnly) => _bwoinkSystem?.Send(type, userId, textMessage, playSound, adminOnly);
        UIHelper.InputTextChanged += (type, channel, text) => _bwoinkSystem?.SendInputTextUpdated(type, channel, text.Length > 0);
        UIHelper.OnClose += (BwoinkType? t) => {
            UpdateButtonPressed();
        };
        UIHelper.OnOpen +=  (BwoinkType? t) => {
            UpdateButtonPressed();
        };
        UpdateButtonPressed();
    }

    public void Open(BwoinkType t)
    {
        var localUser = _playerManager.LocalUser;
        if (localUser == null)
        {
            return;
        }
        EnsureUIHelper();
        if (UIHelper!.IsOpen(t))
            return;
        UIHelper!.Open(t, localUser.Value, _discordRelayActive);
    }

    public void Open(NetUserId userId)
    {
        EnsureUIHelper();
        if (!UIHelper!.IsAdmin)
            return;
        UIHelper?.Open(BwoinkType.AHelp, userId, _discordRelayActive);
    }

    public void ToggleWindow(BwoinkType t)
    {
        EnsureUIHelper();
        UIHelper?.ToggleWindow(t);
    }

    public void PopOut()
    {
        EnsureUIHelper();
        if (UIHelper is not AdminAHelpUIHandler helper)
            return;

        if (helper.Window == null || helper.Control == null)
        {
            return;
        }

        helper.Control.Orphan();
        helper.Window.Dispose();
        helper.Window = null;
        helper.EverOpened = false;

        var monitor = _clyde.EnumerateMonitors().First();

        helper.ClydeWindow = _clyde.CreateWindow(new WindowCreateParameters
        {
            Maximized = false,
            Title = Loc.GetString("bwoink-admin-title"),
            Monitor = monitor,
            Width = 900,
            Height = 500
        });

        helper.ClydeWindow.RequestClosed += helper.OnRequestClosed;
        helper.ClydeWindow.DisposeOnClose = true;

        helper.WindowRoot = _uiManager.CreateWindowRoot(helper.ClydeWindow);
        helper.WindowRoot.AddChild(helper.Control);

        helper.Control.PopOut.Disabled = true;
        helper.Control.PopOut.Visible = false;
    }

    private void UnreadAHelpReceived()
    {
        GameAHelpButton?.StyleClasses.Add(StyleClass.Negative);
        LobbyAHelpButton?.StyleClasses.Add(StyleClass.Negative);
        _hasUnreadAHelp = true;
    }

    private void UnreadAHelpRead()
    {
        GameAHelpButton?.StyleClasses.Remove(StyleClass.Negative);
        LobbyAHelpButton?.StyleClasses.Remove(StyleClass.Negative);
        _hasUnreadAHelp = false;
    }

    public void OnStateEntered(GameplayState state)
    {
        if (GameRPHelpButton != null)
        {
            GameRPHelpButton.OnPressed -= RPHelpButtonPressed;
            GameRPHelpButton.OnPressed += RPHelpButtonPressed;
            GameRPHelpButton.Pressed = UIHelper?.IsOpen(BwoinkType.RPHelp) ?? false;
        }

        if (GameAHelpButton != null)
        {
            GameAHelpButton.OnPressed -= AHelpButtonPressed;
            GameAHelpButton.OnPressed += AHelpButtonPressed;
            GameAHelpButton.Pressed = UIHelper?.IsOpen(BwoinkType.AHelp) ?? false;

            if (_hasUnreadAHelp)
            {
                UnreadAHelpReceived();
            }
            else
            {
                UnreadAHelpRead();
            }
        }
    }

    public void OnStateExited(GameplayState state)
    {
        if (GameAHelpButton != null)
            GameAHelpButton.OnPressed -= AHelpButtonPressed;

        if (GameRPHelpButton != null)
            GameRPHelpButton.OnPressed -= RPHelpButtonPressed;
    }

    public void OnStateEntered(LobbyState state)
    {
        if (LobbyAHelpButton != null)
        {
            LobbyAHelpButton.OnPressed -= AHelpButtonPressed;
            LobbyAHelpButton.OnPressed += AHelpButtonPressed;
            LobbyAHelpButton.Pressed = UIHelper?.IsOpen(BwoinkType.AHelp) ?? false;

            if (_hasUnreadAHelp)
            {
                UnreadAHelpReceived();
            }
            else
            {
                UnreadAHelpRead();
            }
        }
    }

    public void OnStateExited(LobbyState state)
    {
        if (LobbyAHelpButton != null)
            LobbyAHelpButton.OnPressed -= AHelpButtonPressed;
    }
}

// please kill all this indirection
// Ratbite: Changed to allow RPHelp
public interface IAHelpUIHandler
{
    public bool IsAdmin { get; }
    public bool IsOpen(BwoinkType type);
    public bool IsAnyOpen { get; }
    public void Receive(SharedBwoinkSystem.BwoinkTextMessage message);
    public void Close(BwoinkType type);
    public void Open(BwoinkType type, NetUserId netUserId, bool relayActive);
    public void ToggleWindow(BwoinkType type);
    public void DiscordRelayChanged(bool active);
    public void PeopleTypingUpdated(BwoinkPlayerTypingUpdated args);
    public event Action<BwoinkType?>? OnClose;
    public event Action<BwoinkType?>? OnOpen;
    public Action<BwoinkType, NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    public event Action<BwoinkType?, NetUserId, string>? InputTextChanged;
}
public sealed class AdminAHelpUIHandler : IAHelpUIHandler
{
    private readonly NetUserId _ownerId;
    public AdminAHelpUIHandler(NetUserId owner)
    {
        _ownerId = owner;
    }
    private readonly Dictionary<NetUserId, BwoinkPanel> _activePanelMap = new();
    public bool IsAdmin => true;
    public bool IsOpen(BwoinkType _) => Window is { Disposed: false, IsOpen: true } || ClydeWindow is { IsDisposed: false };
    public bool IsAnyOpen { get => IsOpen(BwoinkType.AHelp); }
    public bool EverOpened;

    public BwoinkWindow? Window;
    public WindowRoot? WindowRoot;
    public IClydeWindow? ClydeWindow;
    public BwoinkControl? Control;

    public void Receive(SharedBwoinkSystem.BwoinkTextMessage message)
    {
        var panel = EnsurePanel(message.UserId);
        panel.ReceiveLine(message);
        Control?.OnBwoink(message.UserId);
    }

    private void OpenWindow()
    {
        if (Window == null)
            return;

        if (EverOpened)
            Window.Open();
        else
            Window.OpenCentered();
    }

    public void Close(BwoinkType _ = BwoinkType.AHelp)
    {
        Window?.Close();

        // popped-out window is being closed
        if (ClydeWindow != null)
        {
            ClydeWindow.RequestClosed -= OnRequestClosed;
            ClydeWindow.Dispose();
            // need to dispose control cause we cant reattach it directly back to the window
            // but orphan panels first so -they- can get readded when the window is opened again
            if (Control != null)
            {
                foreach (var (_, panel) in _activePanelMap)
                {
                    panel.Orphan();
                }
                Control?.Dispose();
            }
            // window wont be closed here so we will invoke ourselves
            OnClose?.Invoke(null);
        }
    }

    public void ToggleWindow(BwoinkType t)
    {
        EnsurePanel(_ownerId);

        if (IsOpen(t))
            Close(t);
        else
            OpenWindow();
    }

    public void DiscordRelayChanged(bool active)
    {
    }

    public void PeopleTypingUpdated(BwoinkPlayerTypingUpdated args)
    {
        if (_activePanelMap.TryGetValue(args.Channel, out var panel))
            panel.UpdatePlayerTyping(args.PlayerName, args.Typing);
    }

    public event Action<BwoinkType?>? OnClose;
    public event Action<BwoinkType?>? OnOpen;
    public Action<BwoinkType, NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    public event Action<BwoinkType?, NetUserId, string>? InputTextChanged;

    public void Open(BwoinkType _, NetUserId channelId, bool relayActive)
    {
        SelectChannel(channelId);
        OpenWindow();
    }

    public void OnRequestClosed(WindowRequestClosedEventArgs args)
    {
        Close();
    }

    private void EnsureControl()
    {
        if (Control is { Disposed: false })
            return;

        Window = new BwoinkWindow();
        Control = Window.Bwoink;
        Window.OnClose += () => { OnClose?.Invoke(null); };
        Window.OnOpen += () =>
        {
            OnOpen?.Invoke(null);
            EverOpened = true;
        };

        // need to readd any unattached panels..
        foreach (var (_, panel) in _activePanelMap)
        {
            if (!Control!.BwoinkArea.Children.Contains(panel))
            {
                Control!.BwoinkArea.AddChild(panel);
            }
            panel.Visible = false;
        }
    }

    public void HideAllPanels()
    {
        foreach (var panel in _activePanelMap.Values)
        {
            panel.Visible = false;
        }
    }

    public BwoinkPanel EnsurePanel(NetUserId channelId)
    {
        EnsureControl();

        if (_activePanelMap.TryGetValue(channelId, out var existingPanel))
            return existingPanel;

        _activePanelMap[channelId] = existingPanel = new BwoinkPanel(text => SendMessageAction?.Invoke(Window?.Bwoink.SelectedType ?? BwoinkType.AHelp, channelId, text, Window?.Bwoink.PlaySound.Pressed ?? true, Window?.Bwoink.AdminOnly.Pressed ?? false));
        existingPanel.InputTextChanged += text => InputTextChanged?.Invoke(null, channelId, text);
        existingPanel.Visible = false;
        if (!Control!.BwoinkArea.Children.Contains(existingPanel))
            Control.BwoinkArea.AddChild(existingPanel);

        return existingPanel;
    }
    public bool TryGetChannel(NetUserId ch, [NotNullWhen(true)] out BwoinkPanel? bp) => _activePanelMap.TryGetValue(ch, out bp);

    private void SelectChannel(NetUserId uid)
    {
        EnsurePanel(uid);
        Control!.SelectChannel(uid);
    }

    public void Dispose()
    {
        Window?.Dispose();
        Window = null;
        Control = null;
        _activePanelMap.Clear();
        EverOpened = false;
    }
}

public sealed class UserAHelpUIHandler : IAHelpUIHandler
{
    private readonly NetUserId _ownerId;
    private readonly static BwoinkType[] _enabledBwoinks = new[] { BwoinkType.AHelp, BwoinkType.RPHelp };
    public UserAHelpUIHandler(NetUserId owner)
    {
        _ownerId = owner;
    }
    public bool IsAdmin => false;
    public bool IsAnyOpen
    {
        get
        {
            foreach (var bwoinkType in _enabledBwoinks)
            {
                if (IsOpen(bwoinkType)) return true;
            }
            return false;
        }
    }
    public bool IsOpen(BwoinkType type)
    {
        if (!_windows.TryGetValue(type, out var tuple) || !(tuple is (var window, var _))) return false;
        return window is { Disposed: false, IsOpen: true };
    }
    private Dictionary<BwoinkType, (DefaultWindow, BwoinkPanel)> _windows = new();
    private bool _discordRelayActive;

    public void Receive(SharedBwoinkSystem.BwoinkTextMessage message)
    {
        Receive(message, true); // Default param doesn't work because of the interface
    }
    public void Receive(SharedBwoinkSystem.BwoinkTextMessage message, bool openWindow = true)
    {
        DebugTools.Assert(message.UserId == _ownerId);
        EnsureInit(_discordRelayActive);
        if (_windows.TryGetValue(message.Type, out var tuple) && tuple is (var window, var panel))
        {
            panel.ReceiveLine(message);
            if (openWindow)
                window.OpenCentered();
        }

    }

    public void Close(BwoinkType type)
    {
        if (!_windows.TryGetValue(type, out var tuple) || !(tuple is (var window, var _))) return;
        window.Close();
    }

    public void ToggleWindow(BwoinkType type)
    {
        EnsureInit(_discordRelayActive);
        if (!_windows.TryGetValue(type, out var tuple) || !(tuple is (var window, var _))) return;
        if (window.IsOpen)
        {
            window.Close();
        }
        else
        {
            window.OpenCentered();
        }
    }

    // user can't pop out their window.
    public void PopOut()
    {
    }

    public void DiscordRelayChanged(bool active)
    {
        _discordRelayActive = active;
        foreach (var (_, panel) in _windows.Values)
            panel.RelayedToDiscordLabel.Visible = active;
    }

    public void PeopleTypingUpdated(BwoinkPlayerTypingUpdated args)
    {
    }

    public event Action<BwoinkType?>? OnClose;
    public event Action<BwoinkType?>? OnOpen;
    public Action<BwoinkType, NetUserId, string, bool, bool>? SendMessageAction { get; set; }
    public event Action<BwoinkType?, NetUserId, string>? InputTextChanged;

    public void Open(BwoinkType type, NetUserId channelId, bool relayActive)
    {
        EnsureInit(relayActive);
        if (!_windows.TryGetValue(type, out var tuple) || !(tuple is (var window, var _))) return;
        window!.OpenCentered();
    }

    private void EnsureInit(bool relayActive)
    {
        if (_windows.Count != 0) return;
        foreach (var bwoinkType in _enabledBwoinks)
        {
            var chatPanel = new BwoinkPanel(text => SendMessageAction?.Invoke(bwoinkType, _ownerId, text, true, false));
            chatPanel.InputTextChanged += text => InputTextChanged?.Invoke(bwoinkType, _ownerId, text);
            chatPanel.RelayedToDiscordLabel.Visible = relayActive;
            var window = new DefaultWindow()
            {
                TitleClass="windowTitleAlert",
                HeaderClass = bwoinkType == BwoinkType.AHelp ? "windowHeaderAlert" : "purple",
                Title=Loc.GetString(bwoinkType == BwoinkType.AHelp ? "bwoink-user-title" : "bwoink-rphelp-title"),
                MinSize = new Vector2(500, 300),
            };
            window.OnClose += () => { OnClose?.Invoke(bwoinkType); };
            window.OnOpen += () => { OnOpen?.Invoke(bwoinkType); };
            window.Contents.AddChild(chatPanel);
            _windows.Add(bwoinkType, (window, chatPanel));

            var introText = Loc.GetString(bwoinkType == BwoinkType.AHelp ? "bwoink-system-introductory-message" : "bwoink-system-rphelp-introductory-message");
            var introMessage = new SharedBwoinkSystem.BwoinkTextMessage(_ownerId, SharedBwoinkSystem.SystemUserId, introText, type: bwoinkType);
            Receive(introMessage, openWindow: false);
        }
    }
}
