using Dapper;
using Schools_API.DTOs.Requests;
using Schools_API.DTOs.Response;
using Schools_API.DTOs.ServiceResponse;
using Schools_API.Repository.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace Schools_API.Repository.Implementations
{
    public class ReportedQuestionRepository : IReportedQuestionRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public ReportedQuestionRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<List<ReportedQuestionResponse>>> GetListOfReportedQuestions(ReportedQuestionRequest request)
        {
            try
            {
                // Fetch the role of the user (Admin or SME) based on EmployeeId
                string roleSql = @"
        SELECT e.RoleID, r.RoleCode
        FROM tblEmployee e
        JOIN tblRole r ON e.RoleID = r.RoleID
        WHERE e.Employeeid = @EmployeeId";

                var roleResult = await _connection.QueryFirstOrDefaultAsync(roleSql, new { request.EmployeeId });

                if (roleResult == null)
                {
                    return new ServiceResponse<List<ReportedQuestionResponse>>(false, "Role not found for the employee", null, 404);
                }

                string roleCode = roleResult.RoleCode;
                bool isAdmin = roleCode == "AD"; // RoleCode 'AD' is Admin
                bool isSME = roleCode == "SM"; // RoleCode 'SM' is SME

                // Modify the main query accordingly
                string sql = @"
        SELECT rq.QueryCode,
               rq.Querydescription,
               rq.QuestionCode,
               rq.DateandTime,
               rq.RQSID,
               rq.Reply,
               rq.Link,
               rq.ImageOrPDF,
               rq.subjectID,
               s.SubjectName AS subjectname,
               rqs.RQSName,
               rq.StudentId,
               u.FirstName + ' ' + u.LastName AS StudentName,
               u.PhoneNumber AS StudentPhone,
               u.Email AS StudentEmail,
               rq.EmployeeId,";

                // Conditionally include EmployeeName based on role
                if (isAdmin)
                {
                    sql += "e.EmpFirstName + ' ' + e.EmpLastName AS EmployeeName,";
                }
                else
                {
                    sql += "NULL AS EmployeeName,";
                }

                // Continue with the rest of the query
                sql += @"
               rq.CategoryId,
               c.APName,
               rq.ClassId,
               cl.ClassName,
               rq.CourseId,
               co.CourseName,
               rq.BoardId,
               b.BoardName,
               rq.ExamTypeId,
               et.ExamTypeName
        FROM tblReportedQuestions rq
        LEFT JOIN tblSubject s ON rq.subjectID = s.SubjectID
        LEFT JOIN tblStatus rqs ON rq.RQSID = rqs.RQSID
        LEFT JOIN tblUser u ON rq.StudentId = u.UserId
        LEFT JOIN tblEmployee e ON rq.EmployeeId = e.Employeeid
        LEFT JOIN tblCategory c ON rq.CategoryId = c.APId
        LEFT JOIN tblClass cl ON rq.ClassId = cl.ClassID
        LEFT JOIN tblCourse co ON rq.CourseId = co.CourseID
        LEFT JOIN tblBoard b ON rq.BoardId = b.BoardID
        LEFT JOIN tblExamType et ON rq.ExamTypeId = et.ExamTypeId
        WHERE (@SubjectId = 0 OR rq.subjectID = @SubjectId)
          AND (@StartDate IS NULL OR rq.DateandTime >= @StartDate)
          AND (@EndDate IS NULL OR rq.DateandTime <= @EndDate)
          AND (@Today IS NULL OR CONVERT(date, rq.DateandTime) = CONVERT(date, @Today))";

                var parameters = new
                {
                    SubjectId = request.SubjectId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Today = request.Today
                };

                var reportedQuestions = await _connection.QueryAsync<ReportedQuestionResponse>(sql, parameters);
                foreach (var data in reportedQuestions)
                {
                    data.ImageOrPDF = GetFile(data.ImageOrPDF);
                }

                if (reportedQuestions != null && reportedQuestions.Any())
                {
                    return new ServiceResponse<List<ReportedQuestionResponse>>(true, "Operation Successful", reportedQuestions.ToList(), 200, reportedQuestions.Count());
                }
                else
                {
                    return new ServiceResponse<List<ReportedQuestionResponse>>(false, "No records found", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ReportedQuestionResponse>>(false, ex.Message, [], 500);
            }
        }
        //public async Task<ServiceResponse<List<ReportedQuestionResponse>>> GetListOfReportedQuestions(ReportedQuestionRequest request)
        //{
        //    try
        //    {
        //        string sql = @"
        //SELECT rq.QueryCode,
        //       rq.Querydescription,
        //       rq.QuestionCode,
        //       rq.DateandTime,
        //       rq.RQSID,
        //       rq.Reply,
        //       rq.Link,
        //       rq.ImageOrPDF,
        //       rq.subjectID,
        //       s.SubjectName AS subjectname,
        //       rqs.RQSName,
        //       rq.StudentId,
        //       u.FirstName + ' ' + u.LastName AS StudentName,
        //       u.PhoneNumber AS StudentPhone,
        //       u.Email AS StudentEmail,
        //       rq.EmployeeId,
        //       e.EmpFirstName + ' ' + e.EmpLastName AS EmployeeName,
        //       rq.CategoryId,
        //       c.APName,
        //       rq.ClassId,
        //       cl.ClassName,
        //       rq.CourseId,
        //       co.CourseName,
        //       rq.BoardId,
        //       b.BoardName,
        //       rq.ExamTypeId,
        //       et.ExamTypeName
        //FROM tblReportedQuestions rq
        //LEFT JOIN tblSubject s ON rq.subjectID = s.SubjectID
        //LEFT JOIN tblStatus rqs ON rq.RQSID = rqs.RQSID
        //LEFT JOIN tblUser u ON rq.StudentId = u.UserId
        //LEFT JOIN tblEmployee e ON rq.EmployeeId = e.Employeeid
        //LEFT JOIN tblCategory c ON rq.CategoryId = c.APId
        //LEFT JOIN tblClass cl ON rq.ClassId = cl.ClassID
        //LEFT JOIN tblCourse co ON rq.CourseId = co.CourseID
        //LEFT JOIN tblBoard b ON rq.BoardId = b.BoardID
        //LEFT JOIN tblExamType et ON rq.ExamTypeId = et.ExamTypeId
        //WHERE (@SubjectId = 0 OR rq.subjectID = @SubjectId)
        //  AND (@StartDate IS NULL OR rq.DateandTime >= @StartDate)
        //  AND (@EndDate IS NULL OR rq.DateandTime <= @EndDate)
        //  AND (@Today IS NULL OR CONVERT(date, rq.DateandTime) = CONVERT(date, @Today))";

        //        var parameters = new
        //        {
        //            SubjectId = request.SubjectId,
        //            StartDate = request.StartDate,
        //            EndDate = request.EndDate,
        //            Today = request.Today
        //        };

        //        var reportedQuestions = await _connection.QueryAsync<ReportedQuestionResponse>(sql, parameters);
        //        foreach(var data in reportedQuestions)
        //        {
        //            data.ImageOrPDF = GetFile(data.ImageOrPDF);
        //        }

        //        if (reportedQuestions != null && reportedQuestions.Any())
        //        {
        //            return new ServiceResponse<List<ReportedQuestionResponse>>(true, "Operation Successful", reportedQuestions.ToList(), 200, reportedQuestions.Count());
        //        }
        //        else
        //        {
        //            return new ServiceResponse<List<ReportedQuestionResponse>>(false, "No records found", [], 404);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<List<ReportedQuestionResponse>>(false, ex.Message, [], 500);
        //    }
        //}
        public async Task<ServiceResponse<ReportedQuestionResponse>> GetReportedQuestionById(int QueryCode)
        {
            try
            {
                string sql = @"
        SELECT rq.QueryCode,
               rq.Querydescription,
               rq.QuestionCode,
               rq.DateandTime,
               rq.RQSID,
               rq.Reply,
               rq.Link,
               rq.ImageOrPDF,
               rq.subjectID,
               s.SubjectName AS subjectname,
               rqs.RQSName,
               rq.StudentId,
               u.FirstName + ' ' + u.LastName AS StudentName,
               u.PhoneNumber AS StudentPhone,
               u.Email AS StudentEmail,
               rq.EmployeeId,
               e.EmpFirstName + ' ' + e.EmpLastName AS EmployeeName,
               rq.CategoryId,
               c.APName,
               rq.ClassId,
               cl.ClassName,
               rq.CourseId,
               co.CourseName,
               rq.BoardId,
               b.BoardName,
               rq.ExamTypeId,
               et.ExamTypeName
        FROM tblReportedQuestions rq
        LEFT JOIN tblSubject s ON rq.subjectID = s.SubjectID
        LEFT JOIN tblStatus rqs ON rq.RQSID = rqs.RQSID
        LEFT JOIN tblUser u ON rq.StudentId = u.UserId
        LEFT JOIN tblEmployee e ON rq.EmployeeId = e.Employeeid
        LEFT JOIN tblCategory c ON rq.CategoryId = c.APId
        LEFT JOIN tblClass cl ON rq.ClassId = cl.ClassID
        LEFT JOIN tblCourse co ON rq.CourseId = co.CourseID
        LEFT JOIN tblBoard b ON rq.BoardId = b.BoardID
        LEFT JOIN tblExamType et ON rq.ExamTypeId = et.ExamTypeId
        WHERE rq.QueryCode = @QueryCode";

                var parameters = new { QueryCode = QueryCode };

                var reportedQuestion = await _connection.QueryFirstOrDefaultAsync<ReportedQuestionResponse>(sql, parameters);

                if (reportedQuestion != null)
                {
                    reportedQuestion.ImageOrPDF = GetFile(reportedQuestion.ImageOrPDF);
                    return new ServiceResponse<ReportedQuestionResponse>(true, "Operation Successful", reportedQuestion, 200);
                }
                else
                {
                    return new ServiceResponse<ReportedQuestionResponse>(false, "No records found", new ReportedQuestionResponse(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ReportedQuestionResponse>(false, ex.Message, new ReportedQuestionResponse(), 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateQueryForReportedQuestion(ReportedQuestionQueryRequest request)
        {
            try
            {
                // Check if the query is already closed
                string checkStatusSql = @"
            SELECT RQSID 
            FROM tblReportedQuestions
            WHERE QueryCode = @QueryCode";

                var currentStatus = await _connection.QueryFirstOrDefaultAsync<int>(checkStatusSql, new { request.QueryCode });

                if (currentStatus == 3)
                {
                    // If the question is already closed, return an error
                    return new ServiceResponse<string>(false, "This query is already closed", string.Empty, 400);
                }

                // If the question is not closed, proceed with the update
                string updateSql = @"
            UPDATE tblReportedQuestions
            SET 
                QuestionCode = @QuestionCode,
                Reply = @Reply,
                Link = @Link,
                ImageOrPDF = @ImageOrPDF,
                RQSID = @RQSID, -- Mark as Closed
                EmployeeId = @EmployeeId
            WHERE 
                QueryCode = @QueryCode";

                var parameters = new
                {
                    request.QuestionCode,
                    request.Reply,
                    request.Link,
                    ImageOrPDF = FileUpload(request.ImageOrPDF),
                    request.QueryCode,
                    RQSID = 3,  // Set status to Closed
                    request.EmployeeId
                };

                var affectedRows = await _connection.ExecuteAsync(updateSql, parameters);

                if (affectedRows > 0)
                {
                    return new ServiceResponse<string>(true, "Query updated and closed successfully", string.Empty, 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Query not found", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> AddUpdateReportedQuestion(ReportedQuestionRequestDTO request)
        {
            try
            {
                _connection.Open();

                // Check if record exists based on QueryCode
                var existingRecord = await _connection.QueryFirstOrDefaultAsync<int>(
                    "SELECT COUNT(*) FROM tblReportedQuestions WHERE QueryCode = @QueryCode",
                    new { request.QueryCode });

                if (existingRecord > 0)
                {
                    // Update existing record
                    var updateQuery = @"
                    UPDATE tblReportedQuestions 
                    SET 
                        Querydescription = @Querydescription,
                        QuestionCode = @QuestionCode,
                        DateandTime = @DateandTime,
                        RQSID = @RQSID,
                        Reply = @Reply,
                        Link = @Link,
                        ImageOrPDF = @ImageOrPDF,
                        subjectID = @subjectID,
                        StudentId = @StudentId,
                        EmployeeId = @EmployeeId,
                        CategoryId = @CategoryId,
                        ClassId = @ClassId,
                        CourseId = @CourseId,
                        BoardId = @BoardId,
                        ExamTypeId = @ExamTypeId,
                        StatusId = @StatusId
                    WHERE QueryCode = @QueryCode";
                    request.RQSID = 1;
                    request.ImageOrPDF = FileUpload(request.ImageOrPDF);
                    var affectedrows = await _connection.ExecuteAsync(updateQuery, request);
                    if (affectedrows > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Reported question updated successfully.", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
                else
                {
                    // Insert new record
                    var insertQuery = @"
                    INSERT INTO tblReportedQuestions 
                    (
                        QueryCode, Querydescription, QuestionCode, DateandTime, 
                        RQSID, Reply, Link, ImageOrPDF, subjectID, StudentId, 
                        EmployeeId, CategoryId, ClassId, CourseId, BoardId, 
                        ExamTypeId, StatusId
                    ) 
                    VALUES 
                    (
                        @QueryCode, @Querydescription, @QuestionCode, @DateandTime, 
                        @RQSID, @Reply, @Link, @ImageOrPDF, @subjectID, @StudentId, 
                        @EmployeeId, @CategoryId, @ClassId, @CourseId, @BoardId, 
                        @ExamTypeId, @StatusId
                    )";
                    request.RQSID = 2;
                    request.ImageOrPDF = FileUpload(request.ImageOrPDF);
                    var insertedvalue = await _connection.ExecuteAsync(insertQuery, request);
                    if (insertedvalue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Reported question Added successfully.", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> ChangeRQStatus(RQStatusRequest request)
        {
            try
            {
                string updateSql = @"
                UPDATE tblReportedQuestions
                SET 
                    RQSID = @RQSID,
                    EmployeeId = @EmployeeId
                WHERE 
                    QueryCode = @QueryCode";

                var parameters = new
                {
                    request.QueryCode,
                    RQSID = 1,
                    request.EmployeeId
                };

                var affectedRows = await _connection.ExecuteAsync(updateSql, parameters);

                if (affectedRows > 0)
                {
                    return new ServiceResponse<string>(true, "Query updated successfully", string.Empty, 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Query not found", string.Empty, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        private string FileUpload(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String == "string")
            {
                return string.Empty;
            }
            if (base64String == string.Empty)
            {
                return string.Empty;
            }
            byte[] data = Convert.FromBase64String(base64String);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ReportedQuestions");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(data) == true ? ".jpg" : IsPng(data) == true ?
                ".png" : IsGif(data) == true ? ".gif" : IsPdf(data) == true ? ".pdf" : string.Empty;

            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, data);
            return filePath;
        }
        private string GetFile(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "ReportedQuestions", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private bool IsJpeg(byte[] bytes)
        {
            // JPEG magic number: 0xFF, 0xD8
            return bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xD8;
        }
        private bool IsPng(byte[] bytes)
        {
            // PNG magic number: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            return bytes.Length > 7 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }
        private bool IsGif(byte[] bytes)
        {
            // GIF magic number: "GIF"
            return bytes.Length > 2 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
        private bool IsPdf(byte[] bytes)
        {
            // PDF magic number: "%PDF"
            return bytes.Length > 4 &&
                   bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
        }
    }
}
