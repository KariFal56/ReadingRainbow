using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReadingRainbowAPI.Models;
using ReadingRainbowAPI.DAL;
using ReadingRainbowAPI.Relationships;

namespace ReadingRainbowAPI.Controllers
{
    [Route("api/person")]
    [ApiController]
    public class PersonController : ControllerBase
    {

        private readonly PersonRepository _personRepository;
 
        public PersonController(INeo4jDBContext context)
        {
            _personRepository = new PersonRepository(context);
        }
        
        [HttpGet]
        [Route("Library/{personName}")]
        public async Task<ActionResult> GetBooksAsync(string personName)
        {
            var person = new Person(){
                Name = personName
            };
            var books = await _personRepository.GetInLibraryBookRelationshipAsync(person, new InLibrary());

            return Ok(books);
        }

        [HttpPost]
        [Route("AddUpdatePerson")]
        public async Task<IActionResult> AddUpdatePersonAsync(Person person)
        {
            await _personRepository.AddOrUpdatePersonAsync(person);

            return Ok();
        }

        [HttpGet]
        [Route("Person/{personName}")]
        public async Task<IActionResult> FindPersonAsync(string personName)
        {
            var person = await _personRepository.GetPersonAsync(personName);

            return Ok(person);
        }
 
        [HttpGet]
        [Route("People")]
        public async Task<IActionResult> GetAllPeopleAsync()
        {
            var people = await _personRepository.GetAllPeopleAsync();

            return Ok(people);
        }
 
    }
}