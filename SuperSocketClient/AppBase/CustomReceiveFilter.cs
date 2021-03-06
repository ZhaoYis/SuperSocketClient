﻿using SuperSocket.ProtoBase;
using System;
using System.Linq;

namespace SuperSocketClient.AppBase
{
    public class CustomReceiveFilter : FixedHeaderReceiveFilter<CustomPackageInfo>
    {
        /// +-------+---+-------------------------------+
        /// |request| l |                               |
        /// | name  | e |    request body               |
        /// |  (2)  | n |                               |
        /// |       |(2)|                               |
        /// +-------+---+-------------------------------+
        public CustomReceiveFilter() : base(4)
        {
        }

        public override CustomPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            //第三个参数用0,1都可以
            byte[] header = bufferStream.Buffers[0].ToArray();
            byte[] bodyBuffer = bufferStream.Buffers[1].ToArray();
            //byte[] allBuffer = bufferStream.Buffers[0].Array.CloneRange(0, (int)bufferStream.Length);
            return new CustomPackageInfo(header, bodyBuffer);
        }

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            ArraySegment<byte> buffers = bufferStream.Buffers[0];
            byte[] array = buffers.ToArray();
            int len = array[length - 2] * 256 + array[length - 1];
            //int len = (int)array[buffers.Offset + 2] * 256 + (int)array[buffers.Offset + 3];
            return len;
        }
    }
}