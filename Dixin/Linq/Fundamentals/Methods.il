.class public auto ansi beforefieldinit Dixin.Linq.LinqToObjects.Methods
    extends [mscorlib]System.Object
{
    .method public hidebysig static 
        bool Same (
            class LinqToObjects.Methods this,
            class LinqToObjects.Methods other
        ) cil managed 
    {
        .maxstack 2
        .locals init (
            [0] bool CS$1$0000
        )

        IL_0000: nop
        IL_0001: ldarg.0 // Loads the first argument this.
        IL_0002: ldarg.1 // Loads the second argument other.
        IL_0003: ceq
        IL_0005: stloc.0
        IL_0006: br.s IL_0008

        IL_0008: ldloc.0
        IL_0009: ret
    }

    .method public hidebysig 
        instance bool SameTo (
            class LinqToObjects.Methods other
        ) cil managed 
    {
        .maxstack 2
        .locals init (
            [0] bool CS$1$0000
        )

        IL_0000: nop
        IL_0001: ldarg.0 // Loads the first argument this.
        IL_0002: ldarg.1 // Loads the second argument other.
        IL_0003: ceq
        IL_0005: stloc.0
        IL_0006: br.s IL_0008

        IL_0008: ldloc.0
        IL_0009: ret
    }
}
