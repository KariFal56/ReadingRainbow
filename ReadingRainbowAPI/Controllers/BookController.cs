
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReadingRainbowAPI.Models;
using ReadingRainbowAPI.DAL;
using ReadingRainbowAPI.Relationships;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using ReadingRainbowAPI.Dto;
using System.Collections.Generic;
using System.Linq;

namespace ReadingRainbowAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/book")]
    public class BookController : ControllerBase
    {

        private readonly BookRepository _bookRepository;

        private readonly IMapper _mapper;
 
        public BookController(BookRepository bookRepository, IMapper mapper)
        {
            _bookRepository = bookRepository;
            _mapper = mapper;
        }
        
        [HttpGet]
        [Route("Library/{bookId}")]
        public async Task<IActionResult> GetPeopleAsync(string bookId)
        {
            var book = new Book(){
                Id = bookId
            };
            var people = (await _bookRepository.GetInLibraryPersonRelationshipAsync(book, new InLibrary())).ToList();
            var peopleDto = _mapper.Map<List<Person>, List<PersonDto>>(people);

            return Ok(peopleDto);
        }

        [HttpPost]
        [Route("AddUpdateBook")]
        public async Task<IActionResult> AddUpdateBookAsync(Book book)
        {
            await _bookRepository.AddOrUpdateAsync(book);

            return Ok();
        }

        [HttpGet]
        [Route("Book/{id}")]
        public async Task<IActionResult> FindBookAsync(string id)
        {
            var book = await _bookRepository.GetBookAsync(id);

            return Ok(book);
        }
 
        [HttpGet]
        [Route("Books")]
        public async Task<IActionResult> GetAllBooksAsync()
        {
            var bookTitles = await _bookRepository.GetAllBooksAsync();

            return Ok(bookTitles);
        }
 
    }
}