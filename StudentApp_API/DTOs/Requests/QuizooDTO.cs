﻿namespace StudentApp_API.DTOs.Requests
{
    public class QuizooDTO
    {
        public int QuizooID { get; set; }
        public string QuizooName { get; set; }
        public DateTime QuizooDate { get; set; }
        public DateTime QuizooStartTime { get; set; }
        public string Duration { get; set; }
        public int NoOfQuestions { get; set; }
        public int NoOfPlayers { get; set; }
        public string QuizooLink { get; set; }
        public int CreatedBy { get; set; }
        public string QuizooDuration { get; set; }
        public bool IsSystemGenerated { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int BoardID { get; set; }
        public List<QuizooSyllabusDTO> QuizooSyllabus { get; set; }
    }

    public class QuizooSyllabusDTO
    {
        public int QSID { get; set; }
        public int QuizooID { get; set; }
        public int SubjectID { get; set; }
        public int ChapterID { get; set; }
    }
    public class SubmitAnswerRequest
    {
        public int QuizID { get; set; }
        public int StudentID { get; set; }
        public int QuestionID { get; set; }
        public int AnswerID { get; set; }
        public bool IsCorrect { get; set; }
    }
    public class QuizooDTOResponse
    {
        public int QuizooID { get; set; }
        public string QuizooName { get; set; }
        public DateTime QuizooDate { get; set; }
        public DateTime QuizooStartTime { get; set; }
        public string Duration { get; set; }
        public int NoOfQuestions { get; set; }
        public int NoOfPlayers { get; set; }
        public string QuizooLink { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedByName { get; set; }
        public string QuizooDuration { get; set; }
        public bool IsSystemGenerated { get; set; }
        public int ClassID { get; set; }
        public int CourseID { get; set; }
        public int BoardID { get; set; }
        public string QuizooStatus {  get; set; }
        public List<QuizooSyllabusDTO> QuizooSyllabus { get; set; }
    }
}