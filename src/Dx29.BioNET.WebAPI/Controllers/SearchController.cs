using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

using Dx29.Data;
using Dx29.Services;

namespace Dx29.BioNET.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SearchController : ControllerBase
    {
        public SearchController(EnsembleService ensembleService)
        {
            EnsembleService = ensembleService;
        }

        public EnsembleService EnsembleService { get; }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SearchAsync([FromBody] IList<string> symptoms, string source = "all", [FromQuery] int skip = 0, [FromQuery] int count = 10, [FromQuery] string lang = "en")
        {
            try
            {
                if (source == "orpha")
                {
                    var results = EnsembleService.DiagnosisOrpha.Predict(symptoms, null, skip, count);
                    return Ok(results);
                }
                else if (source == "omim")
                {
                    var results  = EnsembleService.DiagnosisOmim.Predict(symptoms, null, skip, count);
                    return Ok(results);
                }
                else
                {
                    var results = EnsembleService.Predict(symptoms, null, skip, count);
                    return Ok(results);
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
