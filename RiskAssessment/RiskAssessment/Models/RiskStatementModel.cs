using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{
    public class RiskLevelModel
    {
        public int AssessmentNo { get; set; }
        public int AssessmentTypeID { get; set; }
        public string AssessmentType { get; set; }
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; }
        public List<RiskStatementModel> RiskList { get; set; }

    }

    public class RiskStatementModel
    {
        public int RiskID { get; set; }
        public string RiskName { get; set; }
        public string RiskStatement { get; set; }
        public string ImgDir { get; set; }

    }

}