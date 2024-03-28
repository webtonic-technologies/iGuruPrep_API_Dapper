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
        public async Task<ServiceResponse<string>> AddNewMagazine(MagazineDTO magazineDTO)
        {
            try
            {
                var magazine = new Magazine
                {
                    MagazineName = magazineDTO.MagazineName,
                    ClassName = magazineDTO.ClassName,
                    CourseName = magazineDTO.CourseName,
                    DateAndTime = DateTime.Now,
                    MagazineTitle = magazineDTO.MagazineTitle,
                    Status = magazineDTO.Status
                };

                if (magazineDTO.File != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine");
                    if (!Directory.Exists(uploads))
                    {
                        Directory.CreateDirectory(uploads);
                    }
                    var fileName = Path.GetFileNameWithoutExtension(magazineDTO.File.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(magazineDTO.File.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await magazineDTO.File.CopyToAsync(fileStream);
                    }
                    magazine.PathURL = fileName;
                }

                string sql = @"INSERT INTO tblMagazine (MagazineName, ClassName, CourseName, DateAndTime, PathURL, MagazineTitle, Status) 
                       VALUES (@MagazineName, @ClassName, @CourseName, @DateAndTime, @PathURL, @MagazineTitle, @Status)";
                int rowsAffected = await _connection.ExecuteAsync(sql, magazine);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Feedback Added Successfully", 200);
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
                if (File.Exists(filePath))
                {
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

        public async Task<ServiceResponse<IEnumerable<MagazineDTO>>> GetAllMagazines()
        {
            try
            {
                var query = @"
                SELECT * FROM tblMagazine";

                var magazines = await _connection.QueryAsync<MagazineDTO>(query);

                if (magazines != null)
                {
                    return new ServiceResponse<IEnumerable<MagazineDTO>>(true, "Records Found", magazines, 200);
                }
                else
                {
                    return new ServiceResponse<IEnumerable<MagazineDTO>>(false, "Records Not Found", new List<MagazineDTO>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<MagazineDTO>>(false, ex.Message, new List<MagazineDTO>(), 500);
            }
        }

        public async Task<ServiceResponse<MagazineDTO>> GetMagazineById(int id)
        {
            try
            {
                var query = @"
                SELECT *
                FROM tblMagazine
                WHERE MagazineId = @MagazineId";

                var magazine = await _connection.QueryFirstOrDefaultAsync<MagazineDTO>(query, new { MagazineId = id });

                if (magazine != null)
                {
                    return new ServiceResponse<MagazineDTO>(true, "Record Found", magazine, 200);
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

        public async Task<ServiceResponse<byte[]>> GetMagazineFileById(int id)
        {
            try
            {
                var magazine = await _connection.QueryFirstOrDefaultAsync<Magazine>(
                    "SELECT PathURL FROM tblMagazine WHERE MagazineId = @MagazineId",
                    new { MagazineId = id });

                if (magazine == null)
                    throw new Exception("Magazine not found");

                var filePath = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine", magazine.PathURL);

                if (!File.Exists(filePath))
                    throw new Exception("File not found");
                var fileBytes = await File.ReadAllBytesAsync(filePath);

                return  new ServiceResponse<byte[]>(true, "Record Found", fileBytes, 200);
            }
            catch (Exception ex)
            {
                return new ServiceResponse<byte[]>(false, ex.Message, Array.Empty<byte>(), 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateMagazine(UpdateMagazineDTO magazineDTO)
        {
            try
            {
                var query = @"
                UPDATE tblMagazine
                SET MagazineName = @MagazineName, ClassName = @ClassName, CourseName = @CourseName, MagazineTitle = @MagazineTitle, Status = @Status
                WHERE MagazineId = @MagazineId";

                int rowsAffected = await _connection.ExecuteAsync(query, new
                {
                    magazineDTO.MagazineName,
                    magazineDTO.ClassName,
                    magazineDTO.CourseName,
                    magazineDTO.MagazineId,
                    magazineDTO.Status,
                    magazineDTO.MagazineTitle
                });
                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Magazine Updated Successfully", 200);
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "Record not found", 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }

        public async Task<ServiceResponse<string>> UpdateMagazineFile(MagazineDTO magazineDTO)
        {
            try
            {
                await _connection.ExecuteAsync(
                    @"UPDATE tblMagazine 
                  SET MagazineName = @MagazineName, ClassName = @ClassName, CourseName = @CourseName, MagazineTitle = @MagazineTitle, Status = @Status
                  WHERE MagazineId = @MagazineId",
                    new
                    {
                        magazineDTO.MagazineName,
                        magazineDTO.ClassName,
                        magazineDTO.CourseName,
                        magazineDTO.MagazineId,
                        magazineDTO.MagazineTitle,
                        magazineDTO.Status
                    });

                if (magazineDTO.File != null)
                {
                    var uploads = Path.Combine(_hostingEnvironment.ContentRootPath, "Assets", "Magazine");
                    if (!Directory.Exists(uploads))
                        Directory.CreateDirectory(uploads);

                    var fileName = Path.GetFileNameWithoutExtension(magazineDTO.File.FileName) + "_" + Guid.NewGuid().ToString() + Path.GetExtension(magazineDTO.File.FileName);
                    var filePath = Path.Combine(uploads, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await magazineDTO.File.CopyToAsync(fileStream);
                    }

                    int rowsAffected = await _connection.ExecuteAsync(
                         @"UPDATE tblMagazine 
                      SET PathURL = @PathURL
                      WHERE MagazineId = @MagazineId",
                         new { PathURL = fileName, magazineDTO.MagazineId });

                    if (rowsAffected > 0)
                    {
                        return new ServiceResponse<string>(true, "Operation Successful", "Magazine Updated Successfully", 200);
                    }
                    else
                    {
                        return new ServiceResponse<string>(false, "Opertion Failed", "Record not found", 500);
                    }
                }
                else
                {
                    return new ServiceResponse<string>(false, "Opertion Failed", "Please add file", 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<string>(false, ex.Message, string.Empty, 500);
            }
        }
    }
}
