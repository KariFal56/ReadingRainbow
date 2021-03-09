using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ReadingRainbowAPI.Models;
using ReadingRainbowAPI.DAL;
using ReadingRainbowAPI.Middleware;
using ReadingRainbowAPI.Relationships;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using ReadingRainbowAPI.Dto;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System;
using System.Text.RegularExpressions;

namespace ReadingRainbowAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/person")]
    public class PersonController : ControllerBase
    {

        private readonly PersonRepository _personRepository;

        private readonly IMapper _mapper;

        private readonly IEmailHelper _emailHelper;
 
        public PersonController(PersonRepository personRepository, IMapper mapper, IEmailHelper emailHelper)
        {
            _personRepository = personRepository;
            _mapper = mapper;
            _emailHelper = emailHelper;
        }
        
        [HttpGet]
        [Route("Library/{username}")]
        public async Task<ActionResult> GetBooksAsync(string username)
        {
            var person = new Person(){
                Name = username
            };
            var books = await _personRepository.GetInLibraryBookRelationshipAsync(person, new InLibrary());
            Console.WriteLine($"books {books}" );
            return Ok(JsonSerializer.Serialize(books));
        }

        [HttpPost]
        [Route("UpdatePerson")]
        public async Task<IActionResult> UpdatePersonAsync(Person person)
        {
            var success = await _personRepository.UpdatePersonAsync(person);
            
            return Ok(success);
        }

        [HttpGet]
        [Route("Person/{username}")]
        public async Task<IActionResult> FindPersonAsync(string username)
        {
            var person = await _personRepository.GetPersonAsync(username);
            var personDto = _mapper.Map<PersonDto>(person);
            Console.WriteLine($"PersonDto {personDto}" );
            return Ok(JsonSerializer.Serialize(personDto));
        }

        
        [AllowAnonymous]
        [HttpPost]
        [Route("AddPerson")]
        public async Task<IActionResult> AddPersonAsync(Person person)
        {
            Console.WriteLine($"person {person.Name}" );

            if (!CheckEmailAddress(person.Email))
            {
                return Ok("Email in incorrect format");
            }

            var success = await _personRepository.AddPersonAsync(person);

            if (success)
            {
                // If user was added, generate token
                var token = TokenClass.CreateToken();
                person.Token = SanitizeToken(token);
                await UpdatePersonAsync(person);

                // var AppBaseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

                //  var callbackUrl = $"{AppBaseUrl}//{token}//{person.Name}";
                // var callbackUrl = Url.Action("ConfirmEmail", "Email", new { token, name = person.Name });
                var callbackUrl = "https://localhost:5001/api/email/AddPerson/" + token + "/" + person.Name;
                var confirmationLink = $"Please confirm your account by clicking this link: <a href='{callbackUrl}'>link</a>";
    
                bool emailResponse = await _emailHelper.SendEmail(person.Name, person.Email, confirmationLink);
             
                if (emailResponse)
                {
                    Console.WriteLine($"Valid Email Address {person.Email}");
                }
                else
                {
                    Console.WriteLine($"Invalid Email Address {person.Email}");
                    return Ok($"Invalid Email Address {person.Email}");
                }
            }

            Console.WriteLine($"sucess {success}" );
            return Ok(success);
        }

        private string SanitizeToken(string token)
        {
            var newToken = token.Replace("/","");
            newToken = newToken.Replace("\\","");

            return newToken;
        }

        private bool CheckEmailAddress(string email)
        {
            string emailRegex = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            
            Regex re = new Regex(emailRegex);
            if (!re.IsMatch(email))
            {
                Console.WriteLine($"Invalid Email Address {email}");
                return false;
            }
            
            return true;
        }
 
        [HttpGet]
        [Route("People")]
        public async Task<IActionResult> GetAllPeopleAsync()
        {
            var people = (await _personRepository.GetAllPeopleAsync()).ToList();
            var peopleDto = _mapper.Map<List<Person>, List<PersonDto>>(people);

            return Ok(JsonSerializer.Serialize(peopleDto));
        }
 
    }
}