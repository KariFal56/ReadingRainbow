using System;
using Xunit;
using ReadingRainbowAPI.Controllers;
using ReadingRainbowAPI.DAL;
using ReadingRainbowAPI.Models;
using System.Collections.Generic;
using System.Linq;

namespace ReadingRainbowAPI.RepositoryTests
{
    [Collection("Database collection")]
    public class PersonRepositoryTests
    {
        DatabaseFixture fixture;

        private PersonRepository _personRepository;
        private BookRepository _bookRepository;

        // Initalize Method used for all tests
        public PersonRepositoryTests(DatabaseFixture fixture)
        {
            this.fixture = fixture;

            _personRepository = new PersonRepository(fixture.dbContext);
            _bookRepository = new BookRepository(fixture.dbContext);
        }

        private Person CreatePerson()
        {
            var random = new Random();
            var personId = random.Next();

            // Arrange
            return new Person(){
                Name = $"newPerson{personId}",
                Profile =$"This is new person number {personId}",
                Portrait = "Https://PortraitLink",
                HashedPassword = $"{personId}"
            };
        }

        private Book CreateBook()
        {
            var random = new Random();
            var bookIdExt = random.Next();

            // Arrange
            return new Book(){
                Id = $"xbn56r{bookIdExt}",
                Title =$"Test Book Title {bookIdExt}",
                PublishDate  = DateTime.Now.ToShortDateString(),
                NumberPages  = $"{bookIdExt}",
                Description  = "Test Book Description",
            };
        }

        [Fact]
        public async void GetPeopleAsync_Test()
        {
            var people = await _personRepository.GetAllPeopleAsync();
            Assert.True(people != null);
        }

        [Fact]
        public async void AddPersonAsync_Test()
        {
            // Arrange
            var newPerson = CreatePerson();

            // Act
            await _personRepository.AddOrUpdatePersonAsync(newPerson);
            var returnedPerson = await _personRepository.GetPersonAsync(newPerson.Name);

            // Assert
            Assert.True(returnedPerson != null);
            Assert.True(returnedPerson.Name == newPerson.Name);
        }

        [Fact]
        public async void UpdatePersonAsync_Test()
        {
            // Arrange
            var random = new Random();
            var NewPersonProfileExt = random.Next();
            var newPersonProfile = $"New Person's Updated Profile {NewPersonProfileExt}";

            var newPerson = CreatePerson();

            // Act
            await _personRepository.AddOrUpdatePersonAsync(newPerson);
            newPerson.Profile = newPersonProfile;
            await _personRepository.AddOrUpdatePersonAsync(newPerson);

            var returnedPerson = await _personRepository.GetPersonAsync(newPerson.Name);

            // Assert
            Assert.True(returnedPerson != null);
            Assert.True(returnedPerson.Profile == newPersonProfile);
        }

        [Fact]
        public async void GetPersonAsync_Test()
        {
            var newPerson = CreatePerson();

            await _personRepository.AddOrUpdatePersonAsync(newPerson);
            var returnedperson = await _personRepository.GetPersonAsync(newPerson.Name);

            Assert.True(newPerson.HashedPassword == returnedperson.HashedPassword);
        }

        [Fact]
        public async void AddFriendsAsync_Test()
        {
            // Arrange
            var person1 = CreatePerson();
            var person2 = CreatePerson();
            var friendsWith = new Relationships.FriendsWith(); 

            await _personRepository.AddOrUpdatePersonAsync(person1);
            await _personRepository.AddOrUpdatePersonAsync(person2);

            // Act
            await _personRepository.CreateFriendRelationshipAsync(person1, person2, friendsWith);
            var friends = await _personRepository.GetFriendsWithRelationshipAsync(person1, friendsWith);

            // Assert
            Assert.True(friends.Count() == 1);
            Assert.True(friends.FirstOrDefault().Name == person2.Name);

            // CleanUp
            await _personRepository.DeleteFriendsWithRelationshipAsync(person1, person2, friendsWith);
            await _personRepository.DeletePersonAsync(person1);
            await _personRepository.DeletePersonAsync(person2);
        }

        [Fact]
        public async void NoFriendsAsync_Test()
        {
            // Arrange
            var person1 = CreatePerson();
            var person2 = CreatePerson();

            await _personRepository.AddOrUpdatePersonAsync(person1);
            await _personRepository.AddOrUpdatePersonAsync(person2);

            // Act
            var friends = await _personRepository.GetFriendsWithRelationshipAsync(person1, new Relationships.FriendsWith());

            // Assert
            Assert.True(friends.Count() == 0);

            // CleanUp
            await _personRepository.DeletePersonAsync(person1);
            await _personRepository.DeletePersonAsync(person2);
        }

        [Fact]
        public async void RemoveFriendAsync_Test()
        {
            // Arrange
            var person = CreatePerson();
            var friendsForever = CreatePerson();
            var noLongerFriend = CreatePerson();
            var friendsWith = new Relationships.FriendsWith();

            await _personRepository.AddOrUpdatePersonAsync(person);
            await _personRepository.AddOrUpdatePersonAsync(friendsForever);
            await _personRepository.AddOrUpdatePersonAsync(noLongerFriend);

            await _personRepository.CreateFriendRelationshipAsync(person, noLongerFriend, friendsWith);
            await _personRepository.CreateFriendRelationshipAsync(person, friendsForever, friendsWith);

            // Act
            var everyoneIsFriends = await _personRepository.GetFriendsWithRelationshipAsync(person, friendsWith);
            await _personRepository.DeleteFriendsWithRelationshipAsync(person, noLongerFriend, friendsWith);
            var someoneIsLeftout = await _personRepository.GetFriendsWithRelationshipAsync(person, friendsWith);

            // Assert
            Assert.True(everyoneIsFriends.Where(f=> f.Name == friendsForever.Name).ToList().Count == 1);
            Assert.True(everyoneIsFriends.Where(f=> f.Name == noLongerFriend.Name).ToList().Count == 1);
            Assert.True(everyoneIsFriends.ToList().Count == 2);

            Assert.True(someoneIsLeftout.Where(f=> f.Name == friendsForever.Name).ToList().Count == 1);
            Assert.True(someoneIsLeftout.Where(f=> f.Name == noLongerFriend.Name).ToList().Count == 0);
            Assert.True(someoneIsLeftout.ToList().Count == 1);

            // CleanUp
            await _personRepository.DeleteFriendsWithRelationshipAsync(person, friendsForever, friendsWith);
            await _personRepository.DeletePersonAsync(person);
            await _personRepository.DeletePersonAsync(friendsForever);
            await _personRepository.DeletePersonAsync(noLongerFriend);
        }

        [Fact]
        public async void GetFriendsAsync_Test()
        {
            // Arrange 
            var person = CreatePerson();
            var friend1 = CreatePerson();
            var friend2 = CreatePerson();
            var notaFriend = CreatePerson();

            await _personRepository.AddOrUpdatePersonAsync(person);
            await _personRepository.AddOrUpdatePersonAsync(friend1);
            await _personRepository.AddOrUpdatePersonAsync(friend2);
            await _personRepository.AddOrUpdatePersonAsync(notaFriend);

            var friendsWith = new Relationships.FriendsWith();

            await _personRepository.CreateFriendRelationshipAsync(person, friend1, friendsWith);
            await _personRepository.CreateFriendRelationshipAsync(person, friend2, friendsWith);
            await _personRepository.CreateFriendRelationshipAsync(friend1, notaFriend, friendsWith);

            // Act
            var personsFriends = await _personRepository.GetFriendsWithRelationshipAsync(person, friendsWith);

            // Assert
            Assert.True(personsFriends.Where(f=> f.Name == friend1.Name).ToList().Count == 1);
            Assert.True(personsFriends.Where(f=> f.Name == friend2.Name).ToList().Count == 1);
            Assert.True(personsFriends.Where(f=> f.Name == notaFriend.Name).ToList().Count == 0);
            Assert.True(personsFriends.ToList().Count == 2);

            // Clean Up
            await _personRepository.DeleteFriendsWithRelationshipAsync(person, friend1, friendsWith);
            await _personRepository.DeleteFriendsWithRelationshipAsync(person, friend2, friendsWith);
            await _personRepository.DeleteFriendsWithRelationshipAsync(friend1, notaFriend, friendsWith);
            await _personRepository.DeletePersonAsync(person);
            await _personRepository.DeletePersonAsync(friend1);
            await _personRepository.DeletePersonAsync(friend2);
            await _personRepository.DeletePersonAsync(notaFriend);
        }

    
    }
}