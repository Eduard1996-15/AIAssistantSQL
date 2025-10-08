using AIAssistantSQL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AIAssistantSQL.Controllers
{
    public class DiagnosticController : Controller
    {
        private readonly ISchemaLoaderService _schemaLoaderService;
        private readonly ILogger<DiagnosticController> _logger;

        public DiagnosticController(ISchemaLoaderService schemaLoaderService, ILogger<DiagnosticController> logger)
        {
            _schemaLoaderService = schemaLoaderService;
            _logger = logger;
        }

        public IActionResult ViewSchema()
        {
            var schema = _schemaLoaderService.GetCurrentSchema();
            
            if (schema == null)
            {
                TempData["Error"] = "No hay esquema cargado";
                return RedirectToAction("Index", "Database");
            }

            return View(schema);
        }

        [HttpGet]
        public IActionResult GetSchemaJson()
        {
            var schema = _schemaLoaderService.GetCurrentSchema();
            
            if (schema == null)
            {
                return Json(new { error = "No hay esquema cargado" });
            }

            return Json(schema);
        }
    }
}
