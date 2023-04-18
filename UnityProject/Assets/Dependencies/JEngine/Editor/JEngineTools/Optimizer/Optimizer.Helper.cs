using ILRuntime.Mono.Cecil.Cil;

namespace JEngine.Editor
{
    public static partial class Optimizer
    {
        private static string GetLdLocName(Instruction instruction)
        {
            var code = instruction.OpCode.Code.ToString();
            if(code.StartsWith("Ldloc"))
            {
                if(code.EndsWith("S"))
                {
                    return instruction.Operand.ToString();
                }
                
                return code.Substring(code.Length - 1);
            }

            return null;
        }
        
        private static string GetStLocName(Instruction instruction)
        {
            var code = instruction.OpCode.Code.ToString();
            if(code.StartsWith("Stloc"))
            {
                if(code.EndsWith("S"))
                {
                    return instruction.Operand.ToString();
                }
                
                return code.Substring(code.Length - 1);
            }

            return null;
        }
        
        private static int GetLdcI4Num(Instruction instruction)
        {
            var code = instruction.OpCode.Code.ToString();
            if(code.StartsWith("Ldc_I4"))
            {
                if(code.EndsWith("S"))
                {
                    return (sbyte)instruction.Operand;
                }

                if(code.EndsWith("M1"))
                {
                    return -1;
                }

                if(code.EndsWith("0"))
                {
                    return 0;
                }

                if(code.EndsWith("1"))
                {
                    return 1;
                }

                if(code.EndsWith("2"))
                {
                    return 2;
                }

                if(code.EndsWith("3"))
                {
                    return 3;
                }

                if(code.EndsWith("4"))
                {
                    return 4;
                }

                if(code.EndsWith("5"))
                {
                    return 5;
                }

                if(code.EndsWith("6"))
                {
                    return 6;
                }

                if(code.EndsWith("7"))
                {
                    return 7;
                }

                if(code.EndsWith("8"))
                {
                    return 8;
                }

                return (int)instruction.Operand;
            }

            return 0;
        }
    }
}