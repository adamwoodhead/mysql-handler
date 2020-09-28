using System;
using System.Collections.Generic;
using System.Text;

namespace MySQL_Handler
{
    public class SelectLimiter
    {
        internal bool OrderResults { get; set; }

        internal string OrderField { get; set; }

        internal OrderBy OrderBy { get; set; } = OrderBy.None;

        internal int Limit { get; set; }

        public SelectLimiter(int limit, string orderBy = null, OrderBy order = OrderBy.None)
        {
            OrderResults = (order != OrderBy.None && orderBy != null) ? true : false;
            OrderField = orderBy;
            OrderBy = order;
            Limit = limit;
        }

        public string QueryData
        {
            get
            {
                string result = null;

                if (OrderResults)
                {
                    result = $"ORDER BY `{OrderField}` {((OrderBy == OrderBy.Ascending) ? "ASC" : "DESC")}";
                    if (Limit > 0)
                    {
                        result += $" LIMIT {Limit};";
                    }
                }
                else
                {
                    if (Limit > 0)
                    {
                        result = $" LIMIT {Limit};";
                    }
                }

                return result;
            }
        }
    }
}
