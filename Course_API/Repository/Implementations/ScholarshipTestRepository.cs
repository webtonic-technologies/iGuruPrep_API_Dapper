using Course_API.DTOs.Requests;
using Course_API.DTOs.Response;
using Course_API.DTOs.ServiceResponse;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class ScholarshipTestRepository: IScholarshipTestRepository
    {
        private readonly IDbConnection _connection;

        public ScholarshipTestRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<int>> AddUpdateScholarshipTest(ScholarshipTestRequestDTO request)
        {
            try
            {
                if (request.ScholarshipTestId == 0)
                {
                    string insertQuery = @"
            INSERT INTO tblScholarshipTest 
            (
                APID, ExamTypeId, PatternName, TotalNumberOfQuestions, Duration,
                Status, createdon, createdby, EmployeeID
            ) 
            VALUES 
            (
                @APID, @ExamTypeId, @PatternName, @TotalNumberOfQuestions, @Duration,
                @Status, @createdon, @createdby, @EmployeeID
            ); 
            SELECT CAST(SCOPE_IDENTITY() as int);";
                    var parameters = new
                    {
                        request.APID,
                        request.ExamTypeId,
                        request.PatternName,
                        request.TotalNumberOfQuestions,
                        request.Duration,
                        request.Status,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID
                    };
                    int newId = await _connection.QuerySingleAsync<int>(insertQuery, parameters);

                    if (newId > 0)
                    {
                        int sub = await ScholarshipTestSubjectMapping(request.ScholarshipSubjects ?? new List<ScholarshipSubjects>(), newId);
                        int cla = await ScholarshipTestClassMapping(request.ScholarshipClasses ?? new List<ScholarshipClass>(), newId);
                        int board = await ScholarshipTestBoardMapping(request.ScholarshipBoards ?? new List<ScholarshipBoards>(), newId);
                        int course = await ScholarshipTestCourseMapping(request.ScholarshipCourses ?? new List<ScholarshipCourse>(), newId);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation successful", newId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
            UPDATE tblScholarshipTest
            SET
                APID = @APID,
                ExamTypeId = @ExamTypeId,
                PatternName = @PatternName,
                TotalNumberOfQuestions = @TotalNumberOfQuestions,
                Duration = @Duration,
                Status = @Status,
                modifiedon = @modifiedon,
                modifiedby = @modifiedby
            WHERE ScholarshipTestId = @ScholarshipTestId;";
                    var parameters = new
                    {
                        request.APID,
                        request.ExamTypeId,
                        request.PatternName,
                        request.TotalNumberOfQuestions,
                        request.Duration,
                        request.Status,
                        modifiedon = DateTime.Now,
                        request.modifiedby,
                        request.ScholarshipTestId
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, parameters);

                    if (rowsAffected > 0)
                    {
                        int sub = await ScholarshipTestSubjectMapping(request.ScholarshipSubjects ?? new List<ScholarshipSubjects>(), request.ScholarshipTestId);
                        int cla = await ScholarshipTestClassMapping(request.ScholarshipClasses ?? new List<ScholarshipClass>(), request.ScholarshipTestId);
                        int board = await ScholarshipTestBoardMapping(request.ScholarshipBoards ?? new List<ScholarshipBoards>(), request.ScholarshipTestId);
                        int course = await ScholarshipTestCourseMapping(request.ScholarshipCourses ?? new List<ScholarshipCourse>(), request.ScholarshipTestId);

                        if (sub > 0 && cla > 0 && board > 0 && course > 0)
                        {
                            return new ServiceResponse<int>(true, "Operation successful", request.ScholarshipTestId, 200);
                        }
                        else
                        {
                            return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<int>(false, "Some error occurred", 0, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipDiscountSchemeMapping(List<ScholarshipTestDiscountScheme>? request, int ScholarshipTestId)
        {
            // Ensure the connection is initialized
            if (_connection == null)
            {
                throw new InvalidOperationException("Database connection is not initialized.");
            }

            try
            {
                // Check if the connection is open
                if (_connection.State != System.Data.ConnectionState.Open)
                {
                     _connection.Open();
                }

                if (request == null || !request.Any())
                {
                    return new ServiceResponse<string>(false, "No discount schemes provided", string.Empty, 400);
                }

                // Begin transaction
                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        // Step 1: Delete existing schemes for the given ScholarshipTestId
                        string deleteQuery = "DELETE FROM tblSSTDiscountScheme WHERE ScholarshipTestId = @ScholarshipTestId";
                        await _connection.ExecuteAsync(deleteQuery, new { ScholarshipTestId }, transaction);

                        // Step 2: Insert new schemes
                        string insertQuery = @"
                INSERT INTO tblSSTDiscountScheme (ScholarshipTestId, PercentageStartRange, PercentageEndRange, Discount)
                VALUES (@ScholarshipTestId, @PercentageStartRange, @PercentageEndRange, @Discount)";

                        await _connection.ExecuteAsync(insertQuery, request.Select(s => new
                        {
                            ScholarshipTestId = s.ScholarshipTestId,
                            PercentageStartRange = s.PercentageStartRange,
                            PercentageEndRange = s.PercentageEndRange,
                            Discount = s.Discount
                        }), transaction);

                        // Commit transaction
                        transaction.Commit();
                    }
                    catch
                    {
                        // Rollback transaction on error
                        transaction.Rollback();
                        throw;
                    }
                }

                return new ServiceResponse<string>(true, "Discount schemes updated successfully", string.Empty, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
            finally
            {
                // Ensure the connection is closed
                if (_connection.State == System.Data.ConnectionState.Open)
                {
                     _connection.Close();
                }
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipContentIndexMapping(List<ScholarshipContentIndex> request, int ScholarshipTestId)
        {
            try
            {
                // Delete existing content index mappings for the given ScholarshipTestId
                await DeleteScholarshipContentIndexes(ScholarshipTestId);

                if (request.Any())
                {
                    string insertQuery = @"
            INSERT INTO tblScholarshipContentIndex
            (ScholarshipTestId, IndexTypeId, ContentIndexId, SubjectId)
            VALUES
            (@ScholarshipTestId, @IndexTypeId, @ContentIndexId, @SubjectId)";

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, request.Select(contentIndex => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        IndexTypeId = contentIndex.IndexTypeId,
                        ContentIndexId = contentIndex.ContentIndexId,
                        SubjectId = contentIndex.SubjectId
                    }));

                    return rowsAffected > 0
                        ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                        : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                }

                return new ServiceResponse<string>(true, "No content indexes to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipInstructionsMapping(List<ScholarshipTestInstructions>? request, int ScholarshipTestId)
        {
            try
            {
                // Delete existing instructions for the given ScholarshipTestId
                await DeleteScholarshipTestInstructions(ScholarshipTestId);

                if (request != null && request.Any())
                {
                    string insertQuery = @"
            INSERT INTO tblSSTInstructions
            (ScholarshipTestId, Instructions)
            VALUES
            (@ScholarshipTestId, @Instructions)";

                    int rowsAffected = await _connection.ExecuteAsync(insertQuery, request.Select(instr => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        Instructions = instr.Instructions
                    }));

                    return rowsAffected > 0
                        ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                        : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                }

                return new ServiceResponse<string>(true, "No instructions to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipQuestionSectionMapping(List<ScholarshipQuestionSection> request, int ScholarshipTestId)
        {
            try
            {
                // Delete existing question sections for the given ScholarshipTestId
                await DeleteScholarshipQuestionSections(ScholarshipTestId);

                if (request != null && request.Any())
                {
                    string insertQuery = @"
            INSERT INTO tblSSQuestionSection
            (
                ScholarshipTestId, DisplayOrder, SectionName,
                LevelId1, QuesPerDifficulty1, LevelId2, QuesPerDifficulty2,
                LevelId3, QuesPerDifficulty3, QuestionTypeId, MarksPerQuestion,
                NegativeMarks, TotalNumberOfQuestions, NoOfQuestionsPerChoice, SubjectId
            )
            VALUES
            (
                @ScholarshipTestId, @DisplayOrder, @SectionName,
                @LevelId1, @QuesPerDifficulty1, @LevelId2, @QuesPerDifficulty2,
                @LevelId3, @QuesPerDifficulty3, @QuestionTypeId, @MarksPerQuestion,
                @NegativeMarks, @TotalNumberOfQuestions, @NoOfQuestionsPerChoice, @SubjectId
            )";

                    var rowsAffected = await _connection.ExecuteAsync(insertQuery, request.Select(section => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        DisplayOrder = section.DisplayOrder,
                        SectionName = section.SectionName,
                        Status = section.Status,
                        LevelId1 = section.LevelId1,
                        QuesPerDifficulty1 = section.QuesPerDifficulty1,
                        LevelId2 = section.LevelId2,
                        QuesPerDifficulty2 = section.QuesPerDifficulty2,
                        LevelId3 = section.LevelId3,
                        QuesPerDifficulty3 = section.QuesPerDifficulty3,
                        QuestionTypeId = section.QuestionTypeId,
                        MarksPerQuestion = section.MarksPerQuestion,
                        NegativeMarks = section.NegativeMarks,
                        TotalNumberOfQuestions = section.TotalNumberOfQuestions,
                        NoOfQuestionsPerChoice = section.NoOfQuestionsPerChoice,
                        SubjectId = section.SubjectId
                    }));

                    return rowsAffected > 0
                        ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                        : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                }

                return new ServiceResponse<string>(true, "No question sections to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> ScholarshipQuestionsMapping(List<ScholarshipTestQuestion> request, int ScholarshipTestId, int SSTSectionId)
        {
            try
            {
                // Delete existing questions for the given ScholarshipTestId and SSTSectionId
                await DeleteScholarshipTestQuestions(ScholarshipTestId, SSTSectionId);

                if (request != null && request.Any())
                {
                    // Fetch QuestionId for each QuestionCode where IsActive = 1
                    var questionCodes = request.Select(q => q.QuestionCode).Distinct().ToList();
                    var questionIdLookup = await GetActiveQuestionIdsByCodes(questionCodes);

                    // Prepare data for insertion
                    var insertData = request.Select(question => new
                    {
                        ScholarshipTestId = ScholarshipTestId,
                        SubjectId = question.SubjectId,
                        DisplayOrder = question.DisplayOrder,
                        SSTSectionId = SSTSectionId,
                        QuestionId = questionIdLookup.ContainsKey(question.QuestionCode) ? questionIdLookup[question.QuestionCode] : (int?)null,
                        QuestionCode = question.QuestionCode
                    }).ToList();

                    // Filter out null QuestionIds if necessary
                    insertData = insertData.Where(q => q.QuestionId.HasValue).ToList();

                    if (insertData.Any())
                    {
                        string insertQuery = @"
                INSERT INTO tblSSTQuestions
                (
                    ScholarshipTestId, SubjectId, DisplayOrder, SSTSectionId,
                    QuestionId, QuestionCode
                )
                VALUES
                (
                    @ScholarshipTestId, @SubjectId, @DisplayOrder, @SSTSectionId,
                    @QuestionId, @QuestionCode
                )";

                        var rowsAffected = await _connection.ExecuteAsync(insertQuery, insertData);

                        return rowsAffected > 0
                            ? new ServiceResponse<string>(true, "Operation successful", null, 200)
                            : new ServiceResponse<string>(false, "Some error occurred", null, 500);
                    }
                    else
                    {
                        return new ServiceResponse<string>(true, "No valid questions to process", null, 200);
                    }
                }

                return new ServiceResponse<string>(true, "No questions to process", null, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<ScholarshipTestResponseDTO>>> GetScholarshipTestList(ScholarshipGetListRequest request)
        {
            try
            {
                // Base query
                string baseQuery = @"
        SELECT DISTINCT
            st.ScholarshipTestId,
            st.APID,
            ap.APName AS APName,
            st.ExamTypeId,
            et.ExamTypeName AS ExamTypeName,
            st.TotalNumberOfQuestions,
            st.Duration,
            st.Status,
            st.createdon,
            st.createdby,
            st.modifiedon,
            st.modifiedby
        FROM tblScholarshipTest st
        LEFT JOIN tblCategory ap ON st.APID = ap.APID
        LEFT JOIN tblExamType et ON st.ExamTypeId = et.ExamTypeID
        WHERE 1=1";

                // Applying filters
                if (request.APId > 0)
                {
                    baseQuery += " AND st.APID = @APId";
                }
                if (request.BoardId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipBoard sb WHERE st.ScholarshipTestId = sb.ScholarshipTestId AND sb.BoardId = @BoardId)";
                }
                if (request.ClassId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipClass sc WHERE st.ScholarshipTestId = sc.ScholarshipTestId AND sc.ClassId = @ClassId)";
                }
                if (request.CourseId > 0)
                {
                    baseQuery += " AND EXISTS (SELECT 1 FROM tblScholarshipCourse sc WHERE st.ScholarshipTestId = sc.ScholarshipTestId AND sc.CourseId = @CourseId)";
                }
                if (request.ExamTypeId > 0)
                {
                    baseQuery += " AND st.ExamTypeId = @ExamTypeId";
                }

                // Parameters for the query
                var parameters = new
                {
                    APId = request.APId,
                    BoardId = request.BoardId,
                    ClassId = request.ClassId,
                    CourseId = request.CourseId,
                    ExamTypeId = request.ExamTypeId
                };

                // Fetch all matching records
                var scholarshipTests = (await _connection.QueryAsync<ScholarshipTestResponseDTO>(baseQuery, parameters)).ToList();

                // Total count before pagination
                int totalCount = scholarshipTests.Count;

                // Apply logical pagination
                var paginatedResponse = scholarshipTests
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                // Check if there are records
                if (paginatedResponse.Any())
                {
                    return new ServiceResponse<List<ScholarshipTestResponseDTO>>(true, "Records found", paginatedResponse, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, "Records not found", new List<ScholarshipTestResponseDTO>(), 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ScholarshipTestResponseDTO>>(false, ex.Message, new List<ScholarshipTestResponseDTO>(), 500);
            }
        }
        public async Task<ServiceResponse<ScholarshipTestResponseDTO>> GetScholarshipTestById(int ScholarshipTestId)
        {
            try
            {
                // Fetch the main ScholarshipTest data
                var query = @"
        SELECT 
            st.ScholarshipTestId,
            st.APID,
            ap.APName AS APName,
            st.ExamTypeId,
            et.ExamTypeName AS ExamTypeName,
            st.PatternName,
            st.TotalNumberOfQuestions,
            st.Duration,
            st.Status,
            st.createdon,
            st.createdby,
            st.modifiedon,
            st.modifiedby,
            st.EmployeeID,
            emp.EmpFirstName AS EmpFirstName
        FROM tblScholarshipTest st
        JOIN tblCategory ap ON st.APID = ap.APID
        JOIN tblEmployee emp ON st.EmployeeID = emp.EmployeeID
        left JOIN tblExamType et ON st.ExamTypeId = et.ExamTypeId
        WHERE st.ScholarshipTestId = @ScholarshipTestId";

                var scholarshipTest = await _connection.QueryFirstOrDefaultAsync<ScholarshipTestResponseDTO>(query, new { ScholarshipTestId });

                if (scholarshipTest == null)
                {
                    return new ServiceResponse<ScholarshipTestResponseDTO>(false, "Scholarship Test not found", new ScholarshipTestResponseDTO(), 404);
                }

                // Fetch related data
                var scholarshipBoards = await GetListOfScholarshipBoards(ScholarshipTestId);
                var scholarshipClasses = await GetListOfScholarshipClasses(ScholarshipTestId);
                var scholarshipCourses = await GetListOfScholarshipCourses(ScholarshipTestId);
                var scholarshipSubjects = await GetListOfScholarshipSubjects(ScholarshipTestId);
                var scholarshipContentIndexes = await GetListOfScholarshipContentIndexes(ScholarshipTestId);
                var scholarshipQuestionSections = await GetListOfScholarshipQuestionSections(ScholarshipTestId);
                var scholarshipInstructions = await GetListOfScholarshipInstructions(ScholarshipTestId);
                var scholarshipDiscounts = await GetListOfScholarshipTestDiscountSchemes(ScholarshipTestId);
                // Initialize the SubjectDetails list
                var scholarshipSubjectDetailsList = new List<ScholarshipSubjectDetails>();

                // Populate ScholarshipSubjectDetails with content indexes and question sections
                foreach (var subject in scholarshipSubjects)
                {
                    var subjectContentIndexes = scholarshipContentIndexes
                        .Where(ci => ci.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectQuestionSections = scholarshipQuestionSections
                        .Where(qs => qs.SubjectId == subject.SubjectID)
                        .ToList();

                    var subjectDetails = new ScholarshipSubjectDetails
                    {
                        SubjectID = subject.SubjectID,
                        SubjectName = subject.SubjectName,
                        ScholarshipContentIndexResponses = subjectContentIndexes,
                        ScholarshipQuestionSections = subjectQuestionSections
                    };

                    scholarshipSubjectDetailsList.Add(subjectDetails);
                }

                // Map the fetched data to the ScholarshipTestResponseDTO
                scholarshipTest.ScholarshipBoards = scholarshipBoards;
                scholarshipTest.ScholarshipClasses = scholarshipClasses;
                scholarshipTest.ScholarshipCourses = scholarshipCourses;
                scholarshipTest.ScholarshipSubjectDetails = scholarshipSubjectDetailsList;
                scholarshipTest.ScholarshipTestInstructions = scholarshipInstructions;
                scholarshipTest.ScholarshipTestDiscountSchemes = scholarshipDiscounts;
                scholarshipTest.ScholarshipSubjects = scholarshipSubjects;
                // Fetch ScholarshipTestQuestions based on ScholarshipQuestionSections
                if (scholarshipQuestionSections != null && scholarshipQuestionSections.Any())
                {
                    scholarshipTest.ScholarshipTestQuestions = new List<ScholarshipTestQuestion>();
                    foreach (var section in scholarshipQuestionSections)
                    {
                        var questions = await GetListOfScholarshipTestQuestions(section.SSTSectionId);
                        if (questions != null)
                        {
                            scholarshipTest.ScholarshipTestQuestions.AddRange(questions);
                        }
                    }
                }

                return new ServiceResponse<ScholarshipTestResponseDTO>(true, "Success", scholarshipTest, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<ScholarshipTestResponseDTO>(false, ex.Message, new ScholarshipTestResponseDTO(), 500);
            }
        }
        private async Task<List<ScholarshipBoardsResponse>> GetListOfScholarshipBoards(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            stsb.SSTBoardId,
            stsb.ScholarshipTestId,
            stsb.BoardId,
            b.BoardName AS Name
        FROM tblScholarshipBoards stsb
        JOIN tblBoard b ON stsb.BoardId = b.BoardId
        WHERE stsb.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipBoardsResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestDiscountScheme>> GetListOfScholarshipTestDiscountSchemes(int ScholarshipTestId)
        {
            // SQL query to fetch discount schemes based on the ScholarshipTestId
            string query = @"
        SELECT 
            SSTDiscountSchemeId,
            ScholarshipTestId,
            PercentageStartRange,
            PercentageEndRange,
            Discount
        FROM tblSSTDiscountScheme
        WHERE ScholarshipTestId = @ScholarshipTestId";

            // Execute the query and map the result to a list of ScholarshipTestDiscountScheme
            var data = await _connection.QueryAsync<ScholarshipTestDiscountScheme>(query, new { ScholarshipTestId });

            return data.ToList();
        }
        private async Task<List<ScholarshipClassResponse>> GetListOfScholarshipClasses(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tsc.SSTClassId,
            tsc.ScholarshipTestId,
            tsc.ClassId,
            c.ClassName AS Name
        FROM tblScholarshipClass tsc
        JOIN tblClass c ON tsc.ClassId = c.ClassId
        WHERE tsc.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipClassResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipCourseResponse>> GetListOfScholarshipCourses(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tsc.SSTCourseId,
            tsc.ScholarshipTestId,
            tsc.CourseId,
            c.CourseName AS Name
        FROM tblScholarshipCourse tsc
        JOIN tblCourse c ON tsc.CourseId = c.CourseId
        WHERE tsc.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipCourseResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipSubjectsResponse>> GetListOfScholarshipSubjects(int ScholarshipTestId)
        {
            string query = @"
        SELECT 
            tss.SSTSubjectId,
            tss.SubjectId,
            tss.ScholarshipTestId,
            s.SubjectName AS SubjectName
        FROM tblScholarshipSubject tss
        JOIN tblSubject s ON tss.SubjectID = s.SubjectID
        WHERE tss.ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipSubjectsResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipContentIndexResponse>> GetListOfScholarshipContentIndexes(int ScholarshipTestId)
        {
            string query = @"
  SELECT 
      sci.ContentIndexId,
      sci.IndexTypeId,
      it.IndexType AS IndexTypeName,
      sci.SubjectId,
      s.SubjectName AS SubjectName,
      sci.SSTContIndId,
      CASE 
          WHEN sci.IndexTypeId = 1 THEN ci.ContentName_Chapter
          WHEN sci.IndexTypeId = 2 THEN ct.ContentName_Topic
          WHEN sci.IndexTypeId = 3 THEN cst.ContentName_SubTopic
      END AS ContentIndexName,
      sci.ScholarshipTestId
  FROM tblScholarshipContentIndex sci
  LEFT JOIN tblSubject s ON sci.SubjectId = s.SubjectId
  LEFT JOIN tblQBIndexType it ON sci.IndexTypeId = it.IndexId
  LEFT JOIN tblContentIndexChapters ci ON sci.ContentIndexId = ci.ContentIndexId AND sci.IndexTypeId = 1
  LEFT JOIN tblContentIndexTopics ct ON sci.ContentIndexId = ct.ContInIdTopic AND sci.IndexTypeId = 2
  LEFT JOIN tblContentIndexSubTopics cst ON sci.ContentIndexId = cst.ContInIdSubTopic AND sci.IndexTypeId = 3
  WHERE sci.ScholarshipTestId = @ScholarshipTestId";

            var data = await _connection.QueryAsync<ScholarshipContentIndexResponse>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipQuestionSection>> GetListOfScholarshipQuestionSections(int ScholarshipTestId)
        {
            string query = "SELECT * FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipQuestionSection>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestInstructions>> GetListOfScholarshipInstructions(int ScholarshipTestId)
        {
            string query = "SELECT * FROM tblSSTInstructions WHERE ScholarshipTestId = @ScholarshipTestId";
            var data = await _connection.QueryAsync<ScholarshipTestInstructions>(query, new { ScholarshipTestId });
            return data.ToList();
        }
        private async Task<List<ScholarshipTestQuestion>> GetListOfScholarshipTestQuestions(int SSTSectionId)
        {
            string query = "SELECT * FROM tblSSTQuestions WHERE SSTSectionId = @SSTSectionId";
            var data = await _connection.QueryAsync<ScholarshipTestQuestion>(query, new { SSTSectionId });
            return data.ToList();
        }
        private async Task<Dictionary<string, int>> GetActiveQuestionIdsByCodes(IEnumerable<string> questionCodes)
        {
            var query = @"
    SELECT QuestionCode, QuestionId
    FROM tblQuestion
    WHERE QuestionCode IN @QuestionCodes AND IsActive = 1";

            var result = await _connection.QueryAsync(query, new { QuestionCodes = questionCodes });

            return result.ToDictionary(row => (string)row.QuestionCode, row => (int)row.QuestionId);
        }
        private async Task DeleteScholarshipTestQuestions(int ScholarshipTestId, int SSTSectionId)
        {
            string query = @"
    DELETE FROM tblSSTQuestions
    WHERE ScholarshipTestId = @ScholarshipTestId AND SSTSectionId = @SSTSectionId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId, SSTSectionId = SSTSectionId });
        }
        private async Task<int> ScholarshipTestSubjectMapping(List<ScholarshipSubjects> subjects, int scholarshipTestId)
        {
            await DeleteScholarshipSubjects(scholarshipTestId);

            if (subjects.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipSubject
        (ScholarshipTestId, SubjectId)
        VALUES
        (@ScholarshipTestId, @SubjectId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, subjects.Select(subject => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    SubjectId = subject.SubjectId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no subjects to insert.
        }
        private async Task<int> ScholarshipTestClassMapping(List<ScholarshipClass> classes, int scholarshipTestId)
        {
            await DeleteScholarshipClasses(scholarshipTestId);

            if (classes.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipClass
        (ScholarshipTestId, ClassId)
        VALUES
        (@ScholarshipTestId, @ClassId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, classes.Select(cls => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    ClassId = cls.ClassId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no classes to insert.
        }
        private async Task<int> ScholarshipTestBoardMapping(List<ScholarshipBoards> boards, int scholarshipTestId)
        {
            await DeleteScholarshipBoards(scholarshipTestId);

            if (boards.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipBoards
        (ScholarshipTestId, BoardId)
        VALUES
        (@ScholarshipTestId, @BoardId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, boards.Select(board => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    BoardId = board.BoardId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no boards to insert.
        }
        private async Task<int> ScholarshipTestCourseMapping(List<ScholarshipCourse> courses, int scholarshipTestId)
        {
            await DeleteScholarshipCourses(scholarshipTestId);

            if (courses.Any())
            {
                string insertQuery = @"
        INSERT INTO tblScholarshipCourse
        (ScholarshipTestId, CourseId)
        VALUES
        (@ScholarshipTestId, @CourseId)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, courses.Select(course => new
                {
                    ScholarshipTestId = scholarshipTestId,
                    CourseId = course.CourseId
                }));

                return rowsAffected;
            }

            return 1; // Return 1 to indicate success even if no courses to insert.
        }
        private async Task DeleteScholarshipSubjects(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipSubject WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipClasses(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipClass WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipBoards(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipBoards WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipCourses(int scholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipCourse WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = scholarshipTestId });
        }
        private async Task DeleteScholarshipContentIndexes(int ScholarshipTestId)
        {
            string query = "DELETE FROM tblScholarshipContentIndex WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId });
        }
        private async Task DeleteScholarshipTestInstructions(int ScholarshipTestId)
        {
            string query = "DELETE FROM tblSSTInstructions WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId });
        }
        private async Task DeleteScholarshipQuestionSections(int ScholarshipTestId)
        {
            string query = "DELETE FROM tblSSQuestionSection WHERE ScholarshipTestId = @ScholarshipTestId";
            await _connection.ExecuteAsync(query, new { ScholarshipTestId = ScholarshipTestId });
        }
    }
}