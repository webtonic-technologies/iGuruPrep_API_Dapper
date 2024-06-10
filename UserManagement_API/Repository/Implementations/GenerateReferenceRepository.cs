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
        INSERT INTO tblReferenceLinkMaster (MobileNo, EmailID, StateId, DistrictID, PAN, ReferenceID, PersonName, StateName, DistrictName, NumberOfRef)
        VALUES (@MobileNo, @EmailID, @StateId, @DistrictID, @PAN, @ReferenceID, @PersonName, @StateName, @DistrictName, @NumberOfRef);
        SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    var newReference = new GenerateReference
                    {
                        DistrictID = request.DistrictID,
                        EmailID = request.EmailID,
                        StateName = request.StateName,
                        DistrictName = request.DistrictName,
                        NumberOfRef = request.NumberOfRef,
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
        SET StateName = @StateName,
            DistrictName = @DistrictName,
            NumberOfRef =  @NumberOfRef,
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
                        StateName = request.StateName,
                        DistrictName = request.DistrictName,
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
        public async Task<ServiceResponse<GenerateReferenceDTO>> GetGenerateReferenceById(int GenerateReferenceID)
        {
            try
            {
                GenerateReferenceDTO resposne = new();

                string selectQuery = @"
        SELECT *
        FROM tblReferenceLinkMaster
        WHERE referenceLinkID = @referenceLinkID;";
                var data = await _connection.QueryFirstOrDefaultAsync<GenerateReference>(selectQuery, new { referenceLinkID = GenerateReferenceID });
                if (data != null)
                {
                    resposne.referenceLinkID = data.referenceLinkID;
                    resposne.ReferenceID = data.ReferenceID;
                    resposne.StateName = data.StateName;
                    resposne.DistrictName = data.DistrictName;
                    resposne.NumberOfRef = data.NumberOfRef;
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
        public async Task<ServiceResponse<List<GenerateReferenceDTO>>> GetGenerateReferenceList(GetAllReferralsRequest request)
        {
            try
            {
                List<GenerateReferenceDTO> resposne = [];
                string selectQuery = @"SELECT * FROM tblReferenceLinkMaster";
                var data = await _connection.QueryAsync<GenerateReference>(selectQuery);
                if (data.Count() > 0)
                {
                    foreach (var item in data)
                    {

                        string selectBankQuery = @" SELECT * FROM tblRefererBankDetails WHERE referenceLinkID = @referenceLinkID;";
                        var data1 = await _connection.QuerySingleOrDefaultAsync<GenRefBankDetail>(selectBankQuery, new { item.referenceLinkID });
                        var record = new GenerateReferenceDTO
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
                            GenRefBankdetail = data1
                        };
                        resposne.Add(record);
                    }
                    var paginatedList = resposne.Skip((request.PageNumber - 1) * request.PageSize)
                   .Take(request.PageSize)
                   .ToList();
                    return new ServiceResponse<List<GenerateReferenceDTO>>(true, "Records found", paginatedList, 500);
                }
                else
                {
                    return new ServiceResponse<List<GenerateReferenceDTO>>(false, "Records not found", [], 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<GenerateReferenceDTO>>(false, ex.Message, [], 500);
            }
        }
    }
}
