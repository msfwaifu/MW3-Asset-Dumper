using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ionic.Zlib;

namespace MW3_Asset_Dumper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private string fixName(string name)
        {
            Directory.CreateDirectory("Dump\\");
            if (name == "$default")
            {
                return "default";
            }
            bool flag = false;
            string fixedName = null;
            for (int index = 0; index < name.Length; ++index)
            {
                if ((int)name[index] == 47)
                {
                    StringBuilder stringBuilder = new StringBuilder(name);
                    stringBuilder[index] = '\\';
                    name = stringBuilder.ToString();
                    fixedName = name;
                    string directoryName = Path.GetDirectoryName(fixedName);
                    if (!Directory.Exists(directoryName))
                        Directory.CreateDirectory("Dump\\" + directoryName);
                    flag = true;
                }
                else
                    flag = false;
            }
            if (flag)
                return fixedName;
            return name;
        }

        public static int id = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            Process[] listOfProcess = Process.GetProcesses();
            foreach (Process proc in listOfProcess)
            {
                if (proc.ProcessName == "iw5sp")
                    id = proc.Id;
            }

            if (id != 0)
            {
                label1.Text = "Success";
            }
            else
            {
                label1.Text = "Connection Failed!";
            }

            Process iw5sp = Process.GetProcessById(id);

            if (!iw5sp.HasExited)
            {

                Memory.pHandel = iw5sp.Handle;
            }
        }

        
        private void button2_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            int num = 1;

            for (int i = 0; i < Memory.getCount(); i++)
            {
                listBox1.Items.Add(fixName(Memory.getName(i)));
                listBox1.SelectedIndex = 0;
                ++num;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FileStream fileStream = new FileStream("Dump\\" + this.listBox1.Text + ".gsc" , FileMode.Create, FileAccess.Write);
            byte[] commpressedBuffer = Memory.ReadBytes(Memory.pointer(this.listBox1.SelectedIndex), Memory.getcSize(this.listBox1.SelectedIndex));
            byte[] uncommpressedBuffer = ZlibStream.UncompressBuffer(commpressedBuffer);
            fileStream.Write(uncommpressedBuffer, 0, uncommpressedBuffer.Length);
            fileStream.Close();

            FileStream fileStream2 = new FileStream("Dump\\" + this.listBox1.Text + ".bytecode", FileMode.Create, FileAccess.Write);
            byte[] memory2 = Memory.ReadBytes(Memory.bytecodepointer(this.listBox1.SelectedIndex), Memory.bytecodeSize(this.listBox1.SelectedIndex));
            fileStream2.Write(memory2, 0, memory2.Length);
            fileStream2.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            uint count1 = Memory.getCount();
            for (int index = 0; (long)index < (long)count1; ++index)
            {
                FileStream fileStream = new FileStream("Dump\\" + fixName(Memory.getName(index)) + ".gsc" , FileMode.Create, FileAccess.Write);
                byte[] commpressedBuffer = Memory.ReadBytes(Memory.pointer(index), Memory.getcSize(index));
                byte[] uncommpressedBuffer = ZlibStream.UncompressBuffer(commpressedBuffer);
                fileStream.Write(uncommpressedBuffer, 0, uncommpressedBuffer.Length);
                fileStream.Close();
            }
            for (int index = 0; (long)index < (long)count1; ++index)
            {
                FileStream fileStream = new FileStream("Dump\\" + fixName(Memory.getName(index)) + ".bytecode", FileMode.Create, FileAccess.Write);
                byte[] commpressedBuffer = Memory.ReadBytes(Memory.bytecodepointer(index), Memory.bytecodeSize(index));
                byte[] uncommpressedBuffer = ZlibStream.UncompressBuffer(commpressedBuffer);
                fileStream.Write(uncommpressedBuffer, 0, uncommpressedBuffer.Length);
                fileStream.Close();
            }
        }

        public class Memory
        {
            #region Basic Stuff
            [DllImport("kernel32.dll")]
            private static extern Int32 ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
            [DllImport("kernel32.dll")]
            private static extern Int32 WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [In, Out] byte[] buffer, UInt32 size, out IntPtr lpNumberOfBytesWritten);
            public static IntPtr pHandel;

            private static byte[] ReadInt(int Address, int Length)
            {
                byte[] Buffer = new byte[Length];
                IntPtr Zero = IntPtr.Zero;
                ReadProcessMemory(pHandel, (IntPtr)Address, Buffer, (UInt32)Buffer.Length, out Zero);
                return Buffer;
            }

            private static byte[] ReadUInt(uint Address, int Length)
            {
                byte[] Buffer = new byte[Length];
                IntPtr Zero = IntPtr.Zero;
                ReadProcessMemory(pHandel, (IntPtr)Address, Buffer, (UInt32)Buffer.Length, out Zero);
                return Buffer;
            }

            private static void Write(int Address, int Value)
            {
                byte[] Buffer = BitConverter.GetBytes(Value);
                IntPtr Zero = IntPtr.Zero;
                WriteProcessMemory(pHandel, (IntPtr)Address, Buffer, (UInt32)Buffer.Length, out Zero);
            }
            #endregion

            #region Write Functions (Integer & String)
            public static void WriteInteger(int Address, int Value)
            {
                Write(Address, Value);
            }
            public static void WriteString(int Address, string Text)
            {
                byte[] Buffer = new ASCIIEncoding().GetBytes(Text);
                IntPtr Zero = IntPtr.Zero;
                WriteProcessMemory(pHandel, (IntPtr)Address, Buffer, (UInt32)Buffer.Length, out Zero);
            }
            public static void WriteBytes(int Address, byte[] Bytes)
            {
                IntPtr Zero = IntPtr.Zero;
                WriteProcessMemory(pHandel, (IntPtr)Address, Bytes, (uint)Bytes.Length, out Zero);
            }
            public static void WriteNOP(int Address)
            {
                byte[] Buffer = new byte[] { 0x90, 0x90, 0x90, 0x90, 0x90 };
                IntPtr Zero = IntPtr.Zero;
                WriteProcessMemory(pHandel, (IntPtr)Address, Buffer, (UInt32)Buffer.Length, out Zero);
            }

            public static void WriteEmpty(int Address, int length)
            {
                int addr = Address;
                for (int i = 0; i < length; i++)
                {
                    WriteBytes(addr, new byte[] { 0x00 });
                    addr += 1;
                }
            }


            #endregion

            #region Read Functions (Integer & String)
            public static int ReadInteger(int Address, int Length = 4)
            {
                return BitConverter.ToInt32(ReadInt(Address, Length), 0);
            }
            public static uint ReadUInteger(uint Address, int Length = 4)
            {
                return BitConverter.ToUInt32(ReadUInt(Address, Length), 0);
            }
            public static string ReadString(int Address, int Length = 4)
            {
                return new ASCIIEncoding().GetString(ReadInt(Address, Length));
            }
            public static byte[] ReadBytes(int Address, int Length)
            {
                return ReadInt(Address, Length);
            }

            public static byte ReadByte(int Address)
            {
                return ReadInt(Address, 1)[0];
            }

            public static string ReadStr(int Address)
            {
                int block = 40;
                int addOffset = 0;
                string str = "";
                repeat:
                byte[] buffer = ReadBytes(Address + addOffset, block);
                str += Encoding.UTF8.GetString(buffer);
                addOffset += block;
                if (str.Contains('\0'))
                {
                    int index = str.IndexOf('\0');
                    string final = str.Substring(0, index);
                    str = String.Empty;
                    return final;
                }
                else
                    goto repeat;
            }

            public static string getName(int index)
            {
                return ReadStr(ReadInteger(0xEFE8CC + index * 0x18, 4));
            }

            public static int GetLength(int Address)
            {
                int length = 0;
                int addr = Address;
                while (ReadBytes(addr, 4).Take(4).SequenceEqual(new byte[] { 0, 0, 0, 0 }) == false)
                {
                    length++;
                    addr++;
                }

                return length;
            }

            public static int getcSize(int index)
            {
                return ReadInteger(0xEFE8D0 + index * 0x18);
            }

            public static int getucSize(int index)
            {
                return ReadInteger(0xEFE8D4 + index * 0x18);
            }

            public static int bytecodeSize(int index)
            {
                return ReadInteger(0xEFE8D8 + index * 0x18);
            }

            public static int pointer(int index)
            {
                return ReadInteger(0xEFE8DC + index * 0x18);
            }

            public static int bufferAddr(int index)
            {
                return 0xEFE8DC + index * 0x18;
            }

            public static int bytecodepointer(int index)
            {
                return ReadInteger(0xEFE8E0 + index * 0x18);
            }

            public static int bytecodeAddr(int index)
            {
                return 0xEFE8E0 + index * 0x18;
            }

            public static uint getCount()
            {
                return (ReadUInteger(0xEFE8C8) - 0xEFE8CC) / 0x18 - 1;
            }
            #endregion
        }
    }
}
