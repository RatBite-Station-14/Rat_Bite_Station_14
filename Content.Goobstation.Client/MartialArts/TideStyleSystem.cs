// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Content.Client.UserInterface.Controls;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared._Shitmed.Targeting;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;
using Robust.Shared.Maths;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Client.MartialArts;

public sealed class TideStyleSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;

    private TideStyleRadialMenu? _activeMenu;
    private EntityUid? _activeMenuOwner;
    private bool _selectionMade;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var localPlayer = _playerManager.LocalEntity;
        if (localPlayer == null)
        {
            CloseMenu();
            return;
        }

        if (!TryComp<TideStyleComponent>(localPlayer.Value, out var comp))
        {
            CloseMenu();
            return;
        }

        if (comp.PendingBodyPartSelection && _activeMenu == null)
        {
            ShowBodyPartSelectionMenu(localPlayer.Value, comp);
        }
        else if (!comp.PendingBodyPartSelection && _activeMenu != null)
        {
            CloseMenu();
        }
    }

    private void CloseMenu()
    {
        if (_activeMenu != null)
        {
            _activeMenu.Close();
            _activeMenu = null;
            _activeMenuOwner = null;
            _selectionMade = false;
        }
    }

    private void ShowBodyPartSelectionMenu(EntityUid user, TideStyleComponent component)
    {
        CloseMenu();

        _activeMenuOwner = user;
        _selectionMade = false;

        _activeMenu = new TideStyleRadialMenu();
        _activeMenu.OnOuterAreaClose += () =>
        {
            if (_activeMenuOwner == null)
                return;

            if (TryComp<TideStyleComponent>(_activeMenuOwner.Value, out var comp) &&
                comp.PendingBodyPartSelection)
            {
                var netUser = GetNetEntity(_activeMenuOwner.Value);
                RaiseNetworkEvent(new TideStyleAbilityCancelEvent(netUser));
            }
        };


        _activeMenu.OnClose += () =>
        {
            if (!_selectionMade && _activeMenuOwner != null)
            {
                if (TryComp<TideStyleComponent>(_activeMenuOwner.Value, out var comp) &&
                    comp.PendingBodyPartSelection)
                {
                    var netUser = GetNetEntity(_activeMenuOwner.Value);
                    RaiseNetworkEvent(new TideStyleAbilityCancelEvent(netUser));
                }
            }

            _activeMenu = null;
            _activeMenuOwner = null;
            _selectionMade = false;
        };

        // Outer ring body parts
        var outerParts = new[]
        {
            TargetBodyPart.Head,
            TargetBodyPart.RightHand,
            TargetBodyPart.RightArm,
            TargetBodyPart.RightLeg,
            TargetBodyPart.RightFoot,
            TargetBodyPart.LeftFoot,
            TargetBodyPart.LeftLeg,
            TargetBodyPart.LeftArm,
            TargetBodyPart.LeftHand
        };

        var sprites = IoCManager.Resolve<IEntityManager>().System<SpriteSystem>();

        foreach (var part in outerParts)
        {
            var partCopy = part;
            _activeMenu.AddOuterButton(GetBodyPartName(part), GetBodyPartSprite(part), sprites, () =>
            {
                _selectionMade = true;
                var netUser = GetNetEntity(user);
                RaiseNetworkEvent(new TideStyleBodyPartSelectedEvent(netUser, partCopy));
                CloseMenu();
            });
        }

        _activeMenu.AddCenterButton("Groin/Chest", GetBodyPartSprite(TargetBodyPart.Groin), sprites, () =>
        {
            _selectionMade = true;
            var netUser = GetNetEntity(user);
            RaiseNetworkEvent(new TideStyleBodyPartSelectedEvent(netUser, TargetBodyPart.Groin));
            CloseMenu();
        });

        _activeMenu.OpenOverMouseScreenPosition();
    }

    private string GetBodyPartName(TargetBodyPart part)
    {
        return part switch
        {
            TargetBodyPart.Head => "Head",
            TargetBodyPart.LeftArm => "Left Arm",
            TargetBodyPart.RightArm => "Right Arm",
            TargetBodyPart.LeftHand => "Left Hand",
            TargetBodyPart.RightHand => "Right Hand",
            TargetBodyPart.Groin => "Groin",
            TargetBodyPart.Chest => "Chest",
            TargetBodyPart.LeftLeg => "Left Leg",
            TargetBodyPart.RightLeg => "Right Leg",
            TargetBodyPart.LeftFoot => "Left Foot",
            TargetBodyPart.RightFoot => "Right Foot",
            _ => "Unknown"
        };
    }

    private SpriteSpecifier? GetBodyPartSprite(TargetBodyPart part)
    {
        var path = part switch
        {
            TargetBodyPart.Head => "/Textures/_Shitmed/Interface/Targeting/Doll/head.png",
            TargetBodyPart.LeftArm => "/Textures/_Shitmed/Interface/Targeting/Doll/leftarm.png",
            TargetBodyPart.RightArm => "/Textures/_Shitmed/Interface/Targeting/Doll/rightarm.png",
            TargetBodyPart.LeftHand => "/Textures/_Shitmed/Interface/Targeting/Doll/lefthand.png",
            TargetBodyPart.RightHand => "/Textures/_Shitmed/Interface/Targeting/Doll/righthand.png",
            TargetBodyPart.Groin => "/Textures/_Shitmed/Interface/Targeting/Doll/groin.png",
            TargetBodyPart.Chest => "/Textures/_Shitmed/Interface/Targeting/Doll/chest.png",
            TargetBodyPart.LeftLeg => "/Textures/_Shitmed/Interface/Targeting/Doll/leftleg.png",
            TargetBodyPart.RightLeg => "/Textures/_Shitmed/Interface/Targeting/Doll/rightleg.png",
            TargetBodyPart.LeftFoot => "/Textures/_Shitmed/Interface/Targeting/Doll/leftfoot.png",
            TargetBodyPart.RightFoot => "/Textures/_Shitmed/Interface/Targeting/Doll/rightfoot.png",
            _ => null
        };

        return path != null ? new SpriteSpecifier.Texture(new ResPath(path)) : null;
    }
}

/// <summary>
/// Custom radial menu showing two concentric RadialContainers
/// </summary>
public sealed class TideStyleRadialMenu : BaseWindow
{
    [Dependency] private readonly IClyde _clyde = default!;
    [Dependency] private readonly IInputManager _inputManager = default!;

    private RotatedRadialContainer _outerContainer;
    private RadialContainer _centerContainer;
    private RadialMenuOuterAreaButton _outerAreaButton;

    public event Action? OnClose;
    public event Action? OnOuterAreaClose;

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var result = base.ArrangeOverride(finalSize);

        var center = finalSize * 0.5f;

        _outerAreaButton.ParentCenter = center;

        var ringRadius = _outerContainer.CalculatedRadius * _outerContainer.OuterRadiusMultiplier;

        _outerAreaButton.OuterRadius = ringRadius * 1.05f;

        return result;
    }

    public TideStyleRadialMenu()
    {
        IoCManager.InjectDependencies(this);

        _outerContainer = new RotatedRadialContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            InitialRadius = 40,
            Rotation = MathF.PI / -9,
            ReserveSpaceForHiddenChildren = false,
            Visible = true
        };
        AddChild(_outerContainer);

        _centerContainer = new RadialContainer
        {
            HorizontalExpand = true,
            VerticalExpand = true,
            InitialRadius = 0,
            ReserveSpaceForHiddenChildren = false,
            Visible = true
        };
        AddChild(_centerContainer);

        _outerAreaButton = new RadialMenuOuterAreaButton
        {
            HorizontalExpand = true,
            VerticalExpand = true
        };
        _outerAreaButton.OnButtonUp += _ =>
        {
            OnOuterAreaClose?.Invoke();
            Close();
        };
        AddChild(_outerAreaButton);
    }


    public void AddOuterButton(string tooltip, SpriteSpecifier? sprite, SpriteSystem sprites, Action onPressed)
    {
        const float sizeMul = 1f / 2f;

        var button = new RadialMenuTextureButtonWithSector
        {
            SetSize = new Vector2(64f, 64f) * sizeMul,
            ToolTip = tooltip,
            DrawBorder = true,
            DrawBackground = true
        };

        if (sprite != null)
        {
            var scale = Vector2.One;
            var texture = sprites.Frame0(sprite);

            if (texture.Width <= 32)
                scale *= 2;

            scale *= sizeMul;

            button.TextureNormal = texture;
            button.Scale = scale;
        }

        button.OnPressed += _ => onPressed();
        _outerContainer.AddChild(button);
    }

    public void AddCenterButton(string tooltip, SpriteSpecifier? sprite, SpriteSystem sprites, Action onPressed)
    {
        const float sizeMul = 1f / 2f;

        var button = new RadialMenuTextureButtonWithSector
        {
            SetSize = new Vector2(48f, 48f) * sizeMul,
            ToolTip = tooltip,
            DrawBorder = true,
            DrawBackground = true
        };

        if (sprite != null)
        {
            var scale = Vector2.One;
            var texture = sprites.Frame0(sprite);

            if (texture.Width <= 32)
                scale *= 2;

            scale *= sizeMul;

            button.TextureNormal = texture;
            button.Scale = scale;
        }

        button.OnPressed += _ => onPressed();
        _centerContainer.AddChild(button);
    }

    public void OpenOverMouseScreenPosition()
    {
        var vpSize = _clyde.ScreenSize;
        OpenCenteredAt(_inputManager.MouseScreenPosition.Position / vpSize);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            OnClose?.Invoke();
        }
        base.Dispose(disposing);
    }
}
