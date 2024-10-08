﻿using ControlPanel_API.DTOs.Requests;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;
using System.Data.SqlClient;

namespace ControlPanel_API.Repository.Implementations
{
    public class DesignationRepository : IDesignationRepository
    {
        private readonly IDbConnection _connection;

        public DesignationRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateDesignation(Designation request)
        {
            try
            {
                if (request.DesgnID == 0)
                {
                    // Construct the SQL insert query
                    string sql = @"INSERT INTO tblDesignation (DesignationName, DesgnCode, Status, createdon, createdby) 
                       VALUES (@DesignationName, @DesgnCode, @Status, @createdon, @createdby);";

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
                else
                {
                    string sql = @"UPDATE tblDesignation 
                       SET DesgnCode = @DesgnCode, DesignationName = @DesignationName, Status = @Status,
                       modifiedon = @modifiedon, modifiedby = @modifiedby
                       WHERE DesgnID = @DesgnID";
                    request.modifiedon = DateTime.Now;
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

            }
            catch (SqlException ex) when (ex.Number == 2627 || ex.Number == 2601) // SQL error numbers for unique key violation
            {
                return new ServiceResponse<string>(false, "Designation already exists.", string.Empty, StatusCodes.Status409Conflict);
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
        public async Task<ServiceResponse<List<Designation>>> GetDesignationListMasters()
        {
            try
            {
                string sql = "SELECT * FROM tblDesignation where Status = 1";

                var designations = await _connection.QueryAsync<Designation>(sql);

                if (designations.Any())
                {
                    return new ServiceResponse<List<Designation>>(true, "Records Found", designations.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Designation>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<Designation>>> GetDesignationList(GetAllDesignationsRequest request)
        {
            try
            {
                string sql = "SELECT * FROM tblDesignation";

                var designations = await _connection.QueryAsync<Designation>(sql);
                var paginatedList = designations.Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();
                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<Designation>>(true, "Records Found", paginatedList.AsList(), 200, designations.Count());
                }
                else
                {
                    return new ServiceResponse<List<Designation>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Designation>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var designation = await GetDesignationByID(id);

                if (designation.Data != null)
                {
                    designation.Data.Status = !designation.Data.Status;

                    string sql = "UPDATE tblDesignation SET Status = @Status WHERE DesgnID = @DesgnID";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { designation.Data.Status, DesgnID = id });
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
