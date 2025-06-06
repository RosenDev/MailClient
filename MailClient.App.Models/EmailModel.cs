﻿namespace MailClient.App.Models
{
    public class EmailModel
    {
        public string Uid { get; set; }
        public string From { get; set; }
        public List<string> To { get; set; }
        public List<string> Cc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
