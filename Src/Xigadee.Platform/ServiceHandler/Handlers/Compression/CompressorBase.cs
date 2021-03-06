﻿using System;
using System.IO;

namespace Xigadee
{
    /// <summary>
    /// This abstract class is used to provide compression support for payloads.
    /// </summary>
    /// <seealso cref="Xigadee.IServiceHandlerCompression" />
    public abstract class CompressorBase: IServiceHandlerCompression
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CompressorBase"/> class.
        /// </summary>
        /// <param name="contentEncoding">The content encoding.</param>
        /// <exception cref="ArgumentNullException">contentEncoding</exception>
        protected CompressorBase(string contentEncoding)
        {
            Id = contentEncoding ?? throw new ArgumentNullException("contentEncoding");
            Name = GetType().Name;
        }
        /// <summary>
        /// Gets the content encoding.
        /// </summary>
        public virtual string Id { get; }
        /// <summary>
        /// Gets the friendly name.
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// Gets the compression stream.
        /// </summary>
        /// <param name="inner">The inner byte stream.</param>
        /// <returns>Returns the compression stream.</returns>
        protected abstract Stream GetCompressionStream(Stream inner);
        /// <summary>
        /// Gets the decompression stream.
        /// </summary>
        /// <param name="inner">The inner byte stream.</param>
        /// <returns>Returns the decompression stream.</returns>
        protected abstract Stream GetDecompressionStream(Stream inner);

        #region SupportsContentEncoding(SerializationHolder holder)
        /// <summary>
        /// A boolean function that returns true if the compression type is supported.
        /// </summary>
        /// <param name="holder">The serialization holder.</param>
        /// <returns>
        /// Returns true when supported.
        /// </returns>
        /// <exception cref="ArgumentNullException">holder</exception>
        public virtual bool SupportsContentEncoding(ServiceHandlerContext holder)
        {
            if (holder == null)
                throw new ArgumentNullException("holder");

            return holder.ContentEncoding != null
                && string.Equals(holder.ContentEncoding, Id, StringComparison.InvariantCultureIgnoreCase);
        }
        #endregion

        #region TryCompression(SerializationHolder holder)
        /// <summary>
        /// Tries to compress the outgoing payload.
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <returns>
        /// Returns true if the Content is compressed correctly to a binary blob.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool TryCompression(ServiceHandlerContext holder)
        {
            return HolderChecks(holder) && Compress(holder, GetCompressionStream, Id);
        }
        #endregion
        #region TryDecompression(SerializationHolder holder)
        /// <summary>
        /// Tries to decompress the incoming holder.
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <returns>
        /// Returns true if the incoming binary payload is successfully decompressed.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual bool TryDecompression(ServiceHandlerContext holder)
        {
            return HolderChecks(holder) && Decompress(holder, GetDecompressionStream);
        }
        #endregion

        #region HolderChecks(SerializationHolder holder)
        /// <summary>
        /// Checks the incoming holder to ensure that it is correctly configured.
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <returns>Returns true if the checks are passed.</returns>
        /// <exception cref="ArgumentNullException">holder</exception>
        protected virtual bool HolderChecks(ServiceHandlerContext holder)
        {
            if (holder == null)
                throw new ArgumentNullException("holder", "The serialization holder cannot be null.");

            return holder.Blob != null;
        }
        #endregion

        #region Compress(SerializationHolder holder, Func<Stream,Stream> getStream, string contentEncoding)
        /// <summary>
        /// Encodes the blobs from uncompressed to compressed.
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <param name="getStream">The get compressor stream function.</param>
        /// <param name="contentEncoding">The content encoding parameter.</param>
        /// <returns>Returns true if encoded without error.</returns>
        /// <exception cref="ArgumentNullException">holder</exception>
        protected virtual bool Compress(ServiceHandlerContext holder, Func<Stream, Stream> getStream, string contentEncoding)
        {
            try
            {
                using (MemoryStream ms = new MemoryStream())
                using (Stream compress = getStream(ms))
                {
                    compress.Write(holder.Blob, 0, holder.Blob.Length);
                    compress.Close();

                    holder.SetBlob(ms.ToArray(), holder.ContentType, contentEncoding);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        }
        #endregion
        #region Decompress(SerializationHolder holder, Func<Stream, Stream> getStream)
        /// <summary>
        /// Encodes the blobs from compressed to uncompressed.
        /// </summary>
        /// <param name="holder">The holder.</param>
        /// <param name="getStream">The decompress stream create function.</param>
        /// <returns>Returns true if encoded without error.</returns>
        protected virtual bool Decompress(ServiceHandlerContext holder, Func<Stream, Stream> getStream)
        {
            try
            {
                using (MemoryStream msIn = new MemoryStream(holder.Blob))
                using (Stream decompress = getStream(msIn))
                using (MemoryStream msOut = new MemoryStream())
                {
                    decompress.CopyTo(msOut);
                    decompress.Close();
                    holder.SetBlob(msOut.ToArray(), holder.ContentType);
                }
            }
            catch (Exception ex)
            {
                return false;
            }

            return true;
        } 
        #endregion
    }
}
