//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by Entitas.CodeGeneration.Plugins.ContextGenerator.
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
public sealed partial class ResourceContext : Entitas.Context<ResourceEntity> {

    public ResourceContext()
        : base(
            ResourceComponentsLookup.TotalComponents,
            0,
            new Entitas.ContextInfo(
                "Resource",
                ResourceComponentsLookup.componentNames,
                ResourceComponentsLookup.componentTypes
            ),
            (entity) =>

#if (ENTITAS_FAST_AND_UNSAFE)
                new Entitas.UnsafeAERC()
#else
                new Entitas.SafeAERC(entity)
#endif

        ) {
    }
}