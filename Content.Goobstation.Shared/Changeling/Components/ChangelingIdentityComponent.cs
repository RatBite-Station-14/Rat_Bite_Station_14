// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.StatusIcon;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Content.Shared.Damage;

namespace Content.Goobstation.Shared.Changeling.Components;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class ChangelingIdentityComponent : Component
{
    #region Prototypes

    [DataField("soundMeatPool")]
    public List<SoundSpecifier?> SoundPool = new()
    {
        new SoundPathSpecifier("/Audio/Effects/gib1.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib2.ogg"),
        new SoundPathSpecifier("/Audio/Effects/gib3.ogg"),
    };

    [DataField("soundShriek")]
    public SoundSpecifier ShriekSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/changeling_shriek.ogg");

    [DataField("shriekPower")]
    public float ShriekPower = 2.5f;

    [DataField("armorTransform")]
    public SoundSpecifier ArmourSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_transform.ogg");
    [DataField("armorStrip")]
    public SoundSpecifier ArmourStripSound = new SoundPathSpecifier("/Audio/_Goobstation/Changeling/Effects/armour_strip.ogg");

    public readonly List<EntProtoId> BaseChangelingActions = new()
    {
        "ActionEvolutionMenu",
        "ActionAbsorbDNA",
        "ActionStingExtractDNA",
        "ActionChangelingTransformCycle",
        "ActionChangelingTransform"
    };

    /// <summary>
    ///     The status icon corresponding to the Changlings.
    /// </summary>

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "HivemindFaction";

    #endregion

    [DataField]
    public bool StrainedMusclesActive = false;

    [DataField]
    public bool IsInLesserForm = false;

    [DataField]
    public bool IsInLastResort = false;

    public List<EntityUid>? ActiveArmor = null;

    public Dictionary<string, EntityUid?> Equipment = new();

    /// <summary>
    ///     Total evolution points gained by the changeling.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TotalEvolutionPoints;

    /// <summary>
    ///     Cooldown between chem regen events.
    /// </summary>
    public TimeSpan UpdateTimer = TimeSpan.Zero;
    public float UpdateCooldown = 1f;

    /// <summary>
    ///     All of the DNA that the changeling had extracted in their lifetime.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedHistory = new();

    /// <summary>
    ///     The DNA that the changeling has stored up.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public List<TransformData> AbsorbedDNA = new();

    /// <summary>
    ///     Index of <see cref="AbsorbedDNA"/>. Used for switching forms.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public int AbsorbedDNAIndex = 0;

    /// <summary>
    ///     Maximum amount of DNA a changeling can absorb.
    /// </summary>
    [DataField]
    public int MaxAbsorbedDNA = 5;

    /// <summary>
    ///     Total absorbed DNA. Counts towards objectives.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int TotalAbsorbedEntities = 0;

    /// <summary>
    ///     Total absorbed changelings. Used as a 'bonus' for its respective objective.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int TotalChangelingsAbsorbed = 0;

    /// <summary>
    ///     Total stolen DNA. Counts towards objectives.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public int TotalStolenDNA = 0;

    [DataField, ViewVariables(VVAccess.ReadOnly)]
    public TransformData? CurrentForm;

    [ViewVariables(VVAccess.ReadOnly)]
    public TransformData? SelectedForm;
    
        // Standard options. Try to fit these in if you can!

    /// <summary>
    ///     Sound to play when clumsy interactions fail.
    /// </summary>
    [DataField]
    public SoundSpecifier ClumsySound = new SoundPathSpecifier("/Audio/Weapons/Xeno/alien_spitacid.ogg");

    /// <summary>
    ///     Default chance to fail a clumsy interaction.
    ///     If a system needs to use something else, add a new variable in the component, do not modify this percentage.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ClumsyDefaultCheck = 20f;

    /// <summary>
    ///     Default stun time.
    ///     If a system needs to use something else, add a new variable in the component, do not modify this number.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ClumsyDefaultStunTime = TimeSpan.FromSeconds(2.5);

    // Specific options

    /// <summary>
    ///     Stun time after failing to shoot a gun.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan GunShootFailStunTime = TimeSpan.FromSeconds(3);

    /// <summary>
    ///     Damage taken after failing to shoot a gun.
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier? GunShootFailDamage = new() {DamageDict = new Dictionary<string, FixedPoint2> {{"Brute", 12}}}; // Goob edit - add a default value (evil)

    /// <summary>
    ///     Noise to play after failing to shoot a gun. Boom!
    /// </summary>
    [DataField]
    public SoundSpecifier GunShootFailSound = new SoundPathSpecifier("/Audio/Weapons/Xeno/alien_spitacid.ogg");

    /// <summary>
    ///      Whether or not to apply Clumsy to guns.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool ClumsyGuns = true;

    [DataField]
    public LocId GunFailedMessage = "clumsy-gun-fail-message";
}

[DataDefinition]
public sealed partial class TransformData
{
    /// <summary>
    ///     Entity's name.
    /// </summary>
    [DataField]
    public string Name;

    /// <summary>
    ///     Entity's fingerprint, if it exists.
    /// </summary>
    [DataField]
    public string? Fingerprint;

    /// <summary>
    ///     Entity's DNA.
    /// </summary>
    [DataField("dna")]
    public string DNA;

    /// <summary>
    ///     Entity's humanoid appearance component.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly), NonSerialized]
    public HumanoidAppearanceComponent Appearance;              
}
