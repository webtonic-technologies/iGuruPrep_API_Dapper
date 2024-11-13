using Dapper;
using StudentApp_API.DTOs.Requests;
using StudentApp_API.DTOs.Responses;
using StudentApp_API.DTOs.ServiceResponse;
using StudentApp_API.Models;
using StudentApp_API.Repository.Interfaces;
using System;
using System.Data;
using System.Threading.Tasks;

namespace StudentApp_API.Repository.Implementations
{
    public class RegistrationRepository : IRegistrationRepository
    {
        private readonly IDbConnection _connection;

        public RegistrationRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<ServiceResponse<int>> AddRegistrationAsync(RegistrationRequest request)
        {
            try
            {
                string query = @"
                    INSERT INTO tblRegistration 
                    (FirstName, LastName, CountryCodeID, MobileNumber, EmailID, Password, CountryID, Location, ReferralCode, 
                     SchoolCode, RegistrationDate, IsActive, IsTermsAgreed, Photo) 
                    VALUES 
                    (@FirstName, @LastName, @CountryCodeID, @MobileNumber, @EmailID, @Password, @CountryID, @Location, 
                     @ReferralCode, @SchoolCode, GETDATE(), 1, @IsTermsAgreed, @Photo);
                    SELECT CAST(SCOPE_IDENTITY() as int)";

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
                // Query to verify email and password
                string query = @"SELECT RegistrationID, FirstName, LastName, EmailID, MobileNumber, Location
                                 FROM tblRegistration 
                                 WHERE EmailID = @EmailID AND Password = @Password AND IsActive = 1";

                var user = await _connection.QueryFirstOrDefaultAsync<LoginResponse>(query, new { request.EmailID, request.Password });

                if (user != null)
                {
                    user.IsLoginSuccessful = true;
                    return new ServiceResponse<LoginResponse>(true, "Login successful.", user, 200);
                }

                return new ServiceResponse<LoginResponse>(false, "Invalid email or password.", null, 401);
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
               CountryID, StatusID, Location, ReferralCode, SchoolCode, RegistrationDate, IsActive, 
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
    }
}
