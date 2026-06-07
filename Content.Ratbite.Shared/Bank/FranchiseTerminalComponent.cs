namespace Content.Ratbite.Shared.Bank;

[RegisterComponent, NetworkedComponent]
public sealed partial class FranchiseTerminalComponent : Component
{
    [DataField]
    public ProtoId<FranchisePrototype>? Franchise;

    [DataField]
    public string CompanyName = "INSERTNAMEHERE";

    [DataField]
    public HashSet<NetEntity> Workers = new();
}
