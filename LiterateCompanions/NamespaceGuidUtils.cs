﻿using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace LiterateCompanions;

/// <summary>
/// Helper to create deterministic, named <see cref="Guid"/>s within a namespace.<br/>
/// https://tools.ietf.org/html/rfc4122
/// </summary>
public static class NamespaceGuidUtils
{

    /// <summary>
    /// The namespace for fully-qualified domain names (from RFC 4122, Appendix C).
    /// </summary>
    public static readonly Guid DnsNamespace = new("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// The namespace for URLs (from RFC 4122, Appendix C).
    /// </summary>
    public static readonly Guid UrlNamespace = new("6ba7b811-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// The namespace for ISO OIDs (from RFC 4122, Appendix C).
    /// </summary>
    public static readonly Guid IsoOidNamespace = new("6ba7b812-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="name">The name (within that namespace).</param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    public static Guid CreateV5Guid(Guid namespaceId, string name) => Create(namespaceId, name, 5);

    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="name">The name (within that namespace).</param>
    /// <param name="version">The version number of the UUID to create; this value must be either
    /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    public static Guid Create(Guid namespaceId, string name, int version)
    {
        if (name is null)
            throw new ArgumentNullException(nameof(name));

        // convert the name to a sequence of octets (as defined by the standard or conventions of its namespace) (step 3)
        // ASSUME: UTF-8 encoding is always appropriate
        return Create(namespaceId, Encoding.UTF8.GetBytes(name), version);
    }

    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="nameBytes">The name (within that namespace).</param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    public static Guid Create(Guid namespaceId, byte[] nameBytes) => Create(namespaceId, nameBytes, 5);

    /// <summary>
    /// Creates a name-based UUID using the algorithm from RFC 4122 §4.3.
    /// </summary>
    /// <param name="namespaceId">The ID of the namespace.</param>
    /// <param name="nameBytes">The name (within that namespace).</param>
    /// <param name="version">The version number of the UUID to create; this value must be either
    /// 3 (for MD5 hashing) or 5 (for SHA-1 hashing).</param>
    /// <returns>A UUID derived from the namespace and name.</returns>
    public static Guid Create(Guid namespaceId, byte[] nameBytes, int version)
    {
        if (version != 3 && version != 5)
            throw new ArgumentOutOfRangeException(nameof(version), "version must be either 3 or 5.");

        // convert the namespace UUID to network order (step 3)
        var namespaceBytes = namespaceId.ToByteArray();
        SwapByteOrder(namespaceBytes);

        // compute the hash of the namespace ID concatenated with the name (step 4)
        var data = namespaceBytes.Concat(nameBytes).ToArray();
        byte[] hash;
        using (HashAlgorithm algorithm = version == 3 ? MD5.Create() : SHA1.Create())
            hash = algorithm.ComputeHash(data);

        // most bytes from the hash are copied straight to the bytes of the new GUID (steps 5-7, 9, 11-12)
        var newGuid = new byte[16];
        Array.Copy(hash, 0, newGuid, 0, 16);

        // set the four most significant bits (bits 12 through 15) of the time_hi_and_version field to the appropriate 4-bit version number from Section 4.1.3 (step 8)
        newGuid[6] = (byte)(newGuid[6] & 0x0F | version << 4);

        // set the two most significant bits (bits 6 and 7) of the clock_seq_hi_and_reserved to zero and one, respectively (step 10)
        newGuid[8] = (byte)(newGuid[8] & 0x3F | 0x80);

        // convert the resulting UUID to local byte order (step 13)
        SwapByteOrder(newGuid);
        return new Guid(newGuid);
    }

    // Converts a GUID (expressed as a byte array) to/from network order (MSB-first).
    internal static void SwapByteOrder(byte[] guid)
    {
        Array.Reverse(guid, 0, 4);
        Array.Reverse(guid, 4, 2);
        Array.Reverse(guid, 6, 2);
    }
}