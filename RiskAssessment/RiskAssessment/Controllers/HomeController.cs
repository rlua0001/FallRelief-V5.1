using RiskAssessment.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Mvc;

namespace RiskAssessment.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult testflaticon()
        {
            return View();

        }
        public ActionResult Index()
        {
            return View();
           
        }
      
        public ActionResult Assessment()
        {
            var _ctx = new fall_reliefEntities();
            ViewBag.Assessment = _ctx.tbl_RiskAss_Assessment.Where(x => x.IsActive == true).Select(x => new { x.AssessmentTypeID, x.AssessmentType }).ToList();

            SessionModel _model = null;

            if (Session["SessionModel"] == null)
                _model = new SessionModel();
            else
                _model = (SessionModel)Session["SessionModel"];


            return View(_model);

        }

        public ActionResult Instruction(SessionModel model)
        {

            if (model != null) 
            {
                var _ctx = new fall_reliefEntities();
                var assessment = _ctx.tbl_RiskAss_Assessment.Where(x => x.AssessmentTypeID == model.AssessmentTypeID).FirstOrDefault();
                if (assessment != null) 
                {
                    ViewBag.AssessmentType = assessment.AssessmentType;
                    ViewBag.AssessmentDescription = assessment.Description;
                
                }
            }
                Session["SessionModel"] = model;

            if (model == null)   
                return RedirectToAction("Assessment");
            

            return View (model);
        }

        public ActionResult RiskAssessment(SessionModel model)
        {
            if (model != null)
            {
                Session["SessionModel"] = model;
            }

            if (model == null)
            {
                return RedirectToAction("Assessment");
            }

            var _ctx = new fall_reliefEntities();
            //To create new assessment
            tbl_RiskAss_Session newAssessment = new tbl_RiskAss_Session()
            {
                timeStamp = DateTime.UtcNow,
                AssessmentTypeID = model.AssessmentTypeID,
                sessionID = Guid.NewGuid()

            };
            _ctx.tbl_RiskAss_Session.Add(newAssessment);
            _ctx.SaveChanges();

            this.Session["SessionID"] = newAssessment.sessionID;

            return RedirectToAction("QuestionAssessment", new { @SessionID = Session["SessionID"]});
        }


        public ActionResult QuestionAssessment(Guid SessionID, int? qno)
        {

            if (SessionID == null)
            {
                return RedirectToAction("Assessment");

            }

            var _ctx = new fall_reliefEntities();

            var asessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();

            if (asessment == null)
            {
                return RedirectToAction("Assessment");
            }

            if (qno.GetValueOrDefault() < 1)
                qno = 1;

            var assQuestionID = _ctx.tbl_RiskAss_AssQuestion
                .Where(x => x.AssessmentTypeID == asessment.AssessmentTypeID && x.QuestionNumber == qno)
                .Select(x => x.ID).FirstOrDefault();



            if (assQuestionID > 0)
            {
                var _model = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.ID == assQuestionID)
                    .Select(x => new QuestionModel()
                    {

                        Question = x.tbl_RiskAss_Questions.Question,
                        QuestionNumber = x.QuestionNumber,
                        QuestionType = x.tbl_RiskAss_Questions.AnswerType,
                        AssessmentTypeID = x.AssessmentTypeID,
                        AssessmentType = x.tbl_RiskAss_Assessment.AssessmentType,
                        QuestionSection = x.tbl_RiskAss_Questions.QuestionSection,
                        Options = x.tbl_RiskAss_Questions.tbl_RiskAss_ResponseChoice.Select(y => new QRmodel()
                        {
                            ResponseID = y.ID,
                            Response = y.Response
                        }).ToList()
                    }).FirstOrDefault();


                //now if it is already answered ealier, set the choice of the user
                var savedAnswers = _ctx.tbl_RiskAss_AssessmentResponse.Where(x => x.AssQuestionID == assQuestionID && x.AssessmentNo == asessment.AssessmentNo)
                    .Select(x => new { x.responseID, x.Answer }).ToList();

                foreach (var savedAnswer in savedAnswers)
                {
                    _model.Options.Where(x => x.ResponseID == savedAnswer.responseID).FirstOrDefault().Answer = savedAnswer.Answer;
                }

                _model.TotalQuestionNo = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.AssessmentTypeID == asessment.AssessmentTypeID).Count();

                return View(_model);
            }

            else
            {

                return View("Error");
            }

        }
        [HttpPost]
        public ActionResult PostAnswer(AnswerModel repsonses)
        {
            var _ctx = new fall_reliefEntities();
            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(repsonses.SessionID)).FirstOrDefault();

            var AssQuestionInfo = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.AssessmentTypeID == assessment.AssessmentTypeID && x.QuestionNumber == repsonses.QuestionID)
                .Select(x => new
                {
                    QID = x.QuestionID,
                    AssQuestionID = x.ID,
                    QType = x.tbl_RiskAss_Questions.AnswerType,
                    QSection = x.tbl_RiskAss_Questions.QuestionSection,
                    Rscore = x.tbl_RiskAss_Questions.RiskScore
                }).FirstOrDefault();

            var existingRecord = _ctx.tbl_RiskAss_AssessmentResponse.Where(x => x.AssessmentNo == assessment.AssessmentNo && x.AssQuestionID == AssQuestionInfo.AssQuestionID).FirstOrDefault();

            //insert reponses into database
            if (existingRecord == null)
            {
                var ReponseResult = (
                 from a in _ctx.tbl_RiskAss_ResponseChoice
                 join b in repsonses.UserSelectedID on a.ID equals b
                 select new { a.ID, a.RiskScore }).AsEnumerable()
                 .Select(x => new tbl_RiskAss_AssessmentResponse()
                 {
                     id = Guid.NewGuid(),
                     AssessmentNo = assessment.AssessmentNo,
                     AssQuestionID = AssQuestionInfo.AssQuestionID,
                     responseID = x.ID,
                     Answer = "Checked"
                 }).ToList();

                _ctx.tbl_RiskAss_AssessmentResponse.AddRange(ReponseResult);
                _ctx.SaveChanges();
            }

            //update existing record
            else
            {
                var dbRecord = _ctx.tbl_RiskAss_AssessmentResponse.Find(existingRecord.id);

                //need to check this

                var ReponseResult = (
                 from a in _ctx.tbl_RiskAss_ResponseChoice
                 join b in repsonses.UserSelectedID on a.ID equals b
                 select new { a.ID, a.RiskScore }).AsEnumerable()
                 .Select(x => new tbl_RiskAss_AssessmentResponse()
                 {
                     responseID = x.ID,
                     Answer = "Checked"
                 }).FirstOrDefault();

                dbRecord.responseID = ReponseResult.responseID;
                dbRecord.Answer = ReponseResult.Answer;

                _ctx.SaveChanges();

            }

            //get the next question depending on the direction
            var nextQuestionNumber = 1;

            if (repsonses.Direction.Equals("forward", StringComparison.CurrentCultureIgnoreCase))
            {
                nextQuestionNumber = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.AssessmentTypeID == repsonses.AssessmentTypeID
                && x.QuestionNumber > repsonses.QuestionID).OrderBy(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault();

            }
            else
            {
                nextQuestionNumber = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.AssessmentTypeID == repsonses.AssessmentTypeID
                && x.QuestionNumber < repsonses.QuestionID).OrderByDescending(x => x.QuestionNumber).Take(1).Select(x => x.QuestionNumber).FirstOrDefault();

            }

            if (nextQuestionNumber < 1)
                nextQuestionNumber = 1;

            return RedirectToAction("QuestionAssessment", new
            {
                @SessionID = Session["SessionID"],
                @qno = nextQuestionNumber
            });

        }


        public ActionResult AssessmentResult(Guid SessionID) {

            var _ctx = new fall_reliefEntities();

            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();
            var RiskLevelResult = _ctx.vw_RiskAss_RiskLevel.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo)).FirstOrDefault();

            var _model =
                  _ctx.vw_RiskAss_RiskLevel.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
                  .Select(x => new RiskLevelModel()
                  {
                      AssessmentNo = x.AssessmentNo,
                      AssessmentType = x.AssessmentType,
                      RiskList = _ctx.vw_RiskAss_RiskStatement.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new RiskStatementModel() 
                      { 
                          RiskID = z.RiskID,
                          RiskName = z.Risk,
                          RiskStatement = z.Risk_Statement,
                          ImgDir = z.RiskImg
                     
                      }).ToList()

                  }).FirstOrDefault();

            ViewBag.AssessmentType = assessment.tbl_RiskAss_Assessment.AssessmentType;
            ViewBag.RiskScore = RiskLevelResult.RiskScore;
            ViewBag.RiskLevel = RiskLevelResult.RiskLevel;

            return View(_model);
        }



        public ActionResult ActionPlan(Guid SessionID)
        {

            var _ctx = new fall_reliefEntities();

            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();
            var assessmentType = assessment.tbl_RiskAss_Assessment.AssessmentType.FirstOrDefault();
        



            //check the assessment type - Health Assessment
            if (assessment.AssessmentTypeID == 1)
            {
                var ActionPlan =
              _ctx.vw_ActionPlan_Nutrition.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
              .Select(x => new ActionPlanModel()
              {
                  AssessmentNo = x.AssessmentNo,
                  AssessTypeID = assessmentType,
                  DietList = _ctx.vw_ActionPlan_Nutrition.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new DietActionPlanModel()
                  {
                      Risk = z.risk,
                      Nutirient = z.Nutrient,
                      FoodName= z.FoodName,
                      

                    //--need to insert description and images
                  }).ToList()

              }).FirstOrDefault();

                return View(ActionPlan);
            }
            //Home Safety Assessment
            else {

                var ActionPlan =
                  _ctx.vw_ActionPlan_HomeSafety.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
                  .Select(x => new ActionPlanModel()
                  {
                      AssessmentNo = x.AssessmentNo,
                      AssessTypeID = assessmentType,
                      HSList = _ctx.vw_ActionPlan_HomeSafety.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new HomeSafetyActionPlanModel()
                      {
                          Risk = z.Risk,
                          ActionRequired= z.Action,
                          Location = z.Location_Room,
                          Who = z.Who,
                          Status = z.Status

                      }).ToList()

                  }).FirstOrDefault();

                return View(ActionPlan);
            }
 
        }
    }
}