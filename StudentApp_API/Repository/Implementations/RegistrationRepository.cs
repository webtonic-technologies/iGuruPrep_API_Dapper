using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
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
        public async Task<ServiceResponse<List<StateResponse>>> GetStatesByCountryId(int countryId)
        {
            try
            {
                // SQL query to fetch states by countryId
                string query = "SELECT StateId, StateName FROM tblStateName WHERE CountryId = @CountryId";

                var states = await _connection.QueryAsync<StateResponse>(query, new { CountryId = countryId });

                if (states.Any())
                {
                    return new ServiceResponse<List<StateResponse>>(true, "States retrieved successfully.", states.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<StateResponse>>(false, "No states found for the given country.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<StateResponse>>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<List<CountryResponse>>> GetCountries()
        {
            try
            {
                // SQL query to fetch countries and their country codes
                string query = "SELECT CountryId, CountryName, CountryCode FROM tblCountries";

                var countries = await _connection.QueryAsync<CountryResponse>(query);

                if (countries.Any())
                {
                    return new ServiceResponse<List<CountryResponse>>(true, "Countries retrieved successfully.", countries.ToList(), 200);
                }
                else
                {
                    return new ServiceResponse<List<CountryResponse>>(false, "No countries found.", null, 404);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<CountryResponse>>(false, ex.Message, null, 500);
            }
        }

        public async Task<ServiceResponse<int>> AddRegistrationAsync(RegistrationRequest request)
        {
            try
            {
                // Check if terms and conditions are agreed
                if (!request.IsTermsAgreed)
                {
                    return new ServiceResponse<int>(false, "Registration failed. You must agree to the terms and conditions to proceed.", 0, 400);
                }

                // Check if the user is already registered
                string checkQuery = @"
            SELECT COUNT(*)
            FROM tblRegistration
            WHERE MobileNumber = @MobileNumber AND EmailID = @EmailID AND IsOTPVerified = 1";

                var isUserRegistered = await _connection.ExecuteScalarAsync<int>(checkQuery, new
                {
                    MobileNumber = request.MobileNumber,
                    EmailID = request.EmailID
                });

                if (isUserRegistered > 0)
                {
                    return new ServiceResponse<int>(false, "Registration failed. User already registered with the provided email and mobile number.", 0, 409);
                }

                // Insert the registration details
                string insertQuery = @"
            INSERT INTO tblRegistration 
            (FirstName, LastName, CountryCodeID, MobileNumber, EmailID, Password, CountryID, Location, ReferralCode, StateId, 
             SchoolCode, RegistrationDate, IsActive, IsTermsAgreed, Photo, IsOTPVerified, IsBoardClassCourseSelected) 
            VALUES 
            (@FirstName, @LastName, @CountryCodeID, @MobileNumber, @EmailID, @Password, @CountryID, @Location, @ReferralCode, 
             @StateId, @SchoolCode, GETDATE(), 1, @IsTermsAgreed, @Photo, 0, 0);
            SELECT CAST(SCOPE_IDENTITY() as int)";

                // Encrypt the password
                request.Password = EncryptionHelper.EncryptString(request.Password);

                // Upload the photo and get the file path
                request.Photo = ImageUpload(request.Photo);

                // Execute the query
                var registrationId = await _connection.ExecuteScalarAsync<int>(insertQuery, request);

                return new ServiceResponse<int>(true, "Registration successful.", registrationId, 201);
            }
            catch (Exception ex)
            {
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
                // Fetch the stored OTP, email, and mobile number for the given registration ID
                string fetchQuery = @"
            SELECT OTP, EmailID, MobileNumber
            FROM tblRegistration
            WHERE RegistrationID = @RegistrationID";

                var record = await _connection.QueryFirstOrDefaultAsync<(string OTP, string EmailID, string MobileNumber)>(fetchQuery, new { request.RegistrationID });

                if (record == default)
                {
                    return new ServiceResponse<VerifyOTPResponse>(false, "Registration not found.", null, 404);
                }

                // Check if any other record exists with the same email and phone but IsOTPVerified = 0
                string checkDuplicateQuery = @"
            SELECT RegistrationID
            FROM tblRegistration
            WHERE EmailID = @EmailID AND MobileNumber = @MobileNumber AND IsOTPVerified = 0 AND RegistrationID <> @RegistrationID";

                var duplicateRegistrationIDs = await _connection.QueryAsync<int>(checkDuplicateQuery, new
                {
                    record.EmailID,
                    record.MobileNumber,
                    request.RegistrationID
                });

                // Perform hard delete if duplicates are found
                if (duplicateRegistrationIDs.Any())
                {
                    string deleteQuery = @"DELETE FROM tblRegistration WHERE RegistrationID IN @RegistrationIDs";
                    await _connection.ExecuteAsync(deleteQuery, new { RegistrationIDs = duplicateRegistrationIDs });
                }

                // Verify the OTP
                if (record.OTP == request.OTP)
                {
                    string updateQuery = @"UPDATE tblRegistration SET IsOTPVerified = 1 WHERE RegistrationID = @RegistrationID";
                    await _connection.ExecuteAsync(updateQuery, new { request.RegistrationID });

                    return new ServiceResponse<VerifyOTPResponse>(true, "OTP verified successfully.", new VerifyOTPResponse
                    {
                        RegistrationID = request.RegistrationID,
                        IsVerified = true,
                        Message = "OTP verified successfully."
                    }, 200);
                }

                // Return response for invalid OTP
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
        public async Task<ServiceResponse<string>> DeviceCapture(DeviceCaptureRequest request)
        {
            try
            {
                // Define the SQL insert query
                string query = @"
            INSERT INTO [tblDeviceCapture] (
                RegistrationId,
                device,
                fingerprint,
                model,
                serialNumber,
                type,
                version_sdkInt,
                version_securityPatch,
                id_buildId,
                isPhysicalDevice,
                systemName,
                systemVersion,
                utsname_version,
                name,
                browserName,
                appName,
                appVersion,
                deviceMemory,
                Platform,
                kernelVersion,
                computerName,
                systemGUID
            ) VALUES (
                @RegistrationId,
                @device,
                @fingerprint,
                @model,
                @serialNumber,
                @type,
                @version_sdkInt,
                @version_securityPatch,
                @id_buildId,
                @isPhysicalDevice,
                @systemName,
                @systemVersion,
                @utsname_version,
                @name,
                @browserName,
                @appName,
                @appVersion,
                @deviceMemory,
                @Platform,
                @kernelVersion,
                @computerName,
                @systemGUID);";

                // Execute the insert query
                var result = await _connection.ExecuteAsync(query, new
                {
                    request.RegistrationId,
                    request.device,
                    request.fingerprint,
                    request.model,
                    request.serialNumber,
                    request.type,
                    request.version_sdkInt,
                    request.version_securityPatch,
                    request.id_buildId,
                    request.isPhysicalDevice,
                    request.systemName,
                    request.systemVersion,
                    request.utsname_version,
                    request.name,
                    request.browserName,
                    request.appName,
                    request.appVersion,
                    request.deviceMemory,
                    request.Platform,
                    request.kernelVersion,
                    request.computerName,
                    request.systemGUID
                });

                // Check if the insert was successful
                if (result > 0)
                {
                    return new ServiceResponse<string>(true, "Device capture recorded successfully", string.Empty, StatusCodes.Status201Created);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Failed to record device capture", string.Empty, StatusCodes.Status500InternalServerError);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, StatusCodes.Status500InternalServerError);
            }
        }
        public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        {
            try
            {
                string input = request.EmailIDOrPhoneNumberOrLicense;
                string password = request.Password;
                RegistrationRequest userWithPassword = null;
                bool isLicenseLogin = false;
                string schoolCode = null;

                // Determine the type of login
                if (IsEmail(input))
                {
                    // Login via email
                    string userQuery = @"
                SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location, Password, CountryCodeID, StateId, CountryID, IsEmailAccess
                FROM tblRegistration 
                WHERE EmailID = @Input AND IsActive = 1";

                    userWithPassword = await _connection.QueryFirstOrDefaultAsync<RegistrationRequest>(userQuery, new { Input = input });

                    if (userWithPassword != null)
                    {
                        // Decrypt and validate password (case insensitive comparison)
                        string decryptedPassword = EncryptionHelper.DecryptString(userWithPassword.Password);
                        if (!decryptedPassword.Equals(password, StringComparison.OrdinalIgnoreCase))
                        {
                            return new ServiceResponse<LoginResponse>(false, "Invalid email or password.", null, 401);
                        }

                        // Mark email login access
                        string updateQuery = "UPDATE tblRegistration SET IsEmailAccess = 1 WHERE RegistrationID = @RegistrationID";
                        await _connection.ExecuteAsync(updateQuery, new { RegistrationID = userWithPassword.RegistrationId });
                    }
                }
                else if (IsPhoneNumber(input))
                {
                    // Login via phone number
                    string userQuery = @"
                SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location, Password, CountryCodeID, StateId, CountryID, IsMobileAccess
                FROM tblRegistration 
                WHERE MobileNumber = @Input AND IsActive = 1";

                    userWithPassword = await _connection.QueryFirstOrDefaultAsync<RegistrationRequest>(userQuery, new { Input = input });

                    if (userWithPassword != null)
                    {
                        // Decrypt and validate password (case insensitive comparison)
                        string decryptedPassword = EncryptionHelper.DecryptString(userWithPassword.Password);
                        if (!decryptedPassword.Equals(password, StringComparison.OrdinalIgnoreCase))
                        {
                            return new ServiceResponse<LoginResponse>(false, "Invalid phone number or password.", null, 401);
                        }

                        // Mark mobile login access
                        string updateQuery = "UPDATE tblRegistration SET IsMobileAccess = 1 WHERE RegistrationID = @RegistrationID";
                        await _connection.ExecuteAsync(updateQuery, new { RegistrationID = userWithPassword.RegistrationId });
                    }
                }
                else
                {
                    // Login via license number
                    string licenseQuery = @"
                SELECT ln.LicenseNumbersId, ln.LicensePassword, ld.GenerateLicenseID, gl.SchoolCode
                FROM tblLicenseNumbers ln
                INNER JOIN tblLicenseDetail ld ON ln.LicenseDetailID = ld.LicenseDetailID
                INNER JOIN tblGenerateLicense gl ON ld.GenerateLicenseID = gl.GenerateLicenseID
                WHERE ln.LicenseNo = @Input";

                    var licenseDetails = await _connection.QueryFirstOrDefaultAsync<dynamic>(licenseQuery, new { Input = input });

                    if (licenseDetails != null && licenseDetails.LicensePassword.Equals(password, StringComparison.OrdinalIgnoreCase))
                    {
                        isLicenseLogin = true;
                        schoolCode = licenseDetails.SchoolCode;

                        // Update registration with SchoolCode and license access
                        string updateQuery = @"
                    UPDATE tblRegistration 
                    SET IsLicenceAccess = 1, SchoolCode = @SchoolCode 
                    WHERE RegistrationID = @RegistrationID";

                        await _connection.ExecuteAsync(updateQuery, new
                        {
                            SchoolCode = schoolCode,
                            RegistrationID = userWithPassword?.RegistrationId
                        });
                    }
                    else
                    {
                        return new ServiceResponse<LoginResponse>(false, "Invalid license number or password.", null, 401);
                    }
                }

                // If no valid login found
                if (userWithPassword == null && !isLicenseLogin)
                {
                    return new ServiceResponse<LoginResponse>(false, "User not found.", null, 404);
                }

                // Calculate profile percentage for non-license login
                decimal profilePercentage = 0;
                if (!isLicenseLogin && userWithPassword != null)
                {
                    profilePercentage = await CalculateProfilePercentageAsync(userWithPassword.RegistrationId, userWithPassword);
                }

                // Create response object
                var loginResponse = new LoginResponse
                {
                    RegistrationID = userWithPassword?.RegistrationId ?? 0,
                    FirstName = userWithPassword?.FirstName,
                    LastName = userWithPassword?.LastName,
                    EmailID = userWithPassword?.EmailID,
                    MobileNumber = userWithPassword?.MobileNumber,
                    Location = userWithPassword?.Location,
                    IsLoginSuccessful = true,
                    ProfilePercentage = isLicenseLogin ? null : $"{profilePercentage}%"
                };

                return new ServiceResponse<LoginResponse>(true, "Login successful.", loginResponse, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponse>(false, ex.Message, null, 500);
            }
        }
        //public async Task<ServiceResponse<LoginResponse>> LoginAsync(LoginRequest request)
        //{
        //    try
        //    {
        //        string input = request.EmailIDOrPhoneNumberOrLicense;
        //        string password = request.Password;
        //        RegistrationRequest userWithPassword = null;
        //        bool isLicenseLogin = false;
        //        string schoolCode = null;

        //        // Determine the type of login
        //        if (IsEmail(input))
        //        {
        //            // Login via email
        //            string userQuery = @"
        //    SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location, Password, CountryCodeID, StateId, CountryID, IsEmailAccess
        //    FROM tblRegistration 
        //    WHERE EmailID = @Input AND IsActive = 1";

        //            userWithPassword = await _connection.QueryFirstOrDefaultAsync<RegistrationRequest>(userQuery, new { Input = input });

        //            if (userWithPassword != null)
        //            {
        //                // Decrypt and validate password
        //                string decryptedPassword = EncryptionHelper.DecryptString(userWithPassword.Password);
        //                if (decryptedPassword != password)
        //                {
        //                    return new ServiceResponse<LoginResponse>(false, "Invalid email or password.", null, 401);
        //                }

        //                // Mark email login access
        //                string updateQuery = "UPDATE tblRegistration SET IsEmailAccess = 1 WHERE RegistrationID = @RegistrationID";
        //                await _connection.ExecuteAsync(updateQuery, new { RegistrationID = userWithPassword.RegistrationId });
        //            }
        //        }
        //        else if (IsPhoneNumber(input))
        //        {
        //            // Login via phone number
        //            string userQuery = @"
        //    SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location, Password, CountryCodeID, StateId, CountryID, IsMobileAccess
        //    FROM tblRegistration 
        //    WHERE MobileNumber = @Input AND IsActive = 1";

        //            userWithPassword = await _connection.QueryFirstOrDefaultAsync<RegistrationRequest>(userQuery, new { Input = input });

        //            if (userWithPassword != null)
        //            {
        //                // Decrypt and validate password
        //                string decryptedPassword = EncryptionHelper.DecryptString(userWithPassword.Password);
        //                if (decryptedPassword != password)
        //                {
        //                    return new ServiceResponse<LoginResponse>(false, "Invalid phone number or password.", null, 401);
        //                }

        //                // Mark mobile login access
        //                string updateQuery = "UPDATE tblRegistration SET IsMobileAccess = 1 WHERE RegistrationID = @RegistrationID";
        //                await _connection.ExecuteAsync(updateQuery, new { RegistrationID = userWithPassword.RegistrationId });
        //            }
        //        }
        //        else
        //        {
        //            // Login via license number
        //            string licenseQuery = @"
        //    SELECT ln.LicenseNumbersId, ln.LicensePassword, ld.GenerateLicenseID, gl.SchoolCode
        //    FROM tblLicenseNumbers ln
        //    INNER JOIN tblLicenseDetail ld ON ln.LicenseDetailID = ld.LicenseDetailID
        //    INNER JOIN tblGenerateLicense gl ON ld.GenerateLicenseID = gl.GenerateLicenseID
        //    WHERE ln.LicenseNo = @Input";

        //            var licenseDetails = await _connection.QueryFirstOrDefaultAsync<dynamic>(licenseQuery, new { Input = input });

        //            if (licenseDetails != null && licenseDetails.LicensePassword == password)
        //            {
        //                isLicenseLogin = true;
        //                schoolCode = licenseDetails.SchoolCode;

        //                // Update registration with SchoolCode and license access
        //                string updateQuery = @"
        //        UPDATE tblRegistration 
        //        SET IsLicenceAccess = 1, SchoolCode = @SchoolCode 
        //        WHERE RegistrationID = @RegistrationID";

        //                await _connection.ExecuteAsync(updateQuery, new
        //                {
        //                    SchoolCode = schoolCode,
        //                    RegistrationID = userWithPassword?.RegistrationId
        //                });
        //            }
        //            else
        //            {
        //                return new ServiceResponse<LoginResponse>(false, "Invalid license number or password.", null, 401);
        //            }
        //        }

        //        // If no valid login found
        //        if (userWithPassword == null && !isLicenseLogin)
        //        {
        //            return new ServiceResponse<LoginResponse>(false, "User not found.", null, 404);
        //        }

        //        // Calculate profile percentage for non-license login
        //        decimal profilePercentage = 0;
        //        if (!isLicenseLogin && userWithPassword != null)
        //        {
        //            profilePercentage = await CalculateProfilePercentageAsync(userWithPassword.RegistrationId, userWithPassword);
        //        }

        //        // Create response object
        //        var loginResponse = new LoginResponse
        //        {
        //            RegistrationID = userWithPassword?.RegistrationId ?? 0,
        //            FirstName = userWithPassword?.FirstName,
        //            LastName = userWithPassword?.LastName,
        //            EmailID = userWithPassword?.EmailID,
        //            MobileNumber = userWithPassword?.MobileNumber,
        //            Location = userWithPassword?.Location,
        //            IsLoginSuccessful = true,
        //            ProfilePercentage = isLicenseLogin ? null : $"{profilePercentage}%"
        //        };

        //        return new ServiceResponse<LoginResponse>(true, "Login successful.", loginResponse, 200);
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ServiceResponse<LoginResponse>(false, ex.Message, null, 500);
        //    }
        //}
        private async Task<decimal> CalculateProfilePercentageAsync(int registrationId, RegistrationRequest userWithPassword)
        {
            // Determine the country type
            bool isIndian = userWithPassword.CountryCodeID == "India"; // Assuming 1 is for India

            // Query to fetch parent info
            string parentQuery = @"
    SELECT ParentType, MobileNo, ParentEmailID
    FROM tblParentsInfo 
    WHERE RegistrationID = @RegistrationID";

            var parents = (await _connection.QueryAsync<ParentsInfo>(parentQuery, new { RegistrationID = registrationId })).ToList();

            // Separate parents by type
            var father = parents.FirstOrDefault(p => string.Equals(p.ParentType, "Father", StringComparison.OrdinalIgnoreCase));
            var mother = parents.FirstOrDefault(p => string.Equals(p.ParentType, "Mother", StringComparison.OrdinalIgnoreCase));

            // Initialize total and completed weights
            int totalWeight = 100; // The total weight is 100
            int completedWeight = 0;

            // Loop through the fields and check if they are valid for the user or the parent
            if (IsFieldValid(userWithPassword, "FirstName")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "LastName")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "MobileNumber")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "EmailID")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "Password")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "CountryID")) completedWeight += 10;
            if (IsFieldValid(userWithPassword, "StateId")) completedWeight += 5;
            if (IsFieldValid(userWithPassword, "Location")) completedWeight += 5;

            // Parent-related fields
            if (father != null)
            {
                if (IsFieldValid(father, "ParentType")) completedWeight += 5;
                if (IsFieldValid(father, "MobileNo")) completedWeight += 5;
                if (IsFieldValid(father, "ParentEmailID")) completedWeight += 5;
            }

            if (mother != null)
            {
                if (IsFieldValid(mother, "ParentType")) completedWeight += 5;
                if (IsFieldValid(mother, "MobileNo")) completedWeight += 5;
                if (IsFieldValid(mother, "ParentEmailID")) completedWeight += 5;
            }

            // Calculate and return the percentage
            return Math.Round((decimal)completedWeight / totalWeight * 100, 2);
        }
        private bool IsFieldValid(object parent, string fieldName)
        {
            // This is just a placeholder. You need to implement the actual validation logic
            // based on how the fields are structured (e.g., checking if the field is not null or empty).
            var propertyValue = parent.GetType().GetProperty(fieldName)?.GetValue(parent);
            return propertyValue != null && !string.IsNullOrEmpty(propertyValue.ToString());
        }
        public async Task<ServiceResponse<List<ClassCourseMappingResponse>>> GetAllClassCoursesMappings(GetAllClassCourseRequest request)
        {
            try
            {
                string query = @"
        SELECT 
            cc.CourseClassMappingID,
            cc.CourseID,
            cc.ClassID,
            cc.Status,
            cc.createdon,
            cc.EmployeeID,
            e.EmpFirstName AS EmpFirstName,
            cc.modifiedon,
            cc.modifiedby,
            cl.classname,
            c.coursename
        FROM 
            tblClassCourses cc
        INNER JOIN tblClass cl ON cc.ClassID = cl.ClassID
        INNER JOIN tblCourse c ON cc.CourseID = c.CourseID
        LEFT JOIN tblEmployee e ON cc.EmployeeID = e.Employeeid";

                var classCourseMappings = await _connection.QueryAsync<dynamic>(query);

                var groupedMappings = classCourseMappings
                    .GroupBy(m => m.ClassID)
                    .Select(g => new ClassCourseMappingResponse
                    {
                        ClassID = g.Key,
                        Status = g.First().Status,
                        createdon = g.First().createdon,
                        EmployeeID = g.First().EmployeeID,
                        EmpFirstName = g.First().EmpFirstName,
                        modifiedon = g.First().modifiedon,
                        modifiedby = g.First().modifiedby,
                        classname = g.First().classname,
                        Courses = g.Select(m => new CourseData
                        {
                            CourseClassMappingID = m.CourseClassMappingID,
                            CourseID = m.CourseID,
                            Coursename = m.coursename,

                        }).ToList()
                    }).ToList();

                var paginatedList = groupedMappings
                    .Skip((request.PageNumber - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .ToList();

                if (paginatedList.Count != 0)
                {
                    return new ServiceResponse<List<ClassCourseMappingResponse>>(true, "Records Found", paginatedList, 200, groupedMappings.Count);
                }
                else
                {
                    return new ServiceResponse<List<ClassCourseMappingResponse>>(false, "Records Not Found", [], 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<ClassCourseMappingResponse>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<AssignStudentMappingResponse>> AssignStudentClassCourseBoardMapping(AssignStudentMappingRequest request)
        {
            try
            {
                // Step 1: Validate that the combination doesn't already exist
                string validationQuery = @"
            SELECT COUNT(1)
            FROM tblStudentClassCourseMapping
            WHERE RegistrationID = @RegistrationID 
              AND CourseID = @CourseID
              AND ClassID = @ClassID
              AND BoardID = @BoardID";

                var existingCount = await _connection.ExecuteScalarAsync<int>(validationQuery, new
                {
                    request.RegistrationID,
                    request.CourseID,
                    request.ClassID,
                    request.BoardID
                });

                if (existingCount > 0)
                {
                    return new ServiceResponse<AssignStudentMappingResponse>(false, "This combination of Board, Class, and Course is already assigned to the student.", null, 400);
                }

                // Step 2: Insert or Update the mapping
                string upsertQuery = @"
            MERGE tblStudentClassCourseMapping AS target
            USING (SELECT @RegistrationID AS RegistrationID, @CourseID AS CourseID, @ClassID AS ClassID, @BoardID AS BoardID) AS source
            ON target.RegistrationID = source.RegistrationID 
               AND target.CourseID = source.CourseID
            WHEN MATCHED THEN
                UPDATE SET ClassID = source.ClassID, BoardID = source.BoardID
            WHEN NOT MATCHED THEN
                INSERT (RegistrationID, CourseID, ClassID, BoardID)
                VALUES (source.RegistrationID, source.CourseID, source.ClassID, source.BoardID)
            OUTPUT INSERTED.SCCMID;";

                var sccmId = await _connection.ExecuteScalarAsync<int>(upsertQuery, new
                {
                    request.RegistrationID,
                    request.CourseID,
                    request.ClassID,
                    request.BoardID
                });

                if (sccmId > 0)
                {
                    var response = new AssignStudentMappingResponse
                    {
                        RegistrationID = request.RegistrationID,
                        CourseID = request.CourseID,
                        ClassID = request.ClassID,
                        BoardID = request.BoardID,
                        IsAssigned = true,
                        Message = "Mapping assigned successfully."
                    };
                    await _connection.ExecuteAsync(@"update tblRegistration set IsBoardClassCourseSelected = 1 where RegistrationID = @RegistrationID", new { request.RegistrationID });
                    return new ServiceResponse<AssignStudentMappingResponse>(true, "Mapping assigned successfully.", response, 200);
                }

                return new ServiceResponse<AssignStudentMappingResponse>(false, "Failed to assign mapping.", null, 400);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<AssignStudentMappingResponse>(false, ex.Message, null, 500);
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
                    int registrationId = request.RegistrationID; // Assume RegistrationID is passed in the request

                    // Update `tblRegistration` for the specified fields
                    string updateRegistrationQuery = @"
                UPDATE tblRegistration
                SET FirstName = @FirstName,
                    LastName = @LastName,
                    CountryCodeID = @CountryCodeID,
                    MobileNumber = @MobileNumber,
                    StateId = @StateId,
                    EmailID = @EmailID,
                    CountryID = @CountryID,
                    Location = @Location,
                    ReferralCode = @ReferralCode,
                    SchoolCode = @SchoolCode,
                    Photo = @Photo,
                    IsActive = @IsActive
                WHERE RegistrationID = @RegistrationID";

                    await _connection.ExecuteAsync(updateRegistrationQuery, new
                    {
                        request.FirstName,
                        request.LastName,
                        request.CountryCodeID,
                        request.MobileNumber,
                        request.StateId,
                        request.EmailID,
                        request.CountryID,
                        request.Location,
                        request.ReferralCode,
                        request.SchoolCode,
                        request.Photo,
                        request.RegistrationID,
                        request.IsActive
                    }, transaction);

                    // Handle parent information (insert or update based on ParentID)
                    if (request.ParentRequests != null && request.ParentRequests.Count > 0)
                    {
                        foreach (var parent in request.ParentRequests)
                        {
                            // Ensure both parent contact numbers are not the same
                            if (request.ParentRequests.Count > 1 &&
                                request.ParentRequests.Any(p => p.MobileNo == parent.MobileNo && p != parent))
                            {
                                throw new ArgumentException("Parent contact numbers must be unique.");
                            }

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
                                ParentEmailID = @EmailID
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
                            INSERT INTO tblParentsInfo (ParentType, MobileNo, ParentEmailID, RegistrationID)
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

                    transaction.Commit(); // Commit the transaction
                    return new ServiceResponse<int>(true, "Profile and parent information updated successfully.", registrationId, 200);
                }
                catch (Exception ex)
                {
                    transaction.Rollback(); // Rollback in case of an error
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
        SELECT ParentID, ParentType, MobileNo, ParentEmailID as EmailID, RegistrationID
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
        public async Task<ServiceResponse<string>> DeleteProfile(int registrationId)
        {
            try
            {
                string query = @"UPDATE tblUsers 
                             SET IsDeleted = 1, StatusID = 0 
                             WHERE RegistrationID = @RegistrationID AND IsDeleted = 0";

                var rowsAffected = await _connection.ExecuteAsync(query, new { RegistrationID = registrationId });

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Success", "Profile deletd successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Failure", string.Empty, 200);
                }

            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, "Failure", ex.Message, 500);
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
        private bool IsEmail(string input)
        {
            return input.Contains("@");
        }
        private bool IsPhoneNumber(string input)
        {
            return input.All(char.IsDigit) && input.Length >= 10;
        }
    }
}
