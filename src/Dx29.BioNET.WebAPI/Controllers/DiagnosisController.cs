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
    public class DiagnosisController : ControllerBase
    {
        public DiagnosisController(EnsembleService ensembleService)
        {
            EnsembleService = ensembleService;
        }

        public EnsembleService EnsembleService { get; }

        [HttpPost("describe")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult Describe([FromBody] IList<string> ids, [FromQuery] string lang = "en")
        {
            try
            {
                var results = EnsembleService.DescribeDiseases(ids, lang);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("calculate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult CalculateAsync([FromBody] DataAnalysisInfo analysisInfo, string source = "all", [FromQuery] string lang = "en", [FromQuery] int count = 100)
        {
            try
            {
                IList<DiffDisease> results = null;
                if (source == "orpha")
                {
                    results = EnsembleService.DiagnosisOrpha.CalculateDx29(analysisInfo, count);
                }
                else if (source == "omim")
                {
                    results = EnsembleService.DiagnosisOmim.CalculateDx29(analysisInfo, count);
                }
                else
                {
                    results = EnsembleService.CalculateDx29(analysisInfo, count);
                }
                if (lang != "en")
                {
                    EnsembleService.TranslateDiseases(results);
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("phrank")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult PhrankAsync([FromBody] DataAnalysisInfo analysisInfo, string source = "all", [FromQuery] string lang = "en", [FromQuery] int count = 100)
        {
            try
            {
                IList<DiffDisease> results = null;
                if (source == "orpha")
                {
                    results = EnsembleService.DiagnosisOrpha.PhrankDx29(analysisInfo, count);
                }
                else if (source == "omim")
                {
                    results = EnsembleService.DiagnosisOmim.PhrankDx29(analysisInfo, count);
                }
                else
                {
                    results = EnsembleService.PhrankDx29(analysisInfo, count);
                }
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
