﻿namespace Schools_API.DTOs.Requests
{
    public class QuestionRejectionRequestDTO
    {
        public int QuestionId {  get; set; }
        public string QuestionCode { get; set; } = string.Empty;
        public int Rejectedby { get; set; }
        public DateTime? RejectedDate { get; set; }
        public string RejectedReason { get; set; } = string.Empty;
        public string FileUpload { get; set; } = string.Empty;
    }
    public class QuestionApprovalRequestDTO
    {
        public int QuestionId { get; set; }
        public int ApprovedBy { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public string QuestionCode { get; set; } = string.Empty;
    }
}
