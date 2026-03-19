namespace Library.Domain
{
    public class Loan
    {
        public int Id { get; set; }

        // FK to Book
        public int BookId { get; set; }
        public Book? Book { get; set; }

        // FK to Member
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public DateTime LoanDate { get; set; } = DateTime.Today;
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(14);
        public DateTime? ReturnedDate { get; set; }
    }
}