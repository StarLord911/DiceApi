using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Data
{ 
    public class PaginatedList<T>
    {
        public List<T> Items { get; private set; }

        public int PageIndex { get; private set; }

        public int TotalPages { get; private set; }

        public PaginatedList(List<T> items, int totalPages, int pageIndex)
        {
            PageIndex = pageIndex;
            TotalPages = totalPages;

            Items = items;
        }

        public static PaginatedList<T> Create(List<T> items, int totalPages, int pageIndex)
        {
            return new PaginatedList<T>(items, totalPages, pageIndex);
        }
    }
}
