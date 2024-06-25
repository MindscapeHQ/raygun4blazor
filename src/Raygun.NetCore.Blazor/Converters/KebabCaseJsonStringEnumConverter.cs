using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Raygun.NetCore.Blazor.Converters
{

    /// <summary>
    /// A <see cref="JsonStringEnumConverter{TEnum}"/> that converts enum values to and from kebab-case strings.
    /// </summary>
    /// <typeparam name="TEnum">The enum type to convert to / from.</typeparam>
    public class KebabCaseJsonStringEnumConverter<TEnum> : JsonStringEnumConverter<TEnum> where TEnum : struct, Enum
    {

        /// <summary>
        /// The default constructor, for use when constructed via attributes.
        /// </summary>
        public KebabCaseJsonStringEnumConverter() : base(JsonNamingPolicy.KebabCaseLower)
        {
        }

    }

}
