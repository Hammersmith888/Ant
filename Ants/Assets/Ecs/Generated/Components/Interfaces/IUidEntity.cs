//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ComponentEntityApiInterfaceGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public partial interface IUidEntity {

    Ecs.Sources.Custom.Components.UidComponent uid { get; }
    bool hasUid { get; }

    void AddUid(Ecs.Utils.Uid newValue);
    void ReplaceUid(Ecs.Utils.Uid newValue);
    void RemoveUid();
}