﻿using Newtonsoft.Json.Linq;
using SocioboardDataServices.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Domain.Socioboard.Helpers;
using System.Text;
using MongoDB.Bson;

namespace SocioboardDataServices.Reports.FacebookReports
{
    public class FacebookPageReports
    {
        public static void CreateFacebookPageReport()
        {
            Helper.Cache cache = new Helper.Cache(Helper.AppSettings.RedisConfiguration);
            while (true)
            {
                DatabaseRepository dbr = new DatabaseRepository();
                List<Domain.Socioboard.Models.Facebookaccounts> lstFbAcc = dbr.Find<Domain.Socioboard.Models.Facebookaccounts>(t => t.IsAccessTokenActive && t.IsActive && t.FbProfileType == Domain.Socioboard.Enum.FbProfileType.FacebookPage).ToList();
                foreach (var item in lstFbAcc)
                {
                    if (item.lastpagereportgenerated.AddHours(24)<=DateTime.UtcNow)
                    {
                        CreateReports(item.FbUserId, item.AccessToken, item.Is90DayDataUpdated);
                        item.Is90DayDataUpdated = true;
                        item.lastpagereportgenerated = DateTime.UtcNow;
                        dbr.Update<Domain.Socioboard.Models.Facebookaccounts>(item);
                        cache.Delete(Domain.Socioboard.Consatants.SocioboardConsts.CacheTwitterMessageReportsByProfileId + item.FbUserId); 
                    }
                }
                Thread.Sleep(120000);
            } 
        }

        public static void CreateReports(string ProfileId, string AccessToken, bool is90daysupdated)
        {
            int day = 1;
            if (!is90daysupdated)
            {
                day = 90;
            }
            double since = SBHelper.ConvertToUnixTimestamp(DateTime.UtcNow.AddDays(-day));
            double until = SBHelper.ConvertToUnixTimestamp(DateTime.UtcNow);

            #region likes
            string facebookpageUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "?access_token=" + AccessToken;
            string outputfacepageUrl = getFacebookResponse(facebookpageUrl);
            JObject pageobj = JObject.Parse(outputfacepageUrl);

            string facebooknewfanUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_fan_adds?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputface = getFacebookResponse(facebooknewfanUrl);
            JArray likesobj = JArray.Parse(JArray.Parse(JObject.Parse(outputface)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region unlikes
            string facebookunlikjeUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_fan_removes?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceunlike = getFacebookResponse(facebookunlikjeUrl);
            JArray unlikesobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceunlike)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region impression
            string facebookimpressionUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceunimpression = getFacebookResponse(facebookimpressionUrl);
            JArray impressionobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceunimpression)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region impression user
            string facebookuniqueUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_unique?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceununoque = getFacebookResponse(facebookuniqueUrl);
            JArray uniqueobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceununoque)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region impression breakdown
            string facebookstory_typeUrl90 = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_by_story_type?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceunstory_type90 = getFacebookResponse(facebookstory_typeUrl90);
            JArray facebookstory_typeUrlobj90 = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceunstory_type90)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region impression breakdown organic
            string facebookorganic90 = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_organic?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceorganic90 = getFacebookResponse(facebookorganic90);
            JArray facebookorganicobj90 = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceorganic90)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region imression breakdowm viral
            string facebookviral90 = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_viral?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceviral90 = getFacebookResponse(facebookviral90);
            JArray facebookviralobj90 = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceviral90)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region impression breakdown paid
            string facebookpaid90 = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_paid?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfacepaid90 = getFacebookResponse(facebookpaid90);
            JArray facebookpaidobj90 = JArray.Parse(JArray.Parse(JObject.Parse(outputfacepaid90)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region page imression by age and gender
            string facebookimpressionbyage = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_impressions_by_age_gender_unique?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceimpressionbyage = getFacebookResponse(facebookimpressionbyage);
            JArray facebookimpressionbyageobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceimpressionbyage)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region story sharing
            string facebookstories = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_stories?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfacestories = getFacebookResponse(facebookstories);
            JArray facebookstoriesobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfacestories)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region sroty sharing by share type
            string facebooksharing_typeUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_stories_by_story_type?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceunsharing_type = getFacebookResponse(facebooksharing_typeUrl);
            JArray facebooksharing_typeUrlobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceunsharing_type)["data"].ToString())[0]["values"].ToString());
            #endregion
            #region story sharing by age and gender
            string facebooksharingagegenderUrl = "https://graph.facebook.com/v2.3/" + ProfileId + "/insights/page_storytellers_by_age_gender?pretty=0&since=" + since.ToString() + "&suppress_http_code=1&until=" + until.ToString() + "&access_token=" + AccessToken;
            string outputfaceunagegender = getFacebookResponse(facebooksharingagegenderUrl);
            JArray facebookagegenderUrlobj = JArray.Parse(JArray.Parse(JObject.Parse(outputfaceunagegender)["data"].ToString())[0]["values"].ToString());
            #endregion
            foreach (JObject obj in likesobj)
            {
                Domain.Socioboard.Models.Mongo.FacaebookPageDailyReports facebookReportViewModal = new Domain.Socioboard.Models.Mongo.FacaebookPageDailyReports();
                string key = obj["end_time"].ToString();
                JObject jounlikes = unlikesobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject joimpressionobj = impressionobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jouniqueobj = uniqueobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookstory_typeUrlobj = facebookstory_typeUrlobj90.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookorganicobj = facebookorganicobj90.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookviralobj = facebookviralobj90.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookpaidobj = facebookpaidobj90.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookimpressionbyageobj = facebookimpressionbyageobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookstoriesobj = facebookstoriesobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebooksharing_typeUrlobj = facebooksharing_typeUrlobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                JObject jofacebookagegenderUrlobj = facebookagegenderUrlobj.Children<JObject>().FirstOrDefault(o => o["end_time"].ToString() == key);
                DateTime dt = DateTime.Parse(key).Date;
                facebookReportViewModal.pageId = ProfileId;
                facebookReportViewModal.date = SBHelper.ConvertToUnixTimestamp(dt);
                try
                {
                    facebookReportViewModal.totalLikes = pageobj["likes"].ToString();
                }
                catch
                {
                    facebookReportViewModal.totalLikes = "0";
                }
                try
                {
                    facebookReportViewModal.talkingAbout = pageobj["talking_about_count"].ToString();
                }
                catch
                {
                    facebookReportViewModal.talkingAbout = "0";
                }
                try
                {
                    facebookReportViewModal.perDayLikes = obj["value"].ToString();
                }
                catch
                {
                    facebookReportViewModal.perDayLikes = "0";
                }
                try
                {
                    facebookReportViewModal.likes = Int32.Parse(obj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.likes = 0;
                }
                try
                {
                    facebookReportViewModal.unlikes = Int32.Parse(jounlikes["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.unlikes = 0;
                }
                try
                {
                    facebookReportViewModal.perDayUnlikes = jounlikes["value"].ToString();
                }
                catch
                {
                    facebookReportViewModal.perDayUnlikes = "0";
                }
                try
                {
                    facebookReportViewModal.impression = Int32.Parse(joimpressionobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impression = 0;
                }
                try
                {
                    facebookReportViewModal.perDayImpression = joimpressionobj["value"].ToString();
                }
                catch
                {
                    facebookReportViewModal.perDayImpression ="0";
                }
                try
                {
                    facebookReportViewModal.uniqueUser = Int32.Parse(jouniqueobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.uniqueUser = 0;
                }
                try
                {
                    facebookReportViewModal.uniqueUser = Int32.Parse(jouniqueobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.uniqueUser = 0;
                }
                try
                {
                    facebookReportViewModal.impressionFans = Int32.Parse(jofacebookstory_typeUrlobj["value"]["fan"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionFans = 0;
                }
                try
                {
                    facebookReportViewModal.impressionPagePost = Int32.Parse(jofacebookstory_typeUrlobj["value"]["page post"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionPagePost = 0;
                }
                try
                {
                    facebookReportViewModal.impressionuserPost = Int32.Parse(jofacebookstory_typeUrlobj["value"]["user post"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionuserPost = 0;
                }
                try
                {
                    facebookReportViewModal.impressionCoupn = Int32.Parse(jofacebookstory_typeUrlobj["value"]["coupon"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionCoupn = 0;
                }
                try
                {
                    facebookReportViewModal.impressionOther = Int32.Parse(jofacebookstory_typeUrlobj["value"]["other"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionOther = 0;
                }
                try
                {
                    facebookReportViewModal.impressionMention = Int32.Parse(jofacebookstory_typeUrlobj["value"]["mention"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionMention = 0;
                }
                try
                {
                    facebookReportViewModal.impressionCheckin = Int32.Parse(jofacebookstory_typeUrlobj["value"]["checkin"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionCheckin = 0;
                }
                try
                {
                    facebookReportViewModal.impressionQuestion = Int32.Parse(jofacebookstory_typeUrlobj["value"]["question"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionQuestion = 0;
                }
                try
                {
                    facebookReportViewModal.impressionEvent = Int32.Parse(jofacebookstory_typeUrlobj["value"]["event"].ToString());
                }
                catch
                {
                    facebookReportViewModal.impressionEvent = 0;
                }
                try
                {
                    facebookReportViewModal.viral = Int32.Parse(jofacebookviralobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.viral = 0;
                }
                try
                {
                    facebookReportViewModal.organic = Int32.Parse(jofacebookorganicobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.organic = 0;
                }
                try
                {
                    facebookReportViewModal.paid = Int32.Parse(jofacebookpaidobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.paid = 0;
                }
                try
                {
                    facebookReportViewModal.f_13_17 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.13-17"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_13_17 = 0;
                }
                try
                {
                    facebookReportViewModal.f_18_24 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.18-24"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_18_24 = 0;
                }
                try
                {
                    facebookReportViewModal.f_25_34 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.25-34"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_25_34 = 0;
                }
                try
                {
                    facebookReportViewModal.f_35_44 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.35-44"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_35_44 = 0;
                }
                try
                {
                    facebookReportViewModal.f_45_54 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.45-54"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_45_54 = 0;
                }
                try
                {
                    facebookReportViewModal.f_55_64 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.55-64"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_55_64 = 0;
                }
                try
                {
                    facebookReportViewModal.f_65 = Int32.Parse(jofacebookimpressionbyageobj["value"]["F.65+"].ToString());
                }
                catch
                {
                    facebookReportViewModal.f_65 = 0;
                }


                try
                {
                    facebookReportViewModal.m_13_17 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.13-17"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_13_17 = 0;
                }
                try
                {
                    facebookReportViewModal.m_18_24 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.18-24"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_18_24 = 0;
                }
                try
                {
                    facebookReportViewModal.m_25_34 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.25-34"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_25_34 = 0;
                }
                try
                {
                    facebookReportViewModal.m_35_44 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.35-44"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_35_44 = 0;
                }
                try
                {
                    facebookReportViewModal.m_45_54 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.45-54"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_45_54 = 0;
                }
                try
                {
                    facebookReportViewModal.m_55_64 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.55-64"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_55_64 = 0;
                }
                try
                {
                    facebookReportViewModal.m_65 = Int32.Parse(jofacebookimpressionbyageobj["value"]["M.65+"].ToString());
                }
                catch
                {
                    facebookReportViewModal.m_65 = 0;
                }
                try
                {
                    facebookReportViewModal.storyShare = Int32.Parse(jofacebookstoriesobj["value"].ToString());
                }
                catch
                {
                    facebookReportViewModal.storyShare = 0;
                }
                try
                {
                    facebookReportViewModal.perDayStoryShare = jofacebookstoriesobj["value"].ToString();
                }
                catch
                {
                    facebookReportViewModal.perDayStoryShare = "0";
                }
                try
                {
                    facebookReportViewModal.story_Fans = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["fan"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Fans = 0;
                }
                try
                {
                    facebookReportViewModal.story_PagePost = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["page post"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_PagePost = 0;
                }
                try
                {
                    facebookReportViewModal.story_UserPost = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["user post"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_UserPost = 0;
                }
                try
                {
                    facebookReportViewModal.story_Question = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["question"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Question = 0;
                }
                try
                {
                    facebookReportViewModal.story_Mention = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["mention"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Mention = 0;
                }
                try
                {
                    facebookReportViewModal.story_Other = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["other"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Other = 0;
                }
                try
                {
                    facebookReportViewModal.story_Coupon = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["coupon"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Coupon = 0;
                }
                try
                {
                    facebookReportViewModal.story_Event = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["event"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Event = 0;
                }
                try
                {
                    facebookReportViewModal.story_Checkin = Int32.Parse(jofacebooksharing_typeUrlobj["value"]["checkin"].ToString());
                }
                catch
                {
                    facebookReportViewModal.story_Checkin = 0;
                }

                try
                {
                    facebookReportViewModal.sharing_F_13_17 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.13-17"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_13_17 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_18_24 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.18-24"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_18_24 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_25_34 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.25-34"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_25_34 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_35_44 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.35-44"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_35_44 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_45_54 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.45-54"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_45_54 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_55_64 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.55-64"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_55_64 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_F_65 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.65+"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_F_65 = 0;
                }



                try
                {
                    facebookReportViewModal.sharing_M_13_17 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.13-17"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_13_17 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_18_24 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.18-24"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_18_24 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_25_34 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.25-34"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_25_34 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_35_44 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.35-44"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_35_44 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_45_54 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.45-54"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_45_54 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_55_64 = Int32.Parse(jofacebookagegenderUrlobj["value"]["M.55-64"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_55_64 = 0;
                }
                try
                {
                    facebookReportViewModal.sharing_M_65 = Int32.Parse(jofacebookagegenderUrlobj["value"]["F.65+"].ToString());
                }
                catch
                {
                    facebookReportViewModal.sharing_M_65 = 0;
                }
                facebookReportViewModal.id = ObjectId.GenerateNewId();
                MongoRepository mongorepo = new MongoRepository("FacaebookPageDailyReports");
                var ret = mongorepo.Find<Domain.Socioboard.Models.Mongo.FacaebookPageDailyReports>(t => t.date == facebookReportViewModal.date);
                var task=Task.Run(async()=>{
                    return await ret;
                });
                if (task.Result!=null)
                {
                    if (task.Result.Count() < 1)
                    {
                        mongorepo.Add<Domain.Socioboard.Models.Mongo.FacaebookPageDailyReports>(facebookReportViewModal);
                    } 
                }
            }




        }

        public static string getFacebookResponse(string Url)
        {
            var facebooklistpagerequest = (HttpWebRequest)WebRequest.Create(Url);
            facebooklistpagerequest.Method = "GET";
            facebooklistpagerequest.Credentials = CredentialCache.DefaultCredentials;
            facebooklistpagerequest.AllowWriteStreamBuffering = true;
            facebooklistpagerequest.ServicePoint.Expect100Continue = false;
            facebooklistpagerequest.PreAuthenticate = false;
            string outputface = string.Empty;
            try
            {
                using (var response = facebooklistpagerequest.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream(), Encoding.GetEncoding(1252)))
                    {
                        outputface = stream.ReadToEnd();
                    }
                }
            }
            catch (Exception e) { }
            return outputface;
        }
    }
}