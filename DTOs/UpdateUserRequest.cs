﻿namespace RealWorldApp.DTOs
{
    public class UpdateUserRequest
    {
        public UpdateUserData User { get; set; } = new();
    }


    public class UpdateUserData
    {
        public string? Email { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Bio { get; set; }
        public string? Image { get; set; }
    }
}
