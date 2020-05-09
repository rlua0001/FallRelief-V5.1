using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{
    public class QuestionModel
    {
        public int TotalQuestionNo { get; set; }
        public int QuestionNumber { get; set; }
        public int AssessmentTypeID { get; set; }
        public string AssessmentType { get; set; }
        public string Question { get; set; }
        public string QuestionSection { get; set; }
        public int RiskScore { get; set; }
        public string QuestionType{ get; set; }
        public List<QRmodel> Options { get; set; }
        
        
    }

    public class QRmodel // question response modelte
    {
        public int ResponseID { get; set; }
        public string Response { get; set; }
        public string Answer { get; set; }
    }
}