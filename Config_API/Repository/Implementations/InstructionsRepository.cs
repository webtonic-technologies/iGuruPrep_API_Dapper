using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class InstructionsRepository : IInstructionsRepository
    {
        private readonly IDbConnection _connection;

        public InstructionsRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateInstruction(Instructions request)
        {
            try
            {
                string insertQuery = @"
            INSERT INTO [tblInstructions] 
            ([InstructionName], [InstructionsDescription]) 
            VALUES (@InstructionName, @InstructionsDescription);";

                string updateQuery = @"
            UPDATE [tblInstructions] 
            SET 
                InstructionName = @InstructionName, 
                InstructionsDescription = @InstructionsDescription 
            WHERE 
                InstructionId = @InstructionId;";

                // If InstructionId is 0, it's a new entry; otherwise, it's an update.
                if (request.InstructionId == 0)
                {
                    await _connection.ExecuteAsync(insertQuery, new
                    {
                        request.InstructionName,
                        request.InstructionsDescription
                    });
                    return new ServiceResponse<string>(true, "Instruction added successfully", null, 200);
                }
                else
                {
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                    {
                        request.InstructionId,
                        request.InstructionName,
                        request.InstructionsDescription
                    });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Instruction updated successfully", null, 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Instruction not found", null, 404);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<Instructions>>> GetAllInstructionsMaster()
        {
            try
            {
                string query = @"
            SELECT 
                InstructionId, 
                InstructionName, 
                InstructionsDescription
            FROM 
                [tblInstructions];";

                var instructionsList = await _connection.QueryAsync<Instructions>(query);

                if (instructionsList == null || !instructionsList.Any())
                {
                    return new ServiceResponse<List<Instructions>>(false, "No instructions found", null, 204);
                }

                return new ServiceResponse<List<Instructions>>(true, "Instructions retrieved successfully", instructionsList.ToList(), 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Instructions>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<List<Instructions>>> GetAllInstructions(GetAllInstructionsRequest request)
        {
            try
            {
                // Step 1: Get all records from the table
                string query = @"
            SELECT 
                InstructionId, 
                InstructionName, 
                InstructionsDescription
            FROM 
                [tblInstructions];";

                var allInstructions = await _connection.QueryAsync<Instructions>(query);

                if (allInstructions == null || !allInstructions.Any())
                {
                    return new ServiceResponse<List<Instructions>>(false, "No instructions found", null, 204);
                }

                // Step 2: Apply pagination in memory
                var paginatedInstructions = allInstructions
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                return new ServiceResponse<List<Instructions>>(true, "Instructions retrieved successfully", paginatedInstructions, 200, allInstructions.Count());
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Instructions>>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<Instructions>> GetInstructionById(int id)
        {
            try
            {
                // Step 1: Query to fetch the instruction by its ID
                string query = @"
            SELECT 
                InstructionId, 
                InstructionName, 
                InstructionsDescription
            FROM 
                [tblInstructions]
            WHERE 
                InstructionId = @Id;";

                // Step 2: Execute the query and fetch the result
                var instruction = await _connection.QueryFirstOrDefaultAsync<Instructions>(query, new { Id = id });

                // Step 3: Check if the instruction is found
                if (instruction == null)
                {
                    return new ServiceResponse<Instructions>(false, "Instruction not found", null, 404);
                }

                // Step 4: Return the fetched instruction
                return new ServiceResponse<Instructions>(true, "Instruction retrieved successfully", instruction, 200);
            }
            catch (Exception ex)
            {
                // Step 5: Handle any exceptions
                return new ServiceResponse<Instructions>(false, ex.Message, null, 500);
            }
        }
    }
}
