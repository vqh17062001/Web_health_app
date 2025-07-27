# UserApiClient

## Mô tả
`UserApiClient` là một service được thiết kế để xử lý tất cả các API calls liên quan đến user management trong ứng dụng Blazor Server. Nó được tạo theo mẫu của `LoginApiClient` để đảm bảo tính nhất quán và centralized management.

## Tính năng chính

### 1. **Automatic Authorization**
- Tự động set Authorization header với Bearer token từ localStorage
- Handle authentication errors một cách nhất quán
- Token được retrieve từ `ProtectedLocalStorage`

### 2. **Complete CRUD Operations**
- **GetAllUsersAsync**: Lấy danh sách users với pagination và search
- **GetUserByIdAsync**: Lấy user theo ID
- **GetUserByUsernameAsync**: Lấy user theo username
- **CreateUserAsync**: Tạo user mới
- **UpdateUserAsync**: Cập nhật thông tin user
- **DeleteUserAsync**: Xóa user (soft delete)
- **HardDeleteUserAsync**: Xóa user vĩnh viễn

### 3. **Additional Operations**
- **CheckUsernameAsync**: Kiểm tra username có tồn tại không
- **GetCurrentUserAsync**: Lấy thông tin user hiện tại từ JWT token

### 4. **Error Handling**
- Standardized error responses với `ApiResponse<T>`
- Proper HTTP status code handling
- Exception catching và logging
- User-friendly error messages

## Cấu trúc

### Classes và DTOs
```csharp
// Response wrapper
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}

// API response cho danh sách users
public class UsersApiResponse
{
    public List<UserInfoDto> Users { get; set; }
    public UsersPaginationInfo Pagination { get; set; }
}

// Pagination info
public class UsersPaginationInfo
{
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage { get; set; }
    public bool HasPreviousPage { get; set; }
}

// Username check response
public class UsernameCheckResponse
{
    public string Username { get; set; }
    public bool Exists { get; set; }
}
```

## Cách sử dụng

### 1. **Dependency Injection Setup** (Program.cs)
```csharp
// Register UserApiClient with HttpClient
builder.Services.AddHttpClient<UserApiClient>(c => c.BaseAddress = new Uri(apiBase));
```

### 2. **Injection trong Blazor Component**
```razor
@inject UserApiClient UserApi
```

### 3. **Sử dụng trong code**

#### Lấy danh sách users với pagination
```csharp
var response = await UserApi.GetAllUsersAsync(pageNumber: 1, pageSize: 12, searchTerm: "john");
if (response.IsSuccess && response.Data != null)
{
    users = response.Data.Users;
    pagination = response.Data.Pagination;
}
```

#### Lấy thông tin user
```csharp
var response = await UserApi.GetUserByIdAsync(userId);
if (response.IsSuccess && response.Data != null)
{
    var user = response.Data;
    // Process user data
}
```

#### Tạo user mới
```csharp
var createDto = new CreateUserDto
{
    UserName = "john.doe",
    Password = "password123",
    FullName = "John Doe",
    // ... other properties
};

var response = await UserApi.CreateUserAsync(createDto);
if (response.IsSuccess)
{
    // Success - user created
}
```

#### Xóa user
```csharp
var response = await UserApi.DeleteUserAsync(userId);
if (response.IsSuccess)
{
    await LoadUsers(); // Refresh list
    // Show success message
}
else
{
    // Show error message
}
```

## Lợi ích

### 1. **Centralized API Logic**
- Tất cả API calls được centralized trong một class
- Dễ maintain và debug
- Consistent error handling

### 2. **Automatic Authentication**
- Không cần manually set headers trong components
- Token management được handle tự động
- Secure và consistent

### 3. **Type Safety**
- Strongly typed responses với generics
- IntelliSense support
- Compile-time error checking

### 4. **Reusability**
- Có thể sử dụng trong multiple components
- Consistent API interface
- Easy to test và mock

### 5. **Error Handling**
- Standardized error responses
- User-friendly error messages
- Proper HTTP status code handling

## Integration với MainViewAccount

### Trước khi có UserApiClient:
```csharp
// Direct HttpClient usage
var response = await Http.GetFromJsonAsync<UsersApiResponse>($"api/user?{queryString}");
// Manual header setting
// Manual error handling
```

### Sau khi có UserApiClient:
```csharp
// Clean và simple
var response = await UserApi.GetAllUsersAsync(pageNumber, pageSize, searchTerm);
if (response.IsSuccess && response.Data != null)
{
    users = response.Data.Users;
    pagination = response.Data.Pagination;
}
```

## Best Practices

1. **Always check IsSuccess** trước khi access Data
2. **Handle null responses** appropriately
3. **Show user-friendly error messages** từ response.Message
4. **Use async/await pattern** consistently
5. **Dispose timers và resources** properly

## Testing

UserApiClient có thể được easily mocked cho unit testing:

```csharp
// Mock UserApiClient
var mockUserApi = new Mock<UserApiClient>();
mockUserApi.Setup(x => x.GetAllUsersAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
          .ReturnsAsync(new ApiResponse<UsersApiResponse> { IsSuccess = true, Data = mockData });
```

## Future Enhancements

1. **Caching support** cho frequently accessed data
2. **Retry logic** cho failed requests
3. **Request cancellation** support
4. **Batch operations** support
5. **Real-time updates** với SignalR integration
