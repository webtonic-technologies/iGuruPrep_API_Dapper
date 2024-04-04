using Course_API.Models;

namespace Course_API.DTOs
{
    public class SyllabusDetailsDTO
    {
        public int SyllabusId {  get; set; }
        public List<SyllabusDetails>? SyllabusDetails {  get; set; }
    }
}
