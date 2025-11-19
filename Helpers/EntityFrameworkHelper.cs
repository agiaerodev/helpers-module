using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using MoreLinq.Extensions;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using Database = Microsoft.EntityFrameworkCore.Infrastructure.DatabaseFacade;




namespace Ihelpers.Helpers
{
   
    public static class EntityFrameworkCoreHelper
    {
        /// <summary>
        /// This class handles all requests dynamically from static entry methods
        /// </summary>
        /// <typeparam name="TElement">Type of query</typeparam>
        public class DbRawSqlQuery<TElement> : IAsyncEnumerable<TElement>
        {
            private readonly IAsyncEnumerable<TElement> _elements;

            internal DbRawSqlQuery(Database database, string sql, System.Data.CommandType commandType = System.Data.CommandType.Text, params object[] parameters) =>
                _elements = ExecuteQuery<TElement>(database, sql, null, null, commandType, parameters);

			internal DbRawSqlQuery(Database database, string sql, JArray? headers = null, JArray? selectedHeaders = null, System.Data.CommandType commandType = System.Data.CommandType.Text, params object[] parameters) =>
				_elements = ExecuteQuery<TElement>(database, sql, headers, selectedHeaders ,commandType, parameters);

			public IAsyncEnumerator<TElement> GetAsyncEnumerator(CancellationToken cancellationToken = new()) =>
                _elements.GetAsyncEnumerator(cancellationToken);

            //Handle most common EF result sets scripts.


            public async Task<TElement> SingleAsync() => await _elements.SingleAsync();
            public TElement Single() => Task.Run(SingleAsync).GetAwaiter().GetResult();
            public async Task<TElement> FirstAsync() => await _elements.FirstAsync();
            public TElement First() => Task.Run(FirstAsync).GetAwaiter().GetResult();
            public async Task<TElement?> SingleOrDefaultAsync() => await _elements.SingleOrDefaultAsync();
            public async Task<int> CountAsync() => await _elements.CountAsync();
            public async Task<List<TElement>> ToListAsync() => await _elements.ToListAsync();
            public List<TElement> ToList() => Task.Run(ToListAsync).GetAwaiter().GetResult();

        }
        /// <summary>
        /// Get attribute from property
        /// </summary>
        public static TAttribute? TryGetAttribute<TAttribute>(this PropertyInfo prop) where TAttribute : Attribute =>
            prop.GetCustomAttributes(true).TryGetAttribute<TAttribute>();

        /// <summary>
        /// Get attribute from type
        /// </summary>
        public static TAttribute? TryGetAttribute<TAttribute>(this Type t) where TAttribute : Attribute =>
            t.GetCustomAttributes(true).TryGetAttribute<TAttribute>();

        /// <summary>
        /// Gets attributes from an attribute list
        /// </summary>
        /// <returns></returns>
        public static TAttribute? TryGetAttribute<TAttribute>(this IEnumerable<object> attrs) where TAttribute : Attribute
        {
            foreach (object attr in attrs)
            {
                switch (attr)
                {
                    case TAttribute t:
                        {
                            return t;
                        }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the source string matches *any* of the passed-in strings (case insensitive)
        /// </summary>
        public static bool EqualsNoCase(this string? s, params string?[]? targets)
        {
            if (s == null && (targets == null || targets.Length == 0))
            {
                return true;
            }

            if (targets == null)
            {
                return false;
            }

            return targets.Any(t => string.Equals(s, t, StringComparison.OrdinalIgnoreCase));
        }


        public class EntityException : Exception
        {
            public EntityException(string message) : base(message)
            {
            }
        }

        public static TEntity GetEntity<TEntity>(this EntityEntry<TEntity> entityEntry)
            where TEntity : class => entityEntry.Entity;

        #region SqlQuery Interop

        /// <summary>
        /// This is a little bit ugly but given that this interop method is used just once,
        /// it is not worth spending more time on it.
        /// Made to handle "non async call" ToList()
        /// </summary>
        public static List<T> ToList<T>(this IOrderedAsyncEnumerable<T> e) =>
            Task.Run(() => e.ToListAsync().AsTask()).GetAwaiter().GetResult();

        private static string GetColumnName(this MemberInfo info) =>
            info.GetCustomAttributes().TryGetAttribute<ColumnAttribute>()?.Name ?? info.Name;

        /// <summary>
        /// Executes raw query with parameters and maps returned values to column property names of Model provided using reflection.
        /// Not all properties are required to be present in the model. If not present then they will be set to nulls.
        /// Openning us the dynamic freedom
        /// </summary>
        private static async IAsyncEnumerable<T> ExecuteQuery<T>(this Database database, string query, JArray? headers = null, JArray? selectedHeaders = null, System.Data.CommandType commandType = System.Data.CommandType.Text, params object[]? parameters)
        {
            await using DbCommand command = database.GetDbConnection().CreateCommand();
            command.CommandText = query;
            command.CommandType = commandType;
            command.CommandTimeout = 100000000;
            if (database.CurrentTransaction != null)
            {
                command.Transaction = database.CurrentTransaction.GetDbTransaction();
            }

            foreach (var parameter in parameters)
            {
                // They are supposed to be of SqlParameter type but are passed as objects.
                command.Parameters.Add(parameter);
            }

            await database.OpenConnectionAsync();
            await using DbDataReader reader = await command.ExecuteReaderAsync();
            var t = typeof(T);

            if (t.IsGenericType && t.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                t = Nullable.GetUnderlyingType(t)!;
            }

            var lstColumns = t
                .GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .ToList();

            //try to instance the Type, only applies for dictionary and clases but not for primitives, so this try is needed.
            T? activator = default(T); try { activator = Activator.CreateInstance<T>(); } catch { };

            //Instance the dictionary if needed.
            Dictionary<string, List<object?>>? resultDic = t.Name.Contains("Dictionary") ? activator as Dictionary<string, List<object?>> : null;
            
            while (await reader.ReadAsync())
            {
                if (t.IsPrimitive || t == typeof(string) || t == typeof(DateTime) || t == typeof(Guid) || t == typeof(decimal))
                {
                    var val = await reader.IsDBNullAsync(0) ? null : reader[0];
                    yield return (T)val!;
                }
                else
                {
                    T newObject = Activator.CreateInstance<T>();


                    if (resultDic is not null)
                    {


                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            string name = reader.GetName(i);

							if (headers != null)
                            {
                                //Only selected columns in report 

                                


                                //
                                var columnCfg = headers.Where(header => header["id"].ToString() == name).FirstOrDefault();

                                if(columnCfg != null )
                                {
                                    name = columnCfg["title"]?.ToString() ?? name;
                                }
                            }
                            

                            object? val = await reader.IsDBNullAsync(i) ? null : reader[i];


                            var isHeaderAdded = resultDic.ContainsKey(name);

                            if (isHeaderAdded)
                            {
                                resultDic[name]?.Add(val);
                            }
                            else
                            {
                                resultDic?.Add(name, new List<object?> { val is null ? null : val });

                            }

                        }
                    }
                    else
                    {
                        for (var i = 0; i < reader.FieldCount; i++)
                        {
                            var name = reader.GetName(i);
                            var val = await reader.IsDBNullAsync(i) ? null : reader[i];
                            var prop = lstColumns.FirstOrDefault(a => a.GetColumnName().EqualsNoCase(name));

                            if (prop == null)
                            {
                                continue;
                            }

                            prop.SetValue(newObject, val, null);
                        }

                    }

                    if (resultDic is null) yield return newObject;

                }
            }

            yield return activator;
        }

        #endregion


        /// <summary>
        /// Static method entry from SQLQUERY
        /// </summary>
        /// <returns></returns>
        public static DbRawSqlQuery<TElement> FromSqlQuery<TElement>(this Database database, string sql, System.Data.CommandType commandType = System.Data.CommandType.Text, params object[] parameters) =>
            new(database, sql, commandType, parameters);

        /// <summary>
        /// Converts a query result to a dictionary of strings for reporting or exporting purposes.
        /// </summary>
        /// <param name="database">The database to query.</param>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="commandType">The type of command to execute Text or StoredProcedure.</param>
        /// <param name="parameters">The parameters to pass to the SQL query.</param>
        /// <returns>A DbRawSqlQuery object containing a dictionary of strings, where each key is a database header field and the values are the corresponding database field rows.</returns>
        public static DbRawSqlQuery<Dictionary<string, List<object?>>> FromSqlQueryToDictionary(this Database database, string sql, System.Data.CommandType commandType = CommandType.Text, params object[] parameters) =>
           new(database, sql, commandType, parameters);


        [Obsolete("Headers no longer used")]
        public static DbRawSqlQuery<Dictionary<string, List<string>>> FromSqlQueryToDictionary(this Database database, string sql, JArray? headers = null, JArray? selectedColumns = null, System.Data.CommandType commandType = CommandType.Text, params object[] parameters) =>
		   new(database, sql, headers, selectedColumns, commandType, parameters);

        /// <summary>
        /// Checks if a given stored procedure exists in the specific database.
        /// </summary>
        /// <param name="database">The database to check procedure.</param>
        /// <param name="procedureName">the procedure name.</param>
        /// <param name="parameters">The parameters to pass to the SQL query.</param>
        /// <returns>A boolean that indicates if stored procedure exists (true) or not exists (false).</returns>
        public static async Task<bool> StoredProcedureExists(this Database database,
        string procedureName, params object[] parameters)
        {

            string procedureExistQuery = String.Format(@"SELECT top 1 name from sys.objects WHERE  object_id = OBJECT_ID(N'{0}') AND type IN ( N'P', N'PC' ) ", procedureName);


            var res = await new DbRawSqlQuery<string?>(database, procedureExistQuery, System.Data.CommandType.Text, parameters).FirstOrDefaultAsync();

            return res is not null;
        }


    }
}
