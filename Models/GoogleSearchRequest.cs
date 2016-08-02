using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace GoogleSearchUrlRankOperation.Models
{
    public class GoogleSearchRequest
    {
        [Display(Name = "Search terms")]
        public string SearchTerms
        { get; set; }

        [Display(Name = "Url to find")]
        public string UrlToFind
        { get; set; }
    }
}