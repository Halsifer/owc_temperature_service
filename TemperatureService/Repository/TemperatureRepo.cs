using Dapper.Contrib.Extensions;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using TemperatureService.Model;

namespace TemperatureService.Repository
{
    public class TemperatureRepo : ITemperatureRepo
    {
        private readonly IConfiguration configuration;

        private const string CONNECTION_STRING_KEY = "TemperatureConnection";

        private string ConnectionString => 
            configuration.GetConnectionString(CONNECTION_STRING_KEY) ?? 
            throw new NullReferenceException($"ConnectionString '{CONNECTION_STRING_KEY}' is not defined");

        public TemperatureRepo(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        private async Task<DbConnection> CreateOpenConnectionAsync()
        {
            var dbConnection = new MySqlConnection(ConnectionString);

            try
            {
                await dbConnection.OpenAsync().ConfigureAwait(false);
                return dbConnection;
            }
            catch (Exception)
            {
                await dbConnection.DisposeAsync().ConfigureAwait(false);

                throw;
            }
        }

        ///<inheritdoc cref="ITemperatureRepo.GetTemperaturesAsync"/>
        public async Task<IEnumerable<Temperature>> GetTemperaturesAsync()
        {
            await using var dbConnection = await CreateOpenConnectionAsync().ConfigureAwait(false);

            return await dbConnection.GetAllAsync<Temperature>().ConfigureAwait(false);
        }

        ///<inheritdoc cref="ITemperatureRepo.GetTemperatureAsync(int)"/>
        ///<exception cref="KeyNotFoundException">Thrown when no temperature entry exists</exception>
        public async Task<Temperature> GetTemperatureAsync(int id)
        {
            await using var dbConnection = await CreateOpenConnectionAsync().ConfigureAwait(false);

            return await GetTemperatureAsync(dbConnection, id).ConfigureAwait(false);
        }

        private async Task<Temperature> GetTemperatureAsync(IDbConnection dbConnection, int id)
        {
            return (await dbConnection.GetAsync<Temperature>(id).ConfigureAwait(false)) ?? throw new KeyNotFoundException($"The temperature id {id} was not found");
        }

        ///<inheritdoc cref="ITemperatureRepo.AddTemperatureAsync(InputTemperature)"/>
        public async Task<Temperature> AddTemperatureAsync(InputTemperature inputTemperature)
        {
            await using var dbConnection = await CreateOpenConnectionAsync().ConfigureAwait(false);

            var temperature = new Temperature(inputTemperature);
            temperature.created_date = temperature.last_updated_date = TruncateTo6DigitFractional(DateTime.UtcNow);

            await dbConnection.InsertAsync(temperature).ConfigureAwait(false);

            return temperature;
        }

        ///<inheritdoc cref="ITemperatureRepo.UpdateTemperatureAsync(InputTemperature)"/>
        public async Task<Temperature> UpdateTemperatureAsync(InputTemperature inputTemperature)
        {
            await using var dbConnection = await CreateOpenConnectionAsync().ConfigureAwait(false);

            var temperature = await GetTemperatureAsync(dbConnection, inputTemperature.id).ConfigureAwait(false);
            temperature.value = inputTemperature.value;
            temperature.unit = inputTemperature.unit;
            temperature.last_updated_date = TruncateTo6DigitFractional(DateTime.UtcNow);

            await dbConnection.UpdateAsync(temperature).ConfigureAwait(false);

            return temperature;
        }

        ///<inheritdoc cref="ITemperatureRepo.DeleteTemperatureAsync(int)"/>
        ///<exception cref="KeyNotFoundException">Thrown when no temperature entry exists</exception>
        public async Task<Temperature> DeleteTemperatureAsync(int id)
        {
            await using var dbConnection = await CreateOpenConnectionAsync().ConfigureAwait(false);

            var temperature = await GetTemperatureAsync(dbConnection, id).ConfigureAwait(false);

            await dbConnection.DeleteAsync(new Temperature { id = id }).ConfigureAwait(false);

            return temperature;
        }

        /// <summary>
        /// Truncates to a 6-digit fractional, do to MySql limitations
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns>Truncated DateTime value</returns>
        private DateTime TruncateTo6DigitFractional(DateTime dateTime) => dateTime.Subtract(TimeSpan.FromTicks(dateTime.Ticks % 10));
    }
}
