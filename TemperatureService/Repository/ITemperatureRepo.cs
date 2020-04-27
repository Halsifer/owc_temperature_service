using System.Collections.Generic;
using System.Threading.Tasks;
using TemperatureService.Model;

namespace TemperatureService.Repository
{
    public interface ITemperatureRepo
    {
        /// <summary>
        /// Queries the temperature table by id
        /// </summary>
        /// <param name="id">Id of the temperature entry</param>
        /// <returns>The temperature entry</returns>
        Task<Temperature> GetTemperatureAsync(int id);

        /// <summary>
        /// Queries the temperature table for all temperature entries
        /// </summary>
        /// <returns>Enumeration of temperature entries</returns>
        Task<IEnumerable<Temperature>> GetTemperaturesAsync();

        /// <summary>
        /// Inserts a temperature input into the temperature table
        /// </summary>
        /// <param name="temperature">The new temperature value</param>
        /// <returns>The newly created temperature entry</returns>
        Task<Temperature> AddTemperatureAsync(InputTemperature temperature);

        /// <summary>
        /// Updates a temperature entry in the temperature table
        /// </summary>
        /// <param name="temperature">The updated temperature value</param>
        /// <returns>The updated temperature entry</returns>
        Task<Temperature> UpdateTemperatureAsync(InputTemperature temperature);

        /// <summary>
        /// Removes a temperature entry from the temperature table by id
        /// </summary>
        /// <param name="id">Id of the temperature entry</param>
        /// <returns>The deleted temperature entry</returns>
        Task<Temperature> DeleteTemperatureAsync(int id);
    }
}
