using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Models
{

    /// <summary>
    /// 
    /// </summary>
    internal class PEDebugDetails
    {

        /// <summary>
        /// The signature of the PE and PDB linking them together - usually a GUID
        /// </summary>
        [JsonInclude]
        public string Signature { get; internal set; }

        /// <summary>
        /// Checksum of the PE & PDB. Format: {algorithm}:{hash:X}
        /// </summary>
        [JsonInclude]
        public string Checksum { get; internal set; }

        /// <summary>
        /// The full location of the PDB at build time
        /// </summary>
        [JsonInclude]
        public string File { get; internal set; }

        /// <summary>
        /// The generated Timestamp of the code at build time stored as hex
        /// </summary>
        [JsonInclude]
        public string Timestamp { get; internal set; }

    }

}
