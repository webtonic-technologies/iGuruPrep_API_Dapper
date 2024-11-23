using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Repository.Interfaces;
using System.Data;

namespace StudentApp_API.Repository.Implementations
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public RegistrationRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }

        public async Task<ServiceResponse<int>> AddRegistrationAsync(RegistrationRequest request)
        {
            try
            {
                string query = @"
                    INSERT INTO tblRegistration 
                    (FirstName, LastName, CountryCodeID, MobileNumber, EmailID, Password, CountryID, Location, ReferralCode, StateId
                     SchoolCode, RegistrationDate, IsActive, IsTermsAgreed, Photo) 
                    VALUES 
                    (@FirstName, @LastName, @CountryCodeID, @MobileNumber, @EmailID, @Password, @CountryID, @Location, 
                     @ReferralCode, @StateId, @SchoolCode, GETDATE(), 1, @IsTermsAgreed, @Photo);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
                request.Password = EncryptionHelper.EncryptString(request.Password);
                request.Photo = ImageUpload(request.Photo);
                var registrationId = await _connection.ExecuteScalarAsync<int>(query, request);

                // Use the parameterized constructor correctly
                return new ServiceResponse<int>(true, "Registration successful.", registrationId, 201);
            }
            catch (Exception ex)
            {
                // Ensure that all parameters are passed correctly
                return new ServiceResponse<int>(false, ex.Message, 0, 500);
            }
        }
        public async Task<ServiceResponse<SendOTPResponse>> SendOTPAsync(SendOTPRequest request)
        {
            try
            {
                // Generate a random OTP
                var otp = new Random().Next(100000, 999999).ToString();

                // Update the OTP in the database
                string updateQuery = @"UPDATE tblRegistration 
                                       SET OTP = @OTP 
                                       WHERE RegistrationID = @RegistrationID";

                var result = await _connection.ExecuteAsync(updateQuery, new { OTP = otp, request.RegistrationID });

                if (result > 0)
                {
                    // Simulate sending the OTP (in production, integrate with SMS service)
                    var response = new SendOTPResponse
                    {
                        RegistrationID = request.RegistrationID,
                        OTP = otp,
                        IsOTPSent = true
                    };

                    return new ServiceResponse<SendOTPResponse>(true, "OTP sent successfully.", response, 200);
                }

                return new ServiceResponse<SendOTPResponse>(false, "Failed to send OTP.", null, 400);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SendOTPResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<VerifyOTPResponse>> VerifyOTPAsync(VerifyOTPRequest request)
        {
            try
            {
                // Check if the OTP matches the one stored in the database
                string query = @"SELECT OTP 
                                 FROM tblRegistration 
                                 WHERE RegistrationID = @RegistrationID";

                var storedOTP = await _connection.QueryFirstOrDefaultAsync<string>(query, new { request.RegistrationID });

                if (storedOTP == null)
                {
                    return new ServiceResponse<VerifyOTPResponse>(false, "Registration not found.", null, 404);
                }

                if (storedOTP == request.OTP)
                {
                    return new ServiceResponse<VerifyOTPResponse>(true, "OTP verified successfully.", new VerifyOTPResponse
                    {
                        RegistrationID = request.RegistrationID,
                        IsVerified = true,
                        Message = "OTP verified successfully."
                    }, 200);
                }

                return new ServiceResponse<VerifyOTPResponse>(false, "Invalid OTP.", new VerifyOTPResponse
                {
                    RegistrationID = request.RegistrationID,
                    IsVerified = false,
                    Message = "Invalid OTP."
                }, 400);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<VerifyOTPResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                // Query to fetch encrypted password and other user details
                string query = @"SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location, Password 
                         FROM tblRegistration 
                         WHERE EmailID = @EmailID AND IsActive = 1";

                // Fetch the user details including the encrypted password
                var userWithPassword = await _connection.QueryFirstOrDefaultAsync<dynamic>(query, new { request.EmailID });

                if (userWithPassword != null)
                {
                    // Extract encrypted password from the result
                    string encryptedPassword = userWithPassword.Password;

                    // Decrypt the password from the database
                    string decryptedPassword = EncryptionHelper.DecryptString(encryptedPassword);

                    // Compare the decrypted password with the input password
                    if (decryptedPassword == request.Password)
                    {
                        // Create a response object without the password
                        var loginResponse = new LoginResponse
                        {
                            RegistrationID = userWithPassword.RegistrationID,
                            FirstName = userWithPassword.FirstName,
                            LastName = userWithPassword.LastName,
                            EmailID = userWithPassword.EmailID,
                            MobileNumber = userWithPassword.MobileNumber,
                            Location = userWithPassword.Location,
                            IsLoginSuccessful = true
                        };

                        return new ServiceResponse<LoginResponse>(true, "Login successful.", loginResponse, 200);
                    }

                    return new ServiceResponse<LoginResponse>(false, "Invalid email or password.", null, 401);
                }

                return new ServiceResponse<LoginResponse>(false, "User not found.", null, 404);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<AssignCourseResponse>> AssignCourseAsync(AssignCourseRequest request)
        {
            try
            {
                // Insert the mapping into tblStudentClassCourseMapping without associating a ClassID (initially NULL)
                string insertQuery = @"INSERT INTO tblStudentClassCourseMapping (RegistrationID, CourseID, ClassID) 
                               VALUES (@RegistrationID, @CourseID, NULL);
                               SELECT CAST(SCOPE_IDENTITY() as int);";

                var sccmId = await _connection.ExecuteScalarAsync<int>(insertQuery, new
                {
                    request.RegistrationID,
                    request.CourseID
                });

                if (sccmId > 0)
                {
                    var response = new AssignCourseResponse
                    {
                        RegistrationID = request.RegistrationID,
                        CourseID = request.CourseID,
                        IsAssigned = true,
                        Message = "Course assigned successfully."
                    };

                    return new ServiceResponse<AssignCourseResponse>(true, "Course assigned successfully.", response, 200);
                }

                return new ServiceResponse<AssignCourseResponse>(false, "Failed to assign course.", null, 400);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AssignCourseResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<AssignClassResponse>> AssignClassAsync(AssignClassRequest request)
        {
            try
            {
                // Update the ClassID based on the RegistrationID and CourseID
                string updateQuery = @"UPDATE tblStudentClassCourseMapping
                                       SET ClassID = @ClassID
                                       WHERE RegistrationID = @RegistrationID AND CourseID = @CourseID";

                var rowsAffected = await _connection.ExecuteAsync(updateQuery, new
                {
                    request.RegistrationID,
                    request.CourseID,
                    request.ClassID
                });

                if (rowsAffected > 0)
                {
                    var response = new AssignClassResponse
                    {
                        RegistrationID = request.RegistrationID,
                        CourseID = request.CourseID,
                        ClassID = request.ClassID,
                        IsClassAssigned = true,
                        Message = "Class assigned successfully."
                    };

                    return new ServiceResponse<AssignClassResponse>(true, "Class assigned successfully.", response, 200);
                }

                return new ServiceResponse<AssignClassResponse>(false, "No record found to update.", null, 404);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AssignClassResponse>(false, ex.Message, null, 500);
            }
        }
        public async Task<ServiceResponse<int>> AddUpdateProfile(UpdateProfileRequest request)
        {
            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            using (var transaction = _connection.BeginTransaction()) // Use a transaction to ensure atomicity
            {
                try
                {
                    // Skip tblRegistration update/insert logic completely
                    int registrationId = request.RegistrationID;  // Assume RegistrationID is passed in request

                    // Now, handle parent information (insert or update based on ParentID)
                    if (request.ParentRequests != null && request.ParentRequests.Count > 0)
                    {
                        foreach (var parent in request.ParentRequests)
                        {
                            // Check if parent info already exists for the given RegistrationID
                            string checkParentQuery = @"
                        SELECT COUNT(1) 
                        FROM tblParentsInfo 
                        WHERE ParentID = @ParentID AND RegistrationID = @RegistrationID";

                            var parentExists = await _connection.ExecuteScalarAsync<int>(checkParentQuery, new
                            {
                                parent.ParentID,
                                RegistrationID = registrationId
                            }, transaction);

                            if (parentExists > 0)
                            {
                                // Update parent information if record exists
                                string updateParentQuery = @"
                        UPDATE tblParentsInfo
                        SET ParentType = @ParentType,
                            MobileNo = @MobileNo,
                            EmailID = @EmailID
                        WHERE ParentID = @ParentID AND RegistrationID = @RegistrationID";

                                await _connection.ExecuteAsync(updateParentQuery, new
                                {
                                    parent.ParentID,
                                    parent.ParentType,
                                    parent.MobileNo,
                                    parent.EmailID,
                                    RegistrationID = registrationId
                                }, transaction);
                            }
                            else
                            {
                                // Insert new parent information if record doesn't exist
                                string insertParentQuery = @"
                        INSERT INTO tblParentsInfo (ParentType, MobileNo, EmailID, RegistrationID)
                        VALUES (@ParentType, @MobileNo, @EmailID, @RegistrationID)";

                                await _connection.ExecuteAsync(insertParentQuery, new
                                {
                                    parent.ParentType,
                                    parent.MobileNo,
                                    parent.EmailID,
                                    RegistrationID = registrationId
                                }, transaction);
                            }
                        }
                    }

                    transaction.Commit();  // Commit the transaction
                    return new ServiceResponse<int>(true, "Parent information updated successfully.", registrationId, 200);
                }
                catch (Exception ex)
                {
                    transaction.Rollback();  // Rollback in case of an error
                    return new ServiceResponse<int>(false, ex.Message, 0, 500);
                }
            }
        }
        public async Task<ServiceResponse<RegistrationDTO>> GetRegistrationByIdAsync(int registrationId)
        {
            var queryRegistration = @"
        SELECT RegistrationID, FirstName, LastName, CountryCodeID, MobileNumber, EmailID, Password, 
               CountryID, StatusID, Location, ReferralCode, SchoolCode, RegistrationDate, IsActive, StateId
               IsTermsAgreed, Photo, OTP
        FROM tblRegistration
        WHERE RegistrationID = @RegistrationID";

            var queryParents = @"
        SELECT ParentID, ParentType, MobileNo, EmailID, RegistrationID
        FROM tblParentsInfo
        WHERE RegistrationID = @RegistrationID";

            if (_connection.State != ConnectionState.Open)
            {
                _connection.Open();
            }

            try
            {
                // Fetch registration details
                var registration = await _connection.QueryFirstOrDefaultAsync<RegistrationDTO>(queryRegistration, new { RegistrationID = registrationId });
                registration.Photo = GetImageFileById(registration.Photo);
                if (registration == null)
                {
                    return new ServiceResponse<RegistrationDTO>(false, "Registration not found.", null, 404);
                }

                // Fetch parent details
                var parents = await _connection.QueryAsync<ParentDTO>(queryParents, new { RegistrationID = registrationId });

                registration.Parents = parents.ToList();

                return new ServiceResponse<RegistrationDTO>(true, "Success", registration, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<RegistrationDTO>(false, ex.Message, null, 500);
            }
        }
        private string ImageUpload(string image)
        {
            if (string.IsNullOrEmpty(image) || image == "string")
            {
                return string.Empty;
            }
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Registration");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileExtension = IsJpeg(imageData) == true ? ".jpg" : IsPng(imageData) == true ? ".png" : IsGif(imageData) == true ? ".gif" : string.Empty;
            string fileName = Guid.NewGuid().ToString() + fileExtension;
            string filePath = Path.Combine(directoryPath, fileName);
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new InvalidOperationException("Incorrect file uploaded");
            }
            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string GetImageFileById(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Registration", Filename);

            if (!File.Exists(filePath))
            {
                return string.Empty;
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private bool IsJpeg(byte[] bytes)
        {
            // JPEG magic number: 0xFF, 0xD8
            return bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xD8;
        }
        private bool IsPng(byte[] bytes)
        {
            // PNG magic number: 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A
            return bytes.Length > 7 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47
                && bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        }
        private bool IsGif(byte[] bytes)
        {
            // GIF magic number: "GIF"
            return bytes.Length > 2 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46;
        }
    }
}
