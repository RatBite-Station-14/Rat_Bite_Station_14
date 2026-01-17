// SPDX-FileCopyrightText: 2025 Your Name
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Numerics;
using Content.Shared.Mining.BUIStates;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Map;
using Robust.Shared.Input;
using Robust.Shared.Maths;

namespace Content.Client.Mining.UI;

public sealed class OreDisplayControl : Control
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private readonly SharedTransformSystem _transformSystem;
    private OreScannerInterfaceState? _state;
    
    // Display constants (matching NavMapControl style)
    protected static float MinDisplayedRange = 8f;
    protected static float MaxDisplayedRange = 256f;
    protected static float DefaultDisplayedRange = 64f;
    
    private float _worldRange = DefaultDisplayedRange;
    private Vector2 _offset = Vector2.Zero;
    private bool _recentering = false;
    
    // Colors
    private Color _backgroundColor = new(30, 67, 30);
    private Color _gridColor = new(102, 217, 102);
    
    private readonly Dictionary<string, string> _oreNames = new()
    {
        // IronRock variants
        { "IronRockIron", "Iron" },
        { "IronRockCoal", "Coal" },
        { "IronRockQuartz", "Quartz" },
        { "IronRockGold", "Gold" },
        { "IronRockSilver", "Silver" },
        { "IronRockPlasma", "Plasma" },
        { "IronRockUranium", "Uranium" },
        { "IronRockBananium", "Bananium" },
        { "IronRockArtifactFragment", "Artifact" },
        { "IronRockDiamond", "Diamond" },
        { "IronRockBSCrystal", "Bluespace" },
        { "IronRockGibtonite", "Gibtonite" },
        { "IronRockSalt", "Salt" },
        
        // AsteroidRock variants
        { "AsteroidRockCoal", "Coal" },
        { "AsteroidRockGold", "Gold" },
        { "AsteroidRockDiamond", "Diamond" },
        { "AsteroidRockPlasma", "Plasma" },
        { "AsteroidRockQuartz", "Quartz" },
        { "AsteroidRockSilver", "Silver" },
        { "AsteroidRockTin", "Iron" },
        { "AsteroidRockUranium", "Uranium" },
        { "AsteroidRockBananium", "Bananium" },
        { "AsteroidRockSalt", "Salt" },
        { "AsteroidRockArtifactFragment", "Artifact" },
        { "AsteroidRockGibtonite", "Gibtonite" },
        
        // MeteorRock variants
        { "MeteorRockBSCrystal", "Bluespace" },
        
        // WallRock variants
        { "WallRockCoal", "Coal" },
        { "WallRockGold", "Gold" },
        { "WallRockDiamond", "Diamond" },
        { "WallRockPlasma", "Plasma" },
        { "WallRockQuartz", "Quartz" },
        { "WallRockSilver", "Silver" },
        { "WallRockTin", "Iron" },
        { "WallRockUranium", "Uranium" },
        { "WallRockBananium", "Bananium" },
        { "WallRockArtifactFragment", "Artifact" },
        { "WallRockSalt", "Salt" },
        { "WallRockBSCrystal", "Bluespace" },
        
        // WallRockBasalt variants
        { "WallRockBasaltCoal", "Coal" },
        { "WallRockBasaltGold", "Gold" },
        { "WallRockBasaltDiamond", "Diamond" },
        { "WallRockBasaltPlasma", "Plasma" },
        { "WallRockBasaltQuartz", "Quartz" },
        { "WallRockBasaltSilver", "Silver" },
        { "WallRockBasaltTin", "Iron" },
        { "WallRockBasaltUranium", "Uranium" },
        { "WallRockBasaltBananium", "Bananium" },
        { "WallRockBasaltArtifactFragment", "Artifact" },
        { "WallRockBasaltSalt", "Salt" },
        { "WallRockBasaltBSCrystal", "Bluespace" },
        
        // WallRockSnow variants
        { "WallRockSnowCoal", "Coal" },
        { "WallRockSnowGold", "Gold" },
        { "WallRockSnowDiamond", "Diamond" },
        { "WallRockSnowPlasma", "Plasma" },
        { "WallRockSnowQuartz", "Quartz" },
        { "WallRockSnowSilver", "Silver" },
        { "WallRockSnowTin", "Iron" },
        { "WallRockSnowUranium", "Uranium" },
        { "WallRockSnowBananium", "Bananium" },
        { "WallRockSnowArtifactFragment", "Artifact" },
        { "WallRockSnowSalt", "Salt" },
        { "WallRockSnowBSCrystal", "Bluespace" },
        
        // WallRockSand variants
        { "WallRockSandCoal", "Coal" },
        { "WallRockSandGold", "Gold" },
        { "WallRockSandDiamond", "Diamond" },
        { "WallRockSandPlasma", "Plasma" },
        { "WallRockSandQuartz", "Quartz" },
        { "WallRockSandSilver", "Silver" },
        { "WallRockSandTin", "Iron" },
        { "WallRockSandUranium", "Uranium" },
        { "WallRockSandBananium", "Bananium" },
        { "WallRockSandArtifactFragment", "Artifact" },
        { "WallRockSandSalt", "Salt" },
        { "WallRockSandBSCrystal", "Bluespace" },
        
        // WallRockChromite variants
        { "WallRockChromiteCoal", "Coal" },
        { "WallRockChromiteGold", "Gold" },
        { "WallRockChromiteDiamond", "Diamond" },
        { "WallRockChromitePlasma", "Plasma" },
        { "WallRockChromiteQuartz", "Quartz" },
        { "WallRockChromiteSilver", "Silver" },
        { "WallRockChromiteTin", "Iron" },
        { "WallRockChromiteUranium", "Uranium" },
        { "WallRockChromiteBananium", "Bananium" },
        { "WallRockChromiteArtifactFragment", "Artifact" },
        { "WallRockChromiteSalt", "Salt" },
        { "WallRockChromiteBSCrystal", "Bluespace" },
        
        // WallRockAndesite variants
        { "WallRockAndesiteCoal", "Coal" },
        { "WallRockAndesiteGold", "Gold" },
        { "WallRockAndesiteDiamond", "Diamond" },
        { "WallRockAndesitePlasma", "Plasma" },
        { "WallRockAndesiteQuartz", "Quartz" },
        { "WallRockAndesiteSilver", "Silver" },
        { "WallRockAndesiteTin", "Iron" },
        { "WallRockAndesiteUranium", "Uranium" },
        { "WallRockAndesiteBananium", "Bananium" },
        { "WallRockAndesiteArtifactFragment", "Artifact" },
        { "WallRockAndesiteSalt", "Salt" },
        { "WallRockAndesiteBSCrystal", "Bluespace" },
    };

    private Vector2? _dragStart = null;
    private Vector2 _dragStartOffset = Vector2.Zero;

    public OreDisplayControl()
    {
        IoCManager.InjectDependencies(this);
        _transformSystem = _entManager.System<SharedTransformSystem>();
        
        MouseFilter = MouseFilterMode.Stop;
        RectClipContent = true;
    }

    protected override void MouseWheel(GUIMouseWheelEventArgs args)
    {
        base.MouseWheel(args);
        
        if (args.Delta.Y > 0)
            _worldRange = Math.Max(_worldRange - 8f, MinDisplayedRange);
        else if (args.Delta.Y < 0)
            _worldRange = Math.Min(_worldRange + 8f, MaxDisplayedRange);
    }

    protected override void MouseMove(GUIMouseMoveEventArgs args)
    {
        base.MouseMove(args);
        
        if (_dragStart != null)
        {
            var delta = args.RelativePosition - _dragStart.Value;
            // Convert screen delta to world delta - flip Y to match world coordinates
            var minimapScale = GetMinimapScale();
            _offset = _dragStartOffset - new Vector2(delta.X / minimapScale, -delta.Y / minimapScale);
            _recentering = false;
        }
    }

    protected override void KeyBindDown(GUIBoundKeyEventArgs args)
    {
        base.KeyBindDown(args);
        
        if (args.Function == EngineKeyFunctions.Use)
        {
            _dragStart = args.RelativePosition;
            _dragStartOffset = _offset;
            args.Handle();
        }
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);
        
        if (args.Function == EngineKeyFunctions.Use)
        {
            _dragStart = null;
        }
    }

    public void UpdateState(OreScannerInterfaceState state)
    {
        _state = state;
    }

    public void ResetView()
    {
        _worldRange = DefaultDisplayedRange;
        _offset = Vector2.Zero;
        _recentering = true;
    }

    private float GetMinimapScale()
    {
        return Math.Min(PixelSize.X, PixelSize.Y) / (2f * _worldRange);
    }

    private Vector2 GetMidpoint()
    {
        return PixelSize / 2f;
    }

    private Vector2 ScalePosition(Vector2 position)
    {
        return position * GetMinimapScale() + GetMidpoint();
    }

    protected override void Draw(DrawingHandleScreen handle)
    {
        base.Draw(handle);

        if (_state == null)
            return;

        var minimapScale = GetMinimapScale();
        var midpoint = GetMidpoint();

        // Handle recentering
        if (_recentering)
        {
            _offset = Vector2.Zero;
        }

        var offsetVec = new Vector2(_offset.X, -_offset.Y);

        // Background
        handle.DrawRect(new UIBox2(Vector2.Zero, PixelSize), Color.Black.WithAlpha(0.9f));

        // Draw grid
        DrawGrid(handle, offsetVec, minimapScale);

        // Draw console position as crosshair
        var consoleScreenPos = ScalePosition(-offsetVec);
        DrawCrosshair(handle, consoleScreenPos);

        // Draw range circles
        DrawRangeCircles(handle, consoleScreenPos, minimapScale);

        // Draw ores as small squares (like tiles on navmap)
        if (_state.Ores != null)
        {
            foreach (var ore in _state.Ores)
            {
                var relativePos = ore.Position - _state.ConsolePosition;
                
                if (_state.RotateWithEntity)
                {
                    var angle = -_state.ConsoleAngle.Theta;
                    var cos = (float)Math.Cos(angle);
                    var sin = (float)Math.Sin(angle);
                    var rotatedX = relativePos.X * cos - relativePos.Y * sin;
                    var rotatedY = relativePos.X * sin + relativePos.Y * cos;
                    relativePos = new Vector2(rotatedX, rotatedY);
                }

                var screenPos = ScalePosition(new Vector2(relativePos.X, -relativePos.Y) - offsetVec);
                
                // Draw as a tile-sized square
                var size = Math.Max(8f, minimapScale * 1.0f); // Minimum 8 pixels, represents one tile
                var halfSize = size / 2f;
                
                var rect = new UIBox2(
                    screenPos.X - halfSize,
                    screenPos.Y - halfSize,
                    screenPos.X + halfSize,
                    screenPos.Y + halfSize
                );

                // Draw with slight glow
                var glowRect = new UIBox2(
                    rect.Left - 1f,
                    rect.Top - 1f,
                    rect.Right + 1f,
                    rect.Bottom + 1f
                );
                handle.DrawRect(glowRect, ore.BlipColor.WithAlpha(0.3f));
                handle.DrawRect(rect, ore.BlipColor);
            }
        }

        DrawLegend(handle);
        DrawInfo(handle);
    }

    private void DrawCrosshair(DrawingHandleScreen handle, Vector2 center)
    {
        var crosshairSize = 6f;
        var crosshairColor = Color.LimeGreen;
        
        // Horizontal line
        handle.DrawLine(
            center + new Vector2(-crosshairSize, 0),
            center + new Vector2(crosshairSize, 0),
            crosshairColor);
        
        // Vertical line
        handle.DrawLine(
            center + new Vector2(0, -crosshairSize),
            center + new Vector2(0, crosshairSize),
            crosshairColor);
        
        // Center dot
        handle.DrawRect(new UIBox2(center - new Vector2(1, 1), center + new Vector2(1, 1)), crosshairColor);
    }

    private void DrawGrid(DrawingHandleScreen handle, Vector2 offsetVec, float minimapScale)
    {
        var gridColor = _gridColor.WithAlpha(0.15f);
        var axisColor = _gridColor.WithAlpha(0.3f);
        var gridSpacing = 32f; // Grid every 32 tiles
        var scaledSpacing = gridSpacing * minimapScale;
        var midpoint = GetMidpoint();

        var offsetScreen = offsetVec * minimapScale;
        var gridOffsetX = (midpoint.X - offsetScreen.X) % scaledSpacing;
        var gridOffsetY = (midpoint.Y + offsetScreen.Y) % scaledSpacing;

        // Draw vertical lines
        for (var x = gridOffsetX; x < PixelSize.X; x += scaledSpacing)
        {
            var isAxis = Math.Abs(x - midpoint.X + offsetScreen.X) < 0.5f;
            handle.DrawLine(new Vector2(x, 0), new Vector2(x, PixelSize.Y), isAxis ? axisColor : gridColor);
        }

        // Draw horizontal lines
        for (var y = gridOffsetY; y < PixelSize.Y; y += scaledSpacing)
        {
            var isAxis = Math.Abs(y - midpoint.Y - offsetScreen.Y) < 0.5f;
            handle.DrawLine(new Vector2(0, y), new Vector2(PixelSize.X, y), isAxis ? axisColor : gridColor);
        }
    }

    private void DrawRangeCircles(DrawingHandleScreen handle, Vector2 center, float minimapScale)
    {
        var circleColor = _gridColor.WithAlpha(0.1f);
        var ranges = new[] { 0.25f, 0.5f, 0.75f, 1.0f };

        foreach (var range in ranges)
        {
            var radius = _state!.MaxRange * range * minimapScale;
            DrawCircleOutline(handle, center, radius, circleColor, 48);
        }
    }

    private void DrawCircleOutline(DrawingHandleScreen handle, Vector2 center, float radius, Color color, int segments = 64)
    {
        for (var i = 0; i < segments; i++)
        {
            var angle1 = (float)(i * 2 * Math.PI / segments);
            var angle2 = (float)((i + 1) * 2 * Math.PI / segments);
            
            var point1 = center + new Vector2(MathF.Cos(angle1), MathF.Sin(angle1)) * radius;
            var point2 = center + new Vector2(MathF.Cos(angle2), MathF.Sin(angle2)) * radius;
            
            handle.DrawLine(point1, point2, color);
        }
    }

    private void DrawLegend(DrawingHandleScreen handle)
    {
        if (_state?.Ores == null)
            return;

        var font = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 10);

        var uniqueOres = _state.Ores
            .GroupBy(o => o.OreType)
            .Select(g => (Type: g.Key, Color: g.First().BlipColor, Count: g.Count()))
            .OrderByDescending(o => o.Count)
            .Take(12)
            .ToList();

        var legendX = 10f;
        var legendY = 10f;
        var lineHeight = 18f;
        var squareSize = 10f;

        var legendWidth = 150f;
        var legendHeight = uniqueOres.Count * lineHeight + 25f;
        
        // Background
        handle.DrawRect(new UIBox2(legendX - 5, legendY - 5, legendX + legendWidth, legendY + legendHeight), 
            Color.Black.WithAlpha(0.8f));
        
        // Title
        handle.DrawString(font, new Vector2(legendX + 5, legendY + 2), "ORE DEPOSITS", Color.White);
        legendY += lineHeight + 5f;

        foreach (var (type, color, count) in uniqueOres)
        {
            // Draw square
            var squareRect = new UIBox2(
                legendX + 8 - squareSize / 2,
                legendY + 6 - squareSize / 2,
                legendX + 8 + squareSize / 2,
                legendY + 6 + squareSize / 2
            );
            handle.DrawRect(squareRect, color);

            var oreName = _oreNames.TryGetValue(type, out var name) ? name : type;
            var text = $"{oreName}: {count}";
            handle.DrawString(font, new Vector2(legendX + 25, legendY), text, Color.White);

            legendY += lineHeight;
        }
    }

    private void DrawInfo(DrawingHandleScreen handle)
    {
        if (_state == null)
            return;

        var font = new VectorFont(_resourceCache.GetResource<FontResource>("/Fonts/NotoSans/NotoSans-Regular.ttf"), 10);

        var infoX = PixelSize.X - 160f;
        var infoY = PixelSize.Y - 65f;

        // Background panel
        handle.DrawRect(new UIBox2(infoX - 5, infoY - 5, PixelSize.X - 10, PixelSize.Y - 10), 
            Color.Black.WithAlpha(0.8f));

        // Title
        handle.DrawString(font, new Vector2(infoX, infoY), "SCANNER INFO", Color.White);
        infoY += 18f;

        // Range
        var rangeText = $"Range: {_state.MaxRange:F0}m";
        handle.DrawString(font, new Vector2(infoX, infoY), rangeText, Color.LimeGreen);
        infoY += 16f;

        // Zoom
        var zoomText = $"Zoom: {(DefaultDisplayedRange / _worldRange):F1}x";
        handle.DrawString(font, new Vector2(infoX, infoY), zoomText, Color.Cyan);
        infoY += 16f;

        // Controls hint
        handle.DrawString(font, new Vector2(infoX, infoY), "Scroll: Zoom | Drag: Pan", 
            Color.Gray.WithAlpha(0.8f));
    }
}