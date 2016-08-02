using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
//using GoogleSearchAPIClient;
using System.Threading.Tasks;
using GoogleSearchUrlRankOperation.Models;
using System.Net.Http;
using System.Configuration;

namespace GoogleSearchUrlRankOperation.Controllers
{
    public class SearchController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public async Task<ActionResult> HandleForm(GoogleSearchRequest googlesearchrequest)
        {
            GoogleSearchAPIRequest obj_APIRequest = new GoogleSearchAPIRequest(googlesearchrequest.SearchTerms, 1);

            GoogleSearchResponse obj_Response = new GoogleSearchResponse(googlesearchrequest.UrlToFind);

            bool b_NoSearchError = true; // flag for any errors picked up during GoofgleSearchAPI requests.
            int int_StartIndex = 1; // index in Google Search is 1-based not zero-based. So we start at 1 instead of zero.

            while (int_StartIndex <= 91 && b_NoSearchError)
            {

                await obj_APIRequest.FireAPISeachRequest();

                while (obj_APIRequest.RequestInProcess())
                {

                }

                if (obj_APIRequest.APIRequestState == GoogleSearchAPIRequest.GoogleSearchAPIRequestState.ResponseComplete) // if request returned successfully, process the result
                {
                    // Process the actual JSON string returned from the GoogleSearchAPI.
                    // Parse it into a custom object we can utitlise to access the results more easily.
                    obj_Response.AddResultsFromAPI_JSONResponse(obj_APIRequest.GetAPISearchResponse());
                }
                else
                {
                    b_NoSearchError = false;
                    //_lbl_ResponseAsText.Text = _obj_SearchRequest.GetAPISearchError();
                }

                int_StartIndex += obj_APIRequest.ResultsPerRequest;
            }

            return View("Results", obj_Response);
        }

        //public ActionResult Results (GoogleSearchResponse obj_Response)
        //{
        //    return RedirectToAction("View", obj_Response);
        //}
    }

    public class GoogleSearchAPIRequest
    {
        private string _mStr_SearchTerms;
        private string _mStr_SearchError;
        private string _mStr_Response;
        private int _mInt_StartIndex;
        private GoogleSearchAPIRequestState _mEn_RequestState;

        public int ResultsPerRequest
        {
            get
            {
                return 10;
            }
        }

        public GoogleSearchAPIRequest(string str_SearchTerms)
        {
            this._mEn_RequestState = GoogleSearchAPIRequestState.Ready;
            this._mStr_SearchTerms = str_SearchTerms;
            this._mInt_StartIndex = 0;
        }

        public GoogleSearchAPIRequest(string str_SearchTerms, int int_StartIndex)
        {
            this._mEn_RequestState = GoogleSearchAPIRequestState.Ready;
            this._mStr_SearchTerms = str_SearchTerms;
            this._mInt_StartIndex = int_StartIndex;
        }

        public async Task<bool> FireAPISeachRequest()
        {
            bool _b_Result = false;
            try
            {
                if (this._mEn_RequestState != GoogleSearchAPIRequestState.Ready)
                {
                    throw new Exception("Request has already been fired with this instance");
                }

                if (this._mStr_SearchTerms == null)
                {
                    throw new Exception("Search terms can not be NULL");
                }

                // Instantiating object of HttpClient
                HttpClient _obj_HttpClient = new HttpClient();

                // Setting base URL for HttpClient object to Google Custom Search API's url
                _obj_HttpClient.BaseAddress = new Uri(ConfigurationManager.AppSettings["GoogleCustomSearchAPIUrl"]);
                //_obj_HttpClient.BaseAddress = new Uri(GoogleSearchAPIRequestUrl.APIRequestUrl());
                _obj_HttpClient.DefaultRequestHeaders.Accept.Clear();

                // Set time out length for request
                TimeSpan _obj_TmSpn = new TimeSpan(0, 0, 0, 30, 0);
                _obj_HttpClient.Timeout = _obj_TmSpn;

                // Setting request mode is now in requesting
                this._mEn_RequestState = GoogleSearchAPIRequestState.Requesting;

                // Firing asynchronus request to Google
                HttpResponseMessage _obj_Response
                    = await _obj_HttpClient.GetAsync(
                        "?key=" + ConfigurationManager.AppSettings["GoogleCustomSearchAPIKey"] +
                        "&q=" + HttpUtility.UrlEncode(_mStr_SearchTerms) +
                        "&cx=" + ConfigurationManager.AppSettings["GoogleCustomSearchAPICX"] +
                        "&start=" + _mInt_StartIndex.ToString() +
                        "&googlehost=google.com.au");

                if (_obj_Response != null && _obj_Response.IsSuccessStatusCode)
                {
                    // Response is fully consumed by program and in state ready to be returned to the user!

                    _mStr_Response = await _obj_Response.Content.ReadAsStringAsync();

                    this._mEn_RequestState = GoogleSearchAPIRequestState.ResponseComplete;

                    _b_Result = true;
                }
                else
                {
                    throw new Exception("Request to Google Custom Search API failed");
                }
            }
            catch (Exception e)
            {
                this._mEn_RequestState = GoogleSearchAPIRequestState.Error;
                this._mStr_SearchError = e.Message;
            }

            // return the overal result of the attempted request
            return _b_Result;

        }

        public string GetAPISearchResponse()
        {
            if (RequestInProcess())
            {
                return null;
            }
            else
            {
                return _mStr_Response;
            }
        }

        public string GetAPISearchError()
        {
            if (RequestInProcess())
            {
                return null;
            }

            return _mStr_SearchError;
        }

        public bool RequestInProcess()
        {
            return (_mEn_RequestState == GoogleSearchAPIRequestState.Requesting);
        }

        public GoogleSearchAPIRequestState APIRequestState
        {
            get
            {
                return _mEn_RequestState;
            }
        }

        public int GetAPIRequestStartIndex
        {
            get
            {
                return _mInt_StartIndex;
            }
        }

        //public enum GoogleSearchAPIRequestDomain
        //{
        //    [Description("google.com.au")]
        //    AUS,
        //    [Description("google.com")]
        //    USA,
        //    [Description("google.co.uk")]
        //    UK,
        //    [Description("google.co.nz")]
        //    NZ
        //}

        public enum GoogleSearchAPIRequestState
        {
            Ready = 0,
            Requesting = 1,
            ResponseComplete = 3,
            Error = 4,
        }
    }
}