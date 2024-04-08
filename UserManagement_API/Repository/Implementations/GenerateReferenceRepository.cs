using System.Data;
using UserManagement_API.DTOs;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;
using Dapper;

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
        INSERT INTO tblReferenceLinkMaster (InstitutionName, InstitutionCode, InstitutionBranchName, InstitutionBranchCode,
                                   MobileNo, EmailID, StateId, DistrictID, PAN, ReferenceID, PersonName)
        VALUES (@InstitutionName, @InstitutionCode, @InstitutionBranchName, @InstitutionBranchCode,
                @MobileNo, @EmailID, @StateId, @DistrictID, @PAN, @ReferenceID, @PersonName);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    var newReference = new GenerateReference
                    {
                        DistrictID = request.DistrictID,
                        EmailID = request.EmailID,
                        InstitutionBranchCode = request.InstitutionBranchCode,
                        InstitutionBranchName = request.InstitutionBranchName,
                        InstitutionCode = request.InstitutionCode,
                        InstitutionName = request.InstitutionName,
                        MobileNo = request.MobileNo,
                        PAN = request.PAN,
                        PersonName = request.PersonName,
                        ReferenceID = request.ReferenceID,
                        StateId = request.StateId
                    };
                    int insertedId = await _connection.ExecuteScalarAsync<int>(insertQuery, newReference);
                    if (insertedId > 0)
                    {
                        string insertBankQuery = @"
        INSERT INTO tblRefererBankDetails (InstitutionName, referenceLinkID, BankName, ACNo, IFSC, ReferenceID)
        VALUES (@InstitutionName, @referenceLinkID, @BankName, @ACNo, @IFSC, @ReferenceID);";
                        if(request.GenRefBankdetail != null)
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
        SET InstitutionName = @InstitutionName, 
            InstitutionCode = @InstitutionCode, 
            InstitutionBranchName = @InstitutionBranchName, 
            InstitutionBranchCode = @InstitutionBranchCode, 
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
                        InstitutionBranchCode = request.InstitutionBranchCode,
                        InstitutionBranchName = request.InstitutionBranchName,
                        InstitutionCode = request.InstitutionCode,
                        InstitutionName = request.InstitutionName,
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
        SET InstitutionName = @InstitutionName,
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
                                BankName = request.GenRefBankdetail != null ? request.GenRefBankdetail.BankName : string.Empty,
                                IFSC = request.GenRefBankdetail != null ? request.GenRefBankdetail.IFSC : string.Empty,
                                InstitutionName = request.GenRefBankdetail != null ? request.GenRefBankdetail.InstitutionName : string.Empty,
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
        INSERT INTO tblRefererBankDetails (InstitutionName, referenceLinkID, BankName, ACNo, IFSC, ReferenceID)
        VALUES (@InstitutionName, @referenceLinkID, @BankName, @ACNo, @IFSC, @ReferenceID);";
                            if (request.GenRefBankdetail != null)
                            {
                                request.GenRefBankdetail.referenceLinkID = request.referenceLinkID;
                            }
                            else
                            {                            ;
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
        public async Task<ServiceResponse<GenerateReferenceDTO>> GetGenerateReferenceById(int GenerateReferenceID)
        {
            try
            {
                GenerateReferenceDTO resposne = new GenerateReferenceDTO();

                string selectQuery = @"
        SELECT *
        FROM tblReferenceLinkMaster
        WHERE referenceLinkID = @referenceLinkID;";
                var data = await _connection.QueryFirstOrDefaultAsync<GenerateReference>(selectQuery, new { referenceLinkID = GenerateReferenceID });
                if (data != null)
                {
                    resposne.referenceLinkID = data.referenceLinkID;
                    resposne.ReferenceID = data.ReferenceID;
                    resposne.InstitutionName = data.InstitutionName;
                    resposne.InstitutionCode = data.InstitutionCode;
                    resposne.InstitutionBranchName = data.InstitutionBranchName;
                    resposne.InstitutionBranchCode = data.InstitutionBranchCode;
                    resposne.MobileNo = data.MobileNo;
                    resposne.EmailID = data.EmailID;
                    resposne.StateId = data.StateId;
                    resposne.DistrictID = data.DistrictID;
                    resposne.PAN = data.PAN;
                    resposne.PersonName = data.PersonName;

                    string selectBankQuery = @" SELECT * FROM tblRefererBankDetails WHERE referenceLinkID = @referenceLinkID;";
                    var data1 = await _connection.QuerySingleOrDefaultAsync<GenRefBankDetail>(selectBankQuery, new { data.referenceLinkID });
                    if (data1 != null)
                    {
                        resposne.GenRefBankdetail = data1;
                        return new ServiceResponse<GenerateReferenceDTO>(true, "record found", resposne, 200);
                    }
                    else
                    {
                        resposne.GenRefBankdetail = new GenRefBankDetail();
                        return new ServiceResponse<GenerateReferenceDTO>(true, "record found", resposne, 200);
                    }
                }
                else
                {
                    return new ServiceResponse<GenerateReferenceDTO>(false, "No record found", new GenerateReferenceDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<GenerateReferenceDTO>(false, ex.Message, new GenerateReferenceDTO(), 500);
            }
        }
        public async Task<ServiceResponse<List<GenerateReferenceListDTO>>> GetGenerateReferenceList()
        {
            try
            {
                List<GenerateReferenceListDTO> resposne = [];
                string selectQuery = @"SELECT * FROM tblReferenceLinkMaster";
                var data = await _connection.QueryAsync<GenerateReference>(selectQuery);
                if (data.Count() > 0)
                {
                    foreach (var item in data)
                    {
                        var record = new GenerateReferenceListDTO
                        {
                           InstitutionCode = item.InstitutionCode,
                           InstitutionName = item.InstitutionName,
                           referenceLinkID = item.referenceLinkID
                        };
                        resposne.Add(record);
                    }
                    return new ServiceResponse<List<GenerateReferenceListDTO>>(true, "Records found", resposne, 500);
                }
                else
                {
                    return new ServiceResponse<List<GenerateReferenceListDTO>>(false, "Records not found", new List<GenerateReferenceListDTO>(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateReferenceListDTO>>(false, ex.Message, new List<GenerateReferenceListDTO>(), 500);
            }
        }
    }
}
