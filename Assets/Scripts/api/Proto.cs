using UnityEngine;
using System;
using System.Text;
using System.Globalization;

namespace Api {

    public struct Slice {
        public int Offset;
        public int Len;
    }

    public class Header {
        public UInt64 Id;
        public UInt16 Seq;
        public UInt32 Ver;
        public string Type;
        public string Cmd;
    }

    public class Message : Header {
        public string Payload;
        public bool Dirty;
        public DateTime Time;

        public byte[] GetBytes() {
            return Encoding.UTF8.GetBytes(GetString());
        }

        public string GetString() {
            return String.Format("id={0:x}&seq={1:x}&ver={2:x}&type={3}&cmd={4}&data={5}",
                                                            this.Id,
                                                            this.Seq,
                                                            this.Ver,
                                                            this.Type,
                                                            this.Cmd,
                                                            this.Payload
                                );
        }

        public override bool Equals(object obj) {
            Message msg = obj as Message;
            if (this.Id == msg.Id) { return true; }
            return false;
        }

    }

    public class Proto {

        #region Public Methods

        public static Message ParseMessage(byte[] msg) {

            Message message = new Message();
            Slice slice = new Slice();

            try {
                slice = findValue(msg, slice, false);
                if (slice.Len > 0) {
                    message.Id = UInt64.Parse(Encoding.UTF8.GetString(msg, slice.Offset, slice.Len), NumberStyles.HexNumber);
                } else {
                    return null;
                }

                slice = findValue(msg, slice, false);
                if (slice.Len > 0) {
                    message.Seq = UInt16.Parse(Encoding.UTF8.GetString(msg, slice.Offset, slice.Len), NumberStyles.HexNumber);
                } else {
                    return null;
                }

                slice = findValue(msg, slice, false);
                if (slice.Len > 0) {
                    message.Ver = UInt32.Parse(Encoding.UTF8.GetString(msg, slice.Offset, slice.Len), NumberStyles.HexNumber);
                } else {
                    return null;
                }

                slice = findValue(msg, slice, false);
                if (slice.Len > 0) {
                    message.Type = Encoding.UTF8.GetString(msg, slice.Offset, slice.Len);
                } else {
                    return null;
                }

                slice = findValue(msg, slice, false);
                if (slice.Len > 0) {
                    message.Cmd = Encoding.UTF8.GetString(msg, slice.Offset, slice.Len);
                } else {
                    return null;
                }

                slice = findValue(msg, slice, true);
                if (slice.Len > 0) {
                    message.Payload = Encoding.UTF8.GetString(msg, slice.Offset, slice.Len);
                } else {
                    return null;
                }

                //message.Time = Time.time;

            } catch {
                return null;
            }
            if (MAIN.IS_TEST)
                Debug.Log("Received msg: " + message.Id + " | " + message.Seq + " | " + message.Type + " | " + message.Cmd + " | " + message.Payload);
            return message;
        }

        public static Message RequestMessage(string cmd, ulong id, string payload) {
            if (MAIN.ApplicationVersion == 0) {
                MAIN.ApplicationVersion = MAIN.MakeVersion();
            }
        
            Message msg = new Message();
            msg.Id = id;
            msg.Seq = 1;
            msg.Ver = MAIN.ApplicationVersion;
            msg.Type = ApiHeaderTypeRequest;
            msg.Cmd = cmd;
            msg.Payload = payload;
            msg.Time = DateTime.Now;
            return msg;
        }

        #endregion

        #region Private Properties

        private const int ApplicationIdLength = 35;
        private static string ApiHeaderTypeRequest = "req";
        private static string ApiHeaderTypeResponse = "res";

        #endregion

        #region Private Methods

        private static Slice findValue(byte[] msg, Slice slice, bool last) {
            int e_idx, s_idx, offset;
            offset = slice.Offset;
            slice.Offset = 0;
            slice.Len = 0;

            if ((s_idx = Array.IndexOf<byte>(msg, (byte)'=', offset)) != -1) {
                if (!last) {
                    if ((e_idx = Array.IndexOf<byte>(msg, (byte)'&', s_idx)) != -1) {
                        slice.Offset = s_idx + 1;
                        slice.Len = e_idx - s_idx - 1;
                    }
                } else {
                    slice.Offset = s_idx + 1;
                    slice.Len = msg.Length - s_idx - 1;
                }
            }
            return slice;
        }

        public static UInt64 LowerPartOfRequestId(string aid) {

            if (aid.Length != ApplicationIdLength) {
                return 0;
            }

            int i;
            UInt64 b1, b2, mask;
            UInt64[] a = new UInt64[4];
            UInt64 r1 = 0, r2 = 0;

            string[] tockens = aid.Split('-');
            if (tockens.Length != 4) {
                return 0;
            }

            for (i = 0; i < tockens.Length; i++) {
                a[i] = UInt64.Parse(tockens[i], NumberStyles.HexNumber);
            }

            b1 = (a[1] << 32) | a[0];
            b2 = (a[3] << 32) | a[2];

            for (i = 0; i < 64; i++) {
                mask = ((UInt64)1) << i;
            	if (i % 2 == 0) {
                    r1 |= mask & b1;
                    r2 |= mask & b2;
            	} else {
                    r2 |= mask & b1;
                    r1 |= mask & b2;
            	}
            }
            return r1 << 32;
        }

        public static UInt64 RequestId(UInt64 higherId, int msgId, System.Random rand) {

            UInt64 r = 0;
            if (higherId != 0) {
                DateTime time = DateTime.Now;
                UInt64 p1 = (UInt64)rand.Next(0, 63);
                UInt64 p2 = (UInt64)((1000 * ((time.Minute * 60) + time.Second)) + time.Millisecond);
                UInt64 p3 = (UInt64)(msgId % 16);

                r |= p1 << 26;
                r |= p2 << 4;
                r |= p3;
                r |= higherId;
            }
            return r;
        }

            #endregion
        }
}
