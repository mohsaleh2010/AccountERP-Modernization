using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccountModelsLib.Models
{
    public class ResponseAfterSend
    {
        private List<RejectedDocument> rejectedDocuments1;

        public List<AcceptedDocument> acceptedDocuments { get; set; }

        public List<RejectedDocument> rejectedDocuments
        {
            get
            {
                return rejectedDocuments1;
            }
            set
            {
                rejectedDocuments1 = value;

                string message = "";
                foreach (var item in rejectedDocuments)
                    message += $@"{item.error.ToString()}
";
                Errors = message;
            }
        }



        public string submissionId { get; set; }
        public string Errors { get; set; }

        public class Detail
        {
            public string code { get; set; }
            public string message { get; set; }
            public string target { get; set; }
            public string propertyPath { get; set; }
            public object details { get; set; }

        }

        public class Error
        {
            public string code { get; set; }
            public string message { get; set; }
            public string target { get; set; }
            public object propertyPath { get; set; }
            public List<Detail> details { get; set; }

            public override string ToString()
            {
                string s = "";
                foreach (var item in details)
                    s += $@"{item.message}
";
                return s;
            }
        }

        public class RejectedDocument
        {
            public string internalId { get; set; }
            public Error error { get; set; }
        }
        public class AcceptedDocument
        {
            public string hashKey { get; set; }

            public string internalId { get; set; }

            public string longId { get; set; }

            public string uuid { get; set; }
        }
    }
}
