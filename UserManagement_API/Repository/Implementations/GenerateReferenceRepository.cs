using System.Data;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;
using Dapper;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.Response;
using System.Text;

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
                        Password = await GenerateRandomPassword()
                    };
                    int insertedId = await _connection.ExecuteScalarAsync<int>(insertQuery, newReference);
                    if (insertedId > 0)
                    {
                        var records = await GenerateAndSaveReferralLinks(insertedId,request.NumberOfRef);
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
                                ACNo = request.GenRefBankdetail != null ? request.GenRefBankdetail.ACNo : 0,
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
        private async Task<int> GenerateAndSaveReferralLinks(int referenceLinkID, int numberOfReferrals)
        {
            try
            {
                // Check if there are existing records for the given referenceLinkID
                string existingRecordsQuery = @"
        SELECT COUNT(*) 
        FROM tblReferralLinks 
        WHERE referenceLinkID = @referenceLinkID";

                int existingRecordsCount = await _connection.QueryFirstOrDefaultAsync<int>(existingRecordsQuery, new { referenceLinkID });

                if (existingRecordsCount > 0)
                {
                    // If records exist, we can append new entries starting from the next available serial number
                    string maxSerialQuery = @"
            SELECT MAX(CAST(SUBSTRING(ReferralCode, 4, LEN(ReferralCode) - 3) AS INT)) 
            FROM tblReferralLinks 
            WHERE referenceLinkID = @referenceLinkID";

                    int maxSerialNumber = await _connection.QueryFirstOrDefaultAsync<int>(maxSerialQuery, new { referenceLinkID });
                    maxSerialNumber = maxSerialNumber >= 10001 ? maxSerialNumber : 10000;

                    return await GenerateAndInsertReferralLinks(referenceLinkID, numberOfReferrals, maxSerialNumber + 1);
                }
                else
                {
                    // If no records exist, start with serial number 10001
                    return await GenerateAndInsertReferralLinks(referenceLinkID, numberOfReferrals, 10001);
                }
            }
            catch (Exception ex)
            {
                // Log the exception (if logging is implemented)
                return 0; // Return 0 to indicate failure
            }
        }
        private async Task<int> GenerateAndInsertReferralLinks(int referenceLinkID, int numberOfReferrals, int startSerialNumber)
        {
            var referralLinks = new List<ReferralLinks>();

            for (int i = 0; i < numberOfReferrals; i++)
            {
                int serialNumber = startSerialNumber + i;
                string referralCode = $"iGP{serialNumber}";
                string referralLink = $"https://iguruprep.2024.link/{referralCode}";

                // Check if the referral code or link already exists
                string checkQuery = @"
        SELECT COUNT(*) 
        FROM tblReferralLinks 
        WHERE ReferralCode = @referralCode OR ReferralLink = @referralLink";

                int existingCount = await _connection.QueryFirstOrDefaultAsync<int>(checkQuery, new { referralCode, referralLink });

                if (existingCount == 0) // Only add if unique
                {
                    referralLinks.Add(new ReferralLinks
                    {
                        referenceLinkID = referenceLinkID,
                        ReferralCode = referralCode,
                        ReferralLink = referralLink
                    });
                }
            }

            // Insert the generated referral links into the database
            if (referralLinks.Count > 0)
            {
                string insertQuery = @"
        INSERT INTO tblReferralLinks (referenceLinkID, ReferralCode, ReferralLink)
        VALUES (@referenceLinkID, @ReferralCode, @ReferralLink);";

                int rowsAffected = await _connection.ExecuteAsync(insertQuery, referralLinks);

                return rowsAffected; // Return the number of rows inserted as a success indicator
            }
            else
            {
                return 0; // Return 0 if no unique links were generated
            }
        }
        private async Task<string> GenerateRandomPassword()
        {
            var random = new Random();
            string password = GenerateRandomString(4, random) + GenerateRandomNumber(4, random);

            while (await IfPasswordExistsAsync(password))
            {
                password = GenerateRandomString(4, random) + GenerateRandomNumber(4, random);
            }

            return password;
        }

        private string GenerateRandomString(int length, Random random)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private string GenerateRandomNumber(int length, Random random)
        {
            const string numbers = "0123456789";
            return new string(Enumerable.Repeat(numbers, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private async Task<bool> IfPasswordExistsAsync(string password)
        {
            string query = @"
    SELECT COUNT(*)
    FROM tblReferenceLinkMaster
    WHERE Password = @Password";

            int count = await _connection.QueryFirstOrDefaultAsync<int>(query, new { Password = password });
            return count > 0;
        }
    }
}