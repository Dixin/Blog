.class private auto ansi beforefieldinit Data
    extends [mscorlib]System.Object
{
    .method assembly hidebysig instance int32 InstanceAdd (
        int32 value1,
        int32 value2) cil managed 
    {
		.maxstack  2
		.locals init ([0] int32 V_0) // Local int variable V_0.
		IL_0000:  nop // Do nothing.
		IL_0001:  ldarg.0 // Load first argument this.
		IL_0002:  ldfld int32 Data::'value' // Load field this.value.
		IL_0007:  ldarg.1 // Load second argument value1.
		IL_0008:  add // Add this.value and value1.
		IL_0009:  ldarg.2 // Load third argument value2.
		IL_000a:  add // Add value2.
		IL_000b:  stloc.0 // Set result to first local variable V_0.
		IL_000c:  br.s IL_000e // Transfer control to IL_000e.
		IL_000e:  ldloc.0 // Load first local variable V_0.
		IL_000f:  ret // Return V_0.
    }

    .method assembly hidebysig static int32 StaticAdd (
        class Data this,
        int32 value1,
        int32 value2) cil managed 
    {
		.maxstack  2
		.locals init ([0] int32 V_0) // Local int variable V_0.
		IL_0000:  nop // Do nothing.
		IL_0001:  ldarg.0 // Load first argument this.
		IL_0002:  ldfld int32 Data::'value' // Load field this.value.
		IL_0007:  ldarg.1 // Load second argument value1.
		IL_0008:  add // Add this.value and value1.
		IL_0009:  ldarg.2 // Load third argument value2.
		IL_000a:  add // Add value2.
		IL_000b:  stloc.0 // Set result to first local variable V_0.
		IL_000c:  br.s IL_000e // Transfer control to IL_000e.
		IL_000e:  ldloc.0 // Load first local variable V_0.
		IL_000f:  ret // Return V_0.
    }

    // Other members.
}
