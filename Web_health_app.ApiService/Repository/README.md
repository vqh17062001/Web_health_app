# Authentication Repository

## Mô tả
Repository này được tạo để xử lý logic authentication thay vì viết trực tiếp trong AuthController. Điều này giúp:
- Tách biệt business logic khỏi controller
- Dễ dàng testing
- Tái sử dụng code
- Tuân thủ nguyên tắc SOLID

## Cấu trúc

### 1. IAuthRepository (Interface)
Định nghĩa các phương thức cho authentication:
- `ValidateUserCredentialsAsync()`: Xác thực thông tin đăng nhập
- `GetUserByUsernameAsync()`: Lấy thông tin user theo username
- `GetUserByCredentialsAsync()`: Lấy user theo username và password
- `GetUserInfoByUsernameAsync()`: Lấy thông tin user an toàn (không có password)
- `RecordLoginHistoryAsync()`: Ghi lại lịch sử đăng nhập
- `GetUserEffectivePermissionsAsync()`: Lấy permissions theo User ID
- `GetUserEffectivePermissionsByUsernameAsync()`: Lấy permissions theo username

### 2. AuthRepository (Implementation)
Implement các phương thức từ interface, sử dụng Entity Framework để truy vấn database.
- Sử dụng stored procedure `usp_GetUserEffectivePermissions` để lấy permissions
- Exception handling cho tất cả methods
- Async/await pattern cho performance tốt

### 3. UserDto
Data Transfer Object để trả về thông tin user an toàn (không chứa password).

### 4. UserPermissionDto
DTO để mapping kết quả từ stored procedure `usp_GetUserEffectivePermissions`.

## Cách sử dụng

### 1. Dependency Injection
Repository đã được đăng ký trong `Program.cs`:
```csharp
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
```

### 2. Sử dụng trong Controller
```csharp
private readonly IAuthRepository _authRepository;

public AuthController(IConfiguration configuration, IAuthRepository authRepository)
{
    _configuration = configuration;
    _authRepository = authRepository;
}
```

### 3. Xác thực người dùng
```csharp
if (await _authRepository.ValidateUserCredentialsAsync(request))
{
    // Generate token
    // Record login history
}
```

## Tính năng

### 1. Xác thực người dùng
- Kiểm tra username và password
- Sử dụng async/await cho performance tốt hơn
- Exception handling

### 2. Ghi lịch sử đăng nhập
- Tự động ghi lại mỗi lần đăng nhập (thành công/thất bại)
- Lưu IP address
- Timestamp

### 3. Lấy thông tin người dùng
- Method an toàn không trả về password
- Chuyển đổi sang DTO

### 4. Lấy permissions của người dùng
- Sử dụng stored procedure `usp_GetUserEffectivePermissions`
- Có thể lấy theo User ID hoặc username
- Trả về danh sách permissions với cấu trúc UserPermissionDto
- Xử lý trường hợp user không tồn tại

## API Endpoints

### 1. Authentication Endpoints
- `POST /api/auth/login`: Đăng nhập
- `GET /api/auth/testauthen`: Test authentication

### 2. Permission Endpoints  
- `GET /api/auth/permissions/{username}`: Lấy permissions theo username
- `GET /api/auth/permissions/current`: Lấy permissions của user hiện tại (từ JWT token)

## Lợi ích

1. **Separation of Concerns**: Logic database tách biệt khỏi controller
2. **Testability**: Dễ dàng mock repository để test
3. **Reusability**: Có thể sử dụng lại trong các controller khác
4. **Maintainability**: Dễ bảo trì và cập nhật
5. **Security**: Ghi lại lịch sử đăng nhập để audit
6. **Permission Management**: Tích hợp sẵn hệ thống phân quyền với stored procedure

## Cách sử dụng Permissions

### 1. Lấy permissions trong Controller
```csharp
// Lấy permissions theo username
var permissions = await _authRepository.GetUserEffectivePermissionsByUsernameAsync("username");

// Lấy permissions theo User ID  
var permissions = await _authRepository.GetUserEffectivePermissionsAsync(userId);
```

### 2. Kiểm tra permission
```csharp
// Kiểm tra user có permission CREATE trên entity Environment không
var hasCreateEnvPermission = permissions.Any(p => 
    p.action_ID == "CREATE" && 
    p.entity_ID == "Environment" && 
    p.isActive == true);
```

### 3. Sử dụng trong Authorization
```csharp
// Có thể tạo custom authorization attribute dựa trên permissions
[PermissionRequired("CREATE", "Environment")]
public async Task<IActionResult> CreateEnvironment()
{
    // Logic here
}
```

## Cải tiến trong tương lai

1. Thêm caching cho user information và permissions
2. Implement password hashing với salt
3. Thêm rate limiting cho login attempts
4. Implement account lockout mechanism
5. Thêm logging với structured logging
6. Tạo custom authorization attributes dựa trên permissions
7. Implement permission caching để tăng performance
8. Thêm audit trail cho permission changes
