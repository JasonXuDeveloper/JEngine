using ILRuntime.Mono.Cecil.Cil;

namespace JEngine.Editor
{
    public static partial class Optimizer
    {
        private static string GetLdLocName(Instruction instruction)
        {
            var code = instruction.OpCode.Code.ToString();
            if (code.StartsWith("Ldloc"))
            {
                if (code.EndsWith("S"))
                {
                    return instruction.Operand.ToString();
                }

                return code.Replace("Ldloc", "").Replace("_", "");
            }

            return null;
        }

        private static string GetStLocName(Instruction instruction)
        {
            var code = instruction.OpCode.Code.ToString();
            if (code.StartsWith("Stloc"))
            {
                if (code.EndsWith("S"))
                {
                    return instruction.Operand.ToString();
                }

                return code.Replace("Stloc", "").Replace("_", "");
            }

            return null;
        }

        private static long GetLdcNumForInteger(Instruction instruction)
        {
            var code = instruction.OpCode.Code;
            switch (code)
            {
                case Code.Ldc_I4_M1:
                    return -1;
                case Code.Ldc_I4_0:
                    return 0;
                case Code.Ldc_I4_1:
                    return 1;
                case Code.Ldc_I4_2:
                    return 2;
                case Code.Ldc_I4_3:
                    return 3;
                case Code.Ldc_I4_4:
                    return 4;
                case Code.Ldc_I4_5:
                    return 5;
                case Code.Ldc_I4_6:
                    return 6;
                case Code.Ldc_I4_7:
                    return 7;
                case Code.Ldc_I4_8:
                    return 8;
                case Code.Ldc_I4_S:
                    return (sbyte)instruction.Operand;
                case Code.Ldc_I4:
                    return (int)instruction.Operand;
                case Code.Ldc_I8:
                    return (long)instruction.Operand;
            }

            return 0;
        }
    }
}