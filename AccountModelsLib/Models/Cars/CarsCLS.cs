
using AccountModelsLib.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


using System.Data;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
namespace AccountModelsLib
{
    public class CarsCLS
    {
        public Car car = new Car();
        //public Customer customer = new Customer();
        AccountModelsLib.operations opp;
        public CarsCLS()
        {
            opp = new AccountModelsLib.operations();
        }
        public CarsCLS(ref AccountModelsLib.operations opp_)
        {
            opp = opp_;
        }
        public Car getCar()
        {
            return car;
        }
        public Car getCar(long Id)
        {
            car = opp.db.Cars.Where(c => c.Id == Id).FirstOrDefault();
            return car;
        }
        public void getCarById(long Id)
        {
            car = opp.db.Cars.Where(c => c.Id == Id).FirstOrDefault();

        }
        public Customer getCarCustomerOwner(Car car)
        {
             
            CustomerCLS customerCLS = new CustomerCLS(ref opp);
            
            customerCLS.GetCustomerById(ref opp,car.CustomerId);
            return  customerCLS.cs;
            
        }
        public List<CarViewModel> getCarViewModelById(long CarsId)
        {
            car = opp.db.Cars.AsNoTracking().Where(c => c.Id == CarsId).Include(c => c.Customer).Include(s => s.Lookup_CarShape).FirstOrDefault();
            List<CarViewModel> carViewModels = new List<CarViewModel>();
            List<Car> carsList = new List<Car>();

            carsList.Add(car);
            carViewModels = getCarViewModel(carsList);
            return carViewModels;
        }
        public List<CarViewModel> getCars(string SearchStr, long CustomerId )
        {
            SearchStr = SearchStr.Trim();
            List<CarViewModel> carViewModels = new List<CarViewModel>();
            IQueryable<Car> query = opp.db.Cars.AsNoTracking().Include(i => i.Customer).Include(i => i.Lookup_CarShape);

            query = query.Where(i => i.Customer.Active == 1 );
            query = query.Where(i => i.Active == 1 );


            
            if (SearchStr.Trim() != "")
            {
                query = query.Where(i => i.CarNumber.Contains(SearchStr) | i.DriverTel.Contains(SearchStr) | i.ChassisNumber.Contains(SearchStr) | i.Customer.Name.Contains(SearchStr) | i.Customer.Tel.Contains(SearchStr));
            }

            if (CustomerId > 0)
            {
                query = query.Where(i => i.CustomerId == CustomerId);
            }

            List<Car> carsList = query.ToList();
            carViewModels = getCarViewModel(carsList);
            return carViewModels;

        }
        public List<CarViewModel> getCarViewModel(List<Car> CarsList)
        {
            int Serial = 1;
            List<CarViewModel> carViewModels = new List<CarViewModel>();
            foreach (var item in CarsList)
            {
                CarViewModel cv = new CarViewModel();
                cv.Serial = Serial++;
                cv.Id = item.Id;
                cv.CustomerId = item.CustomerId;
                cv.ChassisNumber = item.ChassisNumber;
                cv.CarNumber = item.CarNumber;
                cv.CarType = item.CarType;
                cv.Model = item.Model;
                cv.EngineNumber = item.EngineNumber;
                cv.FuelType = item.FuelType;
                cv.Capacity = item.Capacity;
                cv.Year = item.Year;
                cv.TransmissionType = item.TransmissionType;
                cv.ColorCode = item.ColorCode;
                cv.CarColor = item.CarColor;

                cv.WarrantyStartDate = item.WarrantyStartDate;
                cv.WarrantyEndDate = item.WarrantyEndDate;
                cv.InsuranceCompany = item.InsuranceCompany;
                cv.Notes = item.Notes;
                cv.Lookup_CarShapeId = item.Lookup_CarShapeId;

                if (item.Customer != null)
                {
                    cv.CustomerName = item.Customer.Name;
                }
                if (item.Lookup_CarShape != null)
                {
                    cv.CarShapeName = item.Lookup_CarShape.Name;
                }

                cv.Active = item.Active;
                    

                carViewModels.Add(cv);
            }

            return carViewModels;

        }
        public void SaveCarWithCustomer(ref int Excp, ref string Msg, CustomerCLS cs)
        {
            AccountModelsLib.operations opp = new AccountModelsLib.operations(2);
            try
            {
                //CustomerCLS cs = new CustomerCLS();
                //cs.cs = customer;
                if (cs.cs.Id == 0)
                {
                    cs.AddNewCustomer(ref Excp, ref Msg, ref opp, cs);
                }
                else
                {
                    cs.EditCustomer(ref opp);
                }

                car.CustomerId = cs.cs.Id;
                if (car.Id == 0)
                {
                    AddCar(ref Excp, ref Msg, ref opp);
                }
                else
                {
                    EditCar(ref opp);
                }
                Excp = 0;
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();

            }

        }
        public void AddCarWithCustomer(ref int Excp, ref string Msg)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations(2);
            try
            {
                CustomerCLS cs = new CustomerCLS();

                cs.AddNewCustomer(ref Excp, ref Msg, ref opp, cs);

                car.CustomerId = cs.cs.Id;
                AddCar(ref Excp, ref Msg, ref opp);

                Excp = 0;
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();

            }

        }
        public void EditCarWithCustomer(ref int Excp, ref string Msg, CustomerCLS cs)
        {

            AccountModelsLib.operations opp = new AccountModelsLib.operations(2);
            try
            {
                //CustomerCLS cs = new CustomerCLS();
                //cs.cs = customer;
                cs.EditCustomer(ref opp);


                EditCar(ref opp);

                Excp = 0;
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                opp.Rollback();

            }

        }
        public void AddCar(ref int Excp, ref string Msg, ref AccountModelsLib.operations opp)
        {
            opp.db.Cars.Add(car);

            opp.db.SaveChanges();


        }
        public void EditCar(ref AccountModelsLib.operations opp)
        {
            opp.db.Entry(car).State = EntityState.Modified;
            opp.db.SaveChanges();

        }

        public void DeletCar(ref int Excp, ref string Msg, long CarId)
        {
            try
            {
                opp = new AccountModelsLib.operations(2);
                Car cc = new Car()
                {
                    Id = CarId
                };
                opp.db.Entry(cc).State = EntityState.Deleted;

                opp.db.SaveChanges();
                Excp = 0;
                Msg = "تم  حذف بيانات السيارة بنجاح";
                opp.Commit();
            }
            catch (Exception ee)
            {
                Excp = 1;
                Msg = " خطأ فى حذف بيانات السيارة ";
                Msg = Msg + ee.Message;
                opp.Rollback();
            }

        }

        public void SetActive(long carId, int active)
        {
            Car  car = opp.db.Cars.Where(c=>c.Id== carId).FirstOrDefault();
            if (car != null)
            {
                car.Active = active;
                opp.db.Entry(car).State = EntityState.Modified;
                opp.db.SaveChanges();
            }
        }

        public void ChangeCarOwner(ref int excp, ref string msg, long customerIdNew, CarViewModel carSelected)
        {
            try
            {
                opp.BeginTransaction();
                long NewId = 0;
                Car car = opp.db.Cars.Where(i => i.Id == carSelected.Id).FirstOrDefault();
                car.IsChangeOwner = 1;
                car.Active = 0;
                car.NewId = NewId;
                car.OldId = NewId;
                opp.db.Entry(car).State = EntityState.Modified;
                opp.db.SaveChanges();

                opp.db.Entry(car).State = EntityState.Detached;

                ////////////car New  ///////////
                Car carNew = new Car();
                carNew = car;
                carNew.Active = 1;
                carNew.CustomerId = customerIdNew;
                carNew.NewId = 0;
                carNew.OldId = car.Id;
                carNew.Id = 0;

                opp.db.Cars.Add(carNew);
                opp.db.SaveChanges();

                opp.Commit();
                excp = 0;
            }
            catch (Exception ee)
            {
                opp.Rollback();
                excp = 1;
                msg = ee.Message;
                 
            }
        }
    }
}
