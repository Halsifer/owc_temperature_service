using Dapper.Contrib.Extensions;
using System;

namespace TemperatureService.Model
{
    /// <summary>
    /// Temperature Entry
    /// </summary>
    [Table("temperatures")]
    public class Temperature : InputTemperature
    {
        public Temperature()
        {

        }

        public Temperature(InputTemperature temperature)
        {
            id = temperature.id;
            value = temperature.value;
            unit = temperature.unit;
        }

        /// <summary>
        /// Temperature Id
        /// </summary>
        [ExplicitKey]
        public override int id { get; set; }

        /// <summary>
        /// Date the Temperature was added/created
        /// </summary>
        public DateTime created_date { get; set; }

        /// <summary>
        /// Date the Temperature was last updated
        /// </summary>
        public DateTime last_updated_date { get; set; }
    }
}
