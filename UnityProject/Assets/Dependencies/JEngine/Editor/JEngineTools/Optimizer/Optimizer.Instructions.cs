using ILRuntime.Mono.Cecil.Cil;

namespace JEngine.Editor
{
    public static partial class Optimizer
    {
        private static Instruction NewLdcInstruction(long i)
        {
            switch (i)
            {
                case -1:
                    return Instruction.Create(OpCodes.Ldc_I4_M1);
                case 0:
                    return Instruction.Create(OpCodes.Ldc_I4_0);
                case 1:
                    return Instruction.Create(OpCodes.Ldc_I4_1);
                case 2:
                    return Instruction.Create(OpCodes.Ldc_I4_2);
                case 3:
                    return Instruction.Create(OpCodes.Ldc_I4_3);
                case 4:
                    return Instruction.Create(OpCodes.Ldc_I4_4);
                case 5:
                    return Instruction.Create(OpCodes.Ldc_I4_5);
                case 6:
                    return Instruction.Create(OpCodes.Ldc_I4_6);
                case 7:
                    return Instruction.Create(OpCodes.Ldc_I4_7);
                case 8:
                    return Instruction.Create(OpCodes.Ldc_I4_8);
                case var _ when i <= 127 && i >= -128:
                    return Instruction.Create(OpCodes.Ldc_I4_S, (sbyte)i);
                case var _ when i <= int.MaxValue && i >= int.MinValue:
                    return Instruction.Create(OpCodes.Ldc_I4, (int)i);
                default:
                    return Instruction.Create(OpCodes.Ldc_I8, i);
            }
        }
    }
}