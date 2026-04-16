using System.Security.Cryptography;
using AutoMapper;
using ELearning.Core.DTOs;
using ELearning.Core.Entities;
using ELearning.Core.Interfaces;

namespace ELearning.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IRepository<User> _repo;
    private readonly IMapper _mapper;

    public UserService(IRepository<User> repo, IMapper mapper)
    {
        _repo = repo;
        _mapper = mapper;
    }

    public async Task<UserDto> RegisterAsync(UserRegisterDto dto)
    {
        var existing = (await _repo.FindAsync(u => u.Email == dto.Email)).FirstOrDefault();
        if (existing != null)
            throw new InvalidOperationException("Email already registered.");

        var user = new User
        {
            FullName = dto.FullName,
            Email = dto.Email,
            PasswordHash = HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(user);
        await _repo.SaveAsync();
        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> GetByIdAsync(int id)
    {
        var user = await _repo.GetByIdAsync(id);
        return user == null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto?> UpdateAsync(int id, UserUpdateDto dto)
    {
        var user = await _repo.GetByIdAsync(id);
        if (user == null) return null;

        user.FullName = dto.FullName;
        if (!string.IsNullOrEmpty(dto.Email)) user.Email = dto.Email;

        _repo.Update(user);
        await _repo.SaveAsync();
        return _mapper.Map<UserDto>(user);
    }

    private static string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, 100_000, HashAlgorithmName.SHA256, 32);
        return $"{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }
}
