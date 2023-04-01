﻿using System.ComponentModel.DataAnnotations.Schema;

namespace QuizuApi.Models
{
    public class UserSettings : AuditModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public required Guid Id { get; set; }
        public virtual User User { get; set; }
        [ForeignKey(nameof(UserId))]
        public required string UserId { get; set; }
        public required bool DarkMode { get; set; }
        public required bool ShowEmail { get; set; }
        public required bool ShowSurname { get; set; }
    }
}
