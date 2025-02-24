﻿using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace WebTracNghiemOnline.Models
{
    public class User : IdentityUser
    {
        [Required(ErrorMessage = "FullName is required.")]
        [StringLength(100, ErrorMessage = "FullName cannot exceed 100 characters.")]
        public string FullName { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime ? DateOfBirth { get; set; }

        [StringLength(200, ErrorMessage = "Address cannot exceed 200 characters.")]
        public string ? Address { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format.")]
        public override string ? PhoneNumber { get; set; }

        [Range(0, double.MaxValue)]
        public decimal ? Balance { get; set; } = 0;
        public string? Provider { get; set; } // Google, Facebook, etc.
        public string? ProviderKey { get; set; } // Unique ID từ Google hoặc provider
        public ICollection<ExamHistory> ? ExamHistories { get; set; }
        public ICollection<UserOnlineRoom>? UserOnlineRooms { get; set; }
        public ICollection<Transaction> ? transactions { get; set; }
        public ICollection<ExerciseHistory>? ExerciseHistories { get; set; }
        public ICollection<Post> ? Posts { get; set; }
        public ICollection<Comment> ? Comments { get; set; }

    }

}
