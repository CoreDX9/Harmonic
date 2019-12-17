using System;
using System.Collections.Generic;
using System.Text;

namespace Harmonic.Networking.Rtmp.Data
{
    public enum UserControlMessageEvents : ushort
    {
        StreamBegin,
        StreamEOF,
        StreamDry,
        SetBufferLength,
        StreamIsRecorded,
        PingRequest,
        PingResponse
    }
}
