using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SapnaBookHouse;

namespace BookManagementPortalProxy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SapnaBookHouseController : ControllerBase
    {
        private readonly SapnaBook _sapnaBook;
        private readonly ILogger<SapnaBookHouseController> _logger;

        public SapnaBookHouseController(SapnaBook sapnabook, ILogger<SapnaBookHouseController> logger)
        {
            _sapnaBook = sapnabook;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMeAllBooks()
        {
            _logger.LogInformation("GetMeAllBooks endpoint is called");

            try
            {
                _logger.LogDebug("Trying to retrive all the data");
                var books = await _sapnaBook.BooksAllAsync();
                _logger.LogInformation("Retrived all the datas");
                return Ok(books);
            }

            catch (ApiException ex)
            {
                _logger.LogError("An unexcepted error occured");
                return StatusCode(500, new { error = "Could not process due some error" });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexcepted error occured");
                return StatusCode(500, new { error = "Could not process due some error" });
            }
        }

        [HttpGet("BookID")]
        public async Task<IActionResult> GetMeBookByID(int BookID)
        {
            _logger.LogInformation("GetMeBookByID endpoint is called");
            try
            {
                _logger.LogDebug($"trying to retrive the book of ID {BookID}");
                var book = await _sapnaBook.BookIDGETAsync(BookID);
                _logger.LogInformation("Retrived the book of given ID");
                return Ok(book);
            }

            catch (ApiException ex)
            {
                _logger.LogError("An unexcepted error occured");
                return StatusCode(500, new { error = "Could not process due some error" });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexcepted error occured");
                return StatusCode(404, new { error = "Could not process due some error" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> PostANewBook(BookDTO bookdto)
        {
            _logger.LogInformation("PostNewBook endpoint is called");
            try
            {
                var book = new BookDTO
                {
                    BookName = bookdto.BookName,
                    Author = bookdto.Author,
                    BookPrice = bookdto.BookPrice
                };

                _logger.LogDebug($"Attempting to add a new book {book}");
                await _sapnaBook.BooksAsync(book);
                _logger.LogInformation("Successfully added the book");
                return CreatedAtAction(nameof(GetMeBookByID), book);
            }

            catch (ApiException ex)
            {
                _logger.LogError("An unexcepted occurred while adding a new book");
                return StatusCode(500, new { error = "Could not process due some error" });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexcepted occurred while adding a new book");
                return StatusCode(500, new { error = "Could not process due some error" });
            }
        }

        [HttpPut("BookID")]
        public async Task<IActionResult> EditBook(int BookID, BookDTO bookdto)
        {
            _logger.LogInformation("EditBook endpoint is called");
            try
            {
                var book = new BookDTO
                {
                    BookName = bookdto.BookName,
                    Author = bookdto.Author,
                    BookPrice = bookdto.BookPrice
                };

                _logger.LogDebug($"Attempting to edit the book {book}");
                await _sapnaBook.BookIDPUTAsync(BookID, book);
                _logger.LogInformation("Edited the book Successfully");
                return NoContent();
            }

            catch (ApiException ex)
            {
                _logger.LogError("An unexcepted occurred");
                return StatusCode(500, new { error = "Could not process due some error" });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexcepted occurred");
                return StatusCode(404, new { error = "Could not process due some error" });
            }
        }

        [HttpDelete("BookID")]
        public async Task<IActionResult> DeleteBookByID(int BookID)
        {
            _logger.LogInformation("DeleteBookByID endpoint is called");
            try
            {
                _logger.LogDebug($"Trying to delete the book with bookID {BookID}");
                await _sapnaBook.BookIDDELETEAsync(BookID);
                _logger.LogInformation("Deleted the book successfully");
                return NoContent();
            }

            catch (ApiException ex)
            {
                _logger.LogError("An unexcepted occurred");
                return StatusCode(500, new { error = "Could not process due some error" });
            }

            catch (Exception ex)
            {
                _logger.LogError("An unexcepted occurred");
                return StatusCode(404, new { error = "Could not process due some error" });
            }
        }
    }
}
