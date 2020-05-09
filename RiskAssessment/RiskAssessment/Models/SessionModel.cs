using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{
    public class SessionModel
    {
        public int AssessmentNo { get; set; }
        public Guid SessionID { get; set; }
        public int AssessmentTypeID { get; set; }
        public string AssessmentType { get; set; }
        public DateTime TimeStamp { get; set; }        
    }
}