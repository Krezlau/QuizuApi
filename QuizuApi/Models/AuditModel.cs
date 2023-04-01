namespace QuizuApi.Models
{
    public class AuditModel
    {
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
    }
}
