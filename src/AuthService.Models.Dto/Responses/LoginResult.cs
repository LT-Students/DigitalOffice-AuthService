﻿using System;

namespace LT.DigitalOffice.AuthService.Models.Dto.Responses
{
    public record LoginResult
    {
        public Guid UserId { get; init; }
        public string AccessToken { get; init; }
        public string RefreshToken { get; init; }
        public double TokenLifeTime { get; init; }
    }
}