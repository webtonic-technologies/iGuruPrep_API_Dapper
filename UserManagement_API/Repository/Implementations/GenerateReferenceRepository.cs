using System.Data;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;
using Dapper;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
using System.Text;
using OfficeOpenXml;
using System.Data.Common;

namespace UserManagement_API.Repository.Implementations
{
    public class GenerateReferenceRepository : IGenerateReferenceRepository
    {
        private readonly IDbConnection _connection;

        public GenerateReferenceRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> AddUpdateGenerateReference(GenerateReferenceDTO request)
        {
            try
            {
                if (request.referenceLinkID == 0)
                {
                    string insertQuery = @"
                    INSERT INTO tblReferenceLinkMaster (MobileNo, EmailID, StateId, DistrictID, PAN, ReferenceID, PersonName, NumberOfRef, Username, Password)
                    VALUES (@MobileNo, @EmailID, @StateId, @DistrictID, @PAN, @ReferenceID, @PersonName, @NumberOfRef, @Username, @Password);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    string generatedPassword = "iGuruPrep@" + request.MobileNo.Substring(Math.Max(0, request.MobileNo.Length - 5));
                    var newReference = new GenerateReference
                    {
                        DistrictID = request.DistrictID,
                        EmailID = request.EmailID,
                        NumberOfRef = request.NumberOfRef,
                        MobileNo = request.MobileNo,
                        PAN = request.PAN,
                        PersonName = request.PersonName,
                        ReferenceID = request.ReferenceID,
                        StateId = request.StateId,
                        Username = request.EmailID,
                        Password = generatedPassword
                    };
                    int insertedId = await _connection.ExecuteScalarAsync<int>(insertQuery, newReference);
                    if (insertedId > 0)
                    {
                        var records = await GenerateAndSaveReferralLinks(insertedId, request.NumberOfRef);
                        string insertBankQuery = @"
                        INSERT INTO tblRefererBankDetails (referenceLinkID, BankName, ACNo, IFSC, ReferenceID, BranchName)
                        VALUES (@referenceLinkID, @BankName, @ACNo, @IFSC, @ReferenceID, @BranchName);";
                        if (request.GenRefBankdetail != null)
                            request.GenRefBankdetail.referenceLinkID = insertedId;
                        var rowsAffected = await _connection.ExecuteAsync(insertBankQuery, request.GenRefBankdetail);
                        if (rowsAffected > 0)
                        {
                            return new ServiceResponse<string>(true, "Operation Successful", "Reference Generated Successfully.", 200);
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
                    UPDATE tblReferenceLinkMaster 
                    SET NumberOfRef =  @NumberOfRef,
                        MobileNo = @MobileNo, 
                        EmailID = @EmailID, 
                        StateId = @StateId, 
                        DistrictID = @DistrictID, 
                        PAN = @PAN,
                        ReferenceID = @ReferenceID, 
                        PersonName = @PersonName 
                    WHERE referenceLinkID = @referenceLinkID;";
                    var newReference = new GenerateReference
                    {
                        referenceLinkID = request.referenceLinkID,
                        DistrictID = request.DistrictID,
                        EmailID = request.EmailID,
                        NumberOfRef = request.NumberOfRef,
                        MobileNo = request.MobileNo,
                        PAN = request.PAN,
                        PersonName = request.PersonName,
                        ReferenceID = request.ReferenceID,
                        StateId = request.StateId
                    };
                    int rowsAffected = await _connection.ExecuteAsync(updateQuery, newReference);
                    if (rowsAffected > 0)
                    {
                        string selectQuery = @" SELECT * FROM tblRefererBankDetails WHERE referenceLinkID = @referenceLinkID;";
                        var data = await _connection.QuerySingleOrDefaultAsync<GenRefBankDetail>(selectQuery, new { request.referenceLinkID });
                        if (data != null)
                        {
                            string updateBankQuery = @"
                            UPDATE tblRefererBankDetails
                            SET BranchName = @BranchName,
                                referenceLinkID = @referenceLinkID,
                                BankName = @BankName,
                                ACNo = @ACNo,
                                IFSC = @IFSC,
                                ReferenceID = @ReferenceID
                            WHERE refBankID = @refBankID;";
                            var bankDetail = new GenRefBankDetail
                            {
                                referenceLinkID = request.referenceLinkID,
                                ACNo = request.GenRefBankdetail != null ? request.GenRefBankdetail.ACNo : string.Empty,
                                BankName = request.GenRefBankdetail != null ? request.GenRefBankdetail.BankName : 0,
                                IFSC = request.GenRefBankdetail != null ? request.GenRefBankdetail.IFSC : string.Empty,
                                BranchName = request.GenRefBankdetail != null ? request.GenRefBankdetail.BranchName : string.Empty,
                                ReferenceID = request.GenRefBankdetail != null ? request.GenRefBankdetail.ReferenceID : 0,
                                refBankID = data.refBankID
                            };
                            int rowsAffected2 = await _connection.ExecuteAsync(updateBankQuery, bankDetail);
                            if (rowsAffected2 > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Reference Generated Successfully.", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                            }
                        }
                        else
                        {
                            string insertBankQuery = @"
                            INSERT INTO tblRefererBankDetails (BranchName, referenceLinkID, BankName, ACNo, IFSC, ReferenceID)
                            VALUES (@BranchName, @referenceLinkID, @BankName, @ACNo, @IFSC, @ReferenceID);";
                            if (request.GenRefBankdetail != null)
                            {
                                request.GenRefBankdetail.referenceLinkID = request.referenceLinkID;
                            }
                            else
                            {
                                ;
                            }
                            var rowsAffected1 = await _connection.ExecuteAsync(insertBankQuery, request.GenRefBankdetail);
                            if (rowsAffected1 > 0)
                            {
                                return new ServiceResponse<string>(true, "Operation Successful", "Reference Generated Successfully.", 200);
                            }
                            else
                            {
                                return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                            }
                        }
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Some error occured", string.Empty, 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<GenerateReferenceResponseDTO>> GetGenerateReferenceById(int GenerateReferenceID)
        {
            try
            {
                GenerateReferenceResponseDTO response = new();

                string selectQuery = @"
                SELECT 
                    gl.*, 
                    s.StateName, 
                    d.DistrictName
                FROM tblReferenceLinkMaster gl
                LEFT JOIN tblStateName s ON gl.StateId = s.StateId
                LEFT JOIN tblDistricts d ON gl.DistrictID = d.DistrictID
                WHERE gl.referenceLinkID = @referenceLinkID;";

                var data = await _connection.QueryFirstOrDefaultAsync<dynamic>(selectQuery, new { referenceLinkID = GenerateReferenceID });

                if (data != null)
                {
                    response.referenceLinkID = data.referenceLinkID;
                    response.ReferenceID = data.ReferenceID;
                    response.StateName = data.StateName;
                    response.DistrictName = data.DistrictName;
                    response.NumberOfRef = data.NumberOfRef;
                    response.MobileNo = data.MobileNo;
                    response.EmailID = data.EmailID;
                    response.StateId = data.StateId;
                    response.DistrictID = data.DistrictID;
                    response.PAN = data.PAN;
                    response.PersonName = data.PersonName;

                    string selectBankQuery = @"SELECT rb.*,
                                             rb.BankName as BankId,
                                             b.BankName as BankName
                                             FROM tblRefererBankDetails rb 
                                             LEFT JOIN tblBank b ON rb.BankName = b.BankId
                                             WHERE referenceLinkID = @referenceLinkID;";
                    var data1 = await _connection.QuerySingleOrDefaultAsync<GenRefBankDetailResponse>(selectBankQuery, new { referenceLinkID = data.referenceLinkID });

                    response.GenRefBankdetail = data1 ?? new GenRefBankDetailResponse();
                    string referralDetailsQuery = @"
                    SELECT 
                        [RefLinksId], [referenceLinkID], [ReferralCode], [ReferralLink]
                    FROM [tblReferralLinks]
                    WHERE [referenceLinkID] = @referenceLinkID;";
                    var referralDetails = await _connection.QueryAsync<ReferenceLinksResposne>(referralDetailsQuery, new { referenceLinkID = GenerateReferenceID });
                    response.ReferenceLinksResposnes = referralDetails.AsList();
                    return new ServiceResponse<GenerateReferenceResponseDTO>(true, "Record found", response, 200);
                }
                else
                {
                    return new ServiceResponse<GenerateReferenceResponseDTO>(false, "No record found", new GenerateReferenceResponseDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateReferenceResponseDTO>(false, ex.Message, new GenerateReferenceResponseDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateReferenceResponseDTO>>> GetGenerateReferenceList(GetAllReferralsRequest request)
        {
            try
            {
                List<GenerateReferenceResponseDTO> response = [];
                var queryBuilder = new StringBuilder();
                queryBuilder.Append(@"
                SELECT 
                    gl.*, 
                    s.StateName, 
                    d.DistrictName
                FROM tblReferenceLinkMaster gl
                LEFT JOIN tblStateName s ON gl.StateId = s.StateId
                LEFT JOIN tblDistricts d ON gl.DistrictID = d.DistrictID
                WHERE 1 = 1");

                var parameters = new DynamicParameters();

                // Add filters based on DTO properties
                if (request.StateId > 0)
                {
                    queryBuilder.Append(" AND gl.StateId = @StateId");
                    parameters.Add("StateId", request.StateId);
                }
                if (request.District > 0)
                {
                    queryBuilder.Append(" AND gl.DistrictID = @DistrictID");
                    parameters.Add("DistrictID", request.District);
                }
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    queryBuilder.Append(" AND (gl.PersonName LIKE @SearchText)");
                    parameters.Add("SearchText", "%" + request.SearchText + "%");
                }

                // Execute the query
                var data = await _connection.QueryAsync<dynamic>(queryBuilder.ToString(), parameters);

                if (data.Any())
                {
                    foreach (var item in data)
                    {
                        string selectBankQuery = @"SELECT rb.*,
                                             rb.BankName as BankId,
                                             b.BankName as BankName
                                             FROM tblRefererBankDetails rb 
                                             LEFT JOIN tblBank b ON rb.BankName = b.BankId
                                             WHERE referenceLinkID = @referenceLinkID;";
                        var bankDetails = await _connection.QuerySingleOrDefaultAsync<GenRefBankDetailResponse>(selectBankQuery, new { item.referenceLinkID });

                        var record = new GenerateReferenceResponseDTO
                        {
                            referenceLinkID = item.referenceLinkID,
                            ReferenceID = item.ReferenceID,
                            StateName = item.StateName,
                            DistrictName = item.DistrictName,
                            NumberOfRef = item.NumberOfRef,
                            MobileNo = item.MobileNo,
                            EmailID = item.EmailID,
                            StateId = item.StateId,
                            DistrictID = item.DistrictID,
                            PAN = item.PAN,
                            PersonName = item.PersonName,
                            GenRefBankdetail = bankDetails
                        };

                        response.Add(record);
                    }

                    var paginatedList = response
                        .Skip((request.PageNumber - 1) * request.PageSize)
                        .Take(request.PageSize)
                        .ToList();

                    return new ServiceResponse<List<GenerateReferenceResponseDTO>>(true, "Records found", paginatedList, 200, response.Count);
                }
                else
                {
                    return new ServiceResponse<List<GenerateReferenceResponseDTO>>(false, "Records not found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateReferenceResponseDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<Bank>>> GetBankListMasters()
        {
            try
            {
                var Query = @"Select * from [tblBank]";
                // Execute the query
                var data = await _connection.QueryAsync<Bank>(Query);

                if (data.Any())
                {
                    return new ServiceResponse<List<Bank>>(true, "Records found", data.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Bank>>(false, "Records not found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Bank>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<States>>> GetStatesListMasters()
        {
            try
            {
                var Query = @"Select * from [tblStateName]";
                // Execute the query
                var data = await _connection.QueryAsync<States>(Query);

                if (data.Any())
                {
                    return new ServiceResponse<List<States>>(true, "Records found", data.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<States>>(false, "Records not found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<States>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<List<Districts>>> GetDistrictsListMasters(int StateID)
        {
            try
            {
                var Query = @"Select * from [tblDistricts] where [StateID] = @StateID";
                // Execute the query
                var data = await _connection.QueryAsync<Districts>(Query, new { StateID });

                if (data.Any())
                {
                    return new ServiceResponse<List<Districts>>(true, "Records found", data.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<Districts>>(false, "Records not found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<Districts>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<byte[]>> DownloadExcelFile(int referenceLinkID)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                // Query to fetch login details
                string loginDetailsQuery = @"
        SELECT 
            [referenceLinkID], [MobileNo], [EmailID], [StateId], 
            [DistrictID], [PAN], [ReferenceID], [PersonName], 
            [NumberOfRef], [Username], [Password]
        FROM [tblReferenceLinkMaster]
        WHERE [referenceLinkID] = @referenceLinkID;";

                // Query to fetch referral details
                string referralDetailsQuery = @"
        SELECT 
            [RefLinksId], [referenceLinkID], [ReferralCode], [ReferralLink]
        FROM [tblReferralLinks]
        WHERE [referenceLinkID] = @referenceLinkID;";

                // Fetch data from database
                var loginDetails = await _connection.QuerySingleOrDefaultAsync<dynamic>(loginDetailsQuery, new { referenceLinkID });
                var referralDetails = await _connection.QueryAsync<dynamic>(referralDetailsQuery, new { referenceLinkID });

                if (loginDetails == null || !referralDetails.Any())
                {
                    return new ServiceResponse<byte[]>(false, "No data found for the provided referenceLinkID.", null, 404);
                }

                using (var package = new ExcelPackage())
                {
                    // Create the Login Details sheet
                    var loginSheet = package.Workbook.Worksheets.Add("LoginDetails");

                    // Define headers for LoginDetails sheet
                    var loginHeaders = new[] { "Person Name", "Mobile No", "Email ID", "Number Of Ref", "Username", "Password" };
                    for (int i = 0; i < loginHeaders.Length; i++)
                    {
                        loginSheet.Cells[1, i + 1].Value = loginHeaders[i];
                    }

                    // Map login details data to cells
                    loginSheet.Cells[2, 1].Value = loginDetails.PersonName;
                    loginSheet.Cells[2, 2].Value = loginDetails.MobileNo;
                    loginSheet.Cells[2, 3].Value = loginDetails.EmailID;
                    loginSheet.Cells[2, 4].Value = loginDetails.NumberOfRef;
                    loginSheet.Cells[2, 5].Value = loginDetails.Username;
                    loginSheet.Cells[2, 6].Value = loginDetails.Password;

                    // Create the Referral Details sheet
                    var referralSheet = package.Workbook.Worksheets.Add("ReferralDetails");

                    // Define headers for ReferralDetails sheet
                    var referralHeaders = new[] {"Referral Code", "Referral Link" };
                    for (int i = 0; i < referralHeaders.Length; i++)
                    {
                        referralSheet.Cells[1, i + 1].Value = referralHeaders[i];
                    }

                    // Map referral details data to cells
                    int row = 2; // Start from the second row for data
                    foreach (var referral in referralDetails)
                    {
                        referralSheet.Cells[row, 1].Value = referral.ReferralCode;
                        referralSheet.Cells[row, 2].Value = referral.ReferralLink;
                        row++;
                    }

                    // Convert package to a byte array
                    byte[] fileBytes = package.GetAsByteArray();

                    return new ServiceResponse<byte[]>(true, "Excel file generated successfully.", fileBytes, 200);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, null, 500);
            }
        }
        private async Task<int> GenerateAndSaveReferralLinks(int referenceLinkID, int numberOfReferrals)
        {
            try
            {
                // Fetch the maximum serial number for the given referenceLinkID
                string maxSerialQuery = @"
        SELECT ISNULL(MAX(CAST(SUBSTRING(ReferralCode, 4, LEN(ReferralCode) - 3) AS INT)), 10000) 
        FROM tblReferralLinks ";

                int maxSerialNumber = await _connection.QueryFirstOrDefaultAsync<int>(maxSerialQuery, new { referenceLinkID });

                // Start generating from the next serial number
                int startSerialNumber = maxSerialNumber + 1;

                // Call the method to generate and insert referral links
                return await GenerateAndInsertReferralLinks(referenceLinkID, numberOfReferrals, startSerialNumber);
            }
            catch (Exception ex)
            {
                // Log the exception if needed (ensure proper logging)
                return 0; // Return 0 indicating a failure
            }
        }
        private async Task<int> GenerateAndInsertReferralLinks(int referenceLinkID, int numberOfReferrals, int startSerialNumber)
        {
            var referralLinks = new List<ReferralLinks>();

            for (int i = 0; i < numberOfReferrals;)
            {
                int serialNumber = startSerialNumber + i;
                string referralCode = $"iGP{serialNumber}";
                string referralLink = $"https://iguruprep.2024.link/{referralCode}";

                // Check if the referral code already exists for any referenceLinkID
                string checkQuery = @"
        SELECT COUNT(*) 
        FROM tblReferralLinks 
        WHERE ReferralCode = @referralCode";

                int existingCount = await _connection.QueryFirstOrDefaultAsync<int>(checkQuery, new { referralCode });

                if (existingCount == 0) // If the code is unique, add it
                {
                    referralLinks.Add(new ReferralLinks
                    {
                        referenceLinkID = referenceLinkID,
                        ReferralCode = referralCode,
                        ReferralLink = referralLink
                    });
                    i++;
                }
                else
                {
                    // If a duplicate is found, skip this and generate a new code (adjust loop index)
                    i++;
                }
            }

            // Insert all generated referral links into the database
            if (referralLinks.Count > 0)
            {
                string insertQuery = @"
        INSERT INTO tblReferralLinks (referenceLinkID, ReferralCode, ReferralLink)
        VALUES (@referenceLinkID, @ReferralCode, @ReferralLink)";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, referralLinks);

                return rowsAffected; // Return the number of inserted rows
            }
            else
            {
                return 0; // Return 0 if no links were generated
            }
        }
    }
}