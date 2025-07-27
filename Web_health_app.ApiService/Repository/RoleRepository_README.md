# Role Repository - Auto RoleId Generation

## Mô tả
Repository này đã được cập nhật để tự động tạo RoleId từ RoleName sử dụng function SQL `fn_RemoveVietnameseDiacritics`.

## Thay đổi chính

### 1. Auto-generated RoleId
- **Trước**: User cần nhập RoleId thủ công
- **Sau**: RoleId được tạo tự động từ RoleName
- **Logic**: `RoleId = fn_RemoveVietnameseDiacritics(REPLACE(role_name, ' ', ''))`

### 2. CreateRoleDto Update
```csharp
// Trước
public class CreateRoleDto
{
    [Required] public string RoleId { get; set; }
    [Required] public string RoleName { get; set; }
    public bool IsActive { get; set; } = true;
}

// Sau
public class CreateRoleDto
{
    [Required] public string RoleName { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 3. CreateRoleAsync Method
```csharp
public async Task<RoleInfoDto> CreateRoleAsync(CreateRoleDto createRoleDto)
{
    // Auto-generate RoleId using SQL function
    var generatedRoleId = await _context.Database
        .SqlQueryRaw<string>("SELECT dbo.fn_RemoveVietnameseDiacritics(REPLACE({0}, ' ', '')) AS Value", createRoleDto.RoleName)
        .FirstOrDefaultAsync();

    // Check if generated role ID already exists
    if (await RoleIdExistsAsync(generatedRoleId))
    {
        throw new InvalidOperationException($"Generated Role ID '{generatedRoleId}' already exists");
    }

    var role = new Role
    {
        RoleId = generatedRoleId,
        RoleName = createRoleDto.RoleName,
        IsActive = createRoleDto.IsActive
    };
    
    // ... save logic
}
```

## Ví dụ sử dụng

### Input RoleName → Output RoleId
- "Quản trị viên" → "Quantrivien"
- "Nhân viên kế toán" → "Nhanvienkekoan"
- "Trưởng phòng IT" → "TruongphongIT"
- "Giám đốc điều hành" → "Giamdocdieuhanh"

### API Call
```json
POST /api/role
{
    "roleName": "Quản trị viên",
    "isActive": true
}

Response:
{
    "roleId": "Quantrivien",
    "roleName": "Quản trị viên",
    "isActive": true,
    "permissions": [],
    "permissionCount": 0
}
```

## UI Changes

### AddRole.razor
- Loại bỏ trường "Mã vai trò" 
- Thêm thông báo: "Mã vai trò sẽ được tạo tự động từ tên vai trò"
- Tối ưu layout form

## Error Handling

### 1. Duplicate RoleId
```csharp
throw new InvalidOperationException($"Generated Role ID '{generatedRoleId}' already exists");
```

### 2. Failed Generation
```csharp
throw new InvalidOperationException("Failed to generate Role ID from Role Name");
```

## SQL Function Dependency

### fn_RemoveVietnameseDiacritics
Function này phải tồn tại trong database để:
- Loại bỏ dấu tiếng Việt
- Loại bỏ khoảng trắng
- Tạo ra RoleId hợp lệ

## Testing

### Test Cases
1. **Normal Case**: "Quản trị viên" → "Quantrivien"
2. **With Spaces**: "Nhân viên IT" → "NhanvienIT"
3. **Special Characters**: "Trưởng phòng (IT)" → "TruongphongIT"
4. **Duplicate Check**: Tạo role với tên tương tự phải báo lỗi

### Sample Test
```csharp
[Test]
public async Task CreateRole_ShouldGenerateCorrectRoleId()
{
    var createDto = new CreateRoleDto
    {
        RoleName = "Quản trị viên",
        IsActive = true
    };

    var result = await _roleRepository.CreateRoleAsync(createDto);

    Assert.AreEqual("Quantrivien", result.RoleId);
    Assert.AreEqual("Quản trị viên", result.RoleName);
}
```

## Migration Note

Đây là breaking change cho:
- Frontend forms cần bỏ trường RoleId input
- API clients cần cập nhật request structure
- Existing roles không bị ảnh hưởng
