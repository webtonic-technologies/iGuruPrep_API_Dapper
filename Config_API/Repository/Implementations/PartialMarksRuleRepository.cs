using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using OfficeOpenXml;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class PartialMarksRuleRepository : IPartialMarksRuleRepository
    {
        private readonly IDbConnection _connection;
        public PartialMarksRuleRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<byte[]>> AddPartialMarksRule(PartialMarksRequest request)
        {

            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            // Step 1: Insert Rule into Database
            var query = @"
            INSERT INTO tbl_PartialMarksRules (QuestionTypeId, RuleName)
            VALUES (@QuestionTypeId, @RuleName);
            SELECT CAST(SCOPE_IDENTITY() AS INT);";

            int ruleId = await _connection.ExecuteScalarAsync<int>(query, new
            {
                QuestionTypeId = request.QuestionTypeId,
                RuleName = request.RuleName
            });
            var questionTypeQuery = @"
            SELECT QuestionType
            FROM tblQBQuestionType
            WHERE QuestionTypeID = @QuestionTypeId";

            var questionTypeName = await _connection.QueryFirstOrDefaultAsync<string>(questionTypeQuery, new
            {
                QuestionTypeId = request.QuestionTypeId
            });

            // Step 2: Generate Excel Template
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("PartialMarksRules");

                // Add headers
                worksheet.Cells[1, 1].Value = "RuleId";
                worksheet.Cells[1, 2].Value = "QuestionTypeId";
                worksheet.Cells[1, 3].Value = "QuestionTypeName";
                worksheet.Cells[1, 4].Value = "MarksPerQuestion";
                worksheet.Cells[1, 5].Value = "NoOfCorrectOptions";
                worksheet.Cells[1, 6].Value = "NumberOfOptionsSelected";
                worksheet.Cells[1, 7].Value = "SuccessRate";
                worksheet.Cells[1, 8].Value = "Acquired Marks";

                // Pre-fill the second row with new rule data
                worksheet.Cells[2, 1].Value = ruleId;
                worksheet.Cells[2, 2].Value = request.QuestionTypeId;
                worksheet.Cells[2, 3].Value = questionTypeName; // Assuming QuestionTypeName is the same as RuleName
                worksheet.Cells[2, 4].Value = 0; // Default MarksPerQuestion
                worksheet.Cells[2, 5].Value = 0; // Default NoOfCorrectOptions
                worksheet.Cells[2, 6].Value = 0; // Default NumberOfOptionsSelected
                worksheet.Cells[2, 7].Value = 0; // Default SuccessRate
                worksheet.Cells[2, 8].Value = 0;
                // Convert Excel to byte array
                var fileContent = package.GetAsByteArray();

                return new ServiceResponse<byte[]>(true, "Excel generated successfully.", fileContent, 200);
            }
        }
        public async Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRules()
        {
            try
            {
                // Query to join tbl_PartialMarksRules, tblQBQuestionType, and tbl_PartialMarksMapping
                var query = @"
        SELECT 
            pmr.PartialMarksId AS RuleId,
            pmr.QuestionTypeId,
            qt.QuestionType AS QuestionTypeName,
            pmr.RuleName,
            pmm.MappingId,
            pmm.MarksPerQuestion,
            pmm.NoOfCorrectOptions,
            pmm.NoOfOptionsSelected,
            pmm.SuccessRate,
            pmm.AcquiredMarks,
            pmm.IsNegative
        FROM tbl_PartialMarksRules pmr
        INNER JOIN tblQBQuestionType qt
        ON pmr.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tbl_PartialMarksMapping pmm
        ON pmr.PartialMarksId = pmm.PartialMarksId where pmr.[IsUploaded] = 1";

                var partialMarksDictionary = new Dictionary<int, PartialMarksResponse>();

                // Query execution with mapping for nested objects
                await _connection.QueryAsync<PartialMarksResponse, PartialMarksMappings, PartialMarksResponse>(
                    query,
                    (rule, mapping) =>
                    {
                        if (!partialMarksDictionary.TryGetValue(rule.RuleId, out var partialMarksResponse))
                        {
                            partialMarksResponse = rule;
                            partialMarksResponse.PartialMarks = new List<PartialMarksMappings>();
                            partialMarksDictionary.Add(rule.RuleId, partialMarksResponse);
                        }

                        if (mapping != null)
                        {
                            partialMarksResponse.PartialMarks.Add(mapping);
                        }

                        return partialMarksResponse;
                    },
                    splitOn: "MappingId"
                );

                return new ServiceResponse<List<PartialMarksResponse>>(
                    true,
                    "Records found",
                    partialMarksDictionary.Values.ToList(),
                    200
                );
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return new ServiceResponse<List<PartialMarksResponse>>(
                    false,
                    ex.Message,
                    new List<PartialMarksResponse>(),
                    500
                );
            }
        }
        public async Task<ServiceResponse<List<PartialMarksResponse>>> GetAllPartialMarksRulesList(GetListRequest request)
        {
            try
            {
                // Query to join tbl_PartialMarksRules, tblQBQuestionType, and tbl_PartialMarksMapping
                var query = @"
        SELECT 
            pmr.PartialMarksId AS RuleId,
            pmr.QuestionTypeId,
            qt.QuestionType AS QuestionTypeName,
            pmr.RuleName,
            pmm.MappingId,
            pmm.MarksPerQuestion,
            pmm.NoOfCorrectOptions,
            pmm.NoOfOptionsSelected,
             pmm.SuccessRate,
            pmm.AcquiredMarks,
            pmm.IsNegative
        FROM tbl_PartialMarksRules pmr
        INNER JOIN tblQBQuestionType qt
        ON pmr.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tbl_PartialMarksMapping pmm
        ON pmr.PartialMarksId = pmm.PartialMarksId";

                var partialMarksDictionary = new Dictionary<int, PartialMarksResponse>();

                // Query execution with mapping for nested objects
                await _connection.QueryAsync<PartialMarksResponse, PartialMarksMappings, PartialMarksResponse>(
                    query,
                    (rule, mapping) =>
                    {
                        if (!partialMarksDictionary.TryGetValue(rule.RuleId, out var partialMarksResponse))
                        {
                            partialMarksResponse = rule;
                            partialMarksResponse.PartialMarks = new List<PartialMarksMappings>();
                            partialMarksDictionary.Add(rule.RuleId, partialMarksResponse);
                        }

                        if (mapping != null)
                        {
                            partialMarksResponse.PartialMarks.Add(mapping);
                        }

                        return partialMarksResponse;
                    },
                    splitOn: "MappingId"
                );
                var paginatedList = partialMarksDictionary.Values
                 .Skip((request.PageNumber - 1) * request.PageSize)
                 .Take(request.PageSize)
                 .ToList();
                return new ServiceResponse<List<PartialMarksResponse>>(
                    true,
                    "Records found",
                   paginatedList,
                    200
                );
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return new ServiceResponse<List<PartialMarksResponse>>(
                    false,
                    ex.Message,
                    new List<PartialMarksResponse>(),
                    500
                );
            }
        }
        public async Task<ServiceResponse<PartialMarksResponse>> GetPartialMarksRuleyId(int RuleId)
        {
            try
            {
                // Query to fetch the rule and its associated mappings
                var query = @"
        SELECT 
            pmr.PartialMarksId AS RuleId,
            pmr.QuestionTypeId,
            qt.QuestionType AS QuestionTypeName,
            pmr.RuleName,
            pmm.MappingId,
            pmm.PartialMarksId,
            pmm.MarksPerQuestion,
            pmm.NoOfCorrectOptions,
            pmm.NoOfOptionsSelected,
            pmm.SuccessRate,
            pmm.AcquiredMarks,
            pmm.IsNegative
        FROM tbl_PartialMarksRules pmr
        INNER JOIN tblQBQuestionType qt
        ON pmr.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tbl_PartialMarksMapping pmm
        ON pmr.PartialMarksId = pmm.PartialMarksId
        WHERE pmr.PartialMarksId = @RuleId";

                PartialMarksResponse partialMarksResponse = null;

                // Execute query and map results
                await _connection.QueryAsync<PartialMarksResponse, PartialMarksMappings, PartialMarksResponse>(
                    query,
                    (rule, mapping) =>
                    {
                        if (partialMarksResponse == null)
                        {
                            partialMarksResponse = rule;
                            partialMarksResponse.PartialMarks = new List<PartialMarksMappings>();
                        }

                        if (mapping != null)
                        {
                            partialMarksResponse.PartialMarks.Add(mapping);
                        }

                        return partialMarksResponse;
                    },
                    new { RuleId },
                    splitOn: "MappingId"
                );

                if (partialMarksResponse == null)
                {
                    return new ServiceResponse<PartialMarksResponse>(false, "Record not found", null, 404);
                }

                return new ServiceResponse<PartialMarksResponse>(true, "Record found", partialMarksResponse, 200);
            }
            catch (Exception ex)
            {
                // Handle any exceptions
                return new ServiceResponse<PartialMarksResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<string>> UploadPartialMarksSheet(IFormFile file, int RuleId)
        {
            if (file == null || file.Length <= 0)
            {
                return new ServiceResponse<string>(false, "Invalid file. Please upload a valid Excel file.", string.Empty, 400);
            }

            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        if (worksheet == null)
                        {
                            return new ServiceResponse<string>(false, "The Excel file does not contain any worksheets.", string.Empty, 400);
                        }

                        // Validate RuleId
                        var ruleQuery = @"
                SELECT 
                    pmr.QuestionTypeId, qt.QuestionType 
                FROM tbl_PartialMarksRules pmr
                INNER JOIN tblQBQuestionType qt
                ON pmr.QuestionTypeId = qt.QuestionTypeID
                WHERE pmr.PartialMarksId = @RuleId";
                        var ruleDetails = await _connection.QueryFirstOrDefaultAsync(ruleQuery, new { RuleId });

                        if (ruleDetails == null)
                        {
                            return new ServiceResponse<string>(false, "Invalid RuleId. Please provide a valid RuleId.", string.Empty, 400);
                        }

                        var questionTypeId = ruleDetails.QuestionTypeId;
                        var questionTypeName = ruleDetails.QuestionType;

                        var partialMarksMappings = new List<PartialMarksMappings>();
                        var errors = new List<string>();

                        for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        {
                            try
                            {
                                // Get the RuleId value
                                var ruleIdCellValue = worksheet.Cells[row, 1]?.Value?.ToString();
                                if (string.IsNullOrWhiteSpace(ruleIdCellValue) || !int.TryParse(ruleIdCellValue, out var rowRuleId))
                                {
                                    break;
                                }

                                // Handle RuleId = 0 or inconsistent RuleId
                                if (rowRuleId == 0)
                                {
                                    // Check if there are more rows with non-zero RuleId
                                    bool hasMoreRecords = false;
                                    for (int nextRow = row + 1; nextRow <= worksheet.Dimension.End.Row; nextRow++)
                                    {
                                        var nextRuleIdCell = worksheet.Cells[nextRow, 1]?.Value?.ToString();
                                        if (!string.IsNullOrWhiteSpace(nextRuleIdCell) && int.TryParse(nextRuleIdCell, out var nextRuleId) && nextRuleId != 0)
                                        {
                                            hasMoreRecords = true;
                                            break;
                                        }
                                    }

                                    if (hasMoreRecords)
                                    {
                                        errors.Add($"Row {row}: RuleId is zero, but more records exist. Validation continues.");
                                        continue; // Continue to the next row
                                    }
                                    else
                                    {
                                        errors.Add($"Row {row}: RuleId is zero. No further records found. Stopping validation.");
                                        break; // Stop processing
                                    }
                                }

                                // Validate RuleId consistency
                                if (rowRuleId != RuleId)
                                {
                                    errors.Add($"Row {row}: RuleId is inconsistent. Expected: {RuleId}, Found: {rowRuleId}");
                                    continue;
                                }

                                // Validate QuestionTypeId and QuestionTypeName
                                var rowQuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 2]?.Value);
                                var rowQuestionTypeName = worksheet.Cells[row, 3]?.Value?.ToString();

                                if (rowQuestionTypeId != questionTypeId || rowQuestionTypeName != questionTypeName)
                                {
                                    errors.Add($"Row {row}: QuestionTypeId or QuestionTypeName is inconsistent. Expected: {questionTypeId} - {questionTypeName}, Found: {rowQuestionTypeId} - {rowQuestionTypeName}");
                                    continue;
                                }

                                // Extract data
                                var marksPerQuestion = Convert.ToDecimal(worksheet.Cells[row, 4]?.Value ?? 0);
                                var noOfCorrectOptions = Convert.ToInt32(worksheet.Cells[row, 5]?.Value ?? 0);
                                var noOfOptionsSelected = Convert.ToInt32(worksheet.Cells[row, 6]?.Value ?? 0);
                                var successRate = Convert.ToInt32(worksheet.Cells[row, 7]?.Value ?? 0);
                                var acquiredMarks = Convert.ToDecimal(worksheet.Cells[row, 8]?.Value ?? 0);

                                if (marksPerQuestion <= 0 || noOfCorrectOptions <= 0 || noOfOptionsSelected == 0)
                                {
                                    errors.Add($"Row {row}: Invalid data values.");
                                    continue;
                                }

                                // Determine if success rate is negative
                                bool isNegative = successRate < 0;

                                // Add valid data to the list
                                partialMarksMappings.Add(new PartialMarksMappings
                                {
                                    PartialMarksId = RuleId,
                                    MarksPerQuestion = marksPerQuestion,
                                    NoOfCorrectOptions = noOfCorrectOptions,
                                    NoOfOptionsSelected = noOfOptionsSelected,
                                    SuccessRate = successRate,
                                    IsNegative = isNegative,
                                    AcquiredMarks = acquiredMarks
                                });
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Row {row}: Error processing row data - {ex.Message}");
                            }
                        }

                        // If errors exist, do not insert any data into the database
                        if (errors.Any())
                        {
                            return new ServiceResponse<string>(false, string.Join("\n", errors), string.Empty, 400);
                        }

                        // Insert collected data into the database
                        if (partialMarksMappings.Any())
                        {
                            var insertQuery = @"
                        INSERT INTO tbl_PartialMarksMapping 
                        (PartialMarksId, MarksPerQuestion, NoOfCorrectOptions, NoOfOptionsSelected, SuccessRate, AcquiredMarks, IsNegative)
                        VALUES (@PartialMarksId, @MarksPerQuestion, @NoOfCorrectOptions, @NoOfOptionsSelected, @SuccessRate, @AcquiredMarks, @IsNegative)";
                            await _connection.ExecuteAsync(insertQuery, partialMarksMappings);

                            // Mark the rule as uploaded
                            await _connection.ExecuteAsync(@"UPDATE tbl_PartialMarksRules SET IsUploaded = 1 WHERE PartialMarksId = @RuleId", new { RuleId });
                        }

                        // Success response
                        return new ServiceResponse<string>(true, "Partial marks mappings uploaded successfully.", string.Empty, 200);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<byte[]> DownloadPartialMarksExcelSheet(int RuleId)
        {
            try
            {
                // Query to fetch data
                var query = @"
        SELECT 
            pmr.PartialMarksId AS RuleId,
            pmr.QuestionTypeId,
            qt.QuestionType AS QuestionTypeName,
            pmm.MarksPerQuestion,
            pmm.NoOfCorrectOptions,
            pmm.NoOfOptionsSelected AS NumberOfOptionsSelected,
            pmm.SuccessRate,
            pmm.AcquiredMarks
        FROM tbl_PartialMarksRules pmr
        INNER JOIN tblQBQuestionType qt
        ON pmr.QuestionTypeId = qt.QuestionTypeID
        LEFT JOIN tbl_PartialMarksMapping pmm
        ON pmr.PartialMarksId = pmm.PartialMarksId
        WHERE pmr.PartialMarksId = @RuleId";

                // Fetch data from the database
                var data = await _connection.QueryAsync<PartialMarksExcelData>(query, new { RuleId });

                // Check if data exists
                if (!data.Any())
                {
                    throw new Exception("No records found for the specified Rule ID.");
                }

                // Generate Excel file using EPPlus
                using (var package = new ExcelPackage())
                {
                    // Create a worksheet
                    var worksheet = package.Workbook.Worksheets.Add("Partial Marks Data");

                    // Add headers
                    var headers = new[]
                    {
                "RuleId", "QuestionTypeId", "QuestionTypeName", "MarksPerQuestion", "NoOfCorrectOptions",
                "NumberOfOptionsSelected", "SuccessRate", "Acquired Marks"
            };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        worksheet.Cells[1, i + 1].Value = headers[i];
                        worksheet.Cells[1, i + 1].Style.Font.Bold = true;
                    }

                    // Add data to the worksheet
                    int row = 2;
                    foreach (var record in data)
                    {
                        worksheet.Cells[row, 1].Value = record.RuleId;
                        worksheet.Cells[row, 2].Value = record.QuestionTypeId;
                        worksheet.Cells[row, 3].Value = record.QuestionTypeName;
                        worksheet.Cells[row, 4].Value = record.MarksPerQuestion;
                        worksheet.Cells[row, 5].Value = record.NoOfCorrectOptions;
                        worksheet.Cells[row, 6].Value = record.NumberOfOptionsSelected;
                        worksheet.Cells[row, 7].Value = record.SuccessRate;
                        worksheet.Cells[row, 8].Value = record.AcquiredMarks;
                        row++;
                    }

                    // Auto-fit columns for better readability
                    worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

                    // Return the Excel file as a byte array
                    return package.GetAsByteArray();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while generating Excel sheet: {ex.Message}");
            }
        }
    }
    public class PartialMarksExcelData
    {
        public int RuleId { get; set; }
        public int QuestionTypeId { get; set; }
        public string QuestionTypeName { get; set; }
        public decimal MarksPerQuestion { get; set; }
        public int NoOfCorrectOptions { get; set; }
        public int NumberOfOptionsSelected { get; set; }
        public decimal SuccessRate { get; set; }
        public decimal AcquiredMarks { get; set; }
    }

}