﻿using Aura_OS.System.Network.IPv4;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aura_OS.System.Network.Config
{
    /// <summary>
    /// Contains DNS configuration
    /// </summary>
    class DNSConfig
    {
        /// <summary>
        /// DNS Addresses list.
        /// </summary>
        public static List<Address> DNSNameservers = new List<Address>();

        /// <summary>
        /// Add IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        public static void Add(Address nameserver)
        {
            foreach (var ns in DNSNameservers)
            {
                if (ns.address.ToString() == nameserver.address.ToString())
                {
                    return;
                }
            }
            DNSNameservers.Add(nameserver);
        }

        /// <summary>
        /// Remove IPv4 configuration.
        /// </summary>
        /// <param name="config"></param>
        public static void Remove(Address nameserver)
        {
            int counter = 0;

            foreach (var ns in DNSNameservers)
            {
                if (ns.address.ToString() == nameserver.address.ToString())
                {
                    DNSNameservers.RemoveAt(counter);
                }
                counter++;
            }
        }

        /// <summary>
        /// Call this to get your adress to request your DNS server
        /// </summary>
        /// <param name="index">Which server you want to get</param>
        /// <returns>DNS Server</returns>
        public static Address Server(int index)
        {
            return DNSNameservers[index];
        }
    }
}
