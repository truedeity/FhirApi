using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FhirApi.Models;

namespace FhirApi.Controllers
{
    [ApiController]
    [Route("fhir/[controller]")]
    public class PatientController : ControllerBase
    {
        private readonly FhirDbContext _context;
        private readonly FhirJsonParser _fhirJsonParser;
        private readonly FhirJsonSerializer _fhirJsonSerializer;

        public PatientController(FhirDbContext context)
        {
            _context = context;
            _fhirJsonParser = new FhirJsonParser(); // Parse incoming FHIR
            _fhirJsonSerializer = new FhirJsonSerializer(); // Serialize FHIR objects to JSON
        }

        // GET: fhir/Patient/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(string id)
        {
            var patientResource = await _context.Patients.FindAsync(id);
            if (patientResource == null)
            {
                return NotFound(new OperationOutcome
                {
                    Issue =
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Details = new CodeableConcept("http://hl7.org/fhir/issue-type", "not-found", "Patient not found")
                        }
                    }
                });
            }

            // Deserialize JSON to Patient object
            var patient = _fhirJsonParser.Parse<Patient>(patientResource.ResourceJson);
            return Ok(patient.ToJson());
        }

        // POST: fhir/Patient
        [HttpPost]
        public async Task<IActionResult> CreatePatient()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                // Parse incoming JSON to Patient
                var patient = _fhirJsonParser.Parse<Patient>(body);

                // Assign a new ID if not provided
                if (string.IsNullOrEmpty(patient.Id))
                    patient.Id = Guid.NewGuid().ToString();

                // Serialize Patient to JSON
                var patientJson = _fhirJsonSerializer.SerializeToString(patient);

                // Save to DB
                var patientResource = new PatientResource
                {
                    Id = patient.Id,
                    ResourceJson = patientJson
                };

                _context.Patients.Add(patientResource);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetPatient), new { id = patient.Id }, patient.ToJson());
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to parse FHIR resource",
                    details = ex.Message
                });
            }
        }

        // PUT: fhir/Patient/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(string id)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                var patient = _fhirJsonParser.Parse<Patient>(body);

                if (id != patient.Id)
                    return BadRequest("Mismatched Patient ID");

                var patientResource = await _context.Patients.FindAsync(id);
                if (patientResource == null)
                {
                    return NotFound();
                }

                patientResource.ResourceJson = _fhirJsonSerializer.SerializeToString(patient);
                _context.Entry(patientResource).State = EntityState.Modified;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = "Failed to parse FHIR resource",
                    details = ex.Message
                });
            }
        }

        // DELETE: fhir/Patient/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(string id)
        {
            var patientResource = await _context.Patients.FindAsync(id);
            if (patientResource == null)
            {
                return NotFound(new OperationOutcome
                {
                    Issue =
                    {
                        new OperationOutcome.IssueComponent
                        {
                            Severity = OperationOutcome.IssueSeverity.Error,
                            Details = new CodeableConcept("http://hl7.org/fhir/issue-type", "not-found", "Patient not found")
                        }
                    }
                });
            }

            _context.Patients.Remove(patientResource);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
