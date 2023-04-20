using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ILRuntime.Mono.Cecil;
using ILRuntime.Mono.Cecil.Cil;
using ILRuntime.Mono.Cecil.Pdb;
using Debug = UnityEngine.Debug;

namespace JEngine.Editor
{
    public static partial class Optimizer
    {
        /// <summary>
        /// 整数常数或栈地址
        /// </summary>
        private static Code[] _ldcOrLdlocCode = new Code[]
        {
            Code.Ldc_I4,
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
        /// <param name="dllOutputPath">symbol</param>
        /// <param name="pdbOutputPath">symbol</param>
        public static void Optimize(string dllPath, string pdbPath, string dllOutputPath, string pdbOutputPath)
        {
            //模块
            var module = LoadModule(dllPath, pdbPath);
            if (module.HasTypes)
            {
                foreach (var t in module.GetTypes()) //获取所有此模块定义的类型
                {
                    //测试
                    if (t.FullName == "HotUpdateScripts.Test")
                    {
                        OptimizeMethod(t.Methods.First(m => m.Name == "Optimized"));
                        OptimizeMethod(t.Methods.First(m => m.Name == "OptimizedJIT"));
                    }
                }
            }

            var optimizedPdbStream = pdbPath != null ? new FileStream(pdbOutputPath, FileMode.Create) : null;
            var writerParameters = pdbPath != null
                ? new WriterParameters
                {
                    WriteSymbols = true, SymbolStream = optimizedPdbStream,
                    SymbolWriterProvider = new PdbWriterProvider()
                }
                : new WriterParameters();
            module.Write(dllOutputPath, writerParameters);
            if (pdbPath != null) optimizedPdbStream.Dispose();
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
        /// 优化方法
        /// </summary>
        /// <param name="method"></param>
        private static void OptimizeMethod(MethodDefinition method)
        {
            var processor = method.Body.GetILProcessor();

            var oldInstructionsCount = method.Body.Instructions.Count;
            Debug.Log($"读取{method.Name}的指令（共{oldInstructionsCount}条指令）：");
            int index = 0;
            Dictionary<string, object> constStack = new Dictionary<string, object>();
            //先优化
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

            index = 0;
            //优化后清理冗余
            Dictionary<string, int> stStack = new Dictionary<string, int>();
            while (true)
            {
                var max = method.Body.Instructions.Count;
                if (index >= max)
                {
                    break;
                }

                var instruction = method.Body.Instructions.ElementAt(index);
                //清理冗余ld st
                if (CleanRedundantStLoc(instruction, stStack, ref index, processor))
                {
                    Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
                }

                //清理冗余方法返回值调用栈
                if (CleanRedundantStackAtRet(instruction, stStack, ref index, processor))
                {
                    Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
                }

                index++;
            }

            var newInstructionsCount = method.Body.Instructions.Count;
            Debug.Log(
                $"从{oldInstructionsCount}条指令优化到{newInstructionsCount}条指令，减少了{oldInstructionsCount - newInstructionsCount}条指令");
            Debug.Log($"当前指令：\n{string.Join("\n", method.Body.Instructions)}");
        }

        /// <summary>
        /// 整理顺序
        /// </summary>
        /// <param name="processor"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        private static void SortInstructionOrder(ILProcessor processor, int from, int to)
        {
            var instructions = processor.Body.Instructions;
            var cur = Math.Max(0, from);
            var max = Math.Min(instructions.Count, to);
            while (cur < max)
            {
                var instruction = instructions.ElementAt(cur);
                var next = cur + 1;
                instruction.Next = next >= instructions.Count ? null : instructions.ElementAt(next);
                var prev = cur - 1;
                instruction.Previous = prev < 0 ? null : instructions.ElementAt(prev);

                cur++;
            }
        }

        /// <summary>
        /// 清理冗余stloc指令
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="stStack"></param>
        /// <param name="index"></param>
        /// <param name="processor"></param>
        private static bool CleanRedundantStLoc(in Instruction instruction, Dictionary<string, int> stStack,
            ref int index, ILProcessor processor)
        {
            //如果是ldloc.x，那可以把之前记录的可以略过的stloc.x的标记删掉
            var ldKey = GetLdLocName(instruction);
            if (ldKey != null)
            {
                stStack.Remove(ldKey);
                return false;
            }

            //如果是stloc.x，那可以把之前记录的可以略过的stloc.x的指令删掉
            var stKey = GetStLocName(instruction);
            if (stKey != null)
            {
                if (stStack.TryGetValue(stKey, out var oldIndex))
                {
                    //获取之前的stloc.x指令
                    var lastSt = processor.Body.Instructions.ElementAt(oldIndex);
                    var lastLd = lastSt.Previous;
                    //判断上一句是不是ldloc.x或设置常量
                    if (lastLd != null && _ldcOrLdlocCode.Contains(lastLd.OpCode.Code))
                    {
                        lastLd.Previous.Next = lastSt.Next;
                        lastSt.Next.Previous = lastLd.Previous;
                        processor.Remove(lastLd);
                        processor.Remove(lastSt);
                        Debug.LogWarning($"删除了不会起作用的ld和st: {lastLd} -> {lastSt}");
                    }

                    stStack[stKey] = index;
                    return true;
                }

                stStack[stKey] = index;
            }

            return false;
        }

        /// <summary>
        /// 清理在返回值时的冗余指令
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="stStack"></param>
        /// <param name="index"></param>
        /// <param name="processor"></param>
        private static bool CleanRedundantStackAtRet(in Instruction instruction, Dictionary<string, int> stStack,
            ref int index, ILProcessor processor)
        {
            //如果是br或br.s，且prev是stloc.x，jump后是ldloc.x，ldloc.x之后是ret，那可以直接ret
            if (instruction.OpCode.Code == Code.Br || instruction.OpCode.Code == Code.Br_S)
            {
                var prev = instruction.Previous;
                var prevStKey = GetStLocName(prev);
                if (prevStKey != null)
                {
                    var jump = instruction.Operand as Instruction;
                    var jumpLdKey = GetLdLocName(jump);
                    if (jump != null && jumpLdKey != null && jumpLdKey == prevStKey)
                    {
                        var ret = jump.Next;
                        if (ret.OpCode.Code == Code.Ret)
                        {
                            prev.Previous.Next = ret;
                            ret.Previous = prev.Previous;
                            processor.Remove(prev);
                            processor.Remove(jump);
                            processor.Remove(instruction);
                            SortInstructionOrder(processor, index - 2, processor.Body.Instructions.Count);
                            Debug.LogWarning($"删除了不会起作用的方法返回调用栈: {prev} -> {instruction} -> {jump}");
                            index -= 3;
                            return true;
                        }
                    }
                }
            }

            return false;
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
                        case Code.Ldc_I8:
                        case Code.Ldc_R4:
                        case Code.Ldc_R8:
                            //获取需要压入栈的值
                            var prevKey = GetLdLocName(previous);
                            if (prevKey != null)
                            {
                                if (constStack.TryGetValue(prevKey, out var value))
                                {
                                    constStack[key] = value;
                                    // Debug.LogWarning($"存了一个常量：{key}= {value}");
                                    return true;
                                }
                            }
                            else
                            {
                                constStack[key] = GetLdcNumForInteger(previous);
                                // Debug.LogWarning($"存了一个常量：{key}= {constStack[key]} （{previous}）");
                                return true;
                            }

                            break;
                        default:
                            //存的如果不是常量，那就把之前的常量删掉
                            constStack.Remove(key);
                            // Debug.LogWarning($"删除了不会起作用的常量：{key}");
                            break;
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
                        // Debug.LogWarning($"读取到常量：{value}");
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
                    nums.Push(GetLdcNumForInteger(current));
                    // Debug.LogWarning($"读取到常量：{nums.Peek()}");
                }

                //下一个
                current = current.Next;
            }

            //如果读到头了
            if (current != instruction && current != null)
            {
                //如果是add/sub/mul/div
                var code = current.OpCode.Code;
                if (code == Code.Add || code == Code.Sub || code == Code.Mul || code == Code.Div ||
                    code == Code.Rem)
                {
                    //压入栈的有2个常数就可以这样优化
                    if (nums.Count >= 2)
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

                        //看result能不能转指令
                        var ldcInstruction = NewLdcInstruction(result);
                        //把current替换成ldcInstruction
                        ldcInstruction.Offset = current.Offset;
                        newIndex = processor.Body.Instructions.IndexOf(current);
                        //替换
                        processor.Replace(newIndex, ldcInstruction);
                        //老的是current.prev和current.prev.prev
                        processor.RemoveAt(newIndex - 1);
                        processor.RemoveAt(newIndex - 2);
                        //整理
                        SortInstructionOrder(processor, newIndex - 3, newIndex + 1);
                        Debug.LogWarning($"成功将第{newIndex - 2}个指令到第{newIndex}的指令优化为{ldcInstruction}");
                        return true;
                    }

                    if (nums.Count == 1)
                    {
                        //判断current的下一个是不是常数
                        var next = current.Next;
                        if (desired.Contains(next.OpCode.Code))
                        {
                            //判断next的下一个是不是和current一个code
                            var nextNext = next.Next;
                            if (!nextNext.OpCode.Code.Equals(code))
                                return false;
                            //获取数值
                            var nextKey = GetLdLocName(next);
                            long nextNum;
                            if (nextKey != null)
                            {
                                //有常量
                                if (!constStack.TryGetValue(nextKey, out var value))
                                {
                                    return false;
                                }

                                if (!long.TryParse(value.ToString(), out nextNum))
                                {
                                    return false;
                                }
                            }
                            else
                            {
                                nextNum = GetLdcNumForInteger(next);
                            }

                            //取出之前压的数值
                            var num = nums.Pop();
                            //计算结果
                            long result;
                            if (code == Code.Add)
                            {
                                result = num + nextNum;
                            }
                            else if (code == Code.Sub)
                            {
                                result = num + nextNum;
                            }
                            else if (code == Code.Mul)
                            {
                                result = num + nextNum;
                            }
                            else if (code == Code.Div)
                            {
                                result = num + nextNum;
                            }
                            else
                            {
                                result = num + nextNum;
                            }

                            //看result能不能转指令
                            var ldcInstruction = NewLdcInstruction(result);
                            //把current替换成ldcInstruction
                            ldcInstruction.Offset = current.Offset;
                            newIndex = processor.Body.Instructions.IndexOf(current);
                            //替换current的上一句
                            processor.Replace(newIndex - 1, ldcInstruction);
                            //替换
                            processor.Replace(newIndex, ldcInstruction);
                            //删除current的下一句和下下一句
                            processor.RemoveAt(newIndex);
                            processor.RemoveAt(newIndex);
                            //整理
                            SortInstructionOrder(processor, newIndex - 1, newIndex + 1);

                            Debug.LogWarning($"成功将第{newIndex - 1}个指令到第{newIndex + 2}的指令优化为{ldcInstruction}");
                            return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}