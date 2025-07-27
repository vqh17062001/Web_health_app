# User Management API

## Mô tả
API hoàn chỉnh để quản lý thông tin người dùng với các chức năng CRUD (Create, Read, Update, Delete), phân trang, tìm kiếm và các tính năng bảo mật.

## Cấu trúc

### 1. UserInfoDto.cs
- **UserInfoDto**: DTO chính để hiển thị thông tin user
- **CreateUserDto**: DTO cho việc tạo user mới
- **UpdateUserDto**: DTO cho việc cập nhật thông tin user
- **ChangePasswordDto**: DTO cho việc đổi mật khẩu

### 2. IUserRepository (Interface)
- `GetAllUsersAsync()`: Lấy danh sách user với phân trang và tìm kiếm
- `GetUserByIdAsync()`: Lấy user theo ID
- `GetUserByUsernameAsync()`: Lấy user theo username
- `CreateUserAsync()`: Tạo user mới
- `UpdateUserAsync()`: Cập nhật thông tin user
- `DeleteUserAsync()`: Xóa mềm user (soft delete)
- `HardDeleteUserAsync()`: Xóa cứng user (hard delete)
- `UsernameExistsAsync()`: Kiểm tra username đã tồn tại
- `GetUsersByManagerAsync()`: Lấy danh sách user theo manager
- `ChangePasswordAsync()`: Đổi mật khẩu user

### 3. UserRepository (Implementation)
- Implement tất cả methods từ interface
- Sử dụng Entity Framework với Include để lấy related data
- Exception handling và validation
- Soft delete logic (UserStatus = -2)

### 4. UserController
- RESTful API endpoints
- Authorization required cho tất cả endpoints
- Proper HTTP status codes
- Error handling

## API Endpoints

### 1. Lấy danh sách user
```http
GET /api/user?pageNumber=1&pageSize=10&searchTerm=john
```
**Response:**
```json
{
  "users": [...],
  "pagination": {
    "currentPage": 1,
    "pageSize": 10,
    "totalCount": 100,
    "totalPages": 10,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### 2. Lấy user theo ID
```http
GET /api/user/{id}
```

### 3. Lấy user theo username
```http
GET /api/user/username/{username}
```

### 4. Tạo user mới
```http
POST /api/user
Content-Type: application/json

{
  "userName": "john.doe",
  "password": "password123",
  "fullName": "John Doe",
  "phoneNumber": "0123456789",
  "department": "IT",
  "userStatus": 1,
  "manageBy": "guid-here",
  "levelSecurity": 1,
  "groupId": "group-id"
}
```

### 5. Cập nhật user
```http
PUT /api/user/{id}
Content-Type: application/json

{
  "fullName": "John Doe Updated",
  "phoneNumber": "0987654321",
  "department": "HR",
  "userStatus": 1
}
```

### 6. Xóa user (soft delete)
```http
DELETE /api/user/{id}
```

### 7. Xóa user vĩnh viễn
```http
DELETE /api/user/{id}/permanent
```

### 8. Kiểm tra username
```http
GET /api/user/check-username/{username}
```

### 9. Lấy user theo manager
```http
GET /api/user/manager/{managerId}
```

### 10. Đổi mật khẩu
```http
PATCH /api/user/{id}/change-password
Content-Type: application/json

{
  "newPassword": "newpassword123"
}
```

### 11. Lấy thông tin user hiện tại
```http
GET /api/user/me
```

## Tính năng

### 1. Phân trang và Tìm kiếm
- Phân trang với page number và page size
- Tìm kiếm theo username, full name, phone number, department
- Metadata về pagination trong response

### 2. Soft Delete
- User không bị xóa vĩnh viễn, chỉ thay đổi status thành -2
- Có option hard delete cho admin

### 3. Validation
- Data annotations validation
- Custom business logic validation
- Username uniqueness check

### 4. Security
- Tất cả endpoints require authentication
- Password change endpoint riêng biệt
- Current user endpoint dựa trên JWT token

### 5. Related Data
- Hiển thị manager name
- Hiển thị group name
- User status string description

## UserStatus Values
```csharp
0 => "Cần đổi MK"
1 => "Active" 
2 => "Tạm khóa"
3 => "Cần reset MK"
-1 => "Khóa vĩnh viễn"
-2 => "Đã xóa"
```

## Cách sử dụng

### 1. Dependency Injection
```csharp
// Đã được đăng ký trong Program.cs
builder.Services.AddScoped<IUserRepository, UserRepository>();
```

### 2. Sử dụng trong Controller
```csharp
private readonly IUserRepository _userRepository;

public UserController(IUserRepository userRepository)
{
    _userRepository = userRepository;
}
```

### 3. Error Handling
- Try-catch blocks trong tất cả methods
- Proper HTTP status codes
- Descriptive error messages

## Lưu ý bảo mật

1. **Password Hashing**: Hiện tại password được lưu plain text, cần implement hashing với bcrypt hoặc similar
2. **Authorization**: Có thể thêm role-based authorization cho các operations khác nhau
3. **Input Validation**: Đã có basic validation, có thể thêm custom validators
4. **Audit Trail**: Có thể thêm logging cho các operations quan trọng

## Testing

API có thể được test với:
- Swagger UI (đã được configure)
- Postman
- Unit tests với mock repository
- Integration tests

## Cải tiến trong tương lai

1. Caching cho frequently accessed users
2. Advanced search với filters
3. Export functionality
4. Bulk operations
5. Activity logging
6. Password complexity validation
7. Account lockout mechanism
8. Email notifications
