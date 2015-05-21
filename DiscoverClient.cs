﻿#region Licence
/*
    Babbacombe SockLib
    https://github.com/trevorprinn/SockLib
    Copyright © 2015 Babbacombe Computers Ltd.

    This library is free software; you can redistribute it and/or
    modify it under the terms of the GNU Lesser General Public
    License as published by the Free Software Foundation; either
    version 2.1 of the License, or (at your option) any later version.

    This library is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public
    License along with this library; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301
    USA
 */
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Babbacombe.SockLib {
    /// <summary>
    /// A UDP client that can be used to find a SockLib server.
    /// </summary>
    public class DiscoverClient {
        private IPEndPoint _broadcast;

        public int Timeout { get; private set; }

        public DiscoverClient(int port) {
            _broadcast = new IPEndPoint(IPAddress.Broadcast, port);
            Timeout = 5000;
        }

        public async Task<IPEndPoint> FindService(string serviceName) {
            using (var client = new UdpClient()) {
                var cts = new CancellationTokenSource(Timeout);
                await client.SendAsync(Encoding.UTF8.GetBytes(serviceName), _broadcast, cts.Token);
                if (cts.IsCancellationRequested) return null;

                cts = new CancellationTokenSource(Timeout);
                var msg = await client.ReceiveAsync(cts.Token);
                if (msg == null) return null; // Timed out


                var advert = Encoding.UTF8.GetString(msg.Value.Buffer);
                if (!advert.Contains(':')) return null;
                var parts = advert.Split(':');
                if (parts[0] != serviceName) return null;

                int port;
                if (!int.TryParse(parts[1], out port)) return null;
                return new IPEndPoint(msg.Value.RemoteEndPoint.Address, port);
            }
        }
    }
}
