.class private abstract auto ansi sealed beforefieldinit Dixin.Console.Program
       extends [mscorlib]System.Object
{
    .method private hidebysig static void  Catch() cil managed
    {
        .maxstack  2
        .locals init ([0] class [mscorlib]System.Exception exception)
        IL_0000:  nop
        .try
        {
            IL_0001:  nop
            IL_0002:  call       void Dixin.Console.Program::A()
            IL_0007:  nop
            IL_0008:  nop
            IL_0009:  leave.s    IL_0017
        }  // end .try
        catch [mscorlib]System.Exception 
        {
            IL_000b:  stloc.0
            IL_000c:  nop
            IL_000d:  ldloc.0
            IL_000e:  ldc.i4.0
            IL_000f:  call       bool Dixin.Console.Program::Log(object,
                                                                bool)
            IL_0014:  pop
            IL_0015:  rethrow
        }  // end handler
        IL_0017:  ret
    } // end of method Program::Catch

.method private hidebysig static void  When() cil managed
{
    .maxstack  2
    .locals init ([0] class [mscorlib]System.Exception exception,
                [1] bool V_1)
    IL_0000:  nop
    .try
    {
        IL_0001:  nop
        IL_0002:  call       void Dixin.Console.Program::A()
        IL_0007:  nop
        IL_0008:  nop
        IL_0009:  leave.s    IL_002a

    }  // end .try
    filter
    {
        IL_000b:  isinst     [mscorlib]System.Exception
        IL_0010:  dup
        IL_0011:  brtrue.s   IL_0017

        IL_0013:  pop
        IL_0014:  ldc.i4.0
        IL_0015:  br.s       IL_0024

        IL_0017:  stloc.0
        IL_0018:  ldloc.0
        IL_0019:  ldc.i4.0
        IL_001a:  call       bool Dixin.Console.Program::Log(object,
                                                            bool)
        IL_001f:  stloc.1
        IL_0020:  ldloc.1
        IL_0021:  ldc.i4.0
        IL_0022:  cgt.un
        IL_0024:  endfilter
    }  // end filter
    {  // handler
        IL_0026:  pop
        IL_0027:  nop
        IL_0028:  rethrow
    }  // end handler
    IL_002a:  ret
} // end of method Program::When
} // end of class Dixin.Console.Program
