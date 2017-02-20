.class private auto ansi beforefieldinit Tutorial.Functional.Data
    extends [mscorlib]System.Object
{
    .method assembly hidebysig instance int32 InstanceAdd (
        int32 value1,
        int64 value2) cil managed 
    {
        .maxstack 2
        .locals init ([0] int64 V_0) // Local variable V_0 of long type.
        IL_0000: nop // Do nothing.
        IL_0001: ldarg.0 // Load 1st argument this.
        IL_0002: ldfld int32 Tutorial.Functional.Data::'value' // Load field this.value.
        IL_0007: ldarg.1 // Load 2nd argument value1.
        IL_0008: add // Add.
        IL_0009: conv.i8 // Convert result to long.
        IL_000a: ldarg.2 // Load 3rd argument value2.
        IL_000b: add // Add.
        IL_000c: stloc.0 // Set result to local variable V_0.
        IL_000d: br.s IL_000f // Transfer control to IL_000f.
        IL_000f: ldloc.0 // Load V_0.
        IL_0010: ret // Return.
    }

    .method assembly hidebysig static int32 StaticAdd (
        class Tutorial.Functional.Data this,
        int32 value1,
        int64 value2) cil managed 
    {
        .maxstack 2
        .locals init ([0] int64 V_0) // Local variable V_0 of long type.
        IL_0000: nop // Do nothing.
        IL_0001: ldarg.0 // Load 1st argument this.
        IL_0002: ldfld int32 Tutorial.Functional.Data::'value' // Load field this.value.
        IL_0007: ldarg.1 // Load 2nd argument value1.
        IL_0008: add // Add.
        IL_0009: conv.i8 // Convert result to long.
        IL_000a: ldarg.2 // Load 3rd argument value2.
        IL_000b: add // Add.
        IL_000c: stloc.0 // Set result to local variable V_0.
        IL_000d: br.s IL_000f // Transfer control to IL_000f.
        IL_000f: ldloc.0 // Load V_0.
        IL_0010: ret // Return.
    }

    // Other members.
}
