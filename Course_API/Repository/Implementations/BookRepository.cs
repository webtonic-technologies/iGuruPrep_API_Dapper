using Course_API.DTOs;
using Course_API.DTOs.ServiceResponse;
using Course_API.Models;
using Course_API.Repository.Interfaces;
using Dapper;
using System.Data;

namespace Course_API.Repository.Implementations
{
    public class BookRepository : IBookRepository
    {
        private readonly IDbConnection _connection;

        public BookRepository(IDbConnection connection)
        {
            _connection = connection;
        }
        public async Task<ServiceResponse<string>> Add(BookDTO bookDTO)
        {
            var book = new Book
            {
                BookName = bookDTO.BookName,
                AuthorName = bookDTO.AuthorName,
                AuthorDetails = bookDTO.AuthorDetails,
                AuthorAffliation = bookDTO.AuthorAffliation,
                Boardname = bookDTO.Boardname,
                ClassName = bookDTO.ClassName,
                CourseName = bookDTO.CourseName,
                SubjectName = bookDTO.SubjectName,
                Status = bookDTO.Status
            };
            try
            {
                int rowsAffected = await _connection.ExecuteAsync(
                    @"INSERT INTO Books (BookName, AuthorName, AuthorDetails, AuthorAffliation, BoardName, ClassName, CourseName, SubjectName, Status)
                  VALUES (@BookName, @AuthorName, @AuthorDetails, @AuthorAffiliation, @BoardName, @ClassName, @CourseName, @SubjectName, @Status)",
                    book);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Book Added Successfully", 200);
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

        public async Task<ServiceResponse<bool>> Delete(int id)
        {
            try
            {
                
                int rowsAffected = await _connection.ExecuteAsync(
                    "DELETE FROM Books WHERE BookId = @BookId", new { BookId = id });

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

        public async Task<ServiceResponse<Book>> Get(int id)
        {
            try
            {
                var book = await _connection.QueryFirstOrDefaultAsync<Book>(
                    "SELECT * FROM Books WHERE BookId = @BookId", new { BookId = id });

                if (book != null)
                {
                    return new ServiceResponse<Book>(true, "Record Found", book, 200);
                }
                else
                {
                    return new ServiceResponse<Book>(false, "Record not Found", new Book(), 500);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Book>(false, ex.Message, new Book(), 500);
            }
        }

        public async Task<ServiceResponse<IEnumerable<Book>>> GetAll()
        {

            try
            {
                var books = await _connection.QueryAsync<Book>(
                    "SELECT * FROM Books");

                if (books != null)
                {
                    return new ServiceResponse<IEnumerable<Book>>(true, "Records Found", books.AsList(), 200);
                }
                else
                {
                    return new ServiceResponse<IEnumerable<Book>>(false, "Records Not Found", new List<Book>(), 204);
                }
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<Book>>(false, ex.Message, new List<Book>(), 200);
            }
        }

        public async Task<ServiceResponse<string>> Update(BookDTO bookDTO)
        {
            var book = new Book
            {
                BookId = bookDTO.BookId,
                BookName = bookDTO.BookName,
                AuthorName = bookDTO.AuthorName,
                AuthorDetails = bookDTO.AuthorDetails,
                AuthorAffliation = bookDTO.AuthorAffliation,
                Boardname = bookDTO.Boardname,
                ClassName = bookDTO.ClassName,
                CourseName = bookDTO.CourseName,
                SubjectName = bookDTO.SubjectName,
                Status = bookDTO.Status
            };

            try
            {
                int rowsAffected = await _connection.ExecuteAsync(
                    @"UPDATE Books 
                  SET BookName = @BookName, 
                      AuthorName = @AuthorName, 
                      AuthorDetails = @AuthorDetails, 
                      AuthorAffliation = @AuthorAffiliation, 
                      BoardName = @BoardName, 
                      ClassName = @ClassName, 
                      CourseName = @CourseName, 
                      SubjectName = @SubjectName, 
                      Status = @Status
                  WHERE BookId = @BookId",
                    book);

                if (rowsAffected > 0)
                {
                    return new ServiceResponse<string>(true, "Operation Successful", "Book Updated Successfully", 200);
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
    }
}
