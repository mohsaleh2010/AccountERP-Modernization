using System.Collections.Generic;
using AccountModelsLib.Models;

namespace AccountERP.SharedUI.ViewModels
{
    public class CustomerListViewModel
    {
        public long? SelectedCustomerId { get; set; }
        public string SearchName { get; set; }
        public string CustomerTypeFilter { get; set; } = "All"; // متنوعون أو دائمون
        public bool IsActiveOnly { get; set; } = true;

        // قائمة النتائج التي ستعرض في الجدول
        public List<Customer> Customers { get; set; } = new List<Customer>();
        public List<CustomerViewModel> CustomersLstVM { get; set; } = new List<CustomerViewModel>();

        
    }
}