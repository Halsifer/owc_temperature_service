using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Threading.Tasks;
using TemperatureService.Model;
using TemperatureService.Repository;

namespace TemperatureService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TemperatureController : ControllerBase
    {
        private readonly ILogger<TemperatureController> log;
        private readonly ITemperatureRepo temperatureRepo;

        public TemperatureController(ILogger<TemperatureController> log, ITemperatureRepo temperatureRepo)
        {
            this.log = log;
            this.temperatureRepo = temperatureRepo;
        }

        /// <summary>
        /// Gets All Temperatures with duplicated Farhenheit Temperatures
        /// </summary>
        /// <returns>Array of Temperature entries</returns>
        /// <response code="200">Returns all Temperatures with duplicated Farhenheit Temperatures</response>
        [HttpGet]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(IEnumerable<Temperature>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get()
        {
            try
            {
                var temperatures = await temperatureRepo.GetTemperaturesAsync().ConfigureAwait(false);

                var temperaturesWithDupes = new List<Temperature>();
                foreach(var temperature in temperatures)
                {
                    temperaturesWithDupes.Add(temperature);

                    if(temperature.unit == TemperatureUnit.F)
                    {
                        temperaturesWithDupes.Add(temperature);
                    }
                }
                
                return Ok(temperaturesWithDupes);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(Get)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Gets a Temperature by id
        /// </summary>
        /// <param name="id">Id of the Temperature</param>
        /// <returns>The Temperature entry</returns>
        /// <response code="200">Returns the Temperature entry</response>
        /// <response code="400">If the parameter is invalid</response>   
        /// <response code="404">If the Temperature is not found</response>   
        [HttpGet("{id}", Name = nameof(GetById))]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                return Ok(await temperatureRepo.GetTemperatureAsync(id).ConfigureAwait(false));
            }
            catch (KeyNotFoundException e)
            {
                return Problem(e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(GetById)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add a new Temperature entry
        /// </summary>
        /// <param name="inputTemperature">The Temperature to add</param>
        /// <returns>The new Temperature entry</returns>
        /// <response code="201">Returns the created Temperature entry</response>
        /// <response code="400">If the json is invalid</response>   
        /// <response code="409">If a Temperature already exists with the provided id</response>   
        [HttpPost()]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostByJson([BindRequired] InputTemperature inputTemperature)
        {
            try
            {
                return await PostAsync(inputTemperature).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(PostByJson)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Add a new Temperature entry
        /// </summary>
        /// <param name="id">Id of the Temperature</param>
        /// <param name="value">Level of the Temperature</param>
        /// <param name="unit">TemperatureUnit of the value</param>
        /// <returns>The new Temperature entry</returns>
        /// <response code="201">Returns the created Temperature entry</response>
        /// <response code="400">If the json is invalid</response>   
        /// <response code="409">If a Temperature already exists with the provided id</response>   
        [HttpPost("{id}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PostByForm(int id, [FromQuery, BindRequired] int value, [FromQuery, BindRequired] TemperatureUnit unit)
        {
            try
            {
                return await PostAsync(new InputTemperature { id = id, value = value, unit = unit }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(PostByForm)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> PostAsync(InputTemperature inputTemperature)
        {
            try
            {
                var newTemperature = await temperatureRepo.AddTemperatureAsync(inputTemperature).ConfigureAwait(false);

                return CreatedAtAction(nameof(GetById), new { newTemperature.id }, newTemperature);
            }
            catch (MySqlException e) when (e.Number == 1062)
            {
                return Problem($"An existing record with the id '{inputTemperature.id}' was already found.", statusCode: StatusCodes.Status409Conflict);
            }
        }

        /// <summary>
        /// Updates a Temperature entry or adds if it does not exist
        /// </summary>
        /// <param name="inputTemperature">The Temperature to update or add</param>
        /// <returns>The updated or created Temperature information</returns>
        /// <response code="200">Returns the updated Temperature entry</response>
        /// <response code="201">Returns the newly created Temperature entry</response>
        /// <response code="400">If the json is invalid</response>   
        [HttpPut()]
        [Consumes(MediaTypeNames.Application.Json)]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutByJson([BindRequired] InputTemperature inputTemperature)
        {
            try
            {
                return await PutAsync(inputTemperature).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(PutByJson)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Updates a Temperature entry or adds if it does not exist
        /// </summary>
        /// <param name="id">Id of the Temperature</param>
        /// <param name="value">Level of the Temperature</param>
        /// <param name="unit">TemperatureUnit of the value</param>
        /// <returns>The updated or created Temperature information</returns>
        /// <response code="200">Returns the updated Temperature entry</response>
        /// <response code="201">Returns the created Temperature entry</response>
        /// <response code="400">If the json is invalid</response>   
        [HttpPut("{id}")]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> PutByForm(int id, [FromQuery, BindRequired] int value, [FromQuery, BindRequired] TemperatureUnit unit)
        {
            try
            {
                return await PutAsync(new InputTemperature { id = id, value = value, unit = unit }).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(PutByForm)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }

        private async Task<IActionResult> PutAsync(InputTemperature inputTemperature)
        {
            try
            {
                var temperature = await temperatureRepo.UpdateTemperatureAsync(inputTemperature).ConfigureAwait(false);

                return Ok(temperature);
            }
            catch (KeyNotFoundException)
            {
                return await PostAsync(inputTemperature).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Deletes a Temperature by id
        /// </summary>
        /// <param name="id">Id of the Temperature</param>
        /// <returns>The deleted Temperature entry</returns>
        /// <response code="200">Returns the Temperature entry</response>
        /// <response code="400">If the parameter is invalid</response>   
        /// <response code="404">If the Temperature is not found</response> 
        [HttpDelete("{id}", Name = nameof(DeleteById))]
        [Produces(MediaTypeNames.Application.Json)]
        [ProducesResponseType(typeof(Temperature), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteById(int id)
        {
            try
            {
                return Ok(await temperatureRepo.DeleteTemperatureAsync(id).ConfigureAwait(false));
            }
            catch (KeyNotFoundException e)
            {
                return Problem(e.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (Exception e)
            {
                log.LogError(e, $"Exception caught in {nameof(DeleteById)}");
                return Problem(e.Message, statusCode: StatusCodes.Status500InternalServerError);
            }
        }
    }
}
