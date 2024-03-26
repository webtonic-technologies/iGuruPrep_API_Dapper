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
                string sql;
                if (request.BoardId == 0)
                {
                    // Insert new board
                    sql = @"INSERT INTO tblBoard (BoardCode, BoardName, ShowCourse, Status, CreatedOn, CreatedBy) 
                        VALUES (@BoardCode, @BoardName, @ShowCourse, @Status, @CreatedOn, @CreatedBy)";
                }
                else
                {
                    // Update existing board
                    sql = @"UPDATE tblBoard 
                        SET BoardName = @BoardName, 
                            BoardCode = @BoardCode, 
                            ShowCourse = @ShowCourse, 
                            Status = @Status, 
                            ModifiedOn = @ModifiedOn,
                            ModifiedBy = @ModifiedBy

                        WHERE BoardId = @BoardId";
                }

                int rowsAffected = await _connection.ExecuteAsync(sql, new
                {
                    request.BoardId,
                    request.BoardCode,
                    request.BoardName,
                    request.ShowCourse,
                    request.Status,
                    CreatedOn = DateTime.Now,
                    CreatedBy = 1,
                    ModifiedOn = DateTime.Now,
                    ModifiedBy = 1
                });
                if (rowsAffected > 0)
                {
                    string response = request.BoardId == 0 ? "Board Created successfully." : "Board Updated successfully.";

                    return new ServiceResponse<string>(true, "Operation Successful", response, 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<List<Board>>> GetAllBoards()
        {
            try
            {
                string sql = "SELECT * FROM tblBoard";

                var boards = await _connection.QueryAsync<Board>(sql);

                if (boards != null)
                {
                    return new ServiceResponse<List<Board>>(true, "Records Found", boards.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Board>>(false, "Records Not Found", new List<Board>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Board>>(false, ex.Message, new List<Board>(), 200);
            }
        }

        public async Task<ServiceResponse<Board>> GetBoardById(int id)
        {
            try
            {
                string sql = "SELECT * FROM tblBoard WHERE BoardId = @BoardId";

                var board = await _connection.QueryFirstOrDefaultAsync<Board>(sql, new { BoardId = id });

                if (board != null)
                {
                    return new ServiceResponse<Board>(true, "Record Found", board, 200);
                }
                else
                {
                    return new ServiceResponse<Board>(false, "Record not Found", new Board(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Board>(false, ex.Message, new Board(), 500);
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
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
    }
}
