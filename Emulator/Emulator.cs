using ArchitectureLibrary;
using System;
using System.IO;

namespace Emulator
{
    class Emulator
    {
        private static ref ushort IP => ref reg[0x1E];
        private static ref ushort SP => ref reg[0x1F];

        private static MemoryMap mem;
        private readonly static ushort[] reg = new ushort[0x20];

        private readonly static Random r = new Random();

        static void Main(string[] vs)
        {
            SP = (ushort)(MemoryMap.progStart - 1);
            IP = 0;

            string file = "factorial.ass";

            mem = new MemoryMap(File.ReadAllBytes($@"C:\Users\Adam Goodwin\Documents\VSCode\Assembly\{file}.bin"));

            Run();

            Console.ReadKey();
        }

        private static void Run()
        {
            unchecked
            {
                string input = "";
                while (mem[0] == 0)
                {
                    if (mem[2] != 0)
                    {
                        Console.Write((char)mem[1]);
                        mem[2] = 0;
                    }

                    if (mem[4] != 0)
                    {
                        Console.Write(mem[3]);
                        mem[4] = 0;
                    }

                    if (mem[6] == 0 && Console.KeyAvailable)
                    {
                        if (mem[7] == 0)
                        {
                            mem[6] = 1;
                            mem[5] = Console.ReadKey().KeyChar;
                        }
                        else
                        {
                            char c = Console.ReadKey().KeyChar;

                            if (c == 8)
                            {
                                if (input.Length > 0)
                                {
                                    input = input.Substring(0, input.Length - 1);
                                    Console.Write(' ');
                                    Console.Write('\x008');
                                }
                            }
                            else if (c < '0' || c > '9')
                            {
                                mem[6] = 1;
                                ushort.TryParse(input, out mem[5]);
                                input = "";
                            }
                            else
                            {
                                input += c;
                            }
                        }
                    }

                    if (mem[9] != 0)
                    {
                        mem[8] = (ushort)r.Next();
                    }

                    byte[] op = mem.GetInstruction(IP).ToArray();

                    switch ((OpCode)op[0])
                    {
                        case OpCode.nop:
                            IP++;
                            break;
                        case OpCode.ret:
                            IP = mem[++SP];
                            break;
                        case OpCode.add:
                            reg[op[1]] = (ushort)(reg[op[2]] + reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.sub:
                            reg[op[1]] = (ushort)(reg[op[2]] - reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.mul:
                            reg[op[1]] = (ushort)(reg[op[2]] * reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.div:
                            reg[op[1]] = (ushort)(reg[op[2]] / reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.mod:
                            reg[op[1]] = (ushort)(reg[op[2]] % reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.eq:
                            reg[op[1]] = (ushort)(reg[op[2]] == reg[op[3]] ? -1 : 0);
                            IP++;
                            break;
                        case OpCode.lt:
                            reg[op[1]] = (ushort)(reg[op[2]] < reg[op[3]] ? -1 : 0);
                            IP++;
                            break;
                        case OpCode.gt:
                            reg[op[1]] = (ushort)(reg[op[2]] > reg[op[3]] ? -1 : 0);
                            IP++;
                            break;
                        case OpCode.lte:
                            reg[op[1]] = (ushort)(reg[op[2]] <= reg[op[3]] ? -1 : 0);
                            IP++;
                            break;
                        case OpCode.gte:
                            reg[op[1]] = (ushort)(reg[op[2]] >= reg[op[3]] ? -1 : 0);
                            IP++;
                            break;
                        case OpCode.nand:
                            reg[op[1]] = (ushort)~(reg[op[2]] & reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.and:
                            reg[op[1]] = (ushort)(reg[op[2]] & reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.xor:
                            reg[op[1]] = (ushort)(reg[op[2]] ^ reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.or:
                            reg[op[1]] = (ushort)(reg[op[2]] | reg[op[3]]);
                            IP++;
                            break;
                        case OpCode.jzi:
                            IP = reg[op[1]] == 0 ? reg[op[3]] : (ushort)(IP + 1);
                            break;
                        case OpCode.jnzi:
                            IP = reg[op[1]] != 0 ? reg[op[3]] : (ushort)(IP + 1);
                            break;
                        case OpCode.loadi:
                            reg[op[1]] = mem[reg[op[3]]];
                            IP++;
                            break;
                        case OpCode.stori:
                            mem[reg[op[3]]] = reg[op[1]];
                            IP++;
                            break;
                        case OpCode.not:
                            reg[op[1]] = (ushort)~reg[op[3]];
                            IP++;
                            break;
                        case OpCode.neg:
                            reg[op[1]] = (ushort)(~reg[op[3]] + 1);
                            IP++;
                            break;
                        case OpCode.mov:
                            reg[op[1]] = reg[op[3]];
                            IP++;
                            break;
                        case OpCode.shl:
                            reg[op[1]] = (ushort)(reg[op[2]] << op[3]);
                            IP++;
                            break;
                        case OpCode.shr:
                            reg[op[1]] = (ushort)(reg[op[2]] >> op[3]);
                            IP++;
                            break;
                        case OpCode.peek:
                            {
                                int offset = (op[2] << 8) | op[3];
                                reg[op[1]] = mem[SP + 1 + offset];
                            }
                            IP++;
                            break;
                        case OpCode.load:
                            {
                                int address = (op[2] << 8) | op[3];
                                reg[op[1]] = mem[address];
                            }
                            IP++;
                            break;
                        case OpCode.stor:
                            {
                                int address = (op[2] << 8) | op[3];
                                mem[address] = reg[op[1]];
                            }
                            IP++;
                            break;
                        case OpCode.jz:
                            {
                                int address = (op[2] << 8) | op[3];
                                IP = reg[op[1]] == 0 ? (ushort)address : (ushort)(IP + 1);
                            }
                            break;
                        case OpCode.jnz:
                            {
                                int address = (op[2] << 8) | op[3];
                                IP = reg[op[1]] != 0 ? (ushort)address : (ushort)(IP + 1);
                            }
                            break;
                        case OpCode.set:
                            {
                                int constant = (op[2] << 8) | op[3];
                                reg[op[1]] = (ushort)constant;
                            }
                            IP++;
                            break;
                        case OpCode.inc:
                            {
                                int constant = (op[2] << 8) | op[3];
                                reg[op[1]] += (ushort)constant;
                            }
                            IP++;
                            break;
                        case OpCode.dec:
                            {
                                int constant = (op[2] << 8) | op[3];
                                reg[op[1]] -= (ushort)constant;
                            }
                            IP++;
                            break;
                        case OpCode.loadprog:
                            {
                                int constant = (op[2] << 8) | op[3];
                                reg[op[1]] = (ushort)(constant + mem.PROGMEM);
                            }
                            IP++;
                            break;
                        case OpCode.call:
                            {
                                int address = (op[2] << 8) | op[3];
                                mem[SP--] = (ushort)(IP + 1);
                                IP = (ushort)address;
                            }
                            break;
                        case OpCode.jmp:
                            {
                                int address = (op[2] << 8) | op[3];
                                IP = (ushort)address;
                            }
                            break;
                        case OpCode.push:
                            mem[SP--] = reg[op[1]];
                            IP++;
                            break;
                        case OpCode.pop:
                            reg[op[1]] = mem[++SP];
                            IP++;
                            break;
                        case OpCode.calli:
                            mem[SP--] = (ushort)(IP + 1);
                            IP = reg[op[3]];
                            break;
                        case OpCode.jmpi:
                            IP = reg[op[3]];
                            break;
                    }
                }
            }
        }
    }
}
