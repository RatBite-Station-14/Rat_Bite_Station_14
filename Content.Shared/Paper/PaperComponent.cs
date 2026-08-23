// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared._BRatbite.Paper;

namespace Content.Shared.Paper;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PaperComponent : Component
{
    public PaperAction Mode;
    [DataField("content"), AutoNetworkedField]
    public string Content { get; set; } = "";

    [DataField("contentSize")]
    public int ContentSize { get; set; } = 10000;

    [DataField("stampedBy"), AutoNetworkedField]
    public List<StampDisplayInfo> StampedBy { get; set; } = new();

    /// <summary>
    ///     Stamp to be displayed on the paper, state from bureaucracy.rsi
    /// </summary>
    [DataField("stampState"), AutoNetworkedField]
    public string? StampState { get; set; }

    [DataField, AutoNetworkedField]
    public bool EditingDisabled;

    /// <summary>
    /// Sound played after writing to the paper.
    /// </summary>
    [DataField("sound")]
    public SoundSpecifier? Sound { get; private set; } = new SoundCollectionSpecifier("PaperScribbles", AudioParams.Default.WithVariation(0.1f));

    [Serializable, NetSerializable]
    public sealed class PaperBoundUserInterfaceState : BoundUserInterfaceState
    {
        public readonly string Text;
        public readonly List<StampDisplayInfo> StampedBy;
        public readonly PaperAction Mode;
        public readonly List<PaperStroke> Strokes; // Ratbite: paper drawing

        public PaperBoundUserInterfaceState(string text, List<StampDisplayInfo> stampedBy, List<PaperStroke> strokes, PaperAction mode = PaperAction.Read)
        {
            Text = text;
            StampedBy = stampedBy;
            Mode = mode;
            Strokes = strokes;
        }
    }

    [Serializable, NetSerializable]
    public sealed class PaperInputTextMessage : BoundUserInterfaceMessage
    {
        public readonly string Text;
        public readonly List<PaperStroke> Strokes; // Ratbite: paper drawing

        public PaperInputTextMessage(string text, List<PaperStroke> strokes)
        {
            Text = text;
            Strokes = strokes;
        }
    }

    // Starlight-start
    [Serializable, NetSerializable]
    public sealed class PaperSignatureRequestMessage : BoundUserInterfaceMessage
    {
        public readonly int SignatureIndex;

        public PaperSignatureRequestMessage(int signatureIndex)
        {
            SignatureIndex = signatureIndex;
        }
    }
    // Starlight-end
    [Serializable, NetSerializable]
    public enum PaperUiKey
    {
        Key
    }

    [Serializable, NetSerializable]
    public enum PaperAction
    {
        Read,
        Write,
    }

    [Serializable, NetSerializable]
    public enum PaperVisuals : byte
    {
        Status,
        Stamp
    }

    [Serializable, NetSerializable]
    public enum PaperStatus : byte
    {
        Blank,
        Written
    }

    // Ratbite change: Drawings
    [DataField("strokes"), AutoNetworkedField]
    public List<PaperStroke> Strokes { get; set; } = new();

    // Worst case scenario this is about 20KB of data, should be manageable
    // (500 strokes with 2 points each, real drawings will have less)
    [DataField]
    public int MaxDrawingPoints { get; set; } = 2000;
}
