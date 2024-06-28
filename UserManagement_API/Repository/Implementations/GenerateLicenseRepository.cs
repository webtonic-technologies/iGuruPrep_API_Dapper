using Dapper;
using System.Data;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
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
                    INSERT INTO [tblGenerateLicense] (SchoolName, SchoolCode, BranchName, BranchCode, ChairmanEmail, ChairmanMobile, PrincipalEmail, PrincipalMobile, StateId, DistrictId, createdon, createdby, EmployeeID)
                    VALUES (@SchoolName, @SchoolCode, @BranchName, @BranchCode, @ChairmanEmail, @ChairmanMobile, @PrincipalEmail, @PrincipalMobile, @StateId, @DistrictId, @CreatedOn, @CreatedBy, @EmployeeID);
                    SELECT SCOPE_IDENTITY();";
                    var newLicense = new GenerateLicense
                    {
                        SchoolName = request.SchoolName,
                        SchoolCode = request.SchoolCode,
                        BranchName = request.BranchName,
                        BranchCode = request.BranchCode,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        stateid = request.stateid,
                        DistrictID = request.DistrictID,
                        createdon = DateTime.Now,
                        createdby = request.createdby,
                        EmployeeID = request.EmployeeID
                    };
                    var generatedId = await _connection.QueryFirstOrDefaultAsync<int>(query, newLicense);
                    if (generatedId != 0)
                    {
                        string insertQuery = @"
                        INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID)
                        VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID);";
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
                        ChairmanEmail = @ChairmanEmail, 
                        ChairmanMobile = @ChairmanMobile, 
                        PrincipalEmail = @PrincipalEmail, 
                        PrincipalMobile = @PrincipalMobile, 
                        StateId = @StateId, 
                        DistrictId = @DistrictId,  
                        modifiedon = @ModifiedOn, 
                        modifiedby = @ModifiedBy,  
                        EmployeeID = @EmployeeID
                    WHERE GenerateLicenseID = @GenerateLicenseID;";
                    var newLicense = new GenerateLicense
                    {
                        SchoolName = request.SchoolName,
                        SchoolCode = request.SchoolCode,
                        BranchName = request.BranchName,
                        BranchCode = request.BranchCode,
                        ChairmanEmail = request.ChairmanEmail,
                        ChairmanMobile = request.ChairmanMobile,
                        PrincipalEmail = request.PrincipalEmail,
                        PrincipalMobile = request.PrincipalMobile,
                        stateid = request.stateid,
                        DistrictID = request.DistrictID,
                        modifiedon = DateTime.Now,
                        modifiedby = request.createdby,
                        EmployeeID = request.EmployeeID,
                        GenerateLicenseID = request.GenerateLicenseID
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newLicense);
                    if (rowsAffected > 0)
                    {
                        int count = await _connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM tblLicenseDetail WHERE GenerateLicenseID = @GenerateLicenseID", new { request.GenerateLicenseID });
                        if (count > 0)
                        {
                            string deleteQuery = @" DELETE FROM tblLicenseDetail WHERE GenerateLicenseID = @GenerateLicenseID";
                            int rowsAffected1 = await _connection.ExecuteAsync(deleteQuery, new { request.GenerateLicenseID });
                            if (rowsAffected1 > 0)
                            {
                                string insertQuery = @"
                                INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID)
                                VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID);";

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
                            INSERT INTO tblLicenseDetail (GenerateLicenseID, BoardID, ClassID, CourseID, NoOfLicense, ValidityID, APID)
                            VALUES (@GenerateLicenseID, @BoardID, @ClassID, @CourseID, @NoOfLicense, @ValidityID, @APID);";
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
        public async Task<ServiceResponse<GenerateLicenseResponseDTO>> GetGenerateLicenseById(int GenerateLicenseID)
        {
            try
            {
                GenerateLicenseResponseDTO response = new();
                string selectQuery = @"
                SELECT gl.*, e.EmpFirstName, s.StateName, d.DistrictName
                FROM tblGenerateLicense gl
                LEFT JOIN tblEmployee e ON gl.EmployeeID = e.Employeeid
                LEFT JOIN tblStateName s ON gl.stateid = s.StateId
                LEFT JOIN tblDistricts d ON gl.DistrictID = d.DistrictID
                WHERE gl.GenerateLicenseID = @GenerateLicenseID;";

                var generateLicense = await _connection.QueryFirstOrDefaultAsync<dynamic>(selectQuery, new { GenerateLicenseID });
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
                    response.modifiedby = generateLicense.modifiedby;
                    response.modifiedon = generateLicense.modifiedon;
                    response.createdon = generateLicense.createdon;
                    response.createdby = generateLicense.createdby;
                    response.EmployeeID = generateLicense.EmployeeID;
                    response.EmpFirstName = generateLicense.EmpFirstName;

                    string query = @"
                    SELECT ld.*, 
                           b.BoardName, 
                           c.ClassName, 
                           co.CourseName, 
                           cat.APName as categoryName, 
                           v.ValidityPeriod AS ValidityName
                    FROM tblLicenseDetail ld
                    LEFT JOIN tblBoard b ON ld.BoardID = b.BoardID
                    LEFT JOIN tblClass c ON ld.ClassID = c.ClassID
                    LEFT JOIN tblCourse co ON ld.CourseID = co.CourseID
                    LEFT JOIN tblCategory cat ON ld.APID = cat.APId
                    LEFT JOIN tblValidity v ON ld.ValidityID = v.ValidityID
                    WHERE ld.GenerateLicenseID = @GenerateLicenseID;";

                    var data = await _connection.QueryAsync<LicenseDetailResponse>(query, new { GenerateLicenseID });
                    response.LicenseDetails = data != null ? data.AsList() : [];

                    return new ServiceResponse<GenerateLicenseResponseDTO>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<GenerateLicenseResponseDTO>(false, "Record not found", new GenerateLicenseResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateLicenseResponseDTO>(false, ex.Message, new GenerateLicenseResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateLicenseResponseDTO>>> GetGenerateLicenseList(GetAllLicensesListRequest request)
        {
            try
            {
                string countSql = @"SELECT COUNT(*) FROM [tblGenerateLicense]";
                int totalCount = await _connection.ExecuteScalarAsync<int>(countSql);
                List<GenerateLicenseResponseDTO> responseList = [];

                // Main query to fetch GenerateLicense data with related names
                string mainQuery = @"
                SELECT gl.*, e.EmpFirstName, s.StateName, d.DistrictName
                FROM tblGenerateLicense gl
                LEFT JOIN tblEmployee e ON gl.EmployeeID = e.Employeeid
                LEFT JOIN tblStateName s ON gl.stateid = s.StateId
                LEFT JOIN tblDistricts d ON gl.DistrictID = d.DistrictID
                WHERE 1 = 1";

                var mainParameters = new DynamicParameters();

                // Add filters based on DTO properties
                if (request.District > 0)
                {
                    mainQuery += " AND gl.DistrictID = @District";
                    mainParameters.Add("District", request.District);
                }
                if (request.StateId > 0)
                {
                    mainQuery += " AND gl.stateid = @StateId";
                    mainParameters.Add("StateId", request.StateId);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    mainQuery += " AND gl.SchoolName LIKE @SearchText";
                    mainParameters.Add("SearchText", "%" + request.SearchText + "%");
                }

                var generateLicenses = await _connection.QueryAsync<dynamic>(mainQuery, mainParameters);

                if (generateLicenses != null)
                {
                    // Detail query to fetch LicenseDetail data with related names
                    string detailQuery = @"
                    SELECT ld.*, 
                           b.BoardName, 
                           c.ClassName, 
                           co.CourseName, 
                           cat.APName as categoryName, 
                           v.ValidityPeriod AS ValidityName
                    FROM tblLicenseDetail ld
                    LEFT JOIN tblBoard b ON ld.BoardID = b.BoardID
                    LEFT JOIN tblClass c ON ld.ClassID = c.ClassID
                    LEFT JOIN tblCourse co ON ld.CourseID = co.CourseID
                    LEFT JOIN tblCategory cat ON ld.APID = cat.APId
                    LEFT JOIN tblValidity v ON ld.ValidityID = v.ValidityID
                    WHERE ld.GenerateLicenseID = @GenerateLicenseID;";
                    foreach (var item in generateLicenses)
                    {
                        var licenseDetails = await _connection.QueryAsync<LicenseDetailResponse>(detailQuery, new { item.GenerateLicenseID });

                        var record = new GenerateLicenseResponseDTO
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
                            modifiedby = item.modifiedby,
                            modifiedon = item.modifiedon,
                            createdon = item.createdon,
                            createdby = item.createdby,
                            EmployeeID = item.EmployeeID,
                            EmpFirstName = item.EmpFirstName,
                            LicenseDetails = licenseDetails.AsList()
                        };

                        responseList.Add(record);
                    }

                    // Apply pagination
                    var paginatedList = responseList
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList();

                    return new ServiceResponse<List<GenerateLicenseResponseDTO>>(true, "Records found", paginatedList, 200, totalCount);
                }
                else
                {
                    return new ServiceResponse<List<GenerateLicenseResponseDTO>>(false, "Records not found", [], 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateLicenseResponseDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}
