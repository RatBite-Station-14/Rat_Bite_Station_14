namespace Content.Ratbite.Shared.Bank;

[Prototype]
public sealed partial class FranchisePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public List<EntProtoId> BasicObjectives = new();

    [DataField]
    public List<EntProtoId> SpecialObjectives = new();

    [DataField]
    public Color CompanyColor = Color.Black;
}
