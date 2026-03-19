using Bogus;
using Library.Domain;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context)
        {
            if (await context.Books.AnyAsync()) return; // Already seeded

            // --- 20 Books ---
            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3, 5))
                .RuleFor(b => b.Author, f => f.Person.FullName)
                .RuleFor(b => b.Isbn, f => f.Random.Replace("###-#-##-#####-#"))
                .RuleFor(b => b.Category, f => f.Commerce.Categories(1)[0])
                .RuleFor(b => b.IsAvailable, true);

            var books = bookFaker.Generate(20);
            await context.Books.AddRangeAsync(books);

            // --- 10 Members ---
            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Person.FullName)
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

            var members = memberFaker.Generate(10);
            await context.Members.AddRangeAsync(members);

            await context.SaveChangesAsync();

            // --- 15 Loans ---
            var loans = new List<Loan>();
            var rnd = new Random();

            var availableBooks = books.ToList();

            // Helper to get random member and book
            Book PickBook() => availableBooks[rnd.Next(availableBooks.Count)];
            Member PickMember() => members[rnd.Next(members.Count)];

            // 5 Active loans
            for (int i = 0; i < 5; i++)
            {
                var book = PickBook();
                var member = PickMember();
                var loanDate = DateTime.Today.AddDays(-rnd.Next(1, 10));
                loans.Add(new Loan
                {
                    BookId = book.Id,
                    MemberId = member.Id,
                    LoanDate = loanDate,
                    DueDate = loanDate.AddDays(14),
                    ReturnedDate = null
                });
                book.IsAvailable = false;
            }

            // 5 Returned loans
            for (int i = 0; i < 5; i++)
            {
                var book = PickBook();
                var member = PickMember();
                var loanDate = DateTime.Today.AddDays(-rnd.Next(20, 40));
                var returnedDate = loanDate.AddDays(rnd.Next(1, 14));
                loans.Add(new Loan
                {
                    BookId = book.Id,
                    MemberId = member.Id,
                    LoanDate = loanDate,
                    DueDate = loanDate.AddDays(14),
                    ReturnedDate = returnedDate
                });
                book.IsAvailable = true;
            }

            // 5 Overdue loans
            for (int i = 0; i < 5; i++)
            {
                var book = PickBook();
                var member = PickMember();
                var loanDate = DateTime.Today.AddDays(-rnd.Next(20, 30));
                loans.Add(new Loan
                {
                    BookId = book.Id,
                    MemberId = member.Id,
                    LoanDate = loanDate,
                    DueDate = loanDate.AddDays(14),
                    ReturnedDate = null
                });
                book.IsAvailable = false;
            }

            await context.Loans.AddRangeAsync(loans);
            await context.SaveChangesAsync();
        }
    }
}