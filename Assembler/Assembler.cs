using ArchitectureLibrary;
using System;
using System.Collections.Generic;
using System.IO;

namespace Assembler
{
    class Assembler
    {
        static void Main(string[] args)
        {
            string file = "factorial.ass";
            string[] instructions = File.ReadAllLines($@"C:\Users\Adam Goodwin\Documents\VSCode\{file}");

            File.WriteAllBytes($@"C:\Users\Adam Goodwin\Documents\VSCode\{file}.bin",
                               Assemble(instructions).ToArray());


            Console.ReadKey();
        }



        public static List<byte> Assemble(string[] instructions)
        {
            ResolveLabels(instructions);

            string[][] arr = new string[instructions.Length][];

            int i;
            for (i = 0; i < instructions.Length; i++)
            {
                arr[i] = instructions[i].Split(' ');
            }

            List<byte> ass = new List<byte>();
            
            for (i = 0; i < arr.Length && arr[i][0] != "PROGMEM"; i++)
            {
                Array.ForEach(GetMachineCode(arr[i]), ass.Add);
            }

            ass.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });

            i++;

            for (; i < instructions.Length; i++)
            {
                foreach (char c in instructions[i])
                {
                    ass.Add((byte)c);
                    ass.Add((byte)(c >> 8));
                }
            }

            return ass;
        }

        public static void ResolveLabels(string[] instructions)
        {
            var map = new Dictionary<string, int>();

            bool isProgmem = false;
            int progmemCounter = 0;

            for (int i = 0; i < instructions.Length; i++)
            {
                string s = instructions[i];

                int colon = s.IndexOf(':');

                if (s == "PROGMEM")
                {
                    isProgmem = true;
                }
                else if (isProgmem)
                {
                    if (colon != -1)
                    {
                        map.Add(s.Substring(0, colon), progmemCounter);

                        s = s.Substring(colon + 2);

                        if (s[0] == '"')
                        {
                            s = s.Substring(1, s.Length - 2);
                        }
                        else
                        {
                            string[] arr = s.Split(' ');

                            string result = "";

                            for (int j = 0; j < arr.Length; j++)
                            {
                                char c = (char)(ushort.Parse(arr[j]));
                                result += c;
                            }
                            s = result;
                        }

                        progmemCounter += s.Length;
                    }

                }
                else
                {
                    if (colon != -1)
                    {
                        map.Add(s.Substring(0, colon), i);
                        s = "nop";
                    }
                }

                if (s == "")
                {
                    s = "nop";
                }

                if (s.StartsWith("//"))
                {
                    s = "nop";
                }

                instructions[i] = s;
            }

            for (int i = 0; i < instructions.Length; i++)
            {
                string[] arr = instructions[i].Split(' ');
                for (int j = 0; j < arr.Length; j++)
                {
                    if (map.ContainsKey(arr[j]))
                    {
                        arr[j] = map[arr[j]].ToString();
                    }
                }
                instructions[i] = String.Join(' ', arr);
            }
        }

        static byte[] GetMachineCode(string[] instruction)
        {
            byte[] arr = new byte[4];

            arr[0] = (byte)Enum.Parse<OpCode>(instruction[0]);

            byte code = arr[0];

            if (code < 0x10)
            {
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = 0;
            }
            else if (code < 0x20)
            {
                arr[1] = byte.Parse(instruction[1].Substring(1));
                arr[2] = byte.Parse(instruction[2].Substring(1));
                arr[3] = byte.Parse(instruction[3].Substring(1));
            }
            else if (code < 0x30)
            {
                arr[1] = byte.Parse(instruction[1].Substring(1));
                arr[2] = 0;
                arr[3] = byte.Parse(instruction[2].Substring(1));
            }
            else if (code < 0x40)
            {
                arr[1] = byte.Parse(instruction[1].Substring(1));
                arr[2] = byte.Parse(instruction[2].Substring(1));
                arr[3] = byte.Parse(GetDecimal(instruction[3]));
            }
            else if (code < 0x50)
            {
                arr[1] = byte.Parse(instruction[1].Substring(1));
                ushort num = ushort.Parse(GetDecimal(instruction[2]));
                arr[2] = (byte)(num >> 8);
                arr[3] = (byte)num;
            }
            else if (code < 0x60)
            {
                arr[1] = 0;
                ushort num = ushort.Parse(GetDecimal(instruction[1]));
                arr[2] = (byte)(num >> 8);
                arr[3] = (byte)num;
            }
            else if (code < 0x70)
            {
                arr[1] = byte.Parse(instruction[1].Substring(1));
                arr[2] = 0;
                arr[3] = 0;
            }
            else if (code < 0x80)
            {
                arr[1] = 0;
                arr[2] = 0;
                arr[3] = byte.Parse(instruction[1].Substring(1));
            }

            return arr;
        }

        private static string GetDecimal(string s) => s.IndexOf('x') == 1 ?
            Convert.ToByte(s, 16).ToString() : s;


        public static string[] Disassemble(byte[] machineCode)
        {
            string[] instructions = new string[machineCode.Length / 4];
            for (int i = 0; i < machineCode.Length / 4; i++)
            {
                instructions[i] = GetInstruction(machineCode.AsSpan(i * 4, 4).ToArray());
            }
            return instructions;
        }

        public static string GetInstruction(byte[] b)
        {
            string s = "";

            byte code = b[0];

            s += Enum.GetName(typeof(OpCode), code) + " ";

            if (code < 0x10)
            {
                //nop, ret
            }
            else if (code < 0x20)
            {
                s += "r" + b[1] + " r" + b[2] + " r" + b[3];
            }
            else if (code < 0x30)
            {
                s += "r" + b[1] + " r" + b[3];
            }
            else if (code < 0x40)
            {
                s += "r" + b[1] + " r" + b[2] + " " + b[3];
            }
            else if (code < 0x50)
            {
                s += "r" + b[1] + " " + ((b[2] << 8) + b[3]);
            }
            else if (code < 0x60)
            {
                s += ((b[2] << 8) + b[3]);
            }
            else if (code < 0x70)
            {
                s += "r" + b[1];
            }
            else if (code < 0x80)
            {
                s += "r" + b[3];
            }

            return s;
        }

    }
}
