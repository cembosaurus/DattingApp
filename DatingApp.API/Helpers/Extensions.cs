using System;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DatingApp.API.Helpers
{
    public static class Extensions
    {
        public static void AddExceptionIntoResponseHeader(this HttpResponse response, string message)
        {
            response.Headers.Add("Shit-Happens", message);
            response.Headers.Add("Access-Control-Expose-Headers", "Shit-Happens");
            response.Headers.Add("Access-Control-Allow-Origin", "*");
        }


        public static void AddPagination(this HttpResponse response, int currentPage, int itemsPerPage, int totalItems, int totalPages )
        {
            // ... OR anonymous object ...
            var paginationHeader = new PaginationHeader(currentPage, itemsPerPage, totalItems, totalPages);

            //... format names of the HTTP parameters in HERADER to CAMELCASE
            var camelCaseFormater = new JsonSerializerSettings();
            camelCaseFormater.ContractResolver = new CamelCasePropertyNamesContractResolver();

            response.Headers.Add("Pagination", JsonConvert.SerializeObject(paginationHeader, camelCaseFormater));
            response.Headers.Add("Access-Control-Expose-Headers", "Pagination");
        }


        public static int Age(this DateTime dateTime)
        {

            var age = DateTime.Now.Year - dateTime.Year;

            if(DateTime.Today < dateTime.AddYears(age))
                age--;

            return age;

        }
    }
}