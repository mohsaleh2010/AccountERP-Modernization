using Microsoft.AspNetCore.Components; // هذا سيعمل فوراً بعد خطوة تعديل الـ SDK أعلاه
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AccountERP.SharedUI.Areas.Customers.Pages
{
    public partial class CustomerDataList : ComponentBase
    {
        // حقن الخدمات المطلوبة (قم بتغيير اسم الخدمة وفقاً لمشروعك)
        // [Inject] private ICustomerService CustomerService { get; set; }

        //protected List<CustomerViewModel> Customers { get; set; } = new List<CustomerViewModel>();
        //protected List<CustomerViewModel> FilteredCustomers { get; set; } = new List<CustomerViewModel>();

        //protected bool IsLoading { get; set; } = true;
        //protected string SearchTerm { get; set; } = string.Empty;

        //protected override async Task OnInitializedAsync()
        //{
        //    await LoadCustomersAsync();
        //}

        //private async Task LoadCustomersAsync()
        //{
        //    try
        //    {
        //        IsLoading = true;
        //        // هنا يتم استدعاء الميثود التي قمنا بتجهيزها سابقاً عبر الـ API أو الـ Service
        //        // var query = await CustomerService.GetCustomersQueryAsync();
        //        // Customers = CustomerService.GetCustomerViewModelList(query);

        //        // مؤقتاً: سنتركها فارغة لحين ربط الـ Service بالفورم
        //        Customers = new List<CustomerViewModel>();
        //        FilteredCustomers = Customers;
        //    }
        //    catch (Exception ex)
        //    {
        //        // التعامل مع الأخطاء (يمكنك إضافة الـ Logger الخاص بك هنا)
        //        Console.WriteLine(ex.Message);
        //    }
        //    finally
        //    {
        //        IsLoading = false;
        //    }
        //}

        //protected void HandleSearch()
        //{
        //    if (string.IsNullOrWhiteSpace(SearchTerm))
        //    {
        //        FilteredCustomers = Customers;
        //    }
        //    else
        //    {
        //        FilteredCustomers = Customers
        //            .Where(c => c.Name.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase) ||
        //                        (c.Code != null && c.Code.Contains(SearchTerm, StringComparison.OrdinalIgnoreCase)))
        //            .ToList();
        //    }
        //}

        //protected void RedirectToCreate()
        //{
        //    // منطق الانتقال لشاشة إضافة عميل جديد
        //}

        //protected void EditCustomer(int id)
        //{
        //    // منطق الانتقال لشاشة التعديل باستخدام الـ Id
        //}

        //protected async Task DeleteCustomer(int id)
        //{
        //    // منطق حذف العميل وتحديث القائمة
        //}
    }
}