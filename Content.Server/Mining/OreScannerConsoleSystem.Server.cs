// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mining.Components;
using Content.Server.Power.Components;
using Content.Shared.Power;
using Content.Server.Power.EntitySystems;
using Content.Shared.ActionBlocker;
using Content.Shared.Mining.BUIStates;
using Content.Shared.Mining.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using System.Numerics;

namespace Content.Server.Mining.Systems;

public sealed class OreScannerConsoleSystem : SharedOreScannerConsoleSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;

    private readonly Dictionary<string, Color> _oreColors = new()
    {
        // IronRock variants
        { "IronRockIron", Color.Gray },
        { "IronRockCoal", Color.Black },
        { "IronRockQuartz", Color.White },
        { "IronRockGold", Color.Gold },
        { "IronRockSilver", Color.Silver },
        { "IronRockPlasma", Color.Purple },
        { "IronRockUranium", Color.Green },
        { "IronRockBananium", Color.Yellow },
        { "IronRockArtifactFragment", Color.Cyan },
        { "IronRockDiamond", Color.LightBlue },
        { "IronRockBSCrystal", Color.Blue },
        { "IronRockGibtonite", Color.Orange },
        { "IronRockSalt", Color.White },
        
        // AsteroidRock variants
        { "AsteroidRockCoal", Color.Black },
        { "AsteroidRockGold", Color.Gold },
        { "AsteroidRockDiamond", Color.LightBlue },
        { "AsteroidRockPlasma", Color.Purple },
        { "AsteroidRockQuartz", Color.White },
        { "AsteroidRockSilver", Color.Silver },
        { "AsteroidRockTin", Color.Gray },
        { "AsteroidRockUranium", Color.Green },
        { "AsteroidRockBananium", Color.Yellow },
        { "AsteroidRockSalt", Color.White },
        { "AsteroidRockArtifactFragment", Color.Cyan },
        { "AsteroidRockGibtonite", Color.Orange },
        
        // MeteorRock variants
        { "MeteorRockBSCrystal", Color.Blue },
        
        // WallRock variants
        { "WallRockCoal", Color.Black },
        { "WallRockGold", Color.Gold },
        { "WallRockDiamond", Color.LightBlue },
        { "WallRockPlasma", Color.Purple },
        { "WallRockQuartz", Color.White },
        { "WallRockSilver", Color.Silver },
        { "WallRockTin", Color.Gray },
        { "WallRockUranium", Color.Green },
        { "WallRockBananium", Color.Yellow },
        { "WallRockArtifactFragment", Color.Cyan },
        { "WallRockSalt", Color.White },
        { "WallRockBSCrystal", Color.Blue },
        
        // WallRockBasalt variants
        { "WallRockBasaltCoal", Color.Black },
        { "WallRockBasaltGold", Color.Gold },
        { "WallRockBasaltDiamond", Color.LightBlue },
        { "WallRockBasaltPlasma", Color.Purple },
        { "WallRockBasaltQuartz", Color.White },
        { "WallRockBasaltSilver", Color.Silver },
        { "WallRockBasaltTin", Color.Gray },
        { "WallRockBasaltUranium", Color.Green },
        { "WallRockBasaltBananium", Color.Yellow },
        { "WallRockBasaltArtifactFragment", Color.Cyan },
        { "WallRockBasaltSalt", Color.White },
        { "WallRockBasaltBSCrystal", Color.Blue },
        
        // WallRockSnow variants
        { "WallRockSnowCoal", Color.Black },
        { "WallRockSnowGold", Color.Gold },
        { "WallRockSnowDiamond", Color.LightBlue },
        { "WallRockSnowPlasma", Color.Purple },
        { "WallRockSnowQuartz", Color.White },
        { "WallRockSnowSilver", Color.Silver },
        { "WallRockSnowTin", Color.Gray },
        { "WallRockSnowUranium", Color.Green },
        { "WallRockSnowBananium", Color.Yellow },
        { "WallRockSnowArtifactFragment", Color.Cyan },
        { "WallRockSnowSalt", Color.White },
        { "WallRockSnowBSCrystal", Color.Blue },
        
        // WallRockSand variants
        { "WallRockSandCoal", Color.Black },
        { "WallRockSandGold", Color.Gold },
        { "WallRockSandDiamond", Color.LightBlue },
        { "WallRockSandPlasma", Color.Purple },
        { "WallRockSandQuartz", Color.White },
        { "WallRockSandSilver", Color.Silver },
        { "WallRockSandTin", Color.Gray },
        { "WallRockSandUranium", Color.Green },
        { "WallRockSandBananium", Color.Yellow },
        { "WallRockSandArtifactFragment", Color.Cyan },
        { "WallRockSandSalt", Color.White },
        { "WallRockSandBSCrystal", Color.Blue },
        
        // WallRockChromite variants
        { "WallRockChromiteCoal", Color.Black },
        { "WallRockChromiteGold", Color.Gold },
        { "WallRockChromiteDiamond", Color.LightBlue },
        { "WallRockChromitePlasma", Color.Purple },
        { "WallRockChromiteQuartz", Color.White },
        { "WallRockChromiteSilver", Color.Silver },
        { "WallRockChromiteTin", Color.Gray },
        { "WallRockChromiteUranium", Color.Green },
        { "WallRockChromiteBananium", Color.Yellow },
        { "WallRockChromiteArtifactFragment", Color.Cyan },
        { "WallRockChromiteSalt", Color.White },
        { "WallRockChromiteBSCrystal", Color.Blue },
        
        // WallRockAndesite variants
        { "WallRockAndesiteCoal", Color.Black },
        { "WallRockAndesiteGold", Color.Gold },
        { "WallRockAndesiteDiamond", Color.LightBlue },
        { "WallRockAndesitePlasma", Color.Purple },
        { "WallRockAndesiteQuartz", Color.White },
        { "WallRockAndesiteSilver", Color.Silver },
        { "WallRockAndesiteTin", Color.Gray },
        { "WallRockAndesiteUranium", Color.Green },
        { "WallRockAndesiteBananium", Color.Yellow },
        { "WallRockAndesiteArtifactFragment", Color.Cyan },
        { "WallRockAndesiteSalt", Color.White },
        { "WallRockAndesiteBSCrystal", Color.Blue },
    };

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SharedOreScannerConsoleComponent, ComponentStartup>(OnConsoleStartup);
        SubscribeLocalEvent<SharedOreScannerConsoleComponent, PowerChangedEvent>(OnConsolePowerChange);
        SubscribeLocalEvent<SharedOreScannerConsoleComponent, AnchorStateChangedEvent>(OnConsoleAnchorChange);
        SubscribeLocalEvent<SharedOreScannerConsoleComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    private void OnConsoleStartup(EntityUid uid, SharedOreScannerConsoleComponent component, ComponentStartup args)
    {
        // Don't update immediately on startup, wait for UI opening
    }

    private void OnConsolePowerChange(EntityUid uid, SharedOreScannerConsoleComponent component, ref PowerChangedEvent args)
    {
        if (_uiSystem.IsUiOpen(uid, OreScannerUiKey.Key))
            UpdateState(uid, component);
    }

    private void OnConsoleAnchorChange(EntityUid uid, SharedOreScannerConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        if (_uiSystem.IsUiOpen(uid, OreScannerUiKey.Key))
            UpdateState(uid, component);
    }

    private void OnUIOpened(EntityUid uid, SharedOreScannerConsoleComponent component, BoundUIOpenedEvent args)
    {
        UpdateState(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SharedOreScannerConsoleComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var console, out var xform))
        {
            if (_uiSystem.IsUiOpen(uid, OreScannerUiKey.Key) &&
                _timing.CurTime - console.LastUpdate > TimeSpan.FromSeconds(console.UpdateRate))
            {
                console.LastUpdate = _timing.CurTime;
                UpdateState(uid, console);
            }
        }
    }

    private void UpdateState(EntityUid uid, SharedOreScannerConsoleComponent component)
    {
        // Validate the entity still exists
        if (!Exists(uid))
            return;

        var xform = Transform(uid);
        
        // Check if map is valid
        if (xform.MapID == MapId.Nullspace)
            return;

        // Get world position
        var consolePos = _transform.GetWorldPosition(xform);
        var consoleAngle = _transform.GetWorldRotation(xform);

        var state = new OreScannerInterfaceState
        {
            ConsolePosition = consolePos,
            ConsoleAngle = consoleAngle,
            MaxRange = component.MaxRange,
            RotateWithEntity = true
        };

        var ores = new List<OreBlip>();
        var scanBox = Box2.CenteredAround(consolePos, new Vector2(component.MaxRange * 2));

        try
        {
            foreach (var entity in _lookup.GetEntitiesIntersecting(xform.MapID, scanBox))
            {
                // Skip invalid entities
                if (!Exists(entity))
                    continue;

                var metadata = MetaData(entity);
                if (metadata.EntityPrototype == null)
                    continue;

                var prototype = metadata.EntityPrototype.ID;

                if (string.IsNullOrEmpty(prototype) || !_oreColors.ContainsKey(prototype))
                    continue;

                if (!HasComp<MiningScannerViewableComponent>(entity))
                    continue;

                var oreXform = Transform(entity);
                var orePos = _transform.GetWorldPosition(oreXform);

                var distance = (orePos - consolePos).Length();

                if (distance > component.MaxRange)
                    continue;

                ores.Add(new OreBlip
                {
                    Position = orePos,
                    OreType = prototype,
                    BlipColor = _oreColors[prototype]
                });
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Error scanning for ores: {ex}");
            return;
        }

        state.Ores = ores;

        // Only set UI state if UI is actually open
        if (_uiSystem.IsUiOpen(uid, OreScannerUiKey.Key))
        {
            _uiSystem.SetUiState(uid, OreScannerUiKey.Key, 
                new OreScannerBoundUserInterfaceState(state));
        }
    }
}
