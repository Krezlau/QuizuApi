﻿namespace QuizuApi.Models.Database
{
    public class AuditModel
    {
        public DateTime CreatedAt { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public required bool IsDeleted { get; set; }
    }
}
