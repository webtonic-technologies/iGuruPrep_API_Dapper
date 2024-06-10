using Dapper;
using System.Data;
using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;

namespace UserManagement_API.Repository.Implementations
{
    public class GenerateLicenseRepository : IGenerateLicenseRepository
    {
        private readonly IDbConnection _connection;

        public GenerateLicenseRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateGenerateLicense(GenerateLicenseDTO request)
        {
            try
            {
                if (request.GenerateLicenseID == 0)
                {
                    string query = @"
        INSERT INTO [tblGenerateLicense] (SchoolName, SchoolCode, BranchName, BranchCode, StateName, DistrictName, ChairmanEmail, ChairmanMobile, PrincipalEmail, PrincipalMobile, StateId, DistrictId, ClassName, CourseName, createdon, createdby, EmployeeID, EmpFirstName)
        VALUES (@SchoolName, @SchoolCode, @BranchName, @BranchCode, @StateName, @DistrictName, @ChairmanEmail, @ChairmanMobile, @PrincipalEmail, @PrincipalMobile, @StateId, @DistrictId, @ClassName, @CourseName, @CreatedOn, @CreatedBy, @EmployeeID, @EmpFirstName);
        SELECT SCOPE_IDENTITY();";
                    var newLicense = new GenerateLicense
                    {
                        SchoolName = request.SchoolName,
                        SchoolCode = request.SchoolCode,
                        BranchName = request.BranchName,
                        BranchCode = request.BranchCode,
                        StateName = request.StateName,
                        DistrictName = request.DistrictName,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        stateid = request.stateid,
                        DistrictID = request.DistrictID,
                        ClassName = request.ClassName,
                        CourseName = request.CourseName,
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        EmployeeID = request.EmployeeID,
                        EmpFirstName = request.EmpFirstName
                    };
                    var generatedId = await _connection.QueryFirstOrDefaultAsync<int>(query, newLicense);
                    if (generatedId != 0)
                    {
                        string insertQuery = @"
        INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID, BoardName, ClassName, CourseName, CategoryName)
        VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID, @BoardName, @ClassName, @CourseName, @CategoryName);";
                        if (request.LicenseDetails != null)
                        {
                            foreach (var item in request.LicenseDetails)
                            {
                                item.GenerateLicenseID = generatedId;
                            }
                        }
                        int rowsAffected = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "License Generated Successfully.", 200);
                        }
                        else
                        {
                            return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
                else
                {
                    string updateQuery = @"
        UPDATE [tblGenerateLicense]
        SET SchoolName = @SchoolName, 
            SchoolCode = @SchoolCode, 
            BranchName = @BranchName, 
            BranchCode = @BranchCode, 
            StateName = @StateName, 
            DistrictName = @DistrictName, 
            ChairmanEmail = @ChairmanEmail, 
            ChairmanMobile = @ChairmanMobile, 
            PrincipalEmail = @PrincipalEmail, 
            PrincipalMobile = @PrincipalMobile, 
            StateId = @StateId, 
            DistrictId = @DistrictId, 
            ClassName = @ClassName, 
            CourseName = @CourseName, 
            modifiedon = @ModifiedOn, 
            modifiedby = @ModifiedBy,  
            EmployeeID = @EmployeeID, 
            EmpFirstName = @EmpFirstName
        WHERE GenerateLicenseID = @GenerateLicenseID;";
                    var newLicense = new GenerateLicense
                    {
                        SchoolName = request.SchoolName,
                        SchoolCode = request.SchoolCode,
                        BranchName = request.BranchName,
                        BranchCode = request.BranchCode,
                        StateName = request.StateName,
                        DistrictName = request.DistrictName,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        stateid = request.stateid,
                        DistrictID = request.DistrictID,
                        ClassName = request.ClassName,
                        CourseName = request.CourseName,
                        modifiedon = DateTime.Now,
                        modifiedby = request.createdby,
                        EmployeeID = request.EmployeeID,
                        EmpFirstName = request.EmpFirstName,
                        GenerateLicenseID = request.GenerateLicenseID
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newLicense);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblLicenseDetail WHERE GenerateLicenseID = @GenerateLicenseID", new { request.GenerateLicenseID });
                        if (count > 0)
                        {
                            string deleteQuery = @"
        DELETE FROM tblLicenseDetail
        WHERE GenerateLicenseID = @GenerateLicenseID";
                            int rowsAffected1 = await _connection.ExecuteAsync(deleteQuery, new { request.GenerateLicenseID });
                            if (rowsAffected1 > 0)
                            {
                                string insertQuery = @"
        INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID, BoardName, ClassName, CourseName, CategoryName)
        VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID, @BoardName, @ClassName, @CourseName, @CategoryName);";

                                if (request.LicenseDetails != null)
                                    foreach (var item in request.LicenseDetails)
                                    {
                                        item.GenerateLicenseID = request.GenerateLicenseID;
                                    }
                                int addedRecords = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                                if (addedRecords > 0)
                                {
                                    return new ServiceResponse<string>(true, "Operation Successful", "License Details Added Successfully", 200);
                                }
                                else
                                {
                                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                                }
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            string insertQuery = @"
        INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID, BoardName, ClassName, CourseName, CategoryName)
        VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID, @BoardName, @ClassName, @CourseName, @CategoryName);";
                            if (request.LicenseDetails != null)
                                foreach (var item in request.LicenseDetails)
                                {
                                    item.GenerateLicenseID = request.GenerateLicenseID;
                                }
                            int addedRecords = await _connection.ExecuteAsync(insertQuery, request.LicenseDetails);

                            if (addedRecords > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "License Details Added Successfully", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                            }
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<GenerateLicenseDTO>> GetGenerateLicenseById(int GenerateLicenseID)
        {
            try
            {
                GenerateLicenseDTO response = new();
                string selectQuery = @"
        SELECT * 
        FROM tblGenerateLicense
        WHERE GenerateLicenseID = @GenerateLicenseID;";
                var generateLicense = await _connection.QueryFirstOrDefaultAsync<GenerateLicense>(selectQuery, new { GenerateLicenseID });
                if (generateLicense != null)
                {
                    response.GenerateLicenseID = generateLicense.GenerateLicenseID;
                    response.SchoolName = generateLicense.SchoolName;
                    response.SchoolCode = generateLicense.SchoolCode;
                    response.BranchName = generateLicense.BranchName;
                    response.BranchCode = generateLicense.BranchCode;
                    response.StateName = generateLicense.StateName;
                    response.DistrictName = generateLicense.DistrictName;
                    response.ChairmanEmail = generateLicense.ChairmanEmail;
                    response.ChairmanMobile = generateLicense.ChairmanMobile;
                    response.PrincipalEmail = generateLicense.PrincipalEmail;
                    response.PrincipalMobile = generateLicense.PrincipalMobile;
                    response.stateid = generateLicense.stateid;
                    response.DistrictID = generateLicense.DistrictID;
                    response.ClassName = generateLicense.ClassName;
                    response.CourseName = generateLicense.CourseName;
                    response.modifiedby = generateLicense.modifiedby;
                    response.modifiedon = generateLicense.modifiedon;
                    response.createdon = generateLicense.createdon;
                    response.createdby = generateLicense.createdby;
                    response.EmployeeID = generateLicense.EmployeeID;
                    response.EmpFirstName = generateLicense.EmpFirstName;

                    string query = @"
                    SELECT * 
                    FROM tblLicenseDetail
                    WHERE GenerateLicenseID = @GenerateLicenseID;";
                    var data = await _connection.QueryAsync<LicenseDetail>(query, new { GenerateLicenseID });
                    if (data != null)
                    {
                        response.LicenseDetails = data.AsList();
                    }
                    else
                    {
                        response.LicenseDetails = [];
                    }
                    return new ServiceResponse<GenerateLicenseDTO>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<GenerateLicenseDTO>(false, "Record not found", new GenerateLicenseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateLicenseDTO>(false, ex.Message, new GenerateLicenseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateLicenseDTO>>> GetGenerateLicenseList(GetAllLicensesListRequest request)
        {
            try
            {
                List<GenerateLicenseDTO> resposne = [];
                string selectQuery = @"SELECT * FROM tblGenerateLicense";
                var data = await _connection.QueryAsync<GenerateLicense>(selectQuery);
                if (data != null)
                {
                    string query = @"
                    SELECT * 
                    FROM tblLicenseDetail
                    WHERE GenerateLicenseID = @GenerateLicenseID;";
                    foreach (var item in data)
                    {
                        var data1 = await _connection.QueryAsync<LicenseDetail>(query, new { item.GenerateLicenseID });
                        var record = new GenerateLicenseDTO
                        {
                            GenerateLicenseID = item.GenerateLicenseID,
                            SchoolName = item.SchoolName,
                            SchoolCode = item.SchoolCode,
                            BranchName = item.BranchName,
                            BranchCode = item.BranchCode,
                            StateName = item.StateName,
                            DistrictName = item.DistrictName,
                            ChairmanEmail = item.ChairmanEmail,
                            ChairmanMobile = item.ChairmanMobile,
                            PrincipalEmail = item.PrincipalEmail,
                            PrincipalMobile = item.PrincipalMobile,
                            stateid = item.stateid,
                            DistrictID = item.DistrictID,
                            ClassName = item.ClassName,
                            CourseName = item.CourseName,
                            modifiedby = item.modifiedby,
                            modifiedon = item.modifiedon,
                            createdon = item.createdon,
                            createdby = item.createdby,
                            EmployeeID = item.EmployeeID,
                            EmpFirstName = item.EmpFirstName,
                            LicenseDetails = data1.AsList()
                        };
                        resposne.Add(record);
                    }
                    var paginatedList = resposne.Skip((request.PageNumber - 1) * request.PageSize)
                     .Take(request.PageSize)
                     .ToList();
                    return new ServiceResponse<List<GenerateLicenseDTO>>(true, "Records found", paginatedList, 500);
                }
                else
                {
                    return new ServiceResponse<List<GenerateLicenseDTO>>(false, "Records not found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateLicenseDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}
