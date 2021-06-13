// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics.Tracing;

namespace SakuraFrpService.WebSocketShim
{
    [EventSource(Name = "Microsoft-System-Net-WebSockets-Client")]
    internal sealed partial class NetEventSource
    {
        public static bool IsEnabled = false;
        public static void Enter(object obj) { }
        public static void Exit(object obj) { }
        public static void Error(object obj, Exception ex) { }
    }
}