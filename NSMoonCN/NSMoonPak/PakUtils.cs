using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NSMoonPak
{
    public static class PakUtils
    {
        /// <summary>
        /// 检查流的开头是否是某个字符串,该方法自动恢复流的位置
        /// </summary>
        /// <param name="stream">流</param>
        /// <param name="magic">字符串</param>
        /// <returns></returns>
        public static bool CheckMagic(Stream stream, string magic)
        {
            long position = stream.Position;
            byte[] temp = new byte[magic.Length];
            stream.Read(temp, 0, magic.Length);
            stream.Position = position;
            return Encoding.ASCII.GetString(temp).Equals(magic);
        }

        /// <summary>
        /// 从当前流读取字节序列并转换为指定类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static T ToStruct<T>(Stream stream)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] temp = new byte[size];
            stream.Read(temp, 0, size);
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(temp, 0, ptr, size);
            T t = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return t;
        }

        /// <summary>
        /// 从字节序列转换为指定类型
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="stream">流</param>
        /// <returns></returns>
        public static T ToStruct<T>(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(T));
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(bytes, 0, ptr, size);
            T t = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return t;
        }

        public static byte[] ToByteArray(object src)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(src);
            byte[] dst = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(src, ptr, false);
            Marshal.Copy(ptr, dst, 0, size);
            Marshal.FreeHGlobal(ptr);
            return dst;
        }

        public static T[] ToStructArray<T>(byte[] bytes)
        {
            int size = Marshal.SizeOf(typeof(T));
            int count = bytes.Length / size;
            T[] ts = new T[count];
            for (int i = 0; i < count; i++)
            {
                IntPtr ptr = Marshal.AllocHGlobal(size);
                Marshal.Copy(bytes, i * size, ptr, size);
                T t = Marshal.PtrToStructure<T>(ptr);
                Marshal.FreeHGlobal(ptr);
                ts[i] = t;
            }

            return ts;
        }

        private static byte getbit(byte value, int pos)
        {
            return (byte)(value >> pos & 1);
        }

        private static byte getbyte(byte value)
        {
            return value;
        }
        public static byte[] LZSS_Compress(byte[] source)
        {
            throw new NotSupportedException();
        }

        [Obsolete("请使用 LZSS.Decommpress 代替")]
        public static byte[] LZSS_Decompress(byte[] form)
        {
            int form_index = 0;
            int windowPosition = 0x0fee;

            int win_size = 4096;
            int[] window = new int[win_size];

            using (MemoryStream memory = new MemoryStream())
            {
                using (BinaryWriter to = new BinaryWriter(memory))
                {
                    while (true)
                    {
                        byte current = getbyte(form[form_index++]);
                        for (int i = 0; i < 8; i++)
                        {
                            //如果bit为1，则接下来一个byte原样输出
                            if (form_index == form.Length)
                            {
                                return memory.ToArray();
                            }
                            if (getbit(current, i) == 1)
                            {
                                byte data;
                                data = getbyte(form[form_index++]);
                                // 输出1字节并放入窗口
                                to.Write(data);
                                window[windowPosition++] = data;

                                //如果移动到了结束的位置就归零
                                windowPosition &= win_size - 1;
                            }
                            else //如果byte为0
                            {
                                int copy_bytes, win_offset;
                                //获取接下来两个byte
                                win_offset = getbyte(form[form_index++]);
                                copy_bytes = getbyte(form[form_index++]);
                                int x = win_offset;
                                int y = copy_bytes;
                                //低四位不变，高四位相或
                                win_offset |= (copy_bytes >> 4) << 8;

                                //只要低8位
                                copy_bytes &= 0x0f;

                                //然后+3
                                copy_bytes += 3;

                                for (int j = 0; j < copy_bytes; j++)
                                {
                                    byte data;

                                    //到达win size则为1，win_offset是在窗口里的位置
                                    data = (byte)(window[(win_offset + j) & (win_size - 1)]);

                                    to.Write(data);
                                    window[windowPosition++] = data;
                                    //如果到达win size则置为1
                                    windowPosition &= win_size - 1;
                                }
                            }
                        }
                    }
                }
            }
        }

        public static class LZSS
        {

            private const int N = 4096;         /* size of ring buffer - must be power of 2 */
            private const int F = 18;           /* upper limit for match_length */
            private const int THRESHOLD = 2;    /* encode string into position and length
						                         if match_length is greater than this */
            private const int NIL = N;          /* index for root of binary search trees */

            public static byte[] Decompress(byte[] src)
            {
                /* ring buffer of size N, with extra F-1 bytes to aid string comparison */
                byte[] text_buf = new byte[N + F - 1];
                int i, j, k, r, c;
                uint flags;
                for (i = 0; i < N - F; i++)
                    text_buf[i] = 0;
                r = N - F;
                flags = 0;

                MemoryStream src_stream = new MemoryStream(src);
                MemoryStream dst_stream = new MemoryStream();
                for (; ; )
                {
                    if (((flags >>= 1) & 0x100) == 0)
                    {
                        if (src_stream.Position < src_stream.Length) c = src_stream.ReadByte(); else break;
                        flags = (uint)(c | 0xFF00);  /* uses higher byte cleverly */
                    }   /* to count eight */
                    if ((flags & 1) == 1)
                    {
                        if (src_stream.Position < src_stream.Length) c = src_stream.ReadByte(); else break;
                        dst_stream.WriteByte((byte)c);
                        text_buf[r++] = (byte)c;
                        r &= (N - 1);
                    }
                    else
                    {
                        if (src_stream.Position < src_stream.Length) i = src_stream.ReadByte(); else break;
                        if (src_stream.Position < src_stream.Length) j = src_stream.ReadByte(); else break;
                        i |= ((j & 0xF0) << 4);
                        j = (j & 0x0F) + THRESHOLD;
                        for (k = 0; k <= j; k++)
                        {
                            c = text_buf[(i + k) & (N - 1)];
                            dst_stream.WriteByte((byte)c);
                            text_buf[r++] = (byte)c;
                            r &= (N - 1);
                        }
                    }
                }

                return dst_stream.ToArray();
            }

            private struct encode_state
            {
                /*
				 * left & right children & parent. These constitute binary search trees.
				 */
                public int[] lchild, rchild, parent;

                /* ring buffer of size N, with extra F-1 bytes to aid string comparison */
                public byte[] text_buf;

                /*
				 * match_length of longest match.
				 * These are set by the insert_node() procedure.
				 */
                public int match_position, match_length;
            };

            /*
			 * initialize state, mostly the trees
			 *
			 * For i = 0 to N - 1, rchild[i] and lchild[i] will be the right and left
			 * children of node i.  These nodes need not be initialized.  Also, parent[i]
			 * is the parent of node i.  These are initialized to NIL (= N), which stands
			 * for 'not used.'  For i = 0 to 255, rchild[N + i + 1] is the root of the
			 * tree for strings that begin with character i.  These are initialized to NIL.
			 * Note there are 256 trees. */
            private static void init_state(ref encode_state sp)
            {
                int i;

                sp.lchild = new int[N + 1];
                sp.rchild = new int[N + 257];
                sp.parent = new int[N + 1];
                sp.text_buf = new byte[N + F - 1];


                for (i = 0; i < N - F; i++)
                    sp.text_buf[i] = 0;
                for (i = N + 1; i <= N + 256; i++)
                    sp.rchild[i] = NIL;
                for (i = 0; i < N; i++)
                    sp.parent[i] = NIL;
            }

            /*
             * Inserts string of length F, text_buf[r..r+F-1], into one of the trees
             * (text_buf[r]'th tree) and returns the longest-match position and length
             * via the global variables match_position and match_length.
             * If match_length = F, then removes the old node in favor of the new one,
             * because the old one will be deleted sooner. Note r plays double role,
             * as tree node and position in buffer.
             */
            private static void insert_node(ref encode_state sp, int r)
            {

                int i, p, cmp;
                int key;

                cmp = 1;
                key = r;
                p = N + 1 + sp.text_buf[key];
                sp.rchild[r] = sp.lchild[r] = NIL;
                sp.match_length = 0;
                for (; ; )
                {
                    if (cmp >= 0)
                    {
                        if (sp.rchild[p] != NIL)
                            p = sp.rchild[p];
                        else
                        {
                            sp.rchild[p] = r;
                            sp.parent[r] = p;
                            return;
                        }
                    }
                    else
                    {
                        if (sp.lchild[p] != NIL)
                            p = sp.lchild[p];
                        else
                        {
                            sp.lchild[p] = r;
                            sp.parent[r] = p;
                            return;
                        }
                    }
                    for (i = 1; i < F; i++)
                    {
                        if ((cmp = sp.text_buf[key + i] - sp.text_buf[p + i]) != 0)
                            break;
                    }
                    if (i > sp.match_length)
                    {
                        sp.match_position = p;
                        if ((sp.match_length = i) >= F)
                            break;
                    }
                }
                sp.parent[r] = sp.parent[p];
                sp.lchild[r] = sp.lchild[p];
                sp.rchild[r] = sp.rchild[p];
                sp.parent[sp.lchild[p]] = r;
                sp.parent[sp.rchild[p]] = r;
                if (sp.rchild[sp.parent[p]] == p)
                    sp.rchild[sp.parent[p]] = r;
                else
                    sp.lchild[sp.parent[p]] = r;
                sp.parent[p] = NIL;  /* remove p */
            }

            /* deletes node p from tree */
            private static void delete_node(ref encode_state sp, int p)
            {

                int q;

                if (sp.parent[p] == NIL)
                    return;  /* not in tree */
                if (sp.rchild[p] == NIL)
                    q = sp.lchild[p];
                else if (sp.lchild[p] == NIL)
                    q = sp.rchild[p];
                else
                {
                    q = sp.lchild[p];
                    if (sp.rchild[q] != NIL)
                    {
                        do
                        {
                            q = sp.rchild[q];
                        } while (sp.rchild[q] != NIL);
                        sp.rchild[sp.parent[q]] = sp.lchild[q];
                        sp.parent[sp.lchild[q]] = sp.parent[q];
                        sp.lchild[q] = sp.lchild[p];
                        sp.parent[sp.lchild[p]] = q;
                    }
                    sp.rchild[q] = sp.rchild[p];
                    sp.parent[sp.rchild[p]] = q;
                }
                sp.parent[q] = sp.parent[p];
                if (sp.rchild[sp.parent[p]] == p)
                    sp.rchild[sp.parent[p]] = q;
                else
                    sp.lchild[sp.parent[p]] = q;
                sp.parent[p] = NIL;
            }

            public static byte[] Compress(byte[] src)
            {
                /* Encoding state, mostly tree but some current match stuff */
                encode_state sp = new encode_state();

                int i, c, len, r, s, last_match_length, code_buf_ptr;
                byte[] code_buf = new byte[17];
                byte mask;
                init_state(ref sp);

                /*
                 * code_buf[1..16] saves eight units of code, and code_buf[0] works
                 * as eight flags, "1" representing that the unit is an unencoded
                 * letter (1 byte), "" a position-and-length pair (2 bytes).
                 * Thus, eight units require at most 16 bytes of code.
                 */
                code_buf[0] = 0;
                code_buf_ptr = mask = 1;

                /* Clear the buffer with any character that will appear often. */
                s = 0; r = N - F;

                MemoryStream src_stream = new MemoryStream(src);
                MemoryStream dst_stream = new MemoryStream();
                /* Read F bytes into the last F bytes of the buffer */
                for (len = 0; len < F && src_stream.Position < src_stream.Length; len++)
                    sp.text_buf[r + len] = (byte)src_stream.ReadByte();
                if (len <= 0)
                {
                    return dst_stream.ToArray();  /* text of size zero */
                }
                /*
                 * Insert the F strings, each of which begins with one or more
                 * 'space' characters.  Note the order in which these strings are
                 * inserted.  This way, degenerate trees will be less likely to occur.
                 */
                for (i = 1; i <= F; i++)
                    insert_node(ref sp, r - i);

                /*
                 * Finally, insert the whole string just read.
                 * The global variables match_length and match_position are set.
                 */
                insert_node(ref sp, r);
                do
                {
                    /* match_length may be spuriously long near the end of text. */
                    if (sp.match_length > len)
                        sp.match_length = len;
                    if (sp.match_length <= THRESHOLD)
                    {
                        sp.match_length = 1;  /* Not long enough match.  Send one byte. */
                        code_buf[0] |= mask;  /* 'send one byte' flag */
                        code_buf[code_buf_ptr++] = sp.text_buf[r];  /* Send uncoded. */
                    }
                    else
                    {
                        /* Send position and length pair. Note match_length > THRESHOLD. */
                        code_buf[code_buf_ptr++] = (byte)sp.match_position;
                        code_buf[code_buf_ptr++] = (byte)
                                    (((sp.match_position >> 4) & 0xF0)
                                        | (sp.match_length - (THRESHOLD + 1)));
                    }
                    //这里位运算有问题，byte为uint，所以这里取低8位
                    if (((mask <<= 1) & 0xFF) == 0)
                    {  /* Shift mask left one bit. */
                        /* Send at most 8 units of code together */
                        for (i = 0; i < code_buf_ptr; i++)
                        {
                            dst_stream.WriteByte(code_buf[i]);
                        }
                        code_buf[0] = 0;
                        code_buf_ptr = mask = 1;
                    }
                    last_match_length = sp.match_length;
                    for (i = 0; i < last_match_length && src_stream.Position < src_stream.Length; i++)
                    {
                        delete_node(ref sp, s);    /* Delete old strings and */
                        c = src_stream.ReadByte();
                        sp.text_buf[s] = (byte)c;    /* read new bytes */

                        /*
                         * If the position is near the end of buffer, extend the buffer
                         * to make string comparison easier.
                         */
                        if (s < F - 1)

                            sp.text_buf[s + N] = (byte)c;

                        /* Since this is a ring buffer, increment the position modulo N. */
                        s = (s + 1) & (N - 1);
                        r = (r + 1) & (N - 1);

                        /* Register the string in text_buf[r..r+F-1] */
                        insert_node(ref sp, r);
                    }
                    while (i++ < last_match_length)
                    {
                        delete_node(ref sp, s);

                        /* After the end of text, no need to read, */
                        s = (s + 1) & (N - 1);
                        r = (r + 1) & (N - 1);
                        /* but buffer may not be empty. */
                        if (--len >= 0)
                            insert_node(ref sp, r);
                    }
                } while (len > 0);   /* until length of string to be processed is zero */

                if (code_buf_ptr > 1)
                {    /* Send remaining code. */
                    for (i = 0; i < code_buf_ptr; i++)
                    {
                        dst_stream.WriteByte(code_buf[i]);
                    }
                }

                return dst_stream.ToArray();
            }
        }
    }
}
