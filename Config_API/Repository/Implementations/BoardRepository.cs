using Config_API.DTOs.Requests;
using Config_API.DTOs.ServiceResponse;
using Config_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Config_API.Repository.Implementations
{
    public class BoardRepository : IBoardRepository
    {
        private readonly IDbConnection _connection;

        public BoardRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateBoard(Board request)
        {
            try
            {
                if (request.BoardId == 0)
                {
                    // Insert new board
                    string query = @"INSERT INTO [tblBoard] (BoardName, BoardCode, Status, createdon, createdby, EmployeeID) 
                             VALUES (@BoardName, @BoardCode, @Status, @createdon, @createdby, @EmployeeID)";
                    int insertedValue = await _connection.ExecuteAsync(query, new
                    {
                        request.BoardId,
                        request.BoardCode,
                        request.BoardName,
                        Status = true,
                        createdon = DateTime.Now,
                        request.createdby,
                        request.EmployeeID,
                    });
                    if (insertedValue > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Board Added Successfully", StatusCodes.Status201Created);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status400BadRequest);
                    }
                }
                else
                {
                    // Update existing board
                    string query = @"UPDATE tblBoard 
                         SET BoardName = @BoardName, 
                             BoardCode = @BoardCode, 
                             Status = @Status,  
                             modifiedon = @modifiedon, 
                             modifiedby = @modifiedby, 
                             EmployeeID = @EmployeeID
                         WHERE BoardId = @BoardId";
                    int rowsAffected = await _connection.ExecuteAsync(query, new
                    {
                        request.BoardId,
                        request.BoardCode,
                        request.BoardName,
                        request.Status,
                        ModifiedOn = DateTime.Now,
                        request.modifiedby,
                        request.EmployeeID
                    });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Board Updated Successfully", StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", string.Empty, StatusCodes.Status404NotFound);
                    }

                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Board>>> GetAllBoards(GetAllBoardsRequest request)
        {
            try
            {
                string sql = @"SELECT [BoardId]
                                  ,[BoardName]
                                  ,[BoardCode]
                                  ,[Status]
                                  ,[showcourse]
                                  ,[createdon]
                                  ,[createdby]
                                  ,[modifiedon]
                                  ,[modifiedby]
                                  ,[EmployeeID]
                                  ,[EmpFirstName]
                            FROM tblBoard";

                var boards = await _connection.QueryAsync<Board>(sql);
                var paginatedList = boards.Skip((request.PageNumber - 1) * request.PageSize)
                           .Take(request.PageSize)
                           .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Board>>(true, "Records Found", paginatedList.AsList(), StatusCodes.Status302Found, boards.Count());
                }
                else
                {
                    return new ServiceResponse<List<Board>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Board>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<List<Board>>> GetAllBoardsMaster()
        {
            try
            {
                string sql = @"SELECT [BoardId]
                                  ,[BoardName]
                                  ,[BoardCode]
                                  ,[Status]
                                  ,[showcourse]
                                  ,[createdon]
                                  ,[createdby]
                                  ,[modifiedon]
                                  ,[modifiedby]
                                  ,[EmployeeID]
                                  ,[EmpFirstName]
                            FROM tblBoard where Status = 1";

                var boards = await _connection.QueryAsync<Board>(sql);
                if (boards.Any())
                {
                    return new ServiceResponse<List<Board>>(true, "Records Found", boards.AsList(), StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<List<Board>>(false, "Records Not Found", [], StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Board>>(false, ex.Message, [], StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<Board>> GetBoardById(int id)
        {
            try
            {
                string sql = @"SELECT [BoardId]
                                  ,[BoardName]
                                  ,[BoardCode]
                                  ,[Status]
                                  ,[showcourse]
                                  ,[createdon]
                                  ,[createdby]
                                  ,[modifiedon]
                                  ,[modifiedby]
                                  ,[EmployeeID] 
                                  ,[EmpFirstName]
                                   FROM tblBoard WHERE BoardId = @BoardId";

                var board = await _connection.QueryFirstOrDefaultAsync<Board>(sql, new { BoardId = id });

                if (board != null)
                {
                    return new ServiceResponse<Board>(true, "Records Found", board, StatusCodes.Status302Found);
                }
                else
                {
                    return new ServiceResponse<Board>(false, "Records Not Found", new Board(), StatusCodes.Status204NoContent);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Board>(false, ex.Message, new Board(), StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var board = await GetBoardById(id);

                if (board.Data != null)
                {
                    board.Data.Status = !board.Data.Status;

                    string sql = "UPDATE tblBoard SET Status = @Status WHERE BoardId = @BoardId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { board.Data.Status, BoardId = id });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, StatusCodes.Status200OK);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, StatusCodes.Status304NotModified);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, StatusCodes.Status404NotFound);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, StatusCodes.Status500InternalServerError);
            }
        }
    }
}
