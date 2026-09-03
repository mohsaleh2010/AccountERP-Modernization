using AccountModelsLib;
 
 
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace AccountModelsLib.Models
{
    public class ServiceCategoryService
    {
        private   operations _opp;

        public ServiceCategoryService(operations opp)
        {
            _opp = opp;
        }
        public ServiceCategoryService( )
        {
            _opp = new operations() ;
        }

        public List<LookUp_ItemServiceCategory> GetAllCategories()
        {
            return _opp.db.LookUp_ItemServiceCategory.OrderBy(c => c.ServiceCategoryName).ToList();
        }
        public BindingList<LookUp_ItemServiceCategory> GetAllCategories(int selectedGender)
        {
            _opp = new operations();

            var query = _opp.db.LookUp_ItemServiceCategory.AsQueryable();

            if (selectedGender >-1)
                query = query.Where(x => x.GenderTypeId == selectedGender);

            //BindingList<LookUp_ItemServiceCategory> result = new BindingList<LookUp_ItemServiceCategory>();

         var   result = new BindingList<LookUp_ItemServiceCategory>(query.ToList());

            //result = query.ToList();
                return result;
        }
        public LookUp_ItemServiceCategory GetCategoryById(long id)
        {
            return _opp.db.LookUp_ItemServiceCategory.FirstOrDefault(c => c.Id == id);
        }

        public void AddCategory(LookUp_ItemServiceCategory category)
        {
            _opp.db.LookUp_ItemServiceCategory.Add(category);
            _opp.db.SaveChanges();
        }

        public void UpdateCategory(LookUp_ItemServiceCategory category)
        {
            var existing = _opp.db.LookUp_ItemServiceCategory.FirstOrDefault(c => c.Id == category.Id);
            if (existing != null)
            {
                _opp.db.Entry(category).State= EntityState.Modified;
                //existing.ServiceCategoryName = category.ServiceCategoryName;
                _opp.db.SaveChanges();
            }
        }

        public void DeleteCategory(long id)
        {
            var category = _opp.db.LookUp_ItemServiceCategory.FirstOrDefault(c => c.Id == id);
            if (category != null)
            {
                _opp.db.LookUp_ItemServiceCategory.Remove(category);
                _opp.db.SaveChanges();
            }
        }
    }
}
