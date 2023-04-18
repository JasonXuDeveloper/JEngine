using System.Collections.Generic;
using System.IO;
using System.Linq;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Pdb;
using JEngine.Core;
using UnityEngine;

namespace JEngine.Editor
{
    public static partial class Optimizer
    {
        private static Code[] _ldcOrLdlocCode = new Code[]
        {
            Code.Ldc_I4_0,
            Code.Ldc_I4_1,
            Code.Ldc_I4_2,
            Code.Ldc_I4_3,
            Code.Ldc_I4_4,
            Code.Ldc_I4_5,
            Code.Ldc_I4_6,
            Code.Ldc_I4_7,
            Code.Ldc_I4_8,
            Code.Ldc_I4_S,
            Code.Ldc_I4,
            Code.Ldc_I4_M1,
            Code.Ldc_I8,
            Code.Ldloc_0,
            Code.Ldloc_1,
            Code.Ldloc_2,
            Code.Ldloc_3,
            Code.Ldloc_S
        };


        /// <summary>
        /// 优化
        /// </summary>
        /// <param name="dllPath">Assembly</param>
        /// <param name="pdbPath">symbol</param>
        public static void Optimize(string dllPath, string pdbPath)
        {
            //模块
            var module = LoadModule(dllPath, pdbPath);
            //类型
            List<TypeDefinition> types = new List<TypeDefinition>();

            if (module.HasTypes)
            {
                foreach (var t in module.GetTypes()) //获取所有此模块定义的类型
                {
                    types.Add(t);

                    //测试
                    if (t.FullName == "HotUpdateScripts.Test")
                    {
                        OptimizeMethod(t.Methods.First(m => m.Name == "Optimized"));
                    }
                }
            }

            var optimizedPdbStream = new FileStream("optimized.pdb", FileMode.Create);
            var writerParameters = pdbPath != null
                ? new WriterParameters
                {
                    WriteSymbols = true, SymbolStream = optimizedPdbStream,
                    SymbolWriterProvider = new PdbWriterProvider()
                }
                : new WriterParameters();
            module.Write("optimized.dll", writerParameters);
            if (pdbPath != null) optimizedPdbStream.Dispose();

            var optimizedModule = LoadModule("optimized.dll", "optimized.pdb");
            //check type matches
            if (optimizedModule.HasTypes)
            {
                foreach (var t in optimizedModule.GetTypes())
                {
                    if (types.All(type => type.MetadataToken != t.MetadataToken))
                    {
                        Log.PrintError("Type not found: " + t);
                    }
                }
            }
        }

        /// <summary>
        /// 从流加载Assembly,以及symbol符号文件(pdb)
        /// </summary>
        /// <param name="dllPath"></param>
        /// <param name="pdbPath"></param>
        /// <returns></returns>
        private static ModuleDefinition LoadModule(string dllPath, string pdbPath)
        {
            MemoryStream dllStream = new MemoryStream(File.ReadAllBytes(dllPath));
            MemoryStream pdbStream = null;
            if (File.Exists(pdbPath))
            {
                pdbStream = new MemoryStream(File.ReadAllBytes(pdbPath));
            }

            var module = ModuleDefinition.ReadModule(dllStream); //从MONO中加载模块

            if (pdbStream != null)
            {
                var symbolReader = new PdbReaderProvider();
                module.ReadSymbols(symbolReader.GetSymbolReader(module, pdbStream)); //加载符号表
            }

            return module;
        }

        /// <summary>
        /// 优化加法
        /// </summary>
        /// <param name="method"></param>
        private static void OptimizeMethod(MethodDefinition method)
        {
            var processor = method.Body.GetILProcessor();

            var oldInstructionsCount = method.Body.Instructions.Count;
            Debug.Log($"读取{method.Name}的指令（共{oldInstructionsCount}条指令）：");
            int index = 0;
            Dictionary<string, object> constStack = new Dictionary<string, object>();
            Dictionary<string, int> loadStack = new Dictionary<string, int>();
            while (true)
            {
                var max = method.Body.Instructions.Count;
                if (index >= max)
                {
                    break;
                }

                var instruction = method.Body.Instructions.ElementAt(index);

                Debug.Log(
                    $"index: {index + 1}/{max} instruction: {instruction}");

                //记录位置
                RecordLastTimeLoadStackValue(instruction, loadStack, ref index, processor);
                
                //更新常量
                if (CheckConst(instruction, constStack, ref index))
                {
                    Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
                    continue;
                }

                //优化数学运算
                if (OptimizeArithConst(instruction, constStack, ref index, processor))
                {
                    Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
                    continue;
                }


                index++;
            }

            var newInstructionsCount = method.Body.Instructions.Count;
            Debug.Log(
                $"从{oldInstructionsCount}条指令优化到{newInstructionsCount}条指令，减少了{oldInstructionsCount - newInstructionsCount}条指令");
            Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
        }

        /// <summary>
        /// 记录上次load位置
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="loadStack"></param>
        /// <param name="index"></param>
        /// <param name="processor"></param>
        private static void RecordLastTimeLoadStackValue(in Instruction instruction, Dictionary<string, int> loadStack,
            ref int index, ILProcessor processor)
        {
            var key = GetStLocName(instruction);
            if (key != null && instruction.Previous != null)
            {
                //stloc.s key 上一句是设置为常量
                if (_ldcOrLdlocCode.Contains(instruction.Previous.OpCode.Code))
                {
                    //看看是不是之前被设置为常量并st过
                    if (loadStack.TryGetValue(key, out var oldIndex))
                    {
                        //是的话判断这两次设置常量期间有没有ldloc.s key过，没的话可以把上次的ld和stloc.s key删除
                        var hasLd = false;
                        for (int i = oldIndex + 1; i < index; i++)
                        {
                            var tempInstruction = processor.Body.Instructions[i];
                            if (tempInstruction.OpCode.Code == Code.Ldloc_S &&
                                tempInstruction.Operand.ToString() == key)
                            {
                                hasLd = true;
                                break;
                            }
                        }

                        //如果没有ld key，说明上次st的值已经被覆盖，可以删除
                        if (!hasLd)
                        {
                            var lastLd = processor.Body.Instructions[oldIndex];
                            var lastSt = processor.Body.Instructions[oldIndex + 1];
                            lastLd.Previous.Next = lastSt.Next;
                            lastSt.Next.Previous = lastLd.Previous;
                            processor.Remove(lastLd);
                            processor.Remove(lastSt);
                            Debug.LogWarning($"删除了不会起作用的ld和st: {lastLd} -> {lastSt}");
                            index = oldIndex + 1;
                        }
                    }

                    loadStack[key] = index - 1;
                }
            }
        }

        /// <summary>
        /// 缓存常量数字
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="constStack"></param>
        /// <param name="index"></param>
        /// <returns>结果</returns>
        private static bool CheckConst(in Instruction instruction, Dictionary<string, object> constStack,
            ref int index)
        {
            string key;
            index++;
            switch (instruction.OpCode.Code)
            {
                case Code.Stloc_0:
                case Code.Stloc_1:
                case Code.Stloc_2:
                case Code.Stloc_3:
                case Code.Stloc_S:
                    key = GetStLocName(instruction);
                    var previous = instruction.Previous;
                    switch (previous.OpCode.Code)
                    {
                        case Code.Ldloc_0:
                        case Code.Ldloc_1:
                        case Code.Ldloc_2:
                        case Code.Ldloc_3:
                        case Code.Ldloc_S:
                            //获取需要压入栈的值
                            var prevKey = GetLdLocName(previous);
                            if (constStack.TryGetValue(prevKey, out var value))
                            {
                                constStack[key] = value;
                                return true;
                            }

                            break;
                        case Code.Ldc_I4:
                        case Code.Ldc_I4_M1:
                        case Code.Ldc_I4_0:
                        case Code.Ldc_I4_1:
                        case Code.Ldc_I4_2:
                        case Code.Ldc_I4_3:
                        case Code.Ldc_I4_4:
                        case Code.Ldc_I4_5:
                        case Code.Ldc_I4_6:
                        case Code.Ldc_I4_7:
                        case Code.Ldc_I4_8:
                        case Code.Ldc_I4_S:
                            //获取需要压入栈的值
                            key = GetStLocName(instruction);
                            constStack[key] = GetLdcI4Num(previous);
                            return true;
                    }

                    break;
            }

            index--;

            return false;
        }

        /// <summary>
        /// 优化数学计算
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="constStack"></param>
        /// <param name="index"></param>
        /// <param name="processor"></param>
        /// <returns>结果</returns>
        private static bool OptimizeArithConst(in Instruction instruction, Dictionary<string, object> constStack,
            ref int index, ILProcessor processor)
        {
            /*
             * TODO 压缩连续计算（需要同级别）
             *
             * IL_001c: ldloc.1
             * IL_001d: add
             * IL_0020: ldc.i4.s 30
             * IL_0021: add
             * =>
             * if ldloc.1 is const:
             * IL_001c: ldloc.1 + 30
             * IL_0021: add
             *
             * TODO 修bug，偶尔会优化出错误结果（负数）
             */
            
            Stack<long> nums = new Stack<long>();
            //如果instruction一直到add之类的都是（ldc.i4.s或ldc.i4.x，x是0~8）或（ldloc.s或ldloc.y，y是0~3）
            Code[] desired = _ldcOrLdlocCode;
            //开始检查后面的是否可以优化
            var current = instruction;
            int newIndex = index;
            //确保读到的东西是想要的
            while (current != null && desired.Contains(current.OpCode.Code))
            {
                newIndex++;
                string key = GetLdLocName(current);
                //读取常量
                if (key != null)
                {
                    //有常量
                    if (constStack.TryGetValue(key, out var value) && long.TryParse(value.ToString(), out var num))
                    {
                        nums.Push(num);
                    }
                    //不符合优化要求
                    else
                    {
                        current = null;
                        break;
                    }
                }
                else
                {
                    nums.Push(GetLdcI4Num(current));
                }

                //下一个
                current = current.Next;
            }

            //如果读到头了
            if (current != instruction && current != null && nums.Count >= 2)
            {
                //如果是add/sub/mul/div
                var code = current.OpCode.Code;
                if (code == Code.Add || code == Code.Sub || code == Code.Mul || code == Code.Div ||
                    code == Code.Rem)
                {
                    //计算结果
                    long result;
                    if (code == Code.Add)
                    {
                        var rhs = nums.Pop();
                        var lhs = nums.Pop();
                        result = lhs + rhs;
                    }
                    else if (code == Code.Sub)
                    {
                        var rhs = nums.Pop();
                        var lhs = nums.Pop();
                        result = lhs - rhs;
                    }
                    else if (code == Code.Mul)
                    {
                        var rhs = nums.Pop();
                        var lhs = nums.Pop();
                        result = lhs * rhs;
                    }
                    else if (code == Code.Div)
                    {
                        var rhs = nums.Pop();
                        var lhs = nums.Pop();
                        result = lhs / rhs;
                    }
                    else
                    {
                        var rhs = nums.Pop();
                        var lhs = nums.Pop();
                        result = lhs % rhs;
                    }

                    //看result能不能转int
                    Instruction newLdcI4Instruction;
                    if (result <= int.MaxValue)
                        newLdcI4Instruction = NewLdcI4Instruction((int)result);
                    else
                        newLdcI4Instruction = NewLdcI8Instruction(result);
                    //把current替换成newLdcI4Instruction
                    newLdcI4Instruction.Offset = current.Offset;
                    newIndex = processor.Body.Instructions.IndexOf(current);
                    //替换
                    processor.Replace(newIndex, newLdcI4Instruction);
                    //老的是current.prev.prev
                    processor.RemoveAt(newIndex - 1);
                    processor.RemoveAt(newIndex - 2);
                    
                    newLdcI4Instruction.Next = processor.Body.Instructions.ElementAt(newIndex - 1);
                    newLdcI4Instruction.Previous = processor.Body.Instructions.ElementAt(newIndex - 3);
                    // Debug.Log($"new: {newLdcI4Instruction}, new.prev: {newLdcI4Instruction.Previous}, new.next: {newLdcI4Instruction.Next}");
                    
                    Debug.LogWarning($"成功将第{newIndex - 2}个指令到第{newIndex}的指令优化为{newLdcI4Instruction}");
                    return true;
                }
            }

            return false;
        }
    }
}