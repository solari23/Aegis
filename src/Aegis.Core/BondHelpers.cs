namespace Aegis.Core
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;

    using Bond;
    using Bond.IO.Safe;
    using Bond.Protocols;

    /// <summary>
    /// A collection of static helper utilities for dealing with Bond (de)serialization.
    /// </summary>
    public static class BondHelpers
    {
        /// <summary>
        /// A per-type cache of Bond serializers to avoid recreating them repeatedly.
        /// </summary>
        private static ConcurrentDictionary<Type, Serializer<CompactBinaryWriter<OutputBuffer>>> BondSerializers { get; }
            = new ConcurrentDictionary<Type, Serializer<CompactBinaryWriter<OutputBuffer>>>();

        /// <summary>
        /// A per-type cache of Bond deserializers to avoid recreating them repeatedly.
        /// </summary>
        private static ConcurrentDictionary<Type, Deserializer<CompactBinaryReader<InputBuffer>>> BondDeserializers { get; }
            = new ConcurrentDictionary<Type, Deserializer<CompactBinaryReader<InputBuffer>>>();

        /// <summary>
        /// A thread-local buffer, pre-constructed to avoid reconstructing every time.
        /// </summary>
        private static ThreadLocal<OutputBuffer> ThreadLocalOutputBuffer { get; }
            = new ThreadLocal<OutputBuffer>(() => new OutputBuffer());

        /// <summary>
        /// Deserializes the given byte array back to its original Bond object of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="data">The data to deserialize.</param>
        /// <returns>The deserialized object.</returns>
        public static T Deserialize<T>(byte[] data)
        {
            if (data is null || data.Length == 0)
            {
                return default;
            }

            if (!BondDeserializers.TryGetValue(typeof(T), out var deserializer))
            {
                deserializer = new Deserializer<CompactBinaryReader<InputBuffer>>(typeof(T));
                BondDeserializers.TryAdd(typeof(T), deserializer);
            }

            T eventData;

            try
            {
                var inputBuffer = new InputBuffer(data);
                var reader = new CompactBinaryReader<InputBuffer>(inputBuffer);
                eventData = (T)deserializer.Deserialize<T>(reader);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Deserializing object of type {typeof(T)} failed.", ex);
            }

            return eventData;
        }

        /// <summary>
        /// Serializes the given Bond object to a byte array.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="bondObject">The object to serialize.</param>
        /// <returns>The serialized bytes.</returns>
        public static byte[] Serialize<T>(T bondObject)
        {
            if (bondObject is null)
            {
                return null;
            }

            if (!BondSerializers.TryGetValue(typeof(T), out var serializer))
            {
                serializer = new Serializer<CompactBinaryWriter<OutputBuffer>>(typeof(T));
                BondSerializers.TryAdd(typeof(T), serializer);
            }

            try
            {
                ThreadLocalOutputBuffer.Value.Position = 0;
                var writer = new CompactBinaryWriter<OutputBuffer>(ThreadLocalOutputBuffer.Value);
                serializer.Serialize(bondObject, writer);

                return ThreadLocalOutputBuffer.Value.Data.ToArray();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Serializing object of type {typeof(T)} failed.", ex);
            }
        }
    }
}
