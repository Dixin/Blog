.method assembly hidebysig static 
	void Optional (
		bool required1,
		char required2,
		[opt] int32 optional1,
		[opt] string optional2,
		[opt] class [System]System.Uri optional3,
		[opt] valuetype [mscorlib]System.Guid optional4,
		[opt] class [System]System.Uri optional5,
		[opt] valuetype [mscorlib]System.Guid optional6
	) cil managed 
{
	.param [3] = int32(2147483647) // optional1 = int.MaxValue
	.param [4] = "Default value." // optional2 = "Default value."
	.param [5] = nullref // optional3 = null
	.param [6] = nullref // optional4 = new Guid()
	.param [7] = nullref // optional5 = default(Uri)
	.param [8] = nullref // optional6 = default(Guid)

	.maxstack 8

	IL_0000: nop
	IL_0001: ret
}