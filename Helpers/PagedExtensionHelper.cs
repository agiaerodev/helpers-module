using Microsoft.EntityFrameworkCore;

namespace Ihelpers.Helpers
{
    /// <summary>
    /// Class `PaginatedList` represents a paginated list of items with additional metadata like current page, total pages, etc.
    /// </summary>
    /// <typeparam name="T">The type of the items in the list.</typeparam>
    public class PaginatedList<T> : List<T>
    {
        /// <summary>
        /// The index of the current page.
        /// </summary>
        public int PageIndex { get; private set; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages { get; private set; }

        /// <summary>
        /// Additional metadata about the pagination, and page position.
        /// </summary>
        public object meta { get; set; }

        /// <summary>
        /// Creates a new instance of `PaginatedList` from the given items and metadata.
        /// </summary>
        /// <param name="items">The items in the list.</param>
        /// <param name="count">The total count of items in the original query, before pagination.</param>
        /// <param name="pageIndex">The index of the current page (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        public PaginatedList(List<T> items, int count, int? pageIndex, int? pageSize)
        {
            if (pageIndex != null)
            {
                PageIndex = (int)pageIndex;
                TotalPages = (int)Math.Ceiling(count / (double)pageSize);
                meta = new
                {
                    page = new
                    {
                        total = count,
                        HasNextPage = (pageIndex < TotalPages),
                        HasPreviousPage = (pageIndex > 1),
                        currentPage = pageIndex,
                        perPage = pageSize
                    }

                };
            }
            this.AddRange(items);
        }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => PageIndex > 1;

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage => PageIndex < TotalPages;

        /// <summary>
        /// Creates a new instance of `PaginatedList` from the given query and pagination parameters.
        /// </summary>
        /// <param name="query">The query to paginate.</param>
        /// <param name="pageIndex">The index of the current page (1-based).</param>
        /// <param name="pageSize">The number of items per page.</param>
        /// <returns>An asynchronous operation that returns a `PaginatedList` instance.</returns>
        public static async Task<PaginatedList<T>> CreateAsync(IQueryable<T> query, int? pageIndex, int? pageSize)
        {
            var count = 0;


            //if isset page
            if (pageIndex != null)
            {

                //first query to know the total of the items
                count = await query.CountAsync();

                //pagination with take

                query = pageSize == null
                        ? query.Skip((int)pageIndex - 1)
                        : query.Skip(((int)pageIndex - 1) * (int)pageSize).Take((int)pageSize);
                               
            }
            else
                //if the query 
                if (pageSize != null) query = query.Take((int)pageSize);

#if DEBUG
            string queryString = query.ToQueryString();
#endif

            var items = await query.ToListAsync();


            return new PaginatedList<T>(items, count, pageIndex, pageSize);
        }


    }
}
