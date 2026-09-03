using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace AccountModelsLib.Models
{
    public class DataDictionaryLogerExceptionCLS : ApplicationException
    {

        public DataDictionaryLogerExceptionCLS()
        {

        }
        public void AddNew(DataDictionaryLogerException dl)
        {

        }
        public void AddException(Exception ee, string MethodName,string InnerException)
        {
            operations opp = new operations();
            AddException(ref opp, ee, MethodName,  InnerException);
        }
        public void AddException(ref operations opp, Exception ee, string MethodName,string InnerException)
        {
            if(opp == null) {
            opp= new operations();
            }
            if(ee == null)
            {
                return;
            }
            DataDictionaryLogerException exl = new DataDictionaryLogerException();
            var Exception = ee;
            try
            {
                exl.Exception = Exception.ToString();
            }
            catch
            {
            }
            try
            {
                exl.Message = ee.Message;
            }
            catch
            {
            }
            try
            {
                if (ee.InnerException != null  )
                {
                    if (  ee.InnerException.InnerException != null)
                    {
                        if (ee.InnerException.InnerException.Message.Trim() == InnerException.Trim())
                        {
                            exl.InnerException = InnerException;
                        }
                        else
                        {
                            exl.InnerException = InnerException + "  " + ee.InnerException.InnerException.Message;
                        }
                    }
                }
                else
                {
                    exl.InnerException = InnerException;

                }
            }
            catch
            {
            }
            try
            {
                exl.Source = ee.Message;
            }
            catch
            {
            }
            try
            {
                exl.StackTrace = ee.Message;
            }
            catch
            {
            }
            try
            {
                exl.MethodName = MethodName;
            }
            catch
            {
            }

            exl.ExcpDateTime = DateTime.Now;
            try
            {
                opp.db.DataDictionaryLogerExceptions.Add(exl);
                opp.db.SaveChanges();
            }
            catch(Exception eee)
            {
            }

        }

    }
}
