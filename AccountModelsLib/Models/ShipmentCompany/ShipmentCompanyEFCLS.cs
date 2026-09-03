using AccountModelsLib;
using AccountModelsLib.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
namespace AccountModelsLib.Services
{
    public class ShipmentCompanyEFCLS
    {
        // Assuming your DbContext is named AppDbContext
        // If using your custom context, replace 'AppDbContext' with your actual class name
        private readonly operations  _opp;

        public ShipmentCompanyEFCLS(operations  opp)
        {
            _opp = opp;
        }
        public ShipmentCompanyEFCLS( )
        {
            _opp = new operations();
        }

        /// <summary>
        /// Fetches all shipment companies (Used in WinForms and Razor Pages)
        /// </summary>
        public async Task<List<ShipmentCompany>> GetAllAsync()
        {
            return await _opp.db.ShipmentCompany.AsNoTracking().ToListAsync();
        }

        /// <summary>
        /// Adds a new company or updates an existing one
        /// </summary>
        public async Task SaveAsync(ShipmentCompany company)
        {
            if (company.Id == 0)
            {
                // Generate Snowflake ID if Id is not IDENTITY in database
                company.Id = CallSnowflakeIdGenerate.GenerateNewId();
                await _opp.db.ShipmentCompany.AddAsync(company);
            }
            else
            {
                _opp.db.ShipmentCompany.Update(company);
            }

            await _opp.db.SaveChangesAsync();
        }

        /// <summary>
        /// Batch save for multiple items (e.g., modified grid lists)
        /// </summary>
        public async Task SaveRangeAsync(List<ShipmentCompany> companies)
        {
            foreach (var item in companies)
            {
                if (item.Id == 0)
                {
                    item.Id = CallSnowflakeIdGenerate.GenerateNewId();
                    await _opp.db.ShipmentCompany.AddAsync(item);
                }
                else
                {
                    var trackedEntity = _opp.db.ShipmentCompany.Local.FirstOrDefault(e => e.Id == item.Id);

                    if (trackedEntity != null)
                    {
                        // Detach the existing instance so we can attach/update the new one
                        _opp.db.Entry(trackedEntity).State = EntityState.Detached;
                    }

                   
                    _opp.db.ShipmentCompany.Update(item);
                }
            }
            await _opp.db.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a company by ID
        /// </summary>
        public async Task<bool> DeleteAsync(long id)
        {
            var entity = await _opp.db.ShipmentCompany.FindAsync(id);
            if (entity == null)
                return false;

            _opp.db.ShipmentCompany.Remove(entity);
            await _opp.db.SaveChangesAsync();
            return true;
        }
    }
}