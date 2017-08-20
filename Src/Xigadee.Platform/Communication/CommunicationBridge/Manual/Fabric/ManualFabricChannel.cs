﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xigadee
{
    /// <summary>
    /// This is the manual channel.
    /// </summary>
    public class ManualFabricChannel
    {
        #region Declarations
        private ConcurrentDictionary<Guid, ManualFabricConnection> mConnections;

        private ConcurrentBag<Guid> mConnectionsTransmit;
        private ConcurrentBag<Guid> mConnectionsListen;

        private ConcurrentQueue<FabricMessage> mIncoming;
        #endregion
        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="ManualFabricChannel"/> class.
        /// </summary>
        /// <param name="id">The channel identifier.</param>
        /// <param name="mode">The optional communication mode. If this is not set, it will be inferred from the first listening client mode.</param>
        /// <exception cref="ArgumentNullException">id</exception>
        public ManualFabricChannel(string id, CommunicationBridgeMode? mode = null)
        {
            Id = id?.ToLowerInvariant() ?? throw new ArgumentNullException("id");

            mConnections = new ConcurrentDictionary<Guid, ManualFabricConnection>();
            mConnectionsTransmit = new ConcurrentBag<Guid>();
            mConnectionsListen = new ConcurrentBag<Guid>();
            mIncoming = new ConcurrentQueue<FabricMessage>();

            if (mode.HasValue)
                Mode = mode;
        } 
        #endregion

        #region Id
        /// <summary>
        /// Gets the channel identifier.
        /// </summary>
        public string Id { get; }
        #endregion
        #region Mode
        /// <summary>
        /// Gets the Communication mode being used by the channel.
        /// </summary>
        public CommunicationBridgeMode? Mode { get; private set; } = null; 
        #endregion

        #region CreateConnection(ManualFabricConnectionMode mode, string subscription = null)
        /// <summary>
        /// Creates a connection.
        /// </summary>
        /// <param name="mode">The connection mode.</param>
        /// <param name="subscription">The optional subscription identifier.</param>
        /// <returns></returns>
        public ManualFabricConnection CreateConnection(ManualFabricConnectionMode mode, string subscription = null)
        {
            var conn = new ManualFabricConnection(mode, Id, subscription);
            RegisterConnection(conn);
            return conn;
        } 
        #endregion

        #region RegisterConnection(ManualFabricConnection conn)
        /// <summary>
        /// Registers the connection.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the connection channel id does not match.</exception>
        private void RegisterConnection(ManualFabricConnection conn)
        {
            if (!conn.Channel.Equals(Id, StringComparison.InvariantCultureIgnoreCase))
                throw new ArgumentOutOfRangeException($"Connection channel identifier '{conn.Channel}' does not match the Channel '{Id}'");

            switch (conn.Mode)
            {
                case ManualFabricConnectionMode.Queue:
                    RegisterConnectionQueue(conn);
                    break;
                case ManualFabricConnectionMode.Subscription:
                    RegisterConnectionSubscription(conn);
                    break;
                case ManualFabricConnectionMode.Transmit:
                    RegisterConnectionTransmit(conn);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("conn.Mode", $"The connection mode was unexpected: {conn.Mode}");
            }
        }
        #endregion

        #region RegisterConnectionQueue(ManualFabricConnection conn)
        /// <summary>
        /// Registers the queue connection and sets the mode if not already set..
        /// </summary>
        /// <param name="conn">The connection.</param>
        private void RegisterConnectionQueue(ManualFabricConnection conn)
        {
            ConnectionAdd(conn, CommunicationBridgeMode.RoundRobin);
            mConnectionsListen.Add(conn.Id);

            conn.Receive = Receive;
        }
        #endregion
        #region RegisterConnectionSubscription(ManualFabricConnection conn)
        /// <summary>
        /// Registers the subscription connection and sets the mode if not already set.
        /// </summary>
        /// <param name="conn">The connection.</param>
        /// <exception cref="ArgumentNullException">conn.Subscription - Subscription Id cannot be null.</exception>
        private void RegisterConnectionSubscription(ManualFabricConnection conn)
        {
            if (conn.Subscription == null)
                throw new ArgumentNullException("conn.Subscription", "Subscription Id cannot be null.");

            ConnectionAdd(conn, CommunicationBridgeMode.Broadcast);
            mConnectionsListen.Add(conn.Id);

            conn.Receive = Receive;
        }
        #endregion
        #region RegisterConnectionTransmit(ManualFabricConnection conn)
        /// <summary>
        /// Registers the transmit connection.
        /// </summary>
        /// <param name="conn">The connection.</param>
        private void RegisterConnectionTransmit(ManualFabricConnection conn)
        {
            ConnectionAdd(conn);
            mConnectionsTransmit.Add(conn.Id);
            conn.Transmit = Transmit;
        }
        #endregion

        private void Transmit(ManualFabricConnection conn, FabricMessage message)
        {
            if (!Mode.HasValue)
                throw new ArgumentException("Mode is not configured.");

            switch (Mode.Value)
            {
                case CommunicationBridgeMode.RoundRobin:
                    break;
                case CommunicationBridgeMode.Broadcast:
                    break;
            }
        }

        private IEnumerable<FabricMessage> Receive(ManualFabricConnection conn, int? count)
        {
            yield break;
        }

        #region ConnectionAdd(ManualFabricConnection conn, CommunicationBridgeMode? validateMode = null)
        private object syncConnection = new object();
        private void ConnectionAdd(ManualFabricConnection conn, CommunicationBridgeMode? validateMode = null)
        {
            lock (syncConnection)
            {
                if (validateMode.HasValue && Mode.HasValue && Mode != validateMode)
                    throw new ArgumentOutOfRangeException("validateMode", $"validateMode does not match {Mode}");

                if (!mConnections.TryAdd(conn.Id, conn))
                    throw new ArgumentOutOfRangeException("conn.Id", "The connection already exists");

                if (validateMode.HasValue && !Mode.HasValue)
                    Mode = validateMode;
            }
        } 
        #endregion
    }
}