using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingApp.API.Helpers
{
    //... this class is generic list taking IQueryable of any entity and instantiating itself as a paged list of any entity 'List<T>'

    public class PagedList<T> : List<T>
    {

        public int CurrentPage;
        public int TotalPages;
        public int PageSize;
        public int TotalCount;


        public PagedList( List<T> items, int count, int pageNumber, int pageSize )
        {

            this.CurrentPage = pageNumber;
            this.PageSize = pageSize;
            this.TotalCount = count;
            this.TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            //... List<T>:
            AddRange(items);

        }

        //...................................................................................

        // ... use STATIC method to create instance of THIS List<T> class (factory)
        // ... insert repository (IQueryable) into this method and returns PagedList IEnumerable

        public static async Task<PagedList<T>> CreateAsync(IQueryable<T> source, int pageNumber, int pageSize)
        {
            var count = await source.CountAsync();
            var items = await source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();
            return new PagedList<T>(items, count, pageNumber, pageSize);
        }


    }
}
