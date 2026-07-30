namespace DynamicFormBuilder.API.DTOs.Menu;


public class MenuResponseDto
{
    public int MenuId { get; set; }

    public int? ParentMenuId { get; set; }

    public string MenuName { get; set; } = null!;

    public int? DisplayOrder { get; set; }

    public string? Href { get; set; }

    public List<MenuResponseDto> SubMenus { get; set; } = new();
}