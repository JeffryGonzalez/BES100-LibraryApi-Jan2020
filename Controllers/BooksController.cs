using LibraryApi.Domain;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;

namespace LibraryApi.Controllers
{
    public class BooksController : Controller
    {
        LibraryDataContext Context;

        public BooksController(LibraryDataContext context)
        {
            Context = context; 
            
        }

        IQueryable<Book> GetBooksInInventory()
        {
            return Context.Books
                .Where(b => b.InInventory);
        }


        [HttpPost("/books")]
        public async Task<IActionResult> AddABook([FromBody] PostBooksRequest bookToAdd)
        {
            // Validate it. (if valid, return a 400 Bad Request)
            if(!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Add it to the domain.
            //  - PostBooksRequest -> Book
            var book = new Book
            {
                Title = bookToAdd.Title,
                Author = bookToAdd.Author,
                Genre = bookToAdd.Genre,
                NumberOfPages = bookToAdd.NumberOfPages,
                InInventory = true
            };

            //  - Add it to the Context.
            Context.Books.Add(book);
            //  - Have the context save everything.
            await Context.SaveChangesAsync();
            // Return a 201 Created Status Code.
            //  - Add a location header on the response e.g. Location: http://server/books/8
            //  - Add the entity
            //  - Book -> Get BooksDetailResponse
            
            var response = new GetBookDetailsResponse
            {
                Id = book.Id,
                Title = book.Title,
                Author = book.Author,
                Genre = book.Genre,
                NumberOfPages = book.NumberOfPages
            };

            return CreatedAtRoute("books#getbookbyid", new { id = response.Id }, response);
        }


        [HttpGet("/books/{id:int}", Name = "books#getbookbyid")]
        public async Task<IActionResult> GetBookById(int id)
        {
            var response = await GetBooksInInventory()
                .Where(b => b.Id == id)
                .Select(b => new GetBookDetailsResponse
                {
                    Id = b.Id,
                    Title = b.Title,
                    Author = b.Author,
                    Genre = b.Genre,
                    NumberOfPages = b.NumberOfPages
                }).SingleOrDefaultAsync();

            if (response == null)
            {
                return NotFound("No book with that Id!"); // the string is optional
            }
            else
            {
                return Ok(response);
            }

        }


        [HttpGet("/books")]
        public async Task<IActionResult> GetAllBooks([FromQuery] string genre = "all")
        {

            var books = GetBooksInInventory();

            if (genre != "all")
            {
                books = books.Where(b => b.Genre == genre);
            }

            var booksListItems = await books.Select(b => new BookSummaryItem
            {
                Id = b.Id,
                Title = b.Title,
                Author = b.Author,
                Genre = b.Genre
            }).ToListAsync();

            var response = new GetBooksResponse
            {
                Data = booksListItems,
                Genre = genre,
                Count = booksListItems.Count()
            };

            return Ok(response); // for right now.
        }
    }
}
