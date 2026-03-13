namespace UserService.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Avatar { get; set; }

    public User(int id, string email, string firstName, string lastName, string avatar)
    {
        Id = id;
        Email = email;
        FirstName = firstName;
        LastName = lastName;
        Avatar = avatar;
    }
}
