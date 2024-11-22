using Config_API.DTOs.Requests;
using Config_API.DTOs.Response;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using OfficeOpenXml;
using System.Data;
using Dapper;
using System.Data.SqlClient;
using Config_API.Models;

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

                // Pre-fill the second row with new rule data
                worksheet.Cells[2, 1].Value = ruleId;
                worksheet.Cells[2, 2].Value = request.QuestionTypeId;
                worksheet.Cells[2, 3].Value = questionTypeName; // Assuming QuestionTypeName is the same as RuleName
                worksheet.Cells[2, 4].Value = 0; // Default MarksPerQuestion
                worksheet.Cells[2, 5].Value = 0; // Default NoOfCorrectOptions
                worksheet.Cells[2, 6].Value = 0; // Default NumberOfOptionsSelected
                worksheet.Cells[2, 7].Value = 0; // Default SuccessRate

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
            pmm.SuccessRate
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
            pmm.SuccessRate
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
                                var rowRuleId = Convert.ToInt32(worksheet.Cells[row, 1]?.Value);
                                var rowQuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 2]?.Value);
                                var rowQuestionTypeName = worksheet.Cells[row, 3]?.Value?.ToString();

                                // Validate RuleId
                                if (rowRuleId != RuleId)
                                {
                                    errors.Add($"Row {row}: RuleId is inconsistent. Expected: {RuleId}, Found: {rowRuleId}");
                                    continue;
                                }

                                // Validate QuestionTypeId and QuestionTypeName
                                if (rowQuestionTypeId != questionTypeId || rowQuestionTypeName != questionTypeName)
                                {
                                    errors.Add($"Row {row}: QuestionTypeId or QuestionTypeName is inconsistent. Expected: {questionTypeId} - {questionTypeName}, Found: {rowQuestionTypeId} - {rowQuestionTypeName}");
                                    continue;
                                }

                                // Further validation and data extraction
                                var marksPerQuestion = Convert.ToDecimal(worksheet.Cells[row, 4]?.Value ?? 0);
                                var noOfCorrectOptions = Convert.ToInt32(worksheet.Cells[row, 5]?.Value ?? 0);
                                var noOfOptionsSelected = Convert.ToInt32(worksheet.Cells[row, 6]?.Value ?? 0);
                                var successRate = Convert.ToInt32(worksheet.Cells[row, 7]?.Value ?? 0);

                                if (marksPerQuestion <= 0 || noOfCorrectOptions <= 0 || noOfOptionsSelected <= 0 || successRate < 0)
                                {
                                    errors.Add($"Row {row}: Invalid data values.");
                                    continue;
                                }

                                // Add to the list if valid
                                partialMarksMappings.Add(new PartialMarksMappings
                                {
                                    PartialMarksId = RuleId,
                                    MarksPerQuestion = marksPerQuestion,
                                    NoOfCorrectOptions = noOfCorrectOptions,
                                    NoOfOptionsSelected = noOfOptionsSelected,
                                    SuccessRate = successRate
                                });
                            }
                            catch (Exception ex)
                            {
                                errors.Add($"Row {row}: Error processing row data - {ex.Message}");
                            }
                        }
                        //for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
                        //{
                        //    try
                        //    {
                        //        // Extract and validate data
                        //        var rowQuestionTypeId = Convert.ToInt32(worksheet.Cells[row, 2]?.Value);
                        //        var rowQuestionTypeName = worksheet.Cells[row, 3]?.Value?.ToString();

                        //        if (rowQuestionTypeId != questionTypeId || rowQuestionTypeName != questionTypeName)
                        //        {
                        //            errors.Add($"Row {row}: QuestionTypeId or QuestionTypeName is inconsistent.");
                        //            continue;
                        //        }

                        //        var marksPerQuestion = Convert.ToDecimal(worksheet.Cells[row, 4]?.Value ?? 0);
                        //        var noOfCorrectOptions = Convert.ToInt32(worksheet.Cells[row, 5]?.Value ?? 0);
                        //        var noOfOptionsSelected = Convert.ToInt32(worksheet.Cells[row, 6]?.Value ?? 0);
                        //        var successRate = Convert.ToInt32(worksheet.Cells[row, 7]?.Value ?? 0);

                        //        if (marksPerQuestion <= 0 || noOfCorrectOptions <= 0 || noOfOptionsSelected <= 0 || successRate < 0)
                        //        {
                        //            errors.Add($"Row {row}: Invalid data values.");
                        //            continue;
                        //        }

                        //        partialMarksMappings.Add(new PartialMarksMappings
                        //        {
                        //            PartialMarksId = RuleId,
                        //            MarksPerQuestion = marksPerQuestion,
                        //            NoOfCorrectOptions = noOfCorrectOptions,
                        //            NoOfOptionsSelected = noOfOptionsSelected,
                        //            SuccessRate = successRate
                        //        });
                        //    }
                        //    catch (Exception ex)
                        //    {
                        //        errors.Add($"Row {row}: Error processing row data - {ex.Message}");
                        //    }
                        //}

                        if (errors.Any())
                        {
                            return new ServiceResponse<string>(false, string.Join("\n", errors), string.Empty, 400);
                        }

                        //// Delete existing mappings for the RuleId
                        //var deleteQuery = "DELETE FROM tbl_PartialMarksMapping WHERE PartialMarksId = @RuleId";
                        //await _connection.ExecuteAsync(deleteQuery, new { RuleId });

                        // Insert new mappings
                        var insertQuery = @"
                INSERT INTO tbl_PartialMarksMapping 
                (PartialMarksId, MarksPerQuestion, NoOfCorrectOptions, NoOfOptionsSelected, SuccessRate)
                VALUES (@PartialMarksId, @MarksPerQuestion, @NoOfCorrectOptions, @NoOfOptionsSelected, @SuccessRate)";
                        await _connection.ExecuteAsync(insertQuery, partialMarksMappings);

                        return new ServiceResponse<string>(true, "Partial marks mappings uploaded successfully.", string.Empty, 200);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}