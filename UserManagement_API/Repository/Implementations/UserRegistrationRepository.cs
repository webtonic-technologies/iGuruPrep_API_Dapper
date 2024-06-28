using Dapper;
using System.Data;
using UserManagement_API.DTOs.Requests;
using UserManagement_API.DTOs.ServiceResponse;
using UserManagement_API.Models;
using UserManagement_API.Repository.Interfaces;

namespace UserManagement_API.Repository.Implementations
{
    public class UserRegistrationRepository : IUserRegistrationRepository
    {

        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserRegistrationRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> UserRegistration(UserRegistrationDto request)
        {
            try
            {
                var imageUrl = "";
                if (request.UserId == 0)
                {
                    if (request.Photo != null)
                    {
                        var folderName = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "UserProfile");
                        if (!Directory.Exists(folderName))
                        {
                            Directory.CreateDirectory(folderName);
                        }
                        var fileName = Path.GetFileNameWithoutExtension(request.Photo.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(request.Photo.FileName);
                        var filePath = Path.Combine(folderName, fileName);
                        using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await request.Photo.CopyToAsync(fileStream);
                        }
                        imageUrl = fileName;
                    }

                    string sql = @"INSERT INTO tblUser (
                            PersonType, UserCode, FirstName, MiddleName, LastName, 
                            DOB, Email, PhoneNumber, Password, ZipCode, 
                            Photo, SchoolCode, CurrentToken, System_Name, Last_Ip_Address, 
                            Login_State, Last_Login_DateTime, Last_Logout_DateTime, Require_ReLogin, Failed_Login_Attempts, 
                            Cannot_Login_Until_Date, Status, CreatedBy, CreatedOn, ModifiedBy, 
                            ModifiedOn, City, State, Country, SchoolName, 
                            FcmDeviceID, DeviceType, PaymentSucessHash, PaymentFailureHash, RoleID, DesgnID)
                      VALUES (
                            @PersonType, @UserCode, @FirstName, @MiddleName, @LastName, 
                            @DOB, @Email, @PhoneNumber, @Password, @ZipCode, 
                            @Photo, @SchoolCode, @CurrentToken, @System_Name, @Last_Ip_Address, 
                            @Login_State, @Last_Login_DateTime, @Last_Logout_DateTime, @Require_ReLogin, @Failed_Login_Attempts, 
                            @Cannot_Login_Until_Date, @Status, @CreatedBy, @CreatedOn, @ModifiedBy, 
                            @ModifiedOn, @City, @State, @Country, @SchoolName, 
                            @FcmDeviceID, @DeviceType, @PaymentSucessHash, @PaymentFailureHash, @RoleID, @DesgnID)";



                    var parameters = new UserRegistration
                    {
                        Country = request.Country,
                        Email = request.Email,
                        FirstName = request.FirstName,
                        LastName = request.LastName,
                        MiddleName = request.MiddleName,
                        ModifiedBy = request.ModifiedBy,
                        ModifiedOn = request.ModifiedOn,
                        Password = request.Password,
                        PhoneNumber = request.PhoneNumber,
                        Photo = imageUrl,
                        SchoolCode = request.SchoolCode,
                        State = request.State,
                        Status = request.Status,
                        PersonType = request.PersonType,
                        RoleID = request.RoleID,
                        CreatedOn = DateTime.Now,
                        CreatedBy = 1
                    };

                    int rowsAffected = await _connection.ExecuteAsync(sql, parameters);

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "User Registered Successfully.", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                    }
                }
                else
                {
                    var sql = "SELECT COUNT(*) FROM tblUser WHERE UserId = @UserId";
                    var count = await _connection.ExecuteScalarAsync<int>(sql, new { request.UserId });
                    if (count == 0)
                    {
                        return new ServiceResponse<string>(false, "Some Error Occured", string.Empty, 500);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation Failed", "User Already Exists.", 500);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
