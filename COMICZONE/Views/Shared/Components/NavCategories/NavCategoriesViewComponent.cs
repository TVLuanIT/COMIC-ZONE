using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

public class NavCategoriesViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync()
    {
        // Các nhóm filter
        var filterGroups = new List<string>
        {
            "Tác giả",
            "Họa sĩ",
            "Dịch giả",
            "Series",
            "Tag",
            "Nhà phát hành",
            "NXB"
        };

        return Task.FromResult((IViewComponentResult)View(filterGroups));
    }
}