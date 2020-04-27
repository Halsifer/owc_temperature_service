
using Newtonsoft.Json;

namespace TemperatureService.Model
{
    /// <summary>
    /// Temperature Input Value for adding or updating Temperatures
    /// </summary>
    public class InputTemperature
    {
        /// <summary>
        /// Temperature Id
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public virtual int id { get; set; }

        /// <summary>
        /// Temperature Value
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public int value { get; set; }

        /// <summary>
        /// Temperature Unit of Value
        /// </summary>
        [JsonProperty(Required = Required.Always)]
        public TemperatureUnit unit { get; set; }
    }
}
