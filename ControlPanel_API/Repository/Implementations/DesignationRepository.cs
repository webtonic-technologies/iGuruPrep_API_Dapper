using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class DesignationRepository : IDesignationRepository
    {
        private readonly IDbConnection _connection;

        public DesignationRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddDesignation(Designation request)
        {

            try
            {
                // Construct the SQL insert query
                string sql = @"INSERT INTO tblDesignation (DesignationName, DesgnCode, DesignationNumber, Status) 
                       VALUES (@DesignationName, @DesgnCode, @DesignationNumber, @Status);";

                // Execute the insert query asynchronously using Dapper
                int rowsAffected = await _connection.ExecuteAsync(sql, request);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Desgnation Added Successfully", 200);
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

        public async Task<ServiceResponse<Designation>> GetDesignationByID(int DesgnID)
        {
            try
            {
                string sql = "SELECT * FROM tblDesignation WHERE DesgnID = @DesgnID";

                var designation = await _connection.QueryFirstOrDefaultAsync<Designation>(sql, new { DesgnID });

                if (designation != null)
                {
                    return new ServiceResponse<Designation>(true, "Record Found", designation, 200);
                }
                else
                {
                    return new ServiceResponse<Designation>(false, "Record not Found", new Designation(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Designation>(false, ex.Message, new Designation(), 500);
            }
        }

        public async Task<ServiceResponse<List<Designation>>> GetDesignationList()
        {
            try
            {
                string sql = "SELECT * FROM tblDesignation";

                var designations = await _connection.QueryAsync<Designation>(sql);

                if (designations != null)
                {
                    return new ServiceResponse<List<Designation>>(true, "Records Found", designations.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Designation>>(false, "Records Not Found", new List<Designation>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, new List<Designation>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateDesignation(Designation request)
        {
            try
            {
                string sql = @"UPDATE tblDesignation 
                       SET DesgnCode = @DesgnCode, DesignationNumber = @DesignationNumber, Status = @Status
                       WHERE DesgnID = @DesgnID";


                int rowsAffected = await _connection.ExecuteAsync(sql, request);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Desgnation Updated Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "Record not found", 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
