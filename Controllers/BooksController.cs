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
            Context = context; // Using DI FTW LOL!
        }

        IQueryable<Book> GetBooksInInventory()
        {
            return Context.Books
                .Where(b => b.InInventory);
        }


        [HttpGet("/books/{id:int}")]
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
