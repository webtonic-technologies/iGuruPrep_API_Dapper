using ControlPanel_API.DTOs;
using ControlPanel_API.DTOs.ServiceResponse;
using ControlPanel_API.Models;
using ControlPanel_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace ControlPanel_API.Repository.Implementations
{
    public class MagazineRepository : IMagazineRepository
    {
        private readonly IDbConnection _connection;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public MagazineRepository(IDbConnection connection, IWebHostEnvironment hostingEnvironment)
        {
            _connection = connection;
            _hostingEnvironment = hostingEnvironment;
        }
        public async Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO request)
        {
            try
            {
                var magazine = new Magazine
                {
                  createdby = request.createdby,
                  createdon = DateTime.Now,
                  EmpFirstName = request.EmpFirstName,
                  Link = PDFUpload(request.Link),
                  MagazineTitle = request.MagazineTitle,
                  PathURL = ImageUpload(request.PathURL),
                  Status = true,
                  EmployeeID = request.EmployeeID,
                  Time = request.Time
                };
                string sql = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazine] 
                   ([Date], [Time], [PathURL], [MagazineTitle], [Status], [Link], [EmployeeID], [EmpFirstName], [createdon], [createdby]) 
                   VALUES 
                   (GETDATE(), @Time, @PathURL, @MagazineTitle, @Status, @Link, @EmployeeID, @EmpFirstName, GETDATE(), @createdby)";
                int insertedValue = await _connection.QueryFirstOrDefaultAsync<int>(sql, magazine);

                if (insertedValue > 0)
                {
                    int category = MagazineCategoryMapping(request.MagazineCategories ??= ([]), insertedValue);
                    int classes = MagazineClassMapping(request.MagazineClasses ??= ([]), insertedValue);
                    int board = MagazineBoardMapping(request.MagazineBoards ??= ([]), insertedValue);
                    int course = MagazineCourseMapping(request.MagazineCourses ??= ([]), insertedValue);
                    int exam = MagazineExamTypeMapping(request.MagazineExamTypes ??= ([]), insertedValue);
                    if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "SOTD Added Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<string>> UpdateMagazine(MagazineDTO request)
        {
            try
            {
                string query = @"UPDATE [iGuruPrep].[dbo].[tblMagazine] 
                   SET [MagazineTitle] = @MagazineTitle, [PathURL] = @PathURL, [Link] = @Link, [EmployeeID] = @EmployeeID, [EmpFirstName] = @EmpFirstName, [modifiedon] = GETDATE(), [modifiedby] = @modifiedby
                   WHERE [MagazineId] = @MagazineId";
                var magazine = new Magazine
                {
                    modifiedby = request.modifiedby,
                    modifiedon = DateTime.Now,
                    EmpFirstName = request.EmpFirstName,
                    Link = PDFUpload(request.Link),
                    MagazineTitle = request.MagazineTitle,
                    PathURL = ImageUpload(request.PathURL),
                    Status = true,
                    EmployeeID = request.EmployeeID,
                    Time = request.Time,
                    MagazineId = request.MagazineId
                };
                int rowsAffected = await _connection.ExecuteAsync(query, magazine);
                if (rowsAffected > 0)
                {
                    int category = MagazineCategoryMapping(request.MagazineCategories ??= ([]), request.MagazineId);
                    int classes = MagazineClassMapping(request.MagazineClasses ??= ([]), request.MagazineId);
                    int board = MagazineBoardMapping(request.MagazineBoards ??= ([]), request.MagazineId);
                    int course = MagazineCourseMapping(request.MagazineCourses ??= ([]), request.MagazineId);
                    int exam = MagazineExamTypeMapping(request.MagazineExamTypes ??= ([]), request.MagazineId);
                    if (category > 0 && classes > 0 && board > 0 && course > 0 && exam > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "SOTD updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Operation failed", string.Empty, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", string.Empty, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
        public async Task<ServiceResponse<bool>> DeleteMagazine(int id)
        {
            try
            {
                var magazine = await _connection.QueryFirstOrDefaultAsync<Magazine>(
                    "SELECT * FROM tblMagazine WHERE MagazineId = @MagazineId",
                    new { MagazineId = id });

                if (magazine == null)
                    throw new Exception("Magazine not found");

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine", magazine.PathURL);
                var filePath1 = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "MagazinePDF", magazine.Link);
                if (File.Exists(filePath) || File.Exists(filePath1))
                {
                    File.Delete(filePath1);
                    File.Delete(filePath);
                }

                int rowsAffected = await _connection.ExecuteAsync(
                     "DELETE FROM tblMagazine WHERE MagazineId = @MagazineId",
                     new { MagazineId = id });
                if (rowsAffected > 0)
                {
                    return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        public async Task<ServiceResponse<List<MagazineDTO>>> GetAllMagazines(MagazineListDTO request)
        {
            try
            {
                var response = new List<MagazineDTO>();
                string query = @"SELECT [MagazineId], [Date], [Time], [PathURL], [MagazineTitle], [Status], [Link], [EmployeeID], [EmpFirstName], [createdon], [createdby], [modifiedby], [modifiedon]
                   FROM [iGuruPrep].[dbo].[tblMagazine]
                   WHERE 1 = 1";

                if (request.APID == 1)
                {
                    if (request.BoardId != 0)
                        query += " AND BoardId = @BoardId";
                    if (request.ClassId != 0)
                        query += " AND ClassId = @ClassId";
                    if (request.CourseId != 0)
                        query += " AND CourseId = @CourseId";
                    if (request.EventTypeId != 0)
                        query += " AND EventTypeId = @EventTypeId";

                    var Magazines = await _connection.QueryAsync<Magazine>(query, request);
                    if (Magazines != null)
                    {
                        foreach (var data in Magazines)
                        {
                            var item = new MagazineDTO
                            {
                                Link = GetPDF(data.Link),
                                PathURL = GetImage(data.PathURL),
                                MagazineId = data.MagazineId,
                                Date = data.Date,
                                Time = data.Time,
                                MagazineTitle = data.MagazineTitle,
                                Status = data.Status,
                                EmpFirstName = data.EmpFirstName,
                                EmployeeID = data.EmployeeID,
                                createdby = data.createdby,
                                createdon = data.createdon,
                                modifiedon = data.modifiedon,
                                modifiedby = data.modifiedby,
                                MagazineCategories = GetListOfMagazineCategory(data.MagazineId),
                                MagazineBoards = GetListOfMagazineBoards(data.MagazineId),
                                MagazineClasses = GetListOfMagazineClass(data.MagazineId),
                                MagazineCourses = GetListOfMagazineCourse(data.MagazineId),
                                MagazineExamTypes = GetListOfMagazineExamType(data.MagazineId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<MagazineDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<MagazineDTO>>(false, "Records Not Found", [], 204);
                    }
                }
                else if (request.APID == 2)
                {
                    if (request.EventTypeId != 0)
                        query += " AND EventTypeId = @EventTypeId";
                    if (request.ExamType != 0)
                        query += " AND ExamType = @ExamType";

                    var Magazines = await _connection.QueryAsync<Magazine>(query, request);
                    if (Magazines != null)
                    {
                        foreach (var data in Magazines)
                        {
                            var item = new MagazineDTO
                            {
                                Link = GetPDF(data.Link),
                                PathURL = GetImage(data.PathURL),
                                MagazineId = data.MagazineId,
                                Date = data.Date,
                                Time = data.Time,
                                MagazineTitle = data.MagazineTitle,
                                Status = data.Status,
                                EmpFirstName = data.EmpFirstName,
                                EmployeeID = data.EmployeeID,
                                createdby = data.createdby,
                                createdon = data.createdon,
                                modifiedon = data.modifiedon,
                                modifiedby = data.modifiedby,
                                MagazineCategories = GetListOfMagazineCategory(data.MagazineId),
                                MagazineBoards = GetListOfMagazineBoards(data.MagazineId),
                                MagazineClasses = GetListOfMagazineClass(data.MagazineId),
                                MagazineCourses = GetListOfMagazineCourse(data.MagazineId),
                                MagazineExamTypes = GetListOfMagazineExamType(data.MagazineId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<MagazineDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<MagazineDTO>>(false, "Records Not Found", [], 204);
                    }
                }
                else
                {
                    string getquery = @"SELECT [MagazineId], [Date], [Time], [PathURL], [MagazineTitle], [Status], [Link], [EmployeeID], [EmpFirstName], [createdon], [createdby], [modifiedby], [modifiedon]
                   FROM [iGuruPrep].[dbo].[tblMagazine]";
                    var Magazines = await _connection.QueryAsync<Magazine>(getquery);
                    if (Magazines != null)
                    {
                        foreach (var data in Magazines)
                        {
                            var item = new MagazineDTO
                            {
                                Link = GetPDF(data.Link),
                                PathURL = GetImage(data.PathURL),
                                MagazineId = data.MagazineId,
                                Date = data.Date,
                                Time = data.Time,
                                MagazineTitle = data.MagazineTitle,
                                Status = data.Status,
                                EmpFirstName = data.EmpFirstName,
                                EmployeeID = data.EmployeeID,
                                createdby = data.createdby,
                                createdon = data.createdon,
                                modifiedon = data.modifiedon,
                                modifiedby = data.modifiedby,
                                MagazineCategories = GetListOfMagazineCategory(data.MagazineId),
                                MagazineBoards = GetListOfMagazineBoards(data.MagazineId),
                                MagazineClasses = GetListOfMagazineClass(data.MagazineId),
                                MagazineCourses = GetListOfMagazineCourse(data.MagazineId),
                                MagazineExamTypes = GetListOfMagazineExamType(data.MagazineId)
                            };
                            response.Add(item);
                        }
                        return new ServiceResponse<List<MagazineDTO>>(true, "Records Found", response.AsList(), 200);
                    }
                    else
                    {
                        return new ServiceResponse<List<MagazineDTO>>(false, "Records Not Found", [], 204);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<List<MagazineDTO>>(false, ex.Message, [], 500);
            }
        }
        public async Task<ServiceResponse<MagazineDTO>> GetMagazineById(int id)
        {
            try
            {
                var response = new MagazineDTO();
                string query = @"SELECT [MagazineId], [Date], [Time], [PathURL], [MagazineTitle], [Status], [Link], [EmployeeID], [EmpFirstName], [createdon], [createdby], [modifiedby], [modifiedon]
                   FROM [iGuruPrep].[dbo].[tblMagazine]
                   WHERE [MagazineId] = @MagazineId";

                var magazine = await _connection.QueryFirstOrDefaultAsync<Magazine>(query, new { MagazineId = id });

                if (magazine != null)
                {
                    response.Link = GetPDF(magazine.Link);
                    response.PathURL = GetImage(magazine.PathURL);
                    response.MagazineId = magazine.MagazineId;
                    response.Date = magazine.Date;
                    response.Time = magazine.Time;
                    response.MagazineTitle = magazine.MagazineTitle;
                    response.Status = magazine.Status;
                    response.EmpFirstName = magazine.EmpFirstName;
                    response.EmployeeID = magazine.EmployeeID;
                    response.createdby = magazine.createdby;
                    response.createdon = magazine.createdon;
                    response.modifiedon = magazine.modifiedon;
                    response.modifiedby = magazine.modifiedby;
                    response.MagazineCategories = GetListOfMagazineCategory(magazine.MagazineId);
                    response.MagazineBoards = GetListOfMagazineBoards(magazine.MagazineId);
                    response.MagazineClasses = GetListOfMagazineClass(magazine.MagazineId);
                    response.MagazineCourses = GetListOfMagazineCourse(magazine.MagazineId);
                    response.MagazineExamTypes = GetListOfMagazineExamType(magazine.MagazineId);

                    return new ServiceResponse<MagazineDTO>(true, "Record Found", response, 200);
                }
                else
                {
                    return new ServiceResponse<MagazineDTO>(false, "Record not Found", new MagazineDTO(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<MagazineDTO>(false, ex.Message, new MagazineDTO(), 500);
            }
        }
        public async Task<ServiceResponse<bool>> StatusActiveInactive(int id)
        {
            try
            {
                var magazine = await GetMagazineById(id);

                if (magazine.Data != null)
                {
                    magazine.Data.Status = !magazine.Data.Status;

                    string sql = "UPDATE [tblMagazine] SET Status = @Status WHERE [MagazineId] = @MagazineId";

                    int rowsAffected = await _connection.ExecuteAsync(sql, new { magazine.Data.Status, MagazineId = id });
                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<bool>(true, "Operation Successful", true, 200);
                    }
                    else
                    {
                        return new ServiceResponse<bool>(false, "Opertion Failed", false, 500);
                    }
                }
                else
                {
                    return new ServiceResponse<bool>(false, "Record not Found", false, 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>(false, ex.Message, false, 500);
            }
        }
        private string ImageUpload(string image)
        {
            byte[] imageData = Convert.FromBase64String(image);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = Guid.NewGuid().ToString() + ".jpg";
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string PDFUpload(string pdf)
        {
            byte[] imageData = Convert.FromBase64String(pdf);
            string directoryPath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "MagazinePDF");

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            string fileName = Guid.NewGuid().ToString() + ".pdf";
            string filePath = Path.Combine(directoryPath, fileName);

            // Write the byte array to the image file
            File.WriteAllBytes(filePath, imageData);
            return filePath;
        }
        private string GetImage(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine", Filename);

            if (!File.Exists(filePath))
            {
                throw new Exception("File not found");
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private string GetPDF(string Filename)
        {
            var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "MagazinePDF", Filename);

            if (!File.Exists(filePath))
            {
                throw new Exception("File not found");
            }
            byte[] fileBytes = File.ReadAllBytes(filePath);
            string base64String = Convert.ToBase64String(fileBytes);
            return base64String;
        }
        private int MagazineCategoryMapping(List<MagazineCategory> request, int MagazineId)
        {
            foreach (var data in request)
            {
                data.MagazineId = MagazineId;
            }
            string query = "SELECT COUNT(*) FROM [dbo].[tblMagazineCategory] WHERE [MagazineId] = @MagazineId";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineId });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblMagazineCategory]
                          WHERE [MagazineId] = @MagazineId;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineId });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineCategory] ([APID], [MagazineId], [APIDName])
                          VALUES (@APID, @MagazineId, @APIDName);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineCategory] ([APID], [MagazineId], [APIDName])
                          VALUES (@APID, @MagazineId, @APIDName);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineClassMapping(List<MagazineClass> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [dbo].[tblMagazineClass] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblMagazineClass]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineClass] ([MagazineID], [ClassID])
                          VALUES (@MagazineID, @ClassID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineClass] ([MagazineID], [ClassID])
                          VALUES (@MagazineID, @ClassID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineBoardMapping(List<MagazineBoard> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [dbo].[tblMagazineBoard] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblMagazineBoard]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineBoard] ([MagazineID], [BoardIDID])
                          VALUES (@MagazineID, @BoardIDID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineBoard] ([MagazineID], [BoardIDID])
                          VALUES (@MagazineID, @BoardIDID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineCourseMapping(List<MagazineCourse> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [dbo].[tblMagazineCourse] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblMagazineCourse]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineCourse] ([MagazineID], [CourseID])
                          VALUES (@MagazineID, @CourseID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineCourse] ([MagazineID], [CourseID])
                          VALUES (@MagazineID, @CourseID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private int MagazineExamTypeMapping(List<MagazineExamType> request, int MagazineID)
        {
            foreach (var data in request)
            {
                data.MagazineID = MagazineID;
            }
            string query = "SELECT COUNT(*) FROM [dbo].[tblMagazineExamType] WHERE [MagazineID] = @MagazineID";
            int count = _connection.QueryFirstOrDefault<int>(query, new { MagazineID });
            if (count > 0)
            {
                var deleteDuery = @"DELETE FROM [iGuruPrep].[dbo].[tblMagazineExamType]
                          WHERE [MagazineID] = @MagazineID;";
                var rowsAffected = _connection.Execute(deleteDuery, new { MagazineID });
                if (rowsAffected > 0)
                {
                    var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineExamType] ([MagazineID], [ExamTypeID])
                          VALUES (@MagazineID, @ExamTypeID);";
                    var valuesInserted = _connection.Execute(insertquery, request);
                    return valuesInserted;
                }
                else
                {
                    return 0;
                }
            }
            else
            {
                var insertquery = @"INSERT INTO [iGuruPrep].[dbo].[tblMagazineExamType] ([MagazineID], [ExamTypeID])
                          VALUES (@MagazineID, @ExamTypeID);";
                var valuesInserted = _connection.Execute(insertquery, request);
                return valuesInserted;
            }
        }
        private List<MagazineBoard> GetListOfMagazineBoards(int MagazineID)
        {
            var boardquery = @"SELECT * FROM [iGuruPrep].[dbo].[tblMagazineBoard] WHERE MagazineID = @MagazineID;";

            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineBoard>(boardquery, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineCategory> GetListOfMagazineCategory(int MagazineId)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblMagazineCategory] WHERE  MagazineId = @MagazineId;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineCategory>(query, new { MagazineId });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineClass> GetListOfMagazineClass(int MagazineID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblMagazineClass] WHERE  MagazineID = @MagazineID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineClass>(query, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineCourse> GetListOfMagazineCourse(int MagazineID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblMagazineCourse] WHERE  MagazineID = @MagazineID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineCourse>(query, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
        private List<MagazineExamType> GetListOfMagazineExamType(int MagazineID)
        {
            var query = @"SELECT * FROM [iGuruPrep].[dbo].[tblMagazineExamType] WHERE  MagazineID = @MagazineID;";
            // Execute the SQL query with the SOTDID parameter
            var data = _connection.Query<MagazineExamType>(query, new { MagazineID });
            return data != null ? data.AsList() : [];
        }
    }
}
