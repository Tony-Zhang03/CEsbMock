﻿using EsbMockEntity;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Services;
using System.Xml;

namespace CtripEsbAsmx
{
    /// <summary>
    /// Summary description for WebService1
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line.
    // [System.Web.Script.Services.ScriptService]
    public class WebService1 : System.Web.Services.WebService
    {
        [WebMethod]
        public string Request(string requestXML)
        {
            //Database.SetInitializer(new DropCreateDatabaseIfModelChanges<MockEntity>());
            
            XmlDocument reqXml = new XmlDocument();
            reqXml.LoadXml(requestXML);
            
            var xmlReqTmp = reqXml.SelectSingleNode("Request/Header") as XmlElement;
            var requestType = xmlReqTmp.Attributes["RequestType"].InnerText;

            reqXml.removeUnNeededTag();
            var reqXmlToCompare = reqXml.InnerXml;

            XmlDocument xmlPattern = new XmlDocument();

            var res = string.Empty;
            ConfigurationOptions config = new ConfigurationOptions
            {
                EndPoints =
                        {
                            { "10.2.24.151", 6388}
                        }
            };

            var redis = ConnectionMultiplexer.Connect(config).GetDatabase(10);
            res = redis.StringGet(reqXmlToCompare.GetHashCode().ToString());
            if (!string.IsNullOrEmpty(res))
            {
                redis.StringSetAsync(reqXmlToCompare.GetHashCode().ToString(), res, expiry: new TimeSpan(1, 0, 0));

                var a = JObject.Parse(res);
                var reqTmp = a["request"].ToString();
                var resTmp = a["response"].ToString();

                res = reqTmp.Equals(reqXmlToCompare) ? resTmp : string.Empty;
            }

            if (string.IsNullOrEmpty(res))
            {
                
                    var m =
                        from info in Temp.ML
                        where
                            info.RequestType.RequestType.Equals(requestType) &&
                            info.RequestXml.Equals(requestXML)
                        select
                            info;

                    if (m.Count() > 0)
                    {
                        res = m.First().ResponseXml;
                    }

                //using (var db = new MockMessageEntity())
                //{
                //    var m =
                //        from info in db.MockMessages
                //        where
                //            info.RequestType.RequestType.Equals(requestType) &&
                //            info.RequestXml.Equals(requestXML)
                //        select
                //            info;

                //    if (m.Count() > 0)
                //    {
                //        res = m.First().ResponseXml;
                //    }
                //}
            }
            return res;
        }
    }
}