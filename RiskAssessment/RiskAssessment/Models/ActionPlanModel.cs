using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{

    public class ActionPlanModel
    {
        public int AssessmentNo { get; set; }
        public int AssessTypeID { get; set; }
        public List<HomeSafetyActionPlanModel> HSList { get; set; }
        public List<DietActionPlanModel> DietList { get; set; }

    }


    public class HomeSafetyActionPlanModel
    {
        public string Risk { get; set; }
        public string ActionRequired { get; set; }
        public string Location { get; set; }
        public string Who { get; set; }
        public string Status { get; set; }
        public string HSimg { get; set; }

    }

    public class DietActionPlanModel
    {
        public string Risk { get; set; }
        public string Nutirient { get; set; }
        public string FoodName { get; set; }
        public string Description { get; set; }
        public int Ranking { get; set; }
        public string FoodImg { get; set; }

    }
}