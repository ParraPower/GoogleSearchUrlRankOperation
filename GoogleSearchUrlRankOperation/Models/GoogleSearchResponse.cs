using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
//using GoogleSearchAPIClient;

namespace GoogleSearchUrlRankOperation.Models
{
    public class GoogleSearchResponse
    {
        private List<GoogleSearchResponseItem> _mLst_ResponseItems;
        public GoogleSearchResponseItem[] ResponseItems
        {
            get
            {
                return _mLst_ResponseItems.OrderBy(x => x.Rank).ToArray();
            }
        }

        private string _mStr_UrlToFind = "";

        public GoogleSearchResponse(string str_UrlToFind)
        {
            this._mLst_ResponseItems = new List<GoogleSearchResponseItem>();
            this._mStr_UrlToFind = str_UrlToFind;
        }

        public void AddResultsFromAPI_JSONResponse(string str_API_JSONResponse)
        {
            if (string.IsNullOrWhiteSpace(str_API_JSONResponse))
                return;

            try
            {
                var _obj_JSONSerializer = new JavaScriptSerializer();

                
                // parse and access the returned JSON into a Dictionary of string keyed objects (_dct_ResponseProperties)
                object obj_API_JSONResponse = _obj_JSONSerializer.Deserialize<object>(str_API_JSONResponse);

                Dictionary<string, object> _dct_ResponseProperties = (Dictionary<string, object>)obj_API_JSONResponse;

                #region Start Index
                // get the Queries object sitting within the KeyValuePairs in _dct_ResponseProperties
                object _obj_ResponseQueries = _dct_ResponseProperties.First(x => x.Key.ToLower().Equals("queries", StringComparison.OrdinalIgnoreCase)).Value;

                // convert it to a dictionary
                Dictionary<string, object> _dct_ResponseQueriesProperties = (Dictionary<string, object>)_obj_ResponseQueries;

                // get the Queries.Request object sitting within the KeyValuePairs in _dct_ResponseQueriesProperties
                object _obj_ResponseQueriesRequest = _dct_ResponseQueriesProperties.First(x => x.Key.ToLower().Equals("request", StringComparison.OrdinalIgnoreCase)).Value;

                // convert it into an array of Objects
                object[] _arr_ResponseQueriesRequestProperties = (object[])_obj_ResponseQueriesRequest;

                // the first index is always the index with the request's information.
                // It itself is a Dictionary of string keyed objects, 
                // so convert it to that and directly access the Queries.Request[0] properties
                Dictionary<string, object> _dct_ResponseQueriesRequestProperties = (Dictionary<string, object>)_arr_ResponseQueriesRequestProperties[0];

                // Now we can first Queries.Request[0].StartIndex and get it from _dct_ResponseQueriesRequestProperties
                int _int_StartIndex = int.Parse(_dct_ResponseQueriesRequestProperties.First(x => x.Key.ToLower().ToLower().Equals("startindex", StringComparison.OrdinalIgnoreCase)).Value.ToString());

                #endregion

                #region Handle Items
                // Parse the search results into individual instances of the the class: "GoogleSearchAPIResponseItem"
                if (_dct_ResponseProperties.Any(x => x.Key.ToLower().Equals("items", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        object _obj_ResponseItems = _dct_ResponseProperties.First(x => x.Key.ToLower().Equals("items", StringComparison.OrdinalIgnoreCase)).Value;

                        ArrayList _arr_ResponseItems = new ArrayList((Object[])_obj_ResponseItems);

                        List<GoogleSearchResponseItem> _lst_GS_APIResponseItems = new List<GoogleSearchResponseItem>();

                        // Build a list of results
                        foreach (object _obj_ResponseItem in _arr_ResponseItems)
                        {
                            Dictionary<string, object> _dct_ResponseItem = (Dictionary<string, object>)_obj_ResponseItem;
                            if (_dct_ResponseItem
                                .First(x => x.Key.ToLower().Equals("link", StringComparison.OrdinalIgnoreCase) && x.Value != null)
                                    .Value.ToString().ToLower()
                                        .Contains(_mStr_UrlToFind.ToLower())) //see if url to find is contained within the url of this response item
                            {

                                // if so, add to list of results along with rank, title of page and link to page
                                GoogleSearchResponseItem _obj_GS_APIResponseItem = GoogleSearchResponseItem.GetFromAPI_JSONResponseItemDictionary(
                                    _int_StartIndex,
                                    ((Dictionary<string, object>)_obj_ResponseItem)
                                );

                                _lst_GS_APIResponseItems.Add(_obj_GS_APIResponseItem);
                                
                            }
                            ++_int_StartIndex;
                        }
                        // move the compiled list of search results into GoogleSearchAPIResponse's search results list
                        this._mLst_ResponseItems.AddRange(_lst_GS_APIResponseItems);
                    }
                    catch
                    {
                        // if anything should happen
                    }
                }

                #endregion

                return;
            }
            catch
            {
                // if anything should happen
            }
        }

        //public GoogleSearchResponse(GoogleSearchAPIResponse obj_APIResponse)
        //{
        //    _mLst_ResponseItems = new List<GoogleSearchResponseItems>();
        //    AddResultsFromAPIResponse(obj_APIResponse);
        //}

        //public void AddResultsFromAPIResponse(GoogleSearchAPIResponse obj_APIResponse)
        //{
        //    if (obj_APIResponse != null && obj_APIResponse.ResponseItems.Length > 0)
        //    {
        //        if (_mLst_ResponseItems == null)
        //            _mLst_ResponseItems = new List<GoogleSearchResponseItems>();

        //        List<GoogleSearchResponseItems> lst_ResponseItems = new List<GoogleSearchResponseItems>();

        //        foreach (GoogleSearchAPIResponseItem obj_responseItem in obj_APIResponse.ResponseItems)
        //        {
        //            _mLst_ResponseItems.Add(new GoogleSearchResponseItems(obj_responseItem));
        //        }
        //    }
        //}
    }

    public class GoogleSearchResponseItem
    {

        public static GoogleSearchResponseItem GetFromAPI_JSONResponseItemDictionary(int int_Rank, Dictionary<string, object> dct_ResponseItem)
        {
            if (dct_ResponseItem != null)
            {
                GoogleSearchResponseItem _obj_GS_APIResponseItem = new GoogleSearchResponseItem();

                _obj_GS_APIResponseItem._mInt_Rank = int_Rank;

                if (dct_ResponseItem.Any(x => x.Key.ToLower().Equals("title", StringComparison.OrdinalIgnoreCase) && x.Value != null))
                    _obj_GS_APIResponseItem._mStr_Title = dct_ResponseItem.First(x => x.Key.ToLower().Equals("title", StringComparison.OrdinalIgnoreCase)).Value.ToString();

                if (dct_ResponseItem.Any(x => x.Key.ToLower().Equals("link", StringComparison.OrdinalIgnoreCase) && x.Value != null))
                    _obj_GS_APIResponseItem._mStr_Link = dct_ResponseItem.First(x => x.Key.ToLower().Equals("link", StringComparison.OrdinalIgnoreCase)).Value.ToString();


                return _obj_GS_APIResponseItem;
            }

            return null;
        }

        //public GoogleSearchResponseItems (GoogleSearchAPIResponseItem obj_APIResponseItem)
        //{
        //    if (obj_APIResponseItem != null)
        //    {
        //        this._mInt_Rank = obj_APIResponseItem.Rank;
        //        this._mStr_Link = obj_APIResponseItem.Link;
        //        this._mStr_Title = obj_APIResponseItem.Title;
        //    }
        //}

        private int _mInt_Rank;
        public int Rank
        {
            get
            {
                return _mInt_Rank;
            }
        }

        private string _mStr_Title;
        public string Title
        {
            get
            {
                return _mStr_Title;
            }
        }

        private string _mStr_Link;
        public string Link
        {
            get
            {
                return _mStr_Link;
            }
        }
    }
}