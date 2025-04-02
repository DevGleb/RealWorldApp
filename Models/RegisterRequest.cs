﻿namespace RealWorldApp.Models
{
    public class RegisterRequest
    {
        public RegisterUserDto User { get; set; } = new();
    }

    public class RegisterUserDto
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
