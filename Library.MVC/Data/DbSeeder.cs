using Bogus;
using Library.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Library.MVC.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(
            ApplicationDbContext context,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // --- Identity: Admin Role + User ---
            const string adminRole = "Admin";
            const string adminEmail = "admin@library.com";
            const string adminPassword = "Admin123!";

            if (!await roleManager.RoleExistsAsync(adminRole))
            {
                await roleManager.CreateAsync(new IdentityRole(adminRole));
            }

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                await userManager.CreateAsync(adminUser, adminPassword);
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
            else if (!await userManager.IsInRoleAsync(adminUser, adminRole))
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            // --- Skip rest if already seeded ---
            if (await context.Books.AnyAsync()) return;

            // --- Books ---
            var bookFaker = new Faker<Book>()
                .RuleFor(b => b.Title, f => f.Lorem.Sentence(3, 5))
                .RuleFor(b => b.Author, f => f.Person.FullName)
                .RuleFor(b => b.Isbn, f => f.Random.Replace("###-#-##-#####-#"))
                .RuleFor(b => b.Category, f => f.Commerce.Categories(1)[0])
                .RuleFor(b => b.IsAvailable, true);

            var books = bookFaker.Generate(20);
            await context.Books.AddRangeAsync(books);

            // --- Members ---
            var memberFaker = new Faker<Member>()
                .RuleFor(m => m.FullName, f => f.Person.FullName)
                .RuleFor(m => m.Email, f => f.Internet.Email())
                .RuleFor(m => m.Phone, f => f.Phone.PhoneNumber());

            var members = memberFaker.Generate(10);
            await context.Members.AddRangeAsync(members);
            await context.SaveChangesAsync();

            // --- Loans ---
            var loans = new List<Loan>();
            var rnd = new Random();
            var availableBooks = books.ToList();

            Book PickBook() => availableBooks[rnd.Next(availableBooks.Count)];
            Member PickMember() => members[rnd.Next(members.Count)];

            // 5 Active
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

            // 5 Returned
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

            // 5 Overdue
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