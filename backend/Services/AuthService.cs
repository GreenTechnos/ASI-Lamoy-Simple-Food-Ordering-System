using backend.Constants;
using backend.DTOs.User;
using backend.Helpers;
using backend.Models;
using backend.Repositories;
using BCrypt.Net;
using Microsoft.Extensions.Logging;

namespace backend.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly JwtTokenHelper _jwtHelper;
        private readonly ILogger<AuthService> _logger; // Logging Service

        public AuthService(IUserRepository userRepository, JwtTokenHelper jwtHelper, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _jwtHelper = jwtHelper;
            _logger = logger;
        }

        public async Task RegisterAsync(RegisterRequest request)
        {
            _logger.LogInformation(AppConstants.AuthServiceLogs.RegistrationAttempt, request.Email);

            // 1. Check if user already exists
            if (await _userRepository.UserExistsByUsernameAsync(request.UserName))
            {
                _logger.LogWarning(AppConstants.AuthServiceLogs.RegistrationFailed, "Username", request.UserName);
                throw new InvalidOperationException(AppConstants.AuthServiceErrors.UsernameAlreadyExists);
            }
                
            if (await _userRepository.UserExistsByEmailAsync(request.Email))
            {
                _logger.LogWarning(AppConstants.AuthServiceLogs.RegistrationFailed, "Email", request.Email);
                throw new InvalidOperationException(AppConstants.AuthServiceErrors.EmailAlreadyExists);
            }
                
            // 2. Hash the password
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            // 3. Create the new User model
            var newUser = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                PasswordHash = passwordHash,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                Address = request.Address,
                Role = UserRole.Customer,
                CreatedAt = DateTime.UtcNow
            };

            // 4. Save to the database via the repository
            await _userRepository.CreateUserAsync(newUser);
            
            _logger.LogInformation(AppConstants.AuthServiceLogs.UserRegisteredSuccessfully, newUser.UserName, newUser.UserId);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            _logger.LogInformation(AppConstants.AuthServiceLogs.LoginAttempt, request.Email);

            // 1. Get user from repository
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            // 2. Validate user and password
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning(AppConstants.AuthServiceLogs.FailedLoginAttempt, request.Email);
                throw new UnauthorizedAccessException(AppConstants.AuthServiceErrors.InvalidEmailOrPassword);
            }

            // 3. Generate Token
            var token = _jwtHelper.GenerateToken(
                user.UserId.ToString(),
                user.Email,
                user.Role.ToString(),
                user.UserName
            );

            _logger.LogInformation(AppConstants.AuthServiceLogs.UserLoggedInSuccessfully, user.UserName, user.UserId);

            // 4. Return the DTO
            return new LoginResponse
            {
                Token = token,
                Role = user.Role,
                UserName = user.UserName
            };
        }
    }
}