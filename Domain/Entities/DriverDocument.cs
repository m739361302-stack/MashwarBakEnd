using Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Entities
{
    public class DriverDocument
    {
        public long Id { get; set; }
        public long DriverUserId { get; set; }
        public DriverDocumentType DocType { get; set; }
        public string FileUrl { get; set; } = null!;
        public DateTime UploadedAt { get; set; }
        public string? Note { get; set; }

        public Driver Driver { get; set; } = null!;
    }
}
