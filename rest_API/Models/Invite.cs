﻿namespace rest_API.Models
{
    public class Invite
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        public string Username { get; set; }
        public string Friend_Username { get; set; }
    }
}